using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Core.IO;
using Sanctuary.Packet;
using Sanctuary.Packet.Common.Attributes;

namespace Sanctuary.Gateway.Handlers;

// op32 BaseCombatPacket, client to server: click-to-attack requests. Ported from Sulphural/main c34648df.
[PacketHandler]
public static class BaseCombatPacketHandler
{
    private static ILogger _logger = null!;

    public static void ConfigureServices(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger(nameof(BaseCombatPacketHandler));
    }

    public static bool HandlePacket(GatewayConnection connection, PacketReader reader)
    {
        if (!reader.TryRead(out short opCode))
        {
            _logger.LogError("Failed to read opcode from combat packet. ( Data: {data} )", Convert.ToHexString(reader.Span));
            return false;
        }

        return opCode switch
        {
            CombatPacketAutoAttackTarget.OpCode => CombatPacketAutoAttackTargetHandler.HandlePacket(connection, reader.Span),
            CombatPacketSingleAttackTarget.OpCode => CombatPacketAutoAttackTargetHandler.HandleSingleAttack(connection, reader.Span),
            _ => false
        };
    }
}
