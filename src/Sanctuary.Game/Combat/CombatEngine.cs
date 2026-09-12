using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Sanctuary.Game.Entities;
using Sanctuary.Game.Resources.Definitions.Combat;
using Sanctuary.Game.Zones;
using Sanctuary.Packet;

namespace Sanctuary.Game.Combat;

/// <summary>
/// A player's attack, from a toolbar press (op36/10) or a click-to-attack (op32/1, 32/3): resolve the target
/// and the equipped weapon's ability from CombatJobs.json / CombatAbilities.json, gate it (swing pace or energy),
/// play the cast, then land the damage as the swing connects. Ported from the linear pipeline in Sulphural/main's
/// AbilityPacketClientRequestStartAbilityHandler.Combat.cs (00c1db34, e9912598, 2567cc0e) with its hard-coded
/// per-job ability tables replaced by our data files, and without projectiles, traits, buffs or summons.
/// </summary>
public static class CombatEngine
{
    /// <summary>The generic "you can't do that" string the item abilities already use on a refused press.</summary>
    public const int FailureStringId = 3079;

    // Effect-tag ids for attached trails. Its own range, clear of the consumable abilities' counter (5000+).
    private static int _trailTagCounter = 20_000;

    public const int BasicSlot = 0;
    public const int SpecialSlot = 1;

    /// <summary>
    /// Start the ability on toolbar slot <paramref name="slot"/> against <paramref name="selectedGuid"/> (0 = let
    /// the server pick the nearest live enemy in reach). Returns false when nothing was started.
    /// </summary>
    public static bool TryAttack(IResourceManager resources, Player player, int slot, ulong selectedGuid)
    {
        if (player.IsDead)
            return false;

        if (!resources.CombatJobs.TryGetValue(player.ActiveProfileId, out var kit))
            return Fail(player);

        var (basic, special) = player.ResolveWeaponAbilities(kit);

        var ability = slot switch
        {
            BasicSlot => basic,
            SpecialSlot => special,
            _ => null
        };

        if (ability is null)
            return Fail(player);

        var isBasic = slot == BasicSlot;
        var settings = resources.CombatSettings.Abilities;
        var zone = player.Zone;

        // Gate. Basic: one swing per recast window (the client sends presses faster than the clip plays; the
        // extras are dropped silently). Special: the energy bar is the cooldown - it costs the whole bar and
        // refills at the captured 4/s, so the button greys for cost / regen seconds.
        int lockMs;

        if (isBasic)
        {
            if (!player.TryGateBasicSwing(kit.BasicRecastMs))
                return false;

            lockMs = kit.BasicRecastMs;
        }
        else
        {
            if (!player.TrySpendEnergy(ability.EnergyCost))
                return Fail(player);

            lockMs = ability.EnergyCost > 0
                ? ability.EnergyCost * 1000 / Math.Max(1, kit.Energy.RegenPerSecond)
                : kit.BasicRecastMs;
        }

        // Target: the client's selection when it is a live enemy within reach (plus slack for lag), else the
        // nearest live enemy in reach. A swing at nothing still animates but lands nothing.
        var reach = isBasic && kit.BasicAutoTargetReach > 0f ? kit.BasicAutoTargetReach : kit.AutoTargetReach;
        var target = ResolveTarget(zone, player, selectedGuid, reach, settings.SelectedTargetRangeSlack);
        var targets = CollectTargets(zone, player, ability, target);

        var actionTime = isBasic ? lockMs / 1000f : settings.SpecialActionTimeSeconds;
        var damageDelay = isBasic ? lockMs * settings.BasicDamageDelayFraction / 1000f : settings.SpecialDamageDelaySeconds;
        var targetGuid = target?.Guid ?? player.Guid;

        // The cast: animation + one-shot cast FX on the caster, broadcast so bystanders see the swing.
        player.SendTunneledToVisible(new AbilityPacketStartCasting
        {
            Unknown = player.Guid,
            Unknown2 = targetGuid,
            CompositeEffectId = ability.CastEffectId,
            Animation = ability.AnimationId > 0 ? ability.AnimationId : -1,
            AbilityId = slot + 1,
            ActionTime = actionTime,
            HasActionProgress = false
        }, sendToSelf: true);

        // An attached trail (Leg Sweep's foot beam) burns for the swing and is then pulled by its tag. The
        // composite effect has no end trigger of its own, so sent as a cast effect it never stopped: the first
        // play-test left a blue streak stuck to the character for the rest of the session, job switch included.
        if (ability.TrailEffectId > 0)
        {
            var tagId = Interlocked.Increment(ref _trailTagCounter);

            player.SendTunneledToVisible(new PlayerUpdatePacketAddEffectTagCompositeEffect
            {
                Guid = player.Guid,
                TagId = tagId,
                CompositeEffectId = ability.TrailEffectId,
                SourceGuid = player.Guid
            }, sendToSelf: true);

            player.SendTunneledToVisibleDelayed(new PlayerUpdatePacketRemoveEffectTagCompositeEffect
            {
                Guid = player.Guid,
                TagId = tagId
            }, ability.TrailDurationMs > 0 ? ability.TrailDurationMs : lockMs, sendToSelf: true);
        }

        // Cooldown sweep + grey on the pressed button for as long as the server actually holds it.
        player.SendTunneled(new AbilityPacketMeleeRefresh { CooldownMs = lockMs });

        // The radial sweep only renders against a real target (Sulphural, live-confirmed 2026-07-25).
        if (target is not null)
        {
            player.SendTunneled(new AbilityPacketLaunchAndLand
            {
                Guid = player.Guid,
                Guid2 = target.Guid,
                Guid3 = target.Guid,
                Position = player.Position
            });
        }

        if (targets.Count == 0)
            return true;

        // Engaging a live enemy is what puts you in world combat (weapon out, damage numbers); swinging at air
        // does not.
        player.EnterWorldCombat();

        var level = player.ActiveProfile.Rank;
        var damage = PlayerDamage.Scale(ability.Damage, level, settings);
        var heal = PlayerDamage.Scale(ability.HealAmount, level, settings);

        var logger = zone.Logger;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Max(0f, damageDelay)));
                ResolveHits(player, ability, targets, damage, heal, settings);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ability {ability} damage resolution failed for {guid}.", ability.Id, player.Guid);
            }
        });

        return true;
    }

    private static void ResolveHits(Player player, AbilityDefinition ability, IReadOnlyList<CombatNpc> targets,
        int damage, int heal, AbilityCombatSettings settings)
    {
        if (player.IsDead)
            return;

        if (heal > 0)
            player.Heal(heal);

        var landed = false;

        foreach (var target in targets)
        {
            if (target.IsDead || !target.IsAlive)
                continue; // died to an earlier hit

            landed = true;

            // TakeDamage drops the hit points, pushes the bar, aggros the enemy onto the attacker and routes a
            // kill to the zone. The per-hit FEEDBACK is op32/7 AttackProcessed, the same packet an enemy uses
            // when it hits us: it carries the floating number, the bar and the hit FX itself. op35/35's number
            // only draws while the global in-world-combat switch is on, and that switch also puts a bar on
            // every nameplate in view, friendly NPCs included (first play-test) - so the switch stays off.
            target.TakeDamage(damage, player, broadcastHitNumber: !settings.SendAttackProcessedOnHit);

            if (settings.SendAttackProcessedOnHit)
            {
                player.SendTunneledToVisible(new CombatPacketAttackProcessed
                {
                    AttackerGuid = player.Guid,
                    TargetGuid = target.Guid,
                    Damage = damage,
                    MaxHealth = target.MaxHealth,
                    CurrentHealth = target.Health,
                    CompositeEffectId = ability.HitEffectId
                }, sendToSelf: true);
            }

            // AttackProcessed already plays the hit effect on the target; only play it separately without it.
            if (ability.HitEffectId > 0 && !settings.SendAttackProcessedOnHit)
            {
                player.SendTunneledToVisible(new PlayerUpdatePacketPlayCompositeEffect
                {
                    Guid = target.Guid,
                    CompositeEffectId = ability.HitEffectId,
                    Position = target.Position
                }, sendToSelf: true);
            }

            if (ability.EnemyExtraEffectId > 0)
            {
                player.SendTunneledToVisible(new PlayerUpdatePacketPlayCompositeEffect
                {
                    Guid = target.Guid,
                    CompositeEffectId = ability.EnemyExtraEffectId,
                    Position = target.Position
                }, sendToSelf: true);
            }
        }

        if (!landed)
            return;

        player.EnterWorldCombat();

        // The caster-side landing FX plays once however many enemies were hit.
        if (ability.CasterEndEffectId > 0)
        {
            player.SendTunneledToVisible(new PlayerUpdatePacketPlayCompositeEffect
            {
                Guid = player.Guid,
                CompositeEffectId = ability.CasterEndEffectId,
                Position = player.Position
            }, sendToSelf: true);
        }
    }

    /// <summary>
    /// Who the swing lands on, in order: the enemy the client has selected, then the one already being fought,
    /// then the nearest. The middle step is what makes a camp fightable - picking the nearest every swing
    /// spread the damage over whichever enemy happened to be closest at that instant, so nothing ever died and
    /// each one looked like it was healing itself (first combat play-test).
    /// </summary>
    public static CombatNpc? ResolveTarget(IZone zone, Player player, ulong selectedGuid, float reach, float slack)
    {
        if (selectedGuid != 0 && zone.TryGetNpc(selectedGuid, out var selected) && selected is CombatNpc enemy
            && IsAttackable(enemy) && DistanceSquared(player, enemy) <= reach * slack * (reach * slack))
        {
            player.CombatTargetGuid = enemy.Guid;
            return enemy;
        }

        if (player.CombatTargetGuid != 0 && zone.TryGetNpc(player.CombatTargetGuid, out var previous)
            && previous is CombatNpc sticky && IsAttackable(sticky)
            && DistanceSquared(player, sticky) <= reach * slack * (reach * slack))
        {
            return sticky;
        }

        CombatNpc? nearest = null;
        var best = reach * reach;

        foreach (var npc in zone.Npcs)
        {
            if (npc is not CombatNpc candidate || !IsAttackable(candidate))
                continue;

            var distance = DistanceSquared(player, candidate);

            if (distance >= best)
                continue;

            best = distance;
            nearest = candidate;
        }

        if (nearest is not null)
            player.CombatTargetGuid = nearest.Guid;

        return nearest;
    }

    // Area abilities hit every live enemy within AoeRadius of the CASTER (the whole pack); everything else hits
    // the one resolved target.
    private static List<CombatNpc> CollectTargets(IZone zone, Player player, AbilityDefinition ability, CombatNpc? target)
    {
        var targets = new List<CombatNpc>();

        if (IsArea(ability) && ability.AoeRadius > 0f)
        {
            var radiusSquared = ability.AoeRadius * ability.AoeRadius;

            foreach (var npc in zone.Npcs)
            {
                if (npc is CombatNpc candidate && IsAttackable(candidate) && DistanceSquared(player, candidate) <= radiusSquared)
                    targets.Add(candidate);
            }

            if (target is not null && !targets.Contains(target))
                targets.Add(target);

            return targets;
        }

        if (target is not null)
            targets.Add(target);

        return targets;
    }

    private static bool IsArea(AbilityDefinition ability) =>
        string.Equals(ability.EffectType, "AoeDamage", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(ability.EffectType, "AoeDamageHeal", StringComparison.OrdinalIgnoreCase);

    private static bool IsAttackable(CombatNpc npc) => npc.Visible && !npc.IsDead && npc.IsAlive && npc.IsHostile;

    private static float DistanceSquared(Player player, Npc npc)
    {
        var dx = npc.Position.X - player.Position.X;
        var dz = npc.Position.Z - player.Position.Z;
        return dx * dx + dz * dz;
    }

    private static bool Fail(Player player)
    {
        player.SendTunneled(new AbilityPacketFailed { StringId = FailureStringId });
        return false;
    }
}
