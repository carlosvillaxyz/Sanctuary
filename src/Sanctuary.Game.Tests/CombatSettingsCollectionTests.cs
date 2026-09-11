using System;
using System.IO;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Sanctuary.Game.Resources;

namespace Sanctuary.Game.Tests;

[TestClass]
public sealed class CombatSettingsCollectionTests
{
    [TestMethod]
    public void MissingFile_KeepsDefaults()
    {
        var settings = new CombatSettingsCollection(NullLogger.Instance);

        Assert.IsTrue(settings.Load(Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.json")));
        Assert.AreEqual(6, settings.Player.OutOfCombatSeconds);
        Assert.AreEqual(10, settings.Player.KnockoutRecoverSeconds);
        Assert.AreEqual(515f, settings.Player.DefenseConstant);
        Assert.AreEqual(2000, settings.Enemy.DeathHoldMs);
        Assert.AreEqual(0.85f, settings.Abilities.BasicDamageDelayFraction);
    }

    [TestMethod]
    public void PartialFile_OverridesOnlyListedValues()
    {
        var path = Path.Combine(Path.GetTempPath(), $"combat-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, """{ "Player": { "OutOfCombatSeconds": 9, "SendInWorldCombatFlag": false }, "Enemy": { "DeathHoldMs": 1500 } }""");

        try
        {
            var settings = new CombatSettingsCollection(NullLogger.Instance);

            Assert.IsTrue(settings.Load(path));
            Assert.AreEqual(9, settings.Player.OutOfCombatSeconds);
            Assert.IsFalse(settings.Player.SendInWorldCombatFlag);
            Assert.AreEqual(1500, settings.Enemy.DeathHoldMs);
            Assert.AreEqual(10, settings.Player.KnockoutRecoverSeconds);
            Assert.AreEqual(11, settings.Enemy.AttackCursorId);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public void InvalidValues_FailToLoad()
    {
        var path = Path.Combine(Path.GetTempPath(), $"combat-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, """{ "Player": { "DefenseConstant": 0 } }""");

        try
        {
            Assert.IsFalse(new CombatSettingsCollection(NullLogger.Instance).Load(path));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
