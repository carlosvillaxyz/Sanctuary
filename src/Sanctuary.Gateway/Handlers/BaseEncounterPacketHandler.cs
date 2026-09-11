using System;
using System.Linq;
using System.Numerics;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Core.Helpers;
using Sanctuary.Core.IO;
using Sanctuary.Database;
using Sanctuary.Game;
using Sanctuary.Game.Entities;
using Sanctuary.Packet;
using Sanctuary.Packet.Common.Attributes;

namespace Sanctuary.Gateway.Handlers;

// op41 BaseEncounterPacket, client to server. Only the respawn window's Revive buttons (sub 122) matter to the
// overworld; the dungeon/arena sub-opcodes (invite response 103, GO! 108, leave 109, cancel 124) belong to the
// instance slice and are ignored here. Ported from Sulphural/main c34648df (HandleResume).
[PacketHandler]
public static class BaseEncounterPacketHandler
{
    // C2S = the "Revive" buttons on the respawn window.
    private const short EncounterParticipantResume = 122;

    // Warpstone / town points of interest carry this notification type.
    private const int WarpstoneNotificationType = 7;

    private static ILogger _logger = null!;
    private static IResourceManager _resourceManager = null!;
    private static IDbContextFactory<DatabaseContext> _dbContextFactory = null!;

    public static void ConfigureServices(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger(nameof(BaseEncounterPacketHandler));

        _resourceManager = serviceProvider.GetRequiredService<IResourceManager>();
        _dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<DatabaseContext>>();
    }

    public static bool HandlePacket(GatewayConnection connection, PacketReader reader)
    {
        if (!reader.TryRead(out short subOpCode))
        {
            _logger.LogError("Failed to read encounter sub-opcode. ( Data: {data} )", Convert.ToHexString(reader.Span));
            return false;
        }

        switch (subOpCode)
        {
            case EncounterParticipantResume:
                return HandleResume(connection, reader);

            default:
                _logger.LogDebug("Ignoring encounter sub-opcode {sub}. ( Data: {data} )", subOpCode, Convert.ToHexString(reader.Span));
                return true;
        }
    }

    // Wire (Sulphural live capture): [int][int][byte option] - option 1 = "Revive here", 0 = "Revive at safe location".
    private static bool HandleResume(GatewayConnection connection, PacketReader reader)
    {
        reader.TryRead(out int _);
        reader.TryRead(out int _);
        reader.TryRead(out byte option);

        var player = connection.Player;

        if (!player.IsDead)
            return true;

        if (option == 1)
        {
            var cost = _resourceManager.CombatSettings.Player.ReviveHereCoinCost;

            if (cost <= 0 || TrySpendCoins(player, cost))
            {
                _logger.LogInformation("Revive here for {name}.", player.Name);
                player.Revive(null);
                return true;
            }

            _logger.LogInformation("Revive here refused for {name} (needs {cost} coins); reviving at the nearest warpstone.", player.Name, cost);
        }

        var safe = NearestWarpstone(player.DeathPosition);

        _logger.LogInformation("Revive at safe location for {name}.", player.Name);
        player.Revive(safe);
        return true;
    }

    // The nearest warpstone/town spawn point; the death spot itself if the zone has none loaded.
    private static Vector4 NearestWarpstone(Vector4 from)
    {
        var best = from;
        var bestDistance = float.MaxValue;

        foreach (var poi in _resourceManager.PointOfInterests.Values)
        {
            if (poi.NotificationType != WarpstoneNotificationType)
                continue;

            var target = poi.SpawnPosition != default ? poi.SpawnPosition : poi.Position;

            var dx = target.X - from.X;
            var dz = target.Z - from.Z;
            var distance = dx * dx + dz * dz;

            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            best = target;
        }

        return best;
    }

    private static bool TrySpendCoins(Player player, int coins)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var dbCharacter = dbContext.Characters.SingleOrDefault(x => x.Id == GuidHelper.GetPlayerId(player.Guid));

        if (dbCharacter is null || dbCharacter.Coins < coins)
            return false;

        dbCharacter.Coins -= coins;
        dbContext.SaveChanges();

        player.Coins = dbCharacter.Coins;
        player.SendTunneled(new ClientUpdatePacketCoinCount { Coins = player.Coins });
        return true;
    }
}
