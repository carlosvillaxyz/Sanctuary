using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Sanctuary.Game.Resources;
using Sanctuary.Game.Resources.Definitions;

namespace Sanctuary.Game.Tests;

/// <summary>
/// Guards on the data we actually ship (src/Resources), not on a synthetic sample. The first combat play-test
/// found three placed hooligans standing inert in a camp: Sony shipped their display name under two different
/// string ids and Enemies.json only listed one, so they spawned as scenery - no bar, no red name, unhittable -
/// next to their hostile friends. These tests fail if that ever happens again to any species.
/// </summary>
[TestClass]
public sealed class ShippedEnemyDataTests
{
    private static string ResourcesDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "Resources", "Enemies.json");

            if (File.Exists(candidate))
                return Path.GetDirectoryName(candidate)!;

            directory = directory.Parent;
        }

        Assert.Inconclusive("src/Resources not found from the test output directory.");
        return string.Empty;
    }

    private static (EnemyDefinitionCollection Enemies, List<NpcDefinition> Npcs) LoadShipped()
    {
        var resources = ResourcesDirectory();

        var enemies = new EnemyDefinitionCollection(NullLogger.Instance);
        Assert.IsTrue(enemies.Load(Path.Combine(resources, "Enemies.json")), "Enemies.json failed to load.");

        using var stream = File.OpenRead(Path.Combine(resources, "Npcs.json"));

        var npcs = JsonSerializer.Deserialize<List<NpcDefinition>>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        });

        Assert.IsNotNull(npcs);
        return (enemies, npcs);
    }

    [TestMethod]
    public void EveryNpcSharingAnEnemyName_IsAlsoAnEnemy()
    {
        var (enemies, npcs) = LoadShipped();

        var hostileNames = npcs
            .Where(npc => !string.IsNullOrWhiteSpace(npc.Name) && enemies.TryResolve(npc, out _))
            .Select(npc => npc.Name)
            .ToHashSet(StringComparer.Ordinal);

        var inert = npcs
            .Where(npc => hostileNames.Contains(npc.Name) && !enemies.TryResolve(npc, out _))
            .Select(npc => $"{npc.Id} \"{npc.Name}\" (NameId {npc.NameId}, ModelId {npc.ModelId})")
            .ToList();

        Assert.AreEqual(0, inert.Count,
            "These NPCs share a display name with an enemy but are not classified as one, so they spawn inert: "
            + string.Join("; ", inert));
    }

    [TestMethod]
    public void BothHooliganNameIds_SpawnHostile()
    {
        var (enemies, _) = LoadShipped();

        // 5100401 is most of Mac's crew; 20483 is the same name on the three placed by archer 23625.
        foreach (var nameId in new[] { 5100401, 20483 })
        {
            var hooligan = new NpcDefinition { Id = 23626, NameId = nameId, Name = "Hooligan", ModelId = 518 };

            Assert.IsTrue(enemies.TryResolve(hooligan, out var stats), $"NameId {nameId} did not resolve hostile.");
            Assert.AreEqual(2, stats.Level);
            Assert.AreEqual(575, stats.MaxHealth);
        }
    }
}
