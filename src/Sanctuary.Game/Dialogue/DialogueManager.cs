using System;
using System.Linq;
using System.Numerics;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Core.Helpers;
using Sanctuary.Database;
using Sanctuary.Database.Entities;
using Sanctuary.Game.Entities;
using Sanctuary.Game.Helpers;
using Sanctuary.Game.Resources.Definitions;
using Sanctuary.Game.Resources.Definitions.Rewards;
using Sanctuary.Packet;

namespace Sanctuary.Game.Dialogue;

/// <summary>
/// NPC conversations in the client's dialogue window (CommandPacketShowDialog), driven by Quests.json.
///
/// Clicking an NPC, in priority order:
///   1. the NPC is the target of one of the player's active quest goals: show that goal's lines, then advance;
///   2. the NPC gives a quest the player can take: offer it ("Yes, I accept!" / "No! I decline.");
///   3. the NPC gave a quest the player is still on: reminder line and "Goodbye".
/// Otherwise nothing opens; ambient NPCs keep their over-head chat bubbles.
///
/// Quest progress lives in CharacterQuests: GoalProgress is the index of the current goal, and the quest is
/// Completed once the last goal is done.
/// </summary>
public static class DialogueManager
{
    /// <summary>Speech-bubble hover cursor (client Resources/Cursors.txt: "cursor_interaction_talk.cur").</summary>
    public const byte TalkCursorId = 13;

    // Button captions are the client's own strings.
    private const int AcceptTextId = 71313;    // "Yes, I accept!"
    private const int DeclineTextId = 71314;   // "No! I decline."
    private const int GoodbyeTextId = 8075;    // "Goodbye"
    private const int ContinueTextId = 386270; // "Continue"

    private const int AcceptButton = 1;
    private const int DeclineButton = 2;
    private const int GoodbyeButton = 3;
    private const int ContinueButton = 4;

    private static IResourceManager? _resourceManager;
    private static IDbContextFactory<DatabaseContext> _dbContextFactory = null!;
    private static ILogger _logger = null!;

    public static void Configure(IServiceProvider serviceProvider)
    {
        _resourceManager = serviceProvider.GetRequiredService<IResourceManager>();
        _dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<DatabaseContext>>();
        _logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DialogueManager));
    }

    /// <summary>True if this NPC takes part in any quest, i.e. clicking it can open a conversation.</summary>
    public static bool IsQuestNpc(IResourceManager resourceManager, ulong npcGuid)
        => resourceManager.Quests.ByGiver.ContainsKey(npcGuid) || resourceManager.Quests.ByTarget.ContainsKey(npcGuid);

    /// <summary>Opens whatever this NPC has to say to the player. Returns false if there is nothing.</summary>
    public static bool Start(Player player, Npc npc)
    {
        if (_resourceManager is null)
            return false;

        var quests = _resourceManager.Quests;
        var characterId = GuidHelper.GetPlayerId(player.Guid);

        using var dbContext = _dbContextFactory.CreateDbContext();
        var states = dbContext.CharacterQuests.AsNoTracking()
            .Where(x => x.CharacterId == characterId)
            .ToDictionary(x => x.QuestId);

        // 1. The player's current goal targets this NPC.
        if (quests.ByTarget.TryGetValue(npc.Guid, out var targetQuestIds))
        {
            foreach (var questId in targetQuestIds)
            {
                if (!states.TryGetValue(questId, out var state) || state.Completed)
                    continue;

                if (!quests.TryGet(questId, out var quest) || state.GoalProgress >= quest.Goals.Count)
                    continue;

                var goal = quest.Goals[state.GoalProgress];
                if (!goal.AllTalkTargetGuids().Contains(npc.Guid))
                    continue;

                var lines = goal.ConversationFor(npc.Guid);
                if (lines.Count == 0)
                    lines = [new QuestDialogueLine { TextId = quest.TurnInDialogueId }];

                Open(player, new DialogueState { Speaker = npc, Mode = DialogueMode.Goal, QuestId = questId, Lines = lines });
                return true;
            }
        }

        if (quests.ByGiver.TryGetValue(npc.Guid, out var giverQuestIds))
        {
            var done = states.ToDictionary(x => x.Key, x => x.Value.Completed);

            // 2. A quest this NPC can offer.
            foreach (var questId in giverQuestIds)
            {
                if (quests.TryGet(questId, out var quest) && quest.IsOfferableFor(done))
                {
                    Open(player, new DialogueState { Speaker = npc, Mode = DialogueMode.Offer, QuestId = questId });
                    return true;
                }
            }

            // 3. A quest from this NPC that is still in progress.
            foreach (var questId in giverQuestIds)
            {
                if (states.TryGetValue(questId, out var state) && !state.Completed)
                {
                    Open(player, new DialogueState { Speaker = npc, Mode = DialogueMode.Reminder, QuestId = questId });
                    return true;
                }
            }
        }

        return false;
    }

    public static void OnResponse(Player player, int buttonId)
    {
        var state = player.ActiveDialogue;
        if (state is null || _resourceManager is null || !_resourceManager.Quests.TryGet(state.QuestId, out var quest))
        {
            End(player);
            return;
        }

        switch (state.Mode)
        {
            case DialogueMode.Offer when buttonId == AcceptButton:
                AcceptQuest(player, state.Speaker, quest);
                break;

            case DialogueMode.Goal when buttonId == ContinueButton:
                if (state.LineIndex + 1 < state.Lines.Count)
                {
                    state.LineIndex++;
                    Show(player, state);
                    return;
                }

                AdvanceGoal(player, state.Speaker, quest);
                break;

            default:
                End(player);
                break;
        }
    }

    /// <summary>The client closed the window itself (Escape).</summary>
    public static void OnClientClosed(Player player)
    {
        if (player.ActiveDialogue is null)
            return;

        player.ActiveDialogue = null;
        player.SendTunneled(new CommandPacketFreeInteractionNpc());
    }

    public static void End(Player player)
    {
        player.ActiveDialogue = null;
        player.SendTunneled(new CommandPacketEndDialog());
        player.SendTunneled(new CommandPacketFreeInteractionNpc());
    }

    private static void Open(Player player, DialogueState state)
    {
        player.ActiveDialogue = state;
        Show(player, state);
    }

    private static void Show(Player player, DialogueState state)
    {
        _resourceManager!.Quests.TryGet(state.QuestId, out var quest);

        var packet = new CommandPacketShowDialog
        {
            SpeakerGuid = state.Speaker.Guid,
            CameraPlacement = CameraFor(player, state.Speaker),
            LookAt = state.Speaker.Position + new Vector4(0f, 1.4f, 0f, 0f),
        };

        switch (state.Mode)
        {
            case DialogueMode.Offer:
                packet.DialogTextId = quest!.GiverDialogueId;
                packet.Choices.Add(new DialogChoice { ButtonId = AcceptButton, ButtonTextId = AcceptTextId });
                packet.Choices.Add(new DialogChoice { ButtonId = DeclineButton, ButtonTextId = DeclineTextId });
                break;

            case DialogueMode.Goal:
                var line = state.Lines[state.LineIndex];
                packet.DialogTextId = line.TextId;
                packet.Choices.Add(new DialogChoice
                {
                    ButtonId = ContinueButton,
                    ButtonTextId = line.ResponseTextId != 0 ? line.ResponseTextId : ContinueTextId
                });
                break;

            case DialogueMode.Reminder:
                packet.DialogTextId = quest!.ObjectiveDescriptionId != 0 ? quest.ObjectiveDescriptionId : quest.GiverDialogueId;
                packet.Choices.Add(new DialogChoice { ButtonId = GoodbyeButton, ButtonTextId = GoodbyeTextId });
                break;
        }

        player.SendTunneled(packet);
    }

    private static void AcceptQuest(Player player, Npc speaker, QuestDefinition quest)
    {
        var characterId = GuidHelper.GetPlayerId(player.Guid);

        using (var dbContext = _dbContextFactory.CreateDbContext())
        {
            if (!dbContext.CharacterQuests.Any(x => x.CharacterId == characterId && x.QuestId == quest.QuestId))
            {
                dbContext.CharacterQuests.Add(new DbCharacterQuest { CharacterId = characterId, QuestId = quest.QuestId });
                dbContext.SaveChanges();
            }
        }

        _logger.LogInformation("{Player} accepted quest {QuestId} from {Npc}", player.Name, quest.QuestId, speaker.Guid);

        End(player);

        // The giver restates the objective as a chat line the player can re-read.
        if (quest.ObjectiveDescriptionId != 0)
            SayTo(player, speaker, quest.ObjectiveDescriptionId);
    }

    private static void AdvanceGoal(Player player, Npc speaker, QuestDefinition quest)
    {
        var characterId = GuidHelper.GetPlayerId(player.Guid);
        var completed = false;

        using (var dbContext = _dbContextFactory.CreateDbContext())
        {
            var state = dbContext.CharacterQuests.FirstOrDefault(x => x.CharacterId == characterId && x.QuestId == quest.QuestId);
            if (state is null || state.Completed)
            {
                End(player);
                return;
            }

            state.GoalProgress++;
            if (state.GoalProgress >= quest.Goals.Count)
            {
                state.Completed = true;
                completed = true;
            }

            dbContext.SaveChanges();

            if (completed)
                GrantRewards(dbContext, player, speaker, quest);
        }

        _logger.LogInformation("{Player} {Result} quest {QuestId} at {Npc}", player.Name,
            completed ? "completed" : "advanced", quest.QuestId, speaker.Guid);

        // Chains continue in the same window: if this NPC has more to say (the next goal, or the next quest in
        // the chain), replace the window's contents. Closing and reopening races the client's close
        // acknowledgement, which would wipe the new conversation's server-side state.
        if (!Start(player, speaker))
            End(player);
    }

    private static void GrantRewards(DatabaseContext dbContext, Player player, Npc speaker, QuestDefinition quest)
    {
        if (quest.RewardCoins > 0)
            RewardHelper.TryGrantCurrency(dbContext, _logger, player, CurrencyType.Coins, quest.RewardCoins, speaker.Guid);

        if (quest.RewardExperience > 0)
            RewardHelper.TryGrantExperience(_resourceManager!, dbContext, _logger, player, player.ActiveProfileId, quest.RewardExperience, speaker.Guid);

        foreach (var itemId in quest.RewardItems)
            RewardHelper.TryGrantItem(_resourceManager!, dbContext, _logger, player, itemId, 0, 1, speaker.Guid);
    }

    private static void SayTo(Player player, Npc speaker, int stringId)
    {
        player.SendTunneled(new ChatPacketFromStringId
        {
            SpeakerGuid = speaker.Guid,
            StringId = stringId,
            IsChatLogged = true
        });
    }

    /// <summary>Camera beside the player, framing the NPC's face.</summary>
    private static Vector4 CameraFor(Player player, Npc npc)
    {
        var flat = new Vector4(npc.Position.X - player.Position.X, 0f, npc.Position.Z - player.Position.Z, 0f);
        var len = flat.Length();
        var dir = len > 0.01f ? flat / len : new Vector4(0f, 0f, 1f, 0f);

        var side = new Vector4(-dir.Z, 0f, dir.X, 0f);
        var camera = npc.Position - dir * 2.5f + side * 0.8f;
        camera.Y = npc.Position.Y + 1.6f;
        camera.W = 0f;
        return camera;
    }
}
