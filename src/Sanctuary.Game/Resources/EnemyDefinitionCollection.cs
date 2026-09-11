using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using Sanctuary.Game.Resources.Definitions;
using Sanctuary.Game.Resources.Definitions.Combat;

namespace Sanctuary.Game.Resources;

/// <summary>
/// Resources/Enemies.json: which placed NPCs fight back, and their stats. Replaces Sulphural/main's hard-coded
/// CombatNpc.InitializeFromLevel formulas, EnemyTiers keyword tables and DungeonCatalog.EnemyModelIds with data.
/// </summary>
public sealed class EnemyDefinitionCollection
{
    private readonly ILogger _logger;
    private readonly object _writeLock = new();

    private EnemyDefinitionFile _file = new();
    private Dictionary<int, EnemySpeciesDefinition> _byNameId = new();
    private Dictionary<int, EnemySpeciesDefinition> _byModelId = new();
    private HashSet<int> _hostileModelIds = [];
    private HashSet<int> _friendlyNpcIds = [];

    public EnemyDefinitionCollection(ILogger logger)
    {
        _logger = logger;
    }

    public EnemyDefaultsDefinition Defaults => _file.Defaults;

    public int SpeciesCount => _file.Species.Count;

    public bool Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Failed to find file \"{file}\". No NPC will spawn hostile.", filePath);
            return true;
        }

        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            var file = JsonSerializer.Deserialize<EnemyDefinitionFile>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            });

            if (file is null)
            {
                _logger.LogError("No content found in \"{file}\".", filePath);
                return false;
            }

            if (file.HealthByLevel.Count == 0 || file.DamageByLevel.Count == 0 || file.XpByLevel.Count == 0)
            {
                _logger.LogError("\"{file}\" needs HealthByLevel, DamageByLevel and XpByLevel tables.", filePath);
                return false;
            }

            if (!TryParseTier(file.Defaults.Tier, out _))
            {
                _logger.LogError("\"{file}\" Defaults.Tier \"{tier}\" is not a tier name.", filePath, file.Defaults.Tier);
                return false;
            }

            var byNameId = new Dictionary<int, EnemySpeciesDefinition>();
            var byModelId = new Dictionary<int, EnemySpeciesDefinition>();

            foreach (var species in file.Species)
            {
                if (species.Tier is not null && !TryParseTier(species.Tier, out _))
                {
                    _logger.LogError("\"{file}\" species \"{comment}\" has unknown tier \"{tier}\".", filePath, species.Comment, species.Tier);
                    return false;
                }

                if (species.NameIds.Count == 0 && species.ModelIds.Count == 0)
                {
                    _logger.LogError("\"{file}\" species \"{comment}\" lists no NameIds or ModelIds.", filePath, species.Comment);
                    return false;
                }

                foreach (var nameId in species.NameIds)
                    if (!byNameId.TryAdd(nameId, species))
                        _logger.LogWarning("\"{file}\": NameId {nameId} is listed by more than one species; the first wins.", filePath, nameId);

                foreach (var modelId in species.ModelIds)
                    if (!byModelId.TryAdd(modelId, species))
                        _logger.LogWarning("\"{file}\": ModelId {modelId} is listed by more than one species; the first wins.", filePath, modelId);
            }

            lock (_writeLock)
            {
                _file = file;
                _byNameId = byNameId;
                _byModelId = byModelId;
                _hostileModelIds = [.. file.HostileModelIds];
                _friendlyNpcIds = [.. file.FriendlyNpcIds];
            }

            _logger.LogInformation("Loaded {species} enemy species and {models} hostile model ids from \"{file}\".",
                file.Species.Count, file.HostileModelIds.Count, filePath);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse file \"{file}\".", filePath);
            return false;
        }
    }

    /// <summary>True when this NPC definition is a monster: a species by NameId/ModelId, or a hostile creature model.</summary>
    public bool TryResolve(NpcDefinition definition, [MaybeNullWhen(false)] out EnemyStats stats)
    {
        stats = null;

        if (_friendlyNpcIds.Contains(definition.Id))
            return false;

        if (_byNameId.TryGetValue(definition.NameId, out var species) ||
            _byModelId.TryGetValue(definition.ModelId, out species))
        {
            stats = Compute(species, definition.Name);
            return true;
        }

        if (!_hostileModelIds.Contains(definition.ModelId))
            return false;

        stats = Compute(null, definition.Name);
        return true;
    }

    /// <summary>Stats for a level and tier straight from the curves (no species overrides).</summary>
    public EnemyStats Compute(int level, EnemyTier tier)
    {
        var defaults = _file.Defaults;

        return new EnemyStats(
            level,
            tier,
            Scale(Curve(_file.HealthByLevel, level), Multipliers(tier).Health),
            Scale(Curve(_file.DamageByLevel, level), Multipliers(tier).Damage),
            Scale(Curve(_file.XpByLevel, level), Multipliers(tier).Xp),
            defaults.AggroRange,
            defaults.LeashRange,
            defaults.AttackRange,
            defaults.AttackIntervalSeconds,
            defaults.RespawnSeconds,
            defaults.ChaseSpeed,
            defaults.Energy);
    }

    private EnemyStats Compute(EnemySpeciesDefinition? species, string? name)
    {
        var defaults = _file.Defaults;

        var level = species?.Level > 0 ? species.Level : defaults.Level;

        var tier = species?.Tier is not null && TryParseTier(species.Tier, out var speciesTier)
            ? speciesTier
            : TierFromName(name);

        var curve = Compute(level, tier);

        return curve with
        {
            MaxHealth = species?.Health > 0 ? species.Health : curve.MaxHealth,
            Damage = species?.Damage > 0 ? species.Damage : curve.Damage,
            Xp = species?.Xp > 0 ? species.Xp : curve.Xp,
            AggroRange = species?.AggroRange ?? defaults.AggroRange,
            LeashRange = species?.LeashRange ?? defaults.LeashRange,
            AttackRange = species?.AttackRange ?? defaults.AttackRange,
            AttackIntervalSeconds = species?.AttackIntervalSeconds ?? defaults.AttackIntervalSeconds,
            RespawnSeconds = species?.RespawnSeconds ?? defaults.RespawnSeconds,
            ChaseSpeed = species?.ChaseSpeed ?? defaults.ChaseSpeed,
            Energy = species?.Energy ?? defaults.Energy
        };
    }

    /// <summary>
    /// Classify an enemy from its display name with the JSON keyword sets, most specific tier first
    /// ("Robgoblin Wizard King" is a Boss, not an Elite). Unknown or unnamed enemies use the default tier.
    /// </summary>
    public EnemyTier TierFromName(string? name)
    {
        TryParseTier(_file.Defaults.Tier, out var fallback);

        if (string.IsNullOrWhiteSpace(name))
            return fallback;

        var lower = name.ToLowerInvariant();

        foreach (var tier in new[] { EnemyTier.Boss, EnemyTier.Elite, EnemyTier.Tough, EnemyTier.Weak })
        {
            if (!_file.TierKeywords.TryGetValue(tier.ToString(), out var keywords))
                continue;

            if (keywords.Any(keyword => lower.Contains(keyword, StringComparison.Ordinal)))
                return tier;
        }

        return fallback;
    }

    private EnemyTierMultipliers Multipliers(EnemyTier tier) =>
        _file.Tiers.TryGetValue(tier.ToString(), out var multipliers) ? multipliers : new EnemyTierMultipliers();

    private static int Curve(List<int> table, int level)
    {
        if (table.Count == 0)
            return 0;

        return table[Math.Clamp(level, 1, table.Count) - 1];
    }

    private static int Scale(int value, float multiplier) => Math.Max(1, (int)Math.Round(value * multiplier));

    private static bool TryParseTier(string? name, out EnemyTier tier) =>
        Enum.TryParse(name, ignoreCase: true, out tier) && Enum.IsDefined(tier);
}
