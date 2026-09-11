using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

/// <summary>Releases the client-side interaction lock taken when a dialog was opened.</summary>
public class CommandPacketFreeInteractionNpc : BaseCommandPacket, ISerializablePacket
{
    public new const short OpCode = 20;

    public CommandPacketFreeInteractionNpc() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        base.Write(writer);

        return writer.Buffer;
    }
}
