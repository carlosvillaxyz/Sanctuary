using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Sanctuary.Game.Helpers;

/// <summary>
/// Player stats by job level, from Resources/LevelStats.json. A job's level is its profile Rank (1-20).
/// Energy is not scaled here: combat jobs take their energy meter from Resources/CombatJobs.json.
/// </summary>
public static class LevelStats
{
    public sealed record Entry(int Level, int MaxHealth, int HealthRegen);

    private static readonly Lazy<Dictionary<int, Entry>> Table = new(Load);

    /// <summary>Stats for a level, clamped to the table's range. Falls back to 2500 health if the file is missing.</summary>
    public static Entry For(int level)
    {
        var table = Table.Value;

        if (table.Count == 0)
            return new Entry(level, 2500, 25);

        var min = int.MaxValue;
        var max = int.MinValue;

        foreach (var key in table.Keys)
        {
            min = Math.Min(min, key);
            max = Math.Max(max, key);
        }

        return table[Math.Clamp(level, min, max)];
    }

    private static Dictionary<int, Entry> Load()
    {
        var result = new Dictionary<int, Entry>();
        var path = Path.Combine(ResourceManager.BaseDirectory, "LevelStats.json");

        if (!File.Exists(path))
            return result;

        using var document = JsonDocument.Parse(File.ReadAllText(path));

        foreach (var row in document.RootElement.GetProperty("Levels").EnumerateArray())
        {
            var entry = new Entry(
                row.GetProperty("Level").GetInt32(),
                row.GetProperty("MaxHealth").GetInt32(),
                row.GetProperty("HealthRegen").GetInt32());

            result[entry.Level] = entry;
        }

        return result;
    }
}
