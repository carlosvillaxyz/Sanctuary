using System;
using System.IO;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using Sanctuary.Game.Resources.Definitions.Combat;

namespace Sanctuary.Game.Resources;

/// <summary>Resources/CombatSettings.json. A missing file leaves the code defaults in place.</summary>
public sealed class CombatSettingsCollection
{
    private readonly ILogger _logger;

    public CombatSettingsCollection(ILogger logger)
    {
        _logger = logger;
    }

    public CombatSettingsDefinition Settings { get; private set; } = new();

    public PlayerCombatSettings Player => Settings.Player;
    public EnemyCombatSettings Enemy => Settings.Enemy;
    public AbilityCombatSettings Abilities => Settings.Abilities;

    public bool Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Failed to find file \"{file}\". Using default combat settings.", filePath);
            Settings = new CombatSettingsDefinition();
            return true;
        }

        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            var settings = JsonSerializer.Deserialize<CombatSettingsDefinition>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            });

            if (settings is null)
            {
                _logger.LogError("No content found in \"{file}\".", filePath);
                return false;
            }

            if (settings.Player.OutOfCombatSeconds < 0 || settings.Player.KnockoutRecoverSeconds < 0 ||
                settings.Player.DefenseConstant <= 0 || settings.Enemy.DeathHoldMs < 0)
            {
                _logger.LogError("\"{file}\" has a negative timing or a non-positive DefenseConstant.", filePath);
                return false;
            }

            Settings = settings;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse file \"{file}\".", filePath);
            return false;
        }
    }
}
