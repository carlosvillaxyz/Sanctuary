using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Game;
using Sanctuary.Game.Combat;
using Sanctuary.Packet;
using Sanctuary.Packet.Common.Attributes;

namespace Sanctuary.Gateway.Handlers;

// op32/1 AutoAttackTarget and op32/3 SingleAttackTarget: the client clicked an enemy. Both are the weapon's
// basic attack (slot 0) against that target, paced by the same swing gate as a toolbar press so click-attacking
// and pressing "1" can never out-swing each other (Sulphural/main e9912598).
[PacketHandler]
public static class CombatPacketAutoAttackTargetHandler
{
    private static ILogger _logger = null!;
    private static IResourceManager _resourceManager = null!;

    public static void ConfigureServices(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger(nameof(CombatPacketAutoAttackTargetHandler));

        _resourceManager = serviceProvider.GetRequiredService<IResourceManager>();
    }

    public static bool HandlePacket(GatewayConnection connection, ReadOnlySpan<byte> data)
    {
        if (!CombatPacketAutoAttackTarget.TryDeserialize(data, out var packet))
        {
            _logger.LogError("Failed to deserialize {packet}.", nameof(CombatPacketAutoAttackTarget));
            return false;
        }

        CombatEngine.TryAttack(_resourceManager, connection.Player, CombatEngine.BasicSlot, packet.TargetGuid);
        return true;
    }

    public static bool HandleSingleAttack(GatewayConnection connection, ReadOnlySpan<byte> data)
    {
        if (!CombatPacketSingleAttackTarget.TryDeserialize(data, out var packet))
        {
            _logger.LogError("Failed to deserialize {packet}.", nameof(CombatPacketSingleAttackTarget));
            return false;
        }

        CombatEngine.TryAttack(_resourceManager, connection.Player, CombatEngine.BasicSlot, packet.TargetGuid);
        return true;
    }
}
