using System.Linq;
using System.Numerics;

using Sanctuary.Game.Entities;
using Sanctuary.Game.Helpers;

namespace Sanctuary.Game.ChatCommands;

// Admin-only combat probes for play-testing: read the state of the enemies around you, set your own health,
// knock yourself out or recover. Ours (Sulphural's !lp / !abil / !fx / !anim probes were not ported).
public class CombatChatCommand : IChatCommand
{
    private readonly IChatCommandManager _chatCommandManager;

    public string KeyWord => "combat";
    public string Usage => "enemies [radius] | hp <amount> | knockout | revive";
    public string Description => "Lists nearby enemies with their stats, sets your health, knocks you out or revives you.";
    public ChatCommandRole RequiredRole => ChatCommandRole.Admin;

    public CombatChatCommand(IChatCommandManager chatCommandManager)
    {
        _chatCommandManager = chatCommandManager;
    }

    public bool Handle(Player invoker, string[] args)
    {
        if (args.Length < 1)
            return false;

        switch (args[0].ToLowerInvariant())
        {
            case "enemies":
                return ListEnemies(invoker, args[1..]);
            case "hp":
                return SetHealth(invoker, args[1..]);
            case "knockout":
                invoker.Knockout();
                _chatCommandManager.LogAction(this, invoker, "Combat knockout", null, null);
                return true;
            case "revive":
                invoker.Revive(null);
                _chatCommandManager.LogAction(this, invoker, "Combat revive", null, null);
                return true;
            default:
                return false;
        }
    }

    private static bool ListEnemies(Player invoker, string[] args)
    {
        var radius = 60f;

        if (args.Length == 1 && !float.TryParse(args[0], out radius))
            return false;

        var origin = new Vector3(invoker.Position.X, invoker.Position.Y, invoker.Position.Z);

        var enemies = invoker.Zone.Npcs
            .OfType<CombatNpc>()
            .Select(enemy => (Enemy: enemy, Distance: Vector3.Distance(origin, new Vector3(enemy.Position.X, enemy.Position.Y, enemy.Position.Z))))
            .Where(entry => entry.Distance <= radius)
            .OrderBy(entry => entry.Distance)
            .Take(12)
            .ToList();

        if (enemies.Count == 0)
        {
            ChatHelper.SendSystemMessage(invoker, $"No enemies within {radius:F0} units.");
            return true;
        }

        ChatHelper.SendSystemMessage(invoker,
            $"You: {invoker.CurrentHealth}/{invoker.MaxHealth} hp, energy {invoker.Energy}, " +
            $"{(invoker.IsDead ? "knocked out" : invoker.InWorldCombat ? "in combat" : "out of combat")}" +
            $"{(invoker.IsInvulnerable ? ", invulnerable" : string.Empty)}.");

        foreach (var (enemy, distance) in enemies)
        {
            // "sees you" is the aggro precondition: an enemy only looks for targets among the players its zone
            // tile has handed it, so an enemy that cannot see you will never wake up however close you stand.
            ChatHelper.SendSystemMessage(invoker,
                $"{enemy.Name} ({enemy.Guid}) L{enemy.Stats.Level} {enemy.Stats.Tier}: {enemy.Health}/{enemy.MaxHealth} hp, " +
                $"dmg {enemy.Stats.Damage}, {enemy.Stats.Xp} stars, {(enemy.IsDead ? "dead" : enemy.State.ToString())}, {distance:F1} u, " +
                $"aggro {enemy.Stats.AggroRange:F0} u, sees you: {(enemy.VisiblePlayers.ContainsKey(invoker.Guid) ? "yes" : "NO")}.");
        }

        return true;
    }

    private bool SetHealth(Player invoker, string[] args)
    {
        if (args.Length != 1 || !int.TryParse(args[0], out var amount))
            return false;

        if (amount <= 0)
        {
            invoker.Knockout();
        }
        else
        {
            invoker.Heal(-invoker.CurrentHealth + invoker.MaxHealth); // to full, then trim down
            var remove = invoker.MaxHealth - amount;

            if (remove > 0)
                invoker.TakeDamage(remove, null);
        }

        _chatCommandManager.LogAction(this, invoker, "Combat set health", null, $"amount={amount}");
        ChatHelper.SendSystemMessage(invoker, $"Health set to {invoker.CurrentHealth}/{invoker.MaxHealth}.");
        return true;
    }
}
