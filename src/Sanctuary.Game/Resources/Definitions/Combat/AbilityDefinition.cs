namespace Sanctuary.Game.Resources.Definitions.Combat;

public sealed class AbilityDefinition
{
    public int Id { get; set; }

    public string Comment { get; set; } = string.Empty;

    public string EffectType { get; set; } = "SweepDamage";

    public int Damage { get; set; }

    /// <summary>Self-heal that lands with the hit (AoeDamageHeal specials such as Triage). Scaled like Damage.</summary>
    public int HealAmount { get; set; }

    public int HitCount { get; set; }
    public float AoeRadius { get; set; }
    public int EnergyCost { get; set; }

    public int AnimationId { get; set; }
    public int HitEffectId { get; set; }
    public int CastEffectId { get; set; }
    public int CasterEndEffectId { get; set; }
    public int EnemyExtraEffectId { get; set; }

    /// <summary>
    /// An ATTACHED effect that burns for the length of the swing (Leg Sweep's foot beam trail). It has no end
    /// trigger of its own, so it is added by effect tag and pulled again after <see cref="TrailDurationMs"/>;
    /// fired as a cast effect instead it never stops, and follows the character across job switches.
    /// </summary>
    public int TrailEffectId { get; set; }

    /// <summary>How long the trail stays attached. 0 = the ability's action lock.</summary>
    public int TrailDurationMs { get; set; }

    public int WeaponEffectId { get; set; }
    public int WeaponEffectDurationMs { get; set; } = 10000;

    public int TargetAnimationId { get; set; }
    public int TargetEffectDurationMs { get; set; }
    public int ContactEffectId { get; set; }

    public int NameId { get; set; }
    public int DescriptionId { get; set; }
    public int IconId { get; set; }
}
