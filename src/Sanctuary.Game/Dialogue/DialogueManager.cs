using System.Collections.Generic;
using System.Numerics;

using Sanctuary.Game.Entities;
using Sanctuary.Packet;

namespace Sanctuary.Game.Dialogue;

/// <summary>
/// Drives the client's conversation window for NPC talk. First iteration: a hard-coded test conversation
/// used to verify the CommandPacketShowDialog wire layout. Data-driven dialogue trees come next.
/// </summary>
public static class DialogueManager
{
    // String ids that exist in the client's locale table (used only for the test conversation).
    private const int WelcomeTextId = 8130;      // "Welcome to Free Realms!"
    private const int IntroduceGiverTextId = 94246; // "Introduce Yourself" quest giver line

    public static void Start(Player player, Npc npc)
    {
        player.ActiveDialogue = new DialogueState { Speaker = npc, NodeId = "greet" };
        ShowNode(player, npc, "greet");
    }

    public static void OnResponse(Player player, int buttonId)
    {
        var state = player.ActiveDialogue;
        if (state is null)
            return;

        // Test tree: greet -> (1) more -> (1) end ; (2) end
        switch (state.NodeId, buttonId)
        {
            case ("greet", 1):
                state.NodeId = "more";
                ShowNode(player, state.Speaker, "more");
                break;
            default:
                End(player);
                break;
        }
    }

    public static void End(Player player)
    {
        if (player.ActiveDialogue is null)
            return;

        player.ActiveDialogue = null;
        player.SendTunneled(new CommandPacketEndDialog());
        player.SendTunneled(new CommandPacketFreeInteractionNpc());
    }

    private static void ShowNode(Player player, Npc npc, string nodeId)
    {
        var packet = new CommandPacketShowDialog
        {
            SpeakerGuid = npc.Guid,
            CameraPlacement = CameraFor(player, npc),
            LookAt = npc.Position + new Vector4(0f, 1.4f, 0f, 0f),
            NewPlayerPosition = player.Position,
        };

        switch (nodeId)
        {
            case "greet":
                packet.DialogTextId = WelcomeTextId;
                packet.Choices.Add(new DialogChoice { ButtonId = 1, ButtonTextId = WelcomeTextId });
                packet.Choices.Add(new DialogChoice { ButtonId = 2, ButtonTextId = WelcomeTextId });
                break;
            case "more":
                packet.DialogTextId = IntroduceGiverTextId;
                packet.Choices.Add(new DialogChoice { ButtonId = 1, ButtonTextId = WelcomeTextId });
                break;
        }

        player.SendTunneled(packet);
    }

    /// <summary>Camera a little in front of and above the player, framing the NPC's face.</summary>
    private static Vector4 CameraFor(Player player, Npc npc)
    {
        var toNpc = npc.Position - player.Position;
        toNpc.W = 0f;
        var flat = new Vector4(toNpc.X, 0f, toNpc.Z, 0f);
        var len = flat.Length();
        var dir = len > 0.01f ? flat / len : new Vector4(0f, 0f, 1f, 0f);

        // Stand 2.5 units from the NPC, offset to the side so the shot is over the player's shoulder.
        var side = new Vector4(-dir.Z, 0f, dir.X, 0f);
        var camera = npc.Position - dir * 2.5f + side * 0.8f;
        camera.Y = npc.Position.Y + 1.6f;
        camera.W = 0f;
        return camera;
    }
}
