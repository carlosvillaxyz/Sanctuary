using System.Collections.Generic;

namespace Sanctuary.Game.Resources.Definitions.Combat;

/// <summary>
/// Difficulty tier of an enemy. Multiplies the per-level curves (Resources/Enemies.json "Tiers"). Ported from
/// Sulphural/main's EnemyTier (12049172); the keyword classification lives in the JSON now.
/// </summary>
public enum EnemyTier
{
    Weak,
    Normal,
    Tough,
    Elite,
    Boss
}

public sealed class EnemyTierMultipliers
{
    public float Health { get; set; } = 1f;
    public float Damage { get; set; } = 1f;
    public float Xp { get; set; } = 1f;
}

/// <summary>Per-enemy defaults; every field can be overridden by a species entry.</summary>
public sealed class EnemyDefaultsDefinition
{
    public string Comment { get; set; } = string.Empty;

    public int Level { get; set; } = 3;
    public string Tier { get; set; } = nameof(EnemyTier.Normal);

    public float AggroRange { get; set; } = 15f;
    public float LeashRange { get; set; } = 40f;
    public float AttackRange { get; set; } = 5f;
    public float AttackIntervalSeconds { get; set; } = 2.4f;
    public float RespawnSeconds { get; set; } = 8f;
    public float ChaseSpeed { get; set; } = 6f;
    public int Energy { get; set; } = 800;
}

/// <summary>One enemy species: matched by NameId (or ModelId), with its level/tier and optional overrides.</summary>
public sealed class EnemySpeciesDefinition
{
    public string Comment { get; set; } = string.Empty;

    public List<int> NameIds { get; set; } = [];
    public List<int> ModelIds { get; set; } = [];

    public int Level { get; set; }
    public string? Tier { get; set; }

    /// <summary>Explicit stat overrides; 0 = use the curve for the level and tier.</summary>
    public int Health { get; set; }
    public int Damage { get; set; }
    public int Xp { get; set; }

    public float? AggroRange { get; set; }
    public float? LeashRange { get; set; }
    public float? AttackRange { get; set; }
    public float? AttackIntervalSeconds { get; set; }
    public float? RespawnSeconds { get; set; }
    public float? ChaseSpeed { get; set; }
    public int? Energy { get; set; }
}

/// <summary>Root shape of Resources/Enemies.json.</summary>
public sealed class EnemyDefinitionFile
{
    public string Comment { get; set; } = string.Empty;

    public List<int> HealthByLevel { get; set; } = [];
    public List<int> DamageByLevel { get; set; } = [];
    public List<int> XpByLevel { get; set; } = [];

    public Dictionary<string, EnemyTierMultipliers> Tiers { get; set; } = new();
    public Dictionary<string, List<string>> TierKeywords { get; set; } = new();

    public EnemyDefaultsDefinition Defaults { get; set; } = new();

    public List<int> HostileModelIds { get; set; } = [];
    public List<int> FriendlyNpcIds { get; set; } = [];

    public List<EnemySpeciesDefinition> Species { get; set; } = [];
}

/// <summary>Fully resolved numbers for one spawned enemy.</summary>
public sealed record EnemyStats(
    int Level,
    EnemyTier Tier,
    int MaxHealth,
    int Damage,
    int Xp,
    float AggroRange,
    float LeashRange,
    float AttackRange,
    float AttackIntervalSeconds,
    float RespawnSeconds,
    float ChaseSpeed,
    int Energy);
