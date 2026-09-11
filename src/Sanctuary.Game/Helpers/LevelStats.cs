using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

using Sanctuary.Packet.Common;

namespace Sanctuary.Game.Helpers;

/// <summary>
/// Player stats by job level, from Resources/LevelStats.json, which holds values decoded from the March 2014 live
/// captures (research/level-stats.md). A job's level is its profile Rank (1-20). Combat jobs (profile Type 2) and
/// non-combat jobs use separate health tables; some jobs carry a multiplier (Warrior x1.1).
/// Energy is not scaled: it is 100 for every job at every level.
/// </summary>
public static class LevelStats
{
    public sealed record Entry(int Level, int MaxHealth, int HealthRegen);

    /// <summary>Profile Type of the six combat jobs (Brawler, Ninja, Warrior, Wizard, Archer, Medic).</summary>
    public const int CombatProfileType = 2;

    private sealed record Tables(
        Dictionary<int, int> CombatHealth,
        Dictionary<int, int> NonCombatHealth,
        int CombatRegenDivisor,
        int NonCombatRegenDivisor,
        Dictionary<int, float> JobMultipliers);

    private static readonly Lazy<Tables> Data = new(Load);

    /// <summary>Stats for a job profile at its current level.</summary>
    public static Entry ForProfile(ClientPcProfile profile)
        => Compute(profile.Rank, profile.Type == CombatProfileType, profile.Id);

    /// <summary>Stats for a combat job at a level (no job multiplier).</summary>
    public static Entry For(int level) => Compute(level, isCombat: true, profileId: 0);

    private static Entry Compute(int level, bool isCombat, int profileId)
    {
        var data = Data.Value;
        var table = isCombat ? data.CombatHealth : data.NonCombatHealth;

        if (table.Count == 0)
            return new Entry(level, 2500, 25);

        var min = int.MaxValue;
        var max = int.MinValue;

        foreach (var key in table.Keys)
        {
            min = Math.Min(min, key);
            max = Math.Max(max, key);
        }

        var clamped = Math.Clamp(level, min, max);
        var health = table[clamped];

        if (data.JobMultipliers.TryGetValue(profileId, out var multiplier))
            health = (int)Math.Round(health * multiplier);

        var divisor = isCombat ? data.CombatRegenDivisor : data.NonCombatRegenDivisor;

        return new Entry(clamped, health, health / Math.Max(1, divisor));
    }

    private static Tables Load()
    {
        var empty = new Tables([], [], 20, 100, []);
        var path = Path.Combine(ResourceManager.BaseDirectory, "LevelStats.json");

        if (!File.Exists(path))
            return empty;

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;

        static Dictionary<int, int> ReadTable(JsonElement section)
        {
            var result = new Dictionary<int, int>();

            foreach (var row in section.GetProperty("Levels").EnumerateArray())
                result[row.GetProperty("Level").GetInt32()] = row.GetProperty("MaxHealth").GetInt32();

            return result;
        }

        var multipliers = new Dictionary<int, float>();

        if (root.TryGetProperty("JobMultipliers", out var jobs))
            foreach (var job in jobs.EnumerateObject())
                if (int.TryParse(job.Name, out var id))
                    multipliers[id] = job.Value.GetSingle();

        return new Tables(
            ReadTable(root.GetProperty("Combat")),
            ReadTable(root.GetProperty("NonCombat")),
            root.GetProperty("Combat").GetProperty("RegenDivisor").GetInt32(),
            root.GetProperty("NonCombat").GetProperty("RegenDivisor").GetInt32(),
            multipliers);
    }
}
