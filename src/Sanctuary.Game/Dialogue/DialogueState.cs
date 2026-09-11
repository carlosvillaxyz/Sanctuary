using Sanctuary.Game.Entities;

namespace Sanctuary.Game.Dialogue;

/// <summary>The conversation a player currently has open: which NPC is speaking and which node is shown.</summary>
public sealed class DialogueState
{
    public required Npc Speaker { get; init; }
    public required string NodeId { get; set; }
}
