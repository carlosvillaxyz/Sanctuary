using Microsoft.VisualStudio.TestTools.UnitTesting;

using Sanctuary.Game.Combat;
using Sanctuary.Game.Entities;
using Sanctuary.Game.Resources.Definitions.Combat;

namespace Sanctuary.Game.Tests;

[TestClass]
public sealed class CombatMathTests
{
    [TestMethod]
    public void PlayerDamage_ReferenceLevelReturnsTheSheetValue()
    {
        var settings = new AbilityCombatSettings { DamageReferenceLevel = 5, DamageGrowthPerLevel = 1.15f };

        Assert.AreEqual(254, PlayerDamage.Scale(254, 5, settings));
    }

    [TestMethod]
    public void PlayerDamage_GrowsByTheHealthRatioPerLevel()
    {
        // Brawler basic 254 at the reference level: four swings on a 500-HP level-1 wolf, ~2,067 at level 20.
        Assert.AreEqual(145, PlayerDamage.Scale(254, 1, 5, 1.15f));
        Assert.AreEqual(2067, PlayerDamage.Scale(254, 20, 5, 1.15f));
    }

    [TestMethod]
    public void PlayerDamage_ClampsLevelAndNeverDropsBelowOne()
    {
        Assert.AreEqual(PlayerDamage.Scale(100, 20, 5, 1.15f), PlayerDamage.Scale(100, 99, 5, 1.15f));
        Assert.AreEqual(PlayerDamage.Scale(100, 1, 5, 1.15f), PlayerDamage.Scale(100, -3, 5, 1.15f));
        Assert.AreEqual(1, PlayerDamage.Scale(1, 1, 20, 1.15f));
        Assert.AreEqual(0, PlayerDamage.Scale(0, 10, 5, 1.15f));
    }

    [TestMethod]
    public void Mitigate_NoDefenseTakesTheRawHit()
    {
        Assert.AreEqual(206, Player.Mitigate(206, 0, 0, 0, 515f));
    }

    [TestMethod]
    public void Mitigate_MatchesTheCapturedDrakeHits()
    {
        // Feathered Drake: 206 raw -> 123 against Defense 348 -> 113 with DamageReductionAmount 10.
        Assert.AreEqual(123, Player.Mitigate(206, 348, 0, 0, 515f));
        Assert.AreEqual(113, Player.Mitigate(206, 348, 10, 0, 515f));
    }

    [TestMethod]
    public void Mitigate_FullPercentReductionIsInvulnerability()
    {
        Assert.AreEqual(0, Player.Mitigate(500, 0, 0, 100, 515f));
        Assert.AreEqual(1, Player.Mitigate(5, 348, 10, 0, 515f));
    }
}
