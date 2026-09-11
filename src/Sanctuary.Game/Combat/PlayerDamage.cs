using System;

using Sanctuary.Game.Resources.Definitions.Combat;

namespace Sanctuary.Game.Combat;

/// <summary>
/// Scales a CombatAbilities.json damage (or heal) value to the caster's job level. The JSON value is the hit at
/// <see cref="AbilityCombatSettings.DamageReferenceLevel"/>; every level above or below multiplies it by
/// <see cref="AbilityCombatSettings.DamageGrowthPerLevel"/>. Our design (documented in docs/combat-port.md):
/// the 1.15 ratio is the captured health curve's, so the swings-to-kill on a same-level enemy stay flat.
/// </summary>
public static class PlayerDamage
{
    public const int MinLevel = 1;
    public const int MaxLevel = 20;

    public static int Scale(int baseValue, int level, AbilityCombatSettings settings)
        => Scale(baseValue, level, settings.DamageReferenceLevel, settings.DamageGrowthPerLevel);

    public static int Scale(int baseValue, int level, int referenceLevel, float growthPerLevel)
    {
        if (baseValue <= 0)
            return 0;

        level = Math.Clamp(level, MinLevel, MaxLevel);
        referenceLevel = Math.Clamp(referenceLevel, MinLevel, MaxLevel);

        if (growthPerLevel <= 0f)
            growthPerLevel = 1f;

        var scaled = baseValue * Math.Pow(growthPerLevel, level - referenceLevel);

        return Math.Max(1, (int)Math.Round(scaled));
    }
}
