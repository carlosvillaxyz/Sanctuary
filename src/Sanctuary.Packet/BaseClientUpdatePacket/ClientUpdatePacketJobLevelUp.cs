using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

/// <summary>
/// Plays the full-screen job level-up celebration (levelup_&lt;job&gt;.gfx, the client's "JobLevelUp" UI event).
/// OpCode 38 (ClientUpdate) / SubOpCode 15. The client reads one length-prefixed payload, parses it as a
/// serialized ClientPcProfile (the same blob ClientUpdatePacketActivateProfile carries) and takes the job's
/// name, icon and level from it. There is no gate: a fully consumed payload always plays the celebration.
/// Layout found by Sulphural (github.com/Sulphural/Sanctuary); confirmed against the client's handler for
/// sub-opcode 15, which calls the "JobLevelUp" script event.
/// </summary>
public class ClientUpdatePacketJobLevelUp : BaseClientUpdatePacket, ISerializablePacket
{
    public new const short OpCode = 15;

    /// <summary>Serialized ClientPcProfile of the job that levelled up.</summary>
    public byte[] Payload = Array.Empty<byte>();

    public ClientUpdatePacketJobLevelUp() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.WritePayload(Payload);

        return writer.Buffer;
    }
}
