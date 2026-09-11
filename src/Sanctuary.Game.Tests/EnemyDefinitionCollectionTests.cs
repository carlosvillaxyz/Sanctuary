using System;
using System.IO;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Sanctuary.Game.Resources;
using Sanctuary.Game.Resources.Definitions;
using Sanctuary.Game.Resources.Definitions.Combat;

namespace Sanctuary.Game.Tests;

[TestClass]
public sealed class EnemyDefinitionCollectionTests
{
    private const string SampleJson = """
        {
          "HealthByLevel": [ 500, 575, 661, 760 ],
          "DamageByLevel": [ 20, 23, 26, 30 ],
          "XpByLevel": [ 15, 17, 18, 20 ],
          "Tiers": {
            "Normal": { "Health": 1.0, "Damage": 1.0, "Xp": 1.0 },
            "Boss": { "Health": 5.0, "Damage": 1.8, "Xp": 4.5 },
            "Weak": { "Health": 0.55, "Damage": 0.8, "Xp": 0.6 }
          },
          "TierKeywords": {
            "Boss": [ "alpha", "king" ],
            "Weak": [ "pup" ]
          },
          "Defaults": { "Level": 3, "Tier": "Normal", "AggroRange": 15, "LeashRange": 40, "AttackRange": 5, "AttackIntervalSeconds": 2.4, "RespawnSeconds": 8, "ChaseSpeed": 6, "Energy": 800 },
          "HostileModelIds": [ 3284 ],
          "FriendlyNpcIds": [ 2529 ],
          "Species": [
            { "Comment": "wolf", "NameIds": [ 5100200 ], "Level": 1, "Tier": "Normal", "RespawnSeconds": 12 },
            { "Comment": "archer", "NameIds": [ 5100400 ], "Level": 2, "AttackRange": 12, "Xp": 40 }
          ]
        }
        """;

    private static EnemyDefinitionCollection LoadSample()
    {
        var path = Path.Combine(Path.GetTempPath(), $"enemies-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, SampleJson);

        try
        {
            var collection = new EnemyDefinitionCollection(NullLogger.Instance);
            Assert.IsTrue(collection.Load(path));
            return collection;
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public void SpeciesByNameId_UsesCurveForLevelAndOverrides()
    {
        var enemies = LoadSample();
        var wolf = new NpcDefinition { Id = 3156, NameId = 5100200, Name = "Hooligan Wolf", ModelId = 3284 };

        Assert.IsTrue(enemies.TryResolve(wolf, out var stats));
        Assert.AreEqual(1, stats.Level);
        Assert.AreEqual(EnemyTier.Normal, stats.Tier);
        Assert.AreEqual(500, stats.MaxHealth);
        Assert.AreEqual(20, stats.Damage);
        Assert.AreEqual(15, stats.Xp);
        Assert.AreEqual(12f, stats.RespawnSeconds);
        Assert.AreEqual(15f, stats.AggroRange);
        Assert.AreEqual(800, stats.Energy);
    }

    [TestMethod]
    public void SpeciesWithoutTier_ClassifiesFromNameKeywords()
    {
        var enemies = LoadSample();
        var archer = new NpcDefinition { Id = 4157, NameId = 5100400, Name = "Hooligan Archer King", ModelId = 520 };

        Assert.IsTrue(enemies.TryResolve(archer, out var stats));
        Assert.AreEqual(EnemyTier.Boss, stats.Tier);
        Assert.AreEqual(2, stats.Level);
        Assert.AreEqual(2875, stats.MaxHealth); // 575 x 5
        Assert.AreEqual(41, stats.Damage);      // 23 x 1.8 rounded
        Assert.AreEqual(40, stats.Xp);          // explicit override beats the curve
        Assert.AreEqual(12f, stats.AttackRange);
    }

    [TestMethod]
    public void HostileModelFallback_UsesDefaultsAndNameTier()
    {
        var enemies = LoadSample();
        var pup = new NpcDefinition { Id = 9999, NameId = 0, Name = "Wolf Pup", ModelId = 3284 };

        Assert.IsTrue(enemies.TryResolve(pup, out var stats));
        Assert.AreEqual(3, stats.Level);
        Assert.AreEqual(EnemyTier.Weak, stats.Tier);
        Assert.AreEqual(364, stats.MaxHealth); // 661 x 0.55 rounded
        Assert.AreEqual(8f, stats.RespawnSeconds);
    }

    [TestMethod]
    public void UnlistedModel_StaysFriendly()
    {
        var enemies = LoadSample();
        var cow = new NpcDefinition { Id = 1, NameId = 1234, Name = "Cow", ModelId = 145 };

        Assert.IsFalse(enemies.TryResolve(cow, out _));
    }

    [TestMethod]
    public void FriendlyNpcId_OverridesEveryRule()
    {
        var enemies = LoadSample();
        var wolf = new NpcDefinition { Id = 2529, NameId = 5100200, Name = "Hooligan Wolf", ModelId = 3284 };

        Assert.IsFalse(enemies.TryResolve(wolf, out _));
    }

    [TestMethod]
    public void LevelBeyondTable_ClampsToLastRow()
    {
        var enemies = LoadSample();
        var stats = enemies.Compute(level: 40, EnemyTier.Normal);

        Assert.AreEqual(760, stats.MaxHealth);
        Assert.AreEqual(30, stats.Damage);
    }

    [TestMethod]
    public void MissingFile_LoadsEmptyAndNothingIsHostile()
    {
        var enemies = new EnemyDefinitionCollection(NullLogger.Instance);

        Assert.IsTrue(enemies.Load(Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.json")));
        Assert.IsFalse(enemies.TryResolve(new NpcDefinition { Id = 3156, NameId = 5100200, ModelId = 3284 }, out _));
    }
}
