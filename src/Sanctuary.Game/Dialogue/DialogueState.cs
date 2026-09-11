using System.Collections.Generic;

using Sanctuary.Game.Entities;
using Sanctuary.Game.Resources.Definitions;

namespace Sanctuary.Game.Dialogue;

public enum DialogueMode
{
    /// <summary>The NPC is offering a quest: Accept / Decline.</summary>
    Offer,

    /// <summary>The NPC is the target of the player's current quest goal: talk through the lines, then advance.</summary>
    Goal,

    /// <summary>The player already has this NPC's quest: a reminder line and Goodbye.</summary>
    Reminder
}

/// <summary>The conversation a player currently has open.</summary>
public sealed class DialogueState
{
    public required Npc Speaker { get; init; }
    public required DialogueMode Mode { get; init; }
    public required int QuestId { get; init; }

    /// <summary>Goal mode: the lines to show and which one is on screen.</summary>
    public IReadOnlyList<QuestDialogueLine> Lines { get; init; } = [];
    public int LineIndex { get; set; }
}
