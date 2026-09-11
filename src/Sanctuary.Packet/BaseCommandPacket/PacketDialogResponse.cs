using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

/// <summary>Sent by the client when the player presses a response button in the conversation window.</summary>
public class PacketDialogResponse : BaseCommandPacket, IDeserializable<PacketDialogResponse>
{
    public new const short OpCode = 6;

    public int ButtonId;

    public int RemainingBytes;

    public PacketDialogResponse() : base(OpCode)
    {
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out PacketDialogResponse value)
    {
        value = new PacketDialogResponse();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out value.ButtonId))
            return false;

        // Layout not fully verified; tolerate and report trailing bytes instead of failing.
        value.RemainingBytes = reader.RemainingLength;

        return true;
    }

    public override string ToString() => $"ButtonId={ButtonId} RemainingBytes={RemainingBytes}";
}
