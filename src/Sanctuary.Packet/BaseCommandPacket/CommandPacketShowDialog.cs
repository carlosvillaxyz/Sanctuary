using System.Collections.Generic;
using System.Numerics;

using Sanctuary.Core.IO;
using Sanctuary.Packet.Common;

namespace Sanctuary.Packet;

/// <summary>
/// Opens the client's conversation window (wndFlashDialog / dialog.swf) with one line of speaker text and a
/// list of response buttons. Field order follows oxide's EnterDialog (Clone Wars Adventures, same engine);
/// the Free Realms layout has not been independently verified yet.
/// </summary>
public class CommandPacketShowDialog : BaseCommandPacket, ISerializablePacket
{
    public new const short OpCode = 3;

    public int DialogTextId;
    public int SpeakerAnimationId;
    public ulong SpeakerGuid;
    public bool EnableEscape = true;
    public float Unknown4;
    public List<DialogChoice> Choices = [];
    public Vector4 CameraPlacement;
    public Vector4 LookAt;
    public bool ChangePlayerPosition;
    public Vector4 NewPlayerPosition;
    public float Unknown8;
    public bool HidePlayers;
    public bool Unknown10 = true;
    public bool Unknown11 = true;
    public float Zoom;
    public int SpeakerSoundId;

    public CommandPacketShowDialog() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        base.Write(writer);

        writer.Write(DialogTextId);
        writer.Write(SpeakerAnimationId);
        writer.Write(SpeakerGuid);
        writer.Write(EnableEscape);
        writer.Write(Unknown4);
        writer.Write(Choices);
        writer.Write(CameraPlacement);
        writer.Write(LookAt);
        writer.Write(ChangePlayerPosition);
        writer.Write(NewPlayerPosition);
        writer.Write(Unknown8);
        writer.Write(HidePlayers);
        writer.Write(Unknown10);
        writer.Write(Unknown11);
        writer.Write(Zoom);
        writer.Write(SpeakerSoundId);

        return writer.Buffer;
    }
}

public class DialogChoice : ISerializableType
{
    public int ButtonId;
    public int Unknown2;
    public int ButtonTextId;
    public int Unknown4;
    public int Unknown5;

    public void Serialize(PacketWriter writer)
    {
        writer.Write(ButtonId);
        writer.Write(Unknown2);
        writer.Write(ButtonTextId);
        writer.Write(Unknown4);
        writer.Write(Unknown5);
    }
}
