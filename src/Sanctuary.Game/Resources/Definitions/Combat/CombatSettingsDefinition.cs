namespace Sanctuary.Game.Resources.Definitions.Combat;

/// <summary>Root shape of Resources/CombatSettings.json. Every value has a default, so a missing key keeps working.</summary>
public sealed class CombatSettingsDefinition
{
    public string Comment { get; set; } = string.Empty;

    public PlayerCombatSettings Player { get; set; } = new();
    public EnemyCombatSettings Enemy { get; set; } = new();
    public AbilityCombatSettings Abilities { get; set; } = new();
}

public sealed class PlayerCombatSettings
{
    public string Comment { get; set; } = string.Empty;

    public int OutOfCombatSeconds { get; set; } = 6;

    public int InCombatHealthRegenDivisorCombatJob { get; set; } = 100;
    public int InCombatHealthRegenDivisorNonCombatJob { get; set; } = 400;

    public float DefenseConstant { get; set; } = 515f;

    public int KnockoutRecoverSeconds { get; set; } = 10;
    public int AutoReviveSeconds { get; set; } = 45;
    public int ReviveInvulnerableSeconds { get; set; } = 7;
    public int LoginInvulnerableSeconds { get; set; } = 42;
    public int ReviveHereCoinCost { get; set; }

    public int KnockoutEffectId { get; set; } = 5017;
    public int ReviveEffectId { get; set; } = 15117;

    public bool SendInWorldCombatFlag { get; set; } = true;
    public bool SendIsFightingFlag { get; set; } = true;
}

public sealed class EnemyCombatSettings
{
    public string Comment { get; set; } = string.Empty;

    public int DeathHoldMs { get; set; } = 2000;
    public int DeathEffectId { get; set; } = 5017;
    public int DeathEffectDurationMs { get; set; } = 1000;

    public float PositionBroadcastDistance { get; set; } = 0.3f;

    public byte AttackCursorId { get; set; } = 11;

    public int HitAnimationHoldMs { get; set; } = 700;
}

public sealed class AbilityCombatSettings
{
    public string Comment { get; set; } = string.Empty;

    public float BasicDamageDelayFraction { get; set; } = 0.85f;
    public float SpecialActionTimeSeconds { get; set; } = 0.4f;
    public float SpecialDamageDelaySeconds { get; set; } = 0.4f;

    public int DodgeAnimationId { get; set; } = 1406;
    public int DodgeChancePercent { get; set; }
}
