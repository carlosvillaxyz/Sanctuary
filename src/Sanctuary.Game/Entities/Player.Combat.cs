using System;
using System.Numerics;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Sanctuary.Game.Helpers;
using Sanctuary.Game.Resources.Definitions.Combat;
using Sanctuary.Packet;
using Sanctuary.Packet.Common;

namespace Sanctuary.Game.Entities;

/// <summary>
/// The player's side of overworld combat: server-authoritative health, damage intake, knockout and revive,
/// regen, energy, and the client's "in world combat" flags. Ported from Sulphural/main's Player.cs (c34648df
/// death system, e8c8a3d4 dodge, 00c1db34 combat-state machine) onto our LevelStats/CombatJobs data.
/// </summary>
public sealed partial class Player
{
    /// <summary>Current health; the client only mirrors what we send.</summary>
    public int CurrentHealth { get; private set; }

    public int MaxHealth => Stats.TryGetValue(CharacterStatId.MaxHealth, out var stat) ? stat.Int : 0;

    public bool IsDead { get; private set; }

    /// <summary>Where the player fell; "Revive here" brings them back to it.</summary>
    public Vector4 DeathPosition { get; private set; }

    /// <summary>When the player last took combat damage; gates health regen.</summary>
    public DateTime LastCombatDamageAt { get; private set; } = DateTime.MinValue;

    private long _lastWorldCombatTicks;
    private bool _worldCombatActive;
    private long _invulnerableUntilTicks;
    private int _reviveGeneration;

    private PlayerCombatSettings CombatSettings => _resourceManager.CombatSettings.Player;
    private AbilityCombatSettings AbilitySettings => _resourceManager.CombatSettings.Abilities;

    public bool IsCombatJob => ActiveProfile.Type == LevelStats.CombatProfileType;

    public bool IsInvulnerable => Environment.TickCount64 < _invulnerableUntilTicks;

    public bool InWorldCombat => _lastWorldCombatTicks != 0
        && Environment.TickCount64 - _lastWorldCombatTicks < CombatSettings.OutOfCombatSeconds * 1000L;

    /// <summary>The captures show DamageReductionPercent 100 for ~7 s after a revive and ~42 s after login.</summary>
    public void SetInvulnerable(int seconds)
    {
        var until = Environment.TickCount64 + seconds * 1000L;

        if (until > _invulnerableUntilTicks)
            _invulnerableUntilTicks = until;
    }

    /// <summary>Called once the client has finished loading into the world.</summary>
    public void OnEnteredWorld()
    {
        // The active job's max health, regen and a full bar. Without this the server keeps ClientPcData's
        // 2,500 placeholder and a CurrentHealth of 0, so the first enemy hit knocked the player out at once
        // and the revive that followed handed back 2,500 hit points (first combat play-test).
        ApplyLevelStats();

        SetInvulnerable(CombatSettings.LoginInvulnerableSeconds);
    }

    #region Health

    /// <summary>Push current/max health to this client and its nameplate for everyone else.</summary>
    public void SendHealth()
    {
        var max = MaxHealth;

        SendTunneled(new ClientUpdatePacketHitpoints { CurrentHitpoints = CurrentHealth, MaxHitpoints = max });

        SendTunneledToVisible(new PlayerUpdatePacketUpdateHitpoints
        {
            Guid = Guid,
            Hitpoints = CurrentHealth,
            MaxHitpoints = max
        });
    }

    public void RefillHealth()
    {
        CurrentHealth = MaxHealth;
        SendHealth();
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0)
            return;

        CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
        SendHealth();
    }

    // Once a second: health regen toward max. Out of combat = the job's HitPointRegen stat (LevelStats.json);
    // within the out-of-combat window after a hit = max / InCombatHealthRegenDivisor (captures: 100 for combat
    // jobs, 400 for the rest).
    private void RegenTick()
    {
        // 0 while alive = not initialised yet (ApplyLevelStats fills it on zone entry).
        if (IsDead || CurrentHealth <= 0)
            return;

        var max = MaxHealth;

        if (max <= 0 || CurrentHealth >= max)
            return;

        var inCombat = DateTime.UtcNow - LastCombatDamageAt < TimeSpan.FromSeconds(CombatSettings.OutOfCombatSeconds);

        int regen;

        if (inCombat)
        {
            var divisor = IsCombatJob
                ? CombatSettings.InCombatHealthRegenDivisorCombatJob
                : CombatSettings.InCombatHealthRegenDivisorNonCombatJob;

            regen = max / Math.Max(1, divisor);
        }
        else
        {
            regen = Stats.TryGetValue(CharacterStatId.HitPointRegen, out var stat) ? stat.Int : max / 20;
        }

        CurrentHealth = Math.Min(max, CurrentHealth + Math.Max(1, regen));
        SendHealth();
    }

    #endregion

    #region Taking damage

    /// <summary>
    /// Reduce an enemy's raw hit by Defense / (Defense + K), then the flat DamageReductionAmount, then
    /// DamageReductionPercent (the empirical fit from the 2014 captures). 0 while invulnerable.
    /// </summary>
    public int MitigateIncomingDamage(int rawDamage)
    {
        if (rawDamage <= 0 || IsInvulnerable)
            return 0;

        var defense = Stats.TryGetValue(CharacterStatId.Defense, out var defenseStat) ? defenseStat.Int : 0;
        var flat = Stats.TryGetValue(CharacterStatId.DamageReductionAmount, out var flatStat) ? flatStat.Int : 0;
        var percent = Stats.TryGetValue(CharacterStatId.DamageReductionPercent, out var percentStat) ? percentStat.Int : 0;

        return Mitigate(rawDamage, defense, flat, percent, CombatSettings.DefenseConstant);
    }

    /// <summary>Pure form of <see cref="MitigateIncomingDamage"/>, shared with the tests.</summary>
    public static int Mitigate(int rawDamage, int defense, int flatReduction, int percentReduction, float defenseConstant)
    {
        if (rawDamage <= 0 || percentReduction >= 100)
            return 0;

        var damage = rawDamage * (1f - defense / (defense + Math.Max(1f, defenseConstant)));

        damage -= flatReduction;

        if (percentReduction > 0)
            damage *= 1f - percentReduction / 100f;

        return Math.Max(1, (int)MathF.Round(damage));
    }

    /// <summary>
    /// Roll to evade an enemy swing. On a dodge the attacker still plays its swing, the client draws the
    /// floating "Miss" text over this player (op32/5 - Sulphural found the dedicated Dodge text is gated
    /// client-side and never renders) and the sidestep clip plays. Returns true so the caller deals no damage.
    /// </summary>
    public bool TryDodge(ulong attackerGuid)
    {
        if (IsDead)
            return false;

        var chance = AbilitySettings.DodgeChancePercent
            + (Stats.TryGetValue(CharacterStatId.MeleeAvoidance, out var avoidance) ? avoidance.Int : 0);

        if (chance <= 0 || Random.Shared.Next(100) >= chance)
            return false;

        SendTunneledToVisible(new CombatPacketAttackAttackerMissed { AttackerGuid = attackerGuid, TargetGuid = Guid }, sendToSelf: true);

        if (AbilitySettings.DodgeAnimationId > 0)
            SendTunneledToVisible(new PlayerUpdatePacketSetAnimation { Guid = Guid, AnimationId = AbilitySettings.DodgeAnimationId, Flags = 2 }, sendToSelf: true);

        return true;
    }

    /// <summary>Apply already-mitigated damage: drop health, push the bar, knock out at 0.</summary>
    public void TakeDamage(int amount, Npc? source)
    {
        if (IsDead || amount <= 0)
            return;

        LastCombatDamageAt = DateTime.UtcNow;

        CurrentHealth = Math.Max(0, CurrentHealth - amount);
        SendHealth();

        if (CurrentHealth <= 0)
            Knockout();
        else
            EnterWorldCombat();
    }

    #endregion

    #region Knockout and revive

    /// <summary>
    /// Health reached 0. Marks the player down (no abilities, enemies stop swinging), pins health at 0, plays
    /// the knockdown state and pops the client's native respawn window with its countdown. The Revive buttons
    /// (op41/122) call <see cref="Revive"/>; a fallback timer revives in place if nothing is pressed.
    /// </summary>
    public void Knockout()
    {
        if (IsDead)
            return;

        IsDead = true;
        CurrentHealth = 0;
        DeathPosition = Position;
        SendHealth();

        // The knocked-out + rooted state: the client plays its knockdown and stops the player moving.
        SendTunneledToVisible(new PlayerUpdatePacketUpdateCharacterState
        {
            Guid = Guid,
            Status = CharacterStatus.IsKnockedOut | CharacterStatus.IsRooted
        }, sendToSelf: true);

        if (CombatSettings.KnockoutEffectId > 0)
            SendTunneledToVisible(new PlayerUpdatePacketPlayCompositeEffect
            {
                Guid = Guid,
                CompositeEffectId = CombatSettings.KnockoutEffectId,
                Position = Position
            }, sendToSelf: true);

        ChatHelper.SendSystemMessage(this, "You have been knocked out!");

        // The respawn window only renders its buttons while the client is in a combat state, so raise it
        // explicitly here regardless of the SendInWorldCombatFlag setting; Revive takes it back down.
        SendTunneled(new EncounterOverworldCombatPacket { InWorldCombat = true });
        SendTunneled(new EncounterPacketIsFighting { InWorldCombat = true });
        SendTunneled(new EncounterShowRespawnWindowPacket(0, 0,
            respawnTimeMs: CombatSettings.KnockoutRecoverSeconds * 1000,
            reviveHereCostRaw: CombatSettings.ReviveHereCoinCost * 1000));

        ScheduleAutoRevive();
    }

    private void ScheduleAutoRevive()
    {
        var generation = ++_reviveGeneration;
        var delay = Math.Max(CombatSettings.KnockoutRecoverSeconds, CombatSettings.AutoReviveSeconds);

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(delay));

                // A newer knockout bumps the generation, so a stale timer never revives mid-countdown.
                if (IsDead && generation == _reviveGeneration)
                    Revive(null);
            }
            catch (Exception ex)
            {
                Zone.Logger.LogError(ex, "Auto-revive failed for {guid}.", Guid);
            }
        });
    }

    /// <summary>
    /// Stand back up with full health and energy, briefly invulnerable (wiki: "come back at full health and
    /// energy"; captures: ~7 s of DamageReductionPercent 100). <paramref name="at"/> teleports first.
    /// </summary>
    public void Revive(Vector4? at)
    {
        if (!IsDead)
            return;

        IsDead = false;
        _reviveGeneration++;

        if (at is { } target)
        {
            UpdatePosition(target, Rotation);
            SendTunneled(new ClientUpdatePacketUpdateLocation { Position = target, Rotation = Rotation, Teleport = true });
        }

        CurrentHealth = MaxHealth;
        LastCombatDamageAt = DateTime.MinValue;
        SendHealth();
        RefillEnergy();

        SendTunneledToVisible(new PlayerUpdatePacketUpdateCharacterState
        {
            Guid = Guid,
            Status = CharacterStatus.None
        }, sendToSelf: true);

        // Drop the combat state the knockout raised and reset the state machine so it does not immediately
        // re-enter combat (the pre-death stamp could still be inside the window).
        _worldCombatActive = false;
        _lastWorldCombatTicks = 0;
        SendWorldCombatState(false);

        if (CombatSettings.ReviveEffectId > 0)
            SendTunneledToVisible(new PlayerUpdatePacketPlayCompositeEffect
            {
                Guid = Guid,
                CompositeEffectId = CombatSettings.ReviveEffectId,
                Position = Position
            }, sendToSelf: true);

        SetInvulnerable(CombatSettings.ReviveInvulnerableSeconds);

        ChatHelper.SendSystemMessage(this, "You have recovered!");
    }

    #endregion

    #region World-combat state

    /// <summary>
    /// Stamp that a combat action just happened (dealt or took damage). Callable from any thread: only the
    /// tick loop sends the op41 flags, so enter and exit can never race and latch the client in combat.
    /// </summary>
    public void EnterWorldCombat()
    {
        _lastWorldCombatTicks = Environment.TickCount64;
    }

    private void WorldCombatStateTick()
    {
        // While knocked out the death flow owns the flags (it raised them for the respawn window).
        if (IsDead)
            return;

        var want = InWorldCombat;

        if (want == _worldCombatActive)
            return;

        _worldCombatActive = want;
        SendWorldCombatState(want);
    }

    /// <summary>
    /// op41/132 SetInWorldCombat draws the floating damage numbers (and, being a global client switch, a bar
    /// on every nameplate); op41/133 SetIsFighting draws the weapon. Both client appliers are edge-guarded,
    /// so clearing pulses true first to guarantee the false transition fires whatever state the client holds.
    /// </summary>
    public void SendWorldCombatState(bool active)
    {
        if (active)
        {
            if (CombatSettings.SendInWorldCombatFlag)
                SendTunneled(new EncounterOverworldCombatPacket { InWorldCombat = true });

            if (CombatSettings.SendIsFightingFlag)
                SendTunneled(new EncounterPacketIsFighting { InWorldCombat = true });

            return;
        }

        if (CombatSettings.SendInWorldCombatFlag)
        {
            SendTunneled(new EncounterOverworldCombatPacket { InWorldCombat = true });
            SendTunneled(new EncounterPacketIsFighting { InWorldCombat = true });
        }

        SendTunneled(new EncounterOverworldCombatPacket { InWorldCombat = false });
        SendTunneled(new EncounterPacketIsFighting { InWorldCombat = false });
    }

    #endregion

    #region Attacking

    private long _nextBasicSwingTicks;

    /// <summary>
    /// One basic swing per recast window: the client fires presses faster than the swing plays, so extra
    /// presses inside the window are dropped (no cast, no number). Shared by the toolbar (op36) and
    /// click-to-attack (op32) paths so neither can out-pace the other.
    /// </summary>
    public bool TryGateBasicSwing(int recastMs)
    {
        var now = Environment.TickCount64;

        if (now < _nextBasicSwingTicks)
            return false;

        _nextBasicSwingTicks = now + Math.Max(0, recastMs);
        return true;
    }

    #endregion

    #region Energy

    /// <summary>Pay for a special. False (and nothing spent) when the bar is too low.</summary>
    public bool TrySpendEnergy(int cost)
    {
        if (cost <= 0)
            return true;

        if (Energy < cost)
            return false;

        Energy -= cost;
        return true;
    }

    public void RefillEnergy()
    {
        Energy = MaxEnergy;
    }

    // Once a second: the captured +4/s refill (CombatJobs.json Energy.RegenPerSecond), only while below max.
    private void EnergyRegenTick()
    {
        if (IsDead || Energy >= MaxEnergy)
            return;

        var regen = _resourceManager.CombatJobs.TryGetValue(ActiveProfileId, out var kit)
            ? kit.Energy.RegenPerSecond
            : 4;

        Energy += Math.Max(1, regen);
    }

    #endregion
}
