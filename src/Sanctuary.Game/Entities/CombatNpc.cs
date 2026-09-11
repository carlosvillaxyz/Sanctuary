using System;
using System.Collections.Generic;
using System.Numerics;

using Sanctuary.Game.Resources.Definitions.Combat;
using Sanctuary.Game.Zones;
using Sanctuary.Packet;

namespace Sanctuary.Game.Entities;

/// <summary>
/// A hostile NPC: aggro on approach, chase, leash back to its post, auto-attack, take damage, die and respawn.
/// Ported from Sulphural/main's CombatNpc (c34648df world combat, a126be4b smooth return-to-spawn, 12049172
/// tiers); the numbers come from Resources/Enemies.json (<see cref="EnemyStats"/>) and the death presentation
/// from Resources/CombatSettings.json. Left out on purpose: scripted marches, idle roaming, sticky animations,
/// snowball-only targets and the obstacle-aware ChaseNavigator (a9e68a17) - chases are straight lines.
/// </summary>
public sealed class CombatNpc : Npc
{
    public enum CombatState
    {
        Idle,
        Pursuing,
        Attacking,
        Returning
    }

    public EnemyStats Stats { get; }

    private readonly EnemyCombatSettings _settings;

    public CombatState State { get; private set; } = CombatState.Idle;
    public Player? AggroTarget { get; private set; }
    public bool IsDead { get; private set; }

    private DateTime _lastAttackAt = DateTime.MinValue;
    private DateTime _deathAt;

    // Throttling for the streamed movement: the last position and ExpectedSpeed the clients were told.
    private Vector4 _lastSentPosition;
    private float _lastSentExpectedSpeed = -1f;

    private readonly object _damageLock = new();

    private const byte RunState = 2;
    private const byte IdleState = 0;

    public CombatNpc(IZone zone, EnemyStats stats, EnemyCombatSettings settings) : base(zone)
    {
        Stats = stats;
        _settings = settings;

        Disposition = 0;          // hostile
        EnemyStatus = true;       // red name
        ActiveProfile = 1;        // re-runs the client's nameplate colour resolver (see Npc.EnemyStatus)
        CursorId = settings.AttackCursorId;
        IsInteractable = false;   // a target, not someone to talk to
        ShowHealthBar = true;

        MaxHealth = stats.MaxHealth;
        Health = stats.MaxHealth;
        Speed = stats.ChaseSpeed;
    }

    #region Update

    // No base call: enemies never follow scripted paths, and the base tick's PathFollower would broadcast an
    // idle-state position for every step.
    public override void UpdateEveryTick()
    {
        if (IsDead || !Visible)
            return;

        switch (State)
        {
            case CombatState.Idle:
                UpdateIdle();
                break;
            case CombatState.Pursuing:
                UpdatePursuing();
                break;
            case CombatState.Attacking:
                UpdateAttacking();
                break;
            case CombatState.Returning:
                UpdateReturning();
                break;
        }
    }

    public override void UpdateEverySecond()
    {
        base.UpdateEverySecond();

        if (IsDead && (DateTime.UtcNow - _deathAt).TotalSeconds >= Stats.RespawnSeconds)
            Respawn();
    }

    private void UpdateIdle()
    {
        var closest = FindClosestPlayer(Stats.AggroRange);

        if (closest is null)
            return;

        AggroTarget = closest;
        State = CombatState.Pursuing;
    }

    private void UpdatePursuing()
    {
        if (!IsValidTarget(AggroTarget))
        {
            StartReturning();
            return;
        }

        // Leash: chased far enough from the post, give up and walk home.
        if (DistanceTo(SpawnPosition) > Stats.LeashRange)
        {
            StartReturning();
            return;
        }

        if (DistanceTo(AggroTarget.Position) <= Stats.AttackRange)
        {
            // Plant cleanly (speed 0 + an idle-state position) so the model stops instead of coasting past.
            BroadcastStop();
            State = CombatState.Attacking;
            return;
        }

        MoveTowards(AggroTarget.Position, Stats.ChaseSpeed);
    }

    private void UpdateAttacking()
    {
        if (!IsValidTarget(AggroTarget))
        {
            StartReturning();
            return;
        }

        // Out of reach again: chase. Deliberately NO leash check here - a mob standing next to the player
        // trading hits is not escaping anything (Sulphural live feedback, 2026-07-29).
        if (DistanceTo(AggroTarget.Position) > Stats.AttackRange * 1.5f)
        {
            State = CombatState.Pursuing;
            return;
        }

        if ((DateTime.UtcNow - _lastAttackAt).TotalSeconds < Stats.AttackIntervalSeconds)
            return;

        _lastAttackAt = DateTime.UtcNow;

        FaceTarget(AggroTarget.Position);
        PerformAttack(AggroTarget);
    }

    private void UpdateReturning()
    {
        var distanceToSpawn = DistanceTo(SpawnPosition);

        // Home. A tight threshold keeps the final settle imperceptible; MoveTowards never overshoots.
        if (distanceToSpawn < 0.5f)
        {
            SetPosition(SpawnPosition, SpawnRotation);
            BroadcastStop();
            State = CombatState.Idle;
            AggroTarget = null;

            // Reset to full health, the classic leash rule.
            if (Health < MaxHealth)
            {
                Health = MaxHealth;
                BroadcastHealth();
            }

            return;
        }

        // Re-aggro only once we are back within leash of the post; re-aggroing while still far out just
        // bounces the mob straight back into a leash reset (the jitter Sulphural fixed in a126be4b).
        if (distanceToSpawn <= Stats.LeashRange * 0.8f)
        {
            var closest = FindClosestPlayer(Stats.AggroRange * 0.5f);

            if (closest is not null)
            {
                AggroTarget = closest;
                State = CombatState.Pursuing;
                return;
            }
        }

        MoveTowards(SpawnPosition, Stats.ChaseSpeed);
    }

    private void StartReturning()
    {
        AggroTarget = null;
        State = CombatState.Returning;
    }

    #endregion

    #region Attacking

    private void PerformAttack(Player target)
    {
        // Never keep hitting a downed player: extra combat packets on a 0-HP target bounce the client's bar.
        if (target.IsDead)
            return;

        var variance = 0.8f + Random.Shared.NextSingle() * 0.4f; // 0.8x-1.2x
        var rawDamage = Math.Max(1, (int)(Stats.Damage * variance));

        if (target.TryDodge(Guid))
            return;

        var damage = target.MitigateIncomingDamage(rawDamage);

        target.TakeDamage(damage, this);

        // The per-hit feedback: the attacker guid plays the model's attack-contact event (its swing or bite),
        // the target gets the floating number, bar, recoil and hit FX. CurrentHealth is the post-hit value so
        // the bar matches the hitpoints packet TakeDamage just sent.
        var attack = new CombatPacketAttackProcessed
        {
            AttackerGuid = Guid,
            TargetGuid = target.Guid,
            Damage = damage,
            MaxHealth = target.MaxHealth,
            CurrentHealth = target.CurrentHealth,
            CompositeEffectId = 0
        };

        foreach (var player in VisiblePlayers.Values)
            player.SendTunneled(attack);
    }

    /// <summary>
    /// Pull this enemy onto a player without dealing damage: a ranged poke from outside aggro range, or a hit
    /// that a zone applied itself.
    /// </summary>
    public void AggroOnto(Player source)
    {
        if (IsDead || source.IsDead)
            return;

        if (IsValidTarget(AggroTarget))
            return;

        AggroTarget = source;

        if (State is CombatState.Idle or CombatState.Returning)
            State = CombatState.Pursuing;
    }

    /// <summary>
    /// Damage from a player. Returns true when this hit landed the kill (exactly once, even with several
    /// hits resolving on delayed tasks). <paramref name="broadcastHitNumber"/> false lets a caller that sends
    /// its own per-hit feedback skip the floating number here so it is not drawn twice.
    /// </summary>
    public bool TakeDamage(int amount, Player source, bool broadcastHitNumber = true)
    {
        bool killed;

        lock (_damageLock)
        {
            if (IsDead || Health <= 0)
                return false;

            Health = Math.Max(0, Health - amount);
            killed = Health == 0;
        }

        // Wire (IDA-confirmed by Sulphural): Guid = attacker, Guid2 = victim, Unknown2 = max HP,
        // Unknown3 = current HP after the hit, Unknown4 = delta (-damage = the floating number).
        if (broadcastHitNumber)
        {
            var hit = new PlayerUpdatePacketHitPointModification
            {
                Guid = source.Guid,
                Guid2 = Guid,
                Unknown = true,
                Unknown2 = MaxHealth,
                Unknown3 = Health,
                Unknown4 = -amount
            };

            foreach (var player in VisiblePlayers.Values)
                player.SendTunneled(hit);

            if (!VisiblePlayers.ContainsKey(source.Guid))
                source.SendTunneled(hit);
        }

        BroadcastHealth();

        if (killed)
        {
            Die(source);
            return true;
        }

        AggroOnto(source);
        Zone.OnNpcDamaged(source, this);

        return false;
    }

    private void Die(Player killer)
    {
        IsDead = true;
        _deathAt = DateTime.UtcNow;
        State = CombatState.Idle;
        AggroTarget = null;

        // The 2014 capture's death recipe: the client plays the model's own death clip, holds the body, then
        // poofs it. Also release the killer's target lock (RemoveNotifications) so a bow can re-fire.
        var clearTarget = new PlayerUpdatePacketRemoveNotifications { Guids = { Guid } };

        var watchers = new List<Player>(VisiblePlayers.Values);

        if (!VisiblePlayers.ContainsKey(killer.Guid))
            watchers.Add(killer);

        foreach (var player in watchers)
        {
            player.SendTunneled(clearTarget);
            player.OnRemoveVisibleNpcGracefully(this, animate: true, _settings.DeathHoldMs, 0,
                _settings.DeathEffectId, _settings.DeathEffectDurationMs);
        }

        VisiblePlayers.Clear();
        Visible = false;

        Zone.OnNpcKilled(killer, this);
    }

    private void Respawn()
    {
        IsDead = false;
        Health = MaxHealth;
        State = CombatState.Idle;
        AggroTarget = null;
        _lastAttackAt = DateTime.MinValue;
        _lastSentExpectedSpeed = -1f;

        SetPosition(SpawnPosition, SpawnRotation);
        _lastSentPosition = SpawnPosition;

        Visible = true;
        UpdateZoneTile();

        // A mid-session respawn is not picked up by the players' load-time visibility sweep, so push it to
        // everyone whose tile can see the post.
        foreach (var player in Zone.Players)
        {
            if (!player.Visible || VisiblePlayers.ContainsKey(player.Guid))
                continue;

            var playerTile = Zone.GetTileFromPosition(player.Position);

            if (playerTile == ZoneTile || playerTile.VisibleTiles.Contains(ZoneTile))
            {
                player.OnAddVisibleNpcs([this]);
                OnAddVisiblePlayers(player);
            }
        }
    }

    #endregion

    #region Movement

    private void MoveTowards(Vector4 target, float speed)
    {
        var dx = target.X - Position.X;
        var dz = target.Z - Position.Z;
        var distance = MathF.Sqrt(dx * dx + dz * dz);

        if (distance < 0.1f)
            return;

        // Tell clients how fast we move so a PHYSICS actor interpolates a smooth grounded run between
        // updates instead of snapping (the "flying" look).
        SendExpectedSpeed(speed);

        var step = MathF.Min(speed * Zone.TickDeltaSeconds, distance);
        var nx = dx / distance;
        var nz = dz / distance;

        // Ease Y toward the target with horizontal progress rather than snapping to it: snapping popped the
        // model to spawn height on the first return tick and let client gravity yank it back down.
        var fraction = step / distance;

        var next = new Vector4(
            Position.X + nx * step,
            Position.Y + (target.Y - Position.Y) * fraction,
            Position.Z + nz * step,
            1f);

        var angle = MathF.Atan2(nx, nz);
        var rotation = new Quaternion(0f, MathF.Sin(angle / 2f), 0f, MathF.Cos(angle / 2f));

        SetPosition(next, rotation);

        var sentDx = next.X - _lastSentPosition.X;
        var sentDz = next.Z - _lastSentPosition.Z;

        if (sentDx * sentDx + sentDz * sentDz >= _settings.PositionBroadcastDistance * _settings.PositionBroadcastDistance)
        {
            BroadcastPosition(RunState);
            _lastSentPosition = next;
        }
    }

    private void SendExpectedSpeed(float speed)
    {
        if (_lastSentExpectedSpeed == speed)
            return;

        _lastSentExpectedSpeed = speed;

        var packet = new PlayerUpdatePacketExpectedSpeed { Guid = Guid, ExpectedSpeed = speed };

        foreach (var player in VisiblePlayers.Values)
            player.SendTunneled(packet);
    }

    // Plant the NPC: stop client-side prediction (speed 0) and send one idle-state position.
    private void BroadcastStop()
    {
        SendExpectedSpeed(0f);
        BroadcastPosition(IdleState);
        _lastSentPosition = Position;
    }

    private void FaceTarget(Vector4 target)
    {
        var angle = MathF.Atan2(target.X - Position.X, target.Z - Position.Z);

        SetPosition(Position, new Quaternion(0f, MathF.Sin(angle / 2f), 0f, MathF.Cos(angle / 2f)));
        BroadcastPosition(IdleState);
    }

    private void BroadcastPosition(byte state)
    {
        var packet = new PlayerUpdatePacketUpdatePosition
        {
            Guid = Guid,
            Position = Position,
            Rotation = Rotation,
            State = state,
            Unknown = 0
        };

        foreach (var player in VisiblePlayers.Values)
            player.SendTunneled(packet);
    }

    #endregion

    public void BroadcastHealth()
    {
        var packet = new PlayerUpdatePacketUpdateHitpoints
        {
            Guid = Guid,
            Hitpoints = Health,
            MaxHitpoints = MaxHealth
        };

        foreach (var player in VisiblePlayers.Values)
            player.SendTunneled(packet);
    }

    private static bool IsValidTarget(Player? player) =>
        player is not null && player.Visible && !player.IsDead;

    private Player? FindClosestPlayer(float range)
    {
        Player? closest = null;
        var closestDistance = range;

        foreach (var player in VisiblePlayers.Values)
        {
            if (!IsValidTarget(player))
                continue;

            var distance = DistanceTo(player.Position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = player;
            }
        }

        return closest;
    }

    private float DistanceTo(Vector4 target)
    {
        var dx = target.X - Position.X;
        var dz = target.Z - Position.Z;
        return MathF.Sqrt(dx * dx + dz * dz);
    }
}
