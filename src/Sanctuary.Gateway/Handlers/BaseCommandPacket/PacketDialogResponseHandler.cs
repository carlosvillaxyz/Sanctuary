using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Game.Dialogue;
using Sanctuary.Packet;
using Sanctuary.Packet.Common.Attributes;

namespace Sanctuary.Gateway.Handlers;

[PacketHandler]
public static class PacketDialogResponseHandler
{
    private static ILogger _logger = null!;

    public static void ConfigureServices(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger(nameof(PacketDialogResponseHandler));

        DialogueManager.Configure(serviceProvider);
    }

    public static bool HandlePacket(GatewayConnection connection, ReadOnlySpan<byte> data)
    {
        if (!PacketDialogResponse.TryDeserialize(data, out var packet))
        {
            _logger.LogError("Failed to deserialize {packet}. ( Data: {data} )", nameof(PacketDialogResponse), Convert.ToHexString(data));
            return false;
        }

        _logger.LogInformation("Dialog response from {player}: {packet} ( Data: {data} )", connection.Player.Name, packet, Convert.ToHexString(data));

        DialogueManager.OnResponse(connection.Player, packet.ButtonId);
        return true;
    }

    /// <summary>The client closed the window itself (Escape): sub-opcode 4 with no body.</summary>
    public static bool HandleClientClosed(GatewayConnection connection)
    {
        _logger.LogInformation("Dialog closed by client for {player}", connection.Player.Name);

        DialogueManager.OnClientClosed(connection.Player);
        return true;
    }
}
