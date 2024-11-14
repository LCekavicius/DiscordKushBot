using Discord;
using KushBot.DataClasses.Enums;
using KushBot.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KushBot.DataClasses;

public class Log
{
    public string Actor { get; init; }
    public string Content { get; init; }
}

public class Loot
{
    public required int Baps { get; init; }
}

public sealed class ParticipatingUser(string userName, KushBotUser user)
{
    public string UserName { get; } = userName;
    public KushBotUser User { get; } = user;
    public BossAbilities Debuffs { get; set; } = BossAbilities.None;
}

public class BossFightSim(Boss Boss, List<ParticipatingUser> Users)
{
    private List<(List<Log> userLogs, List<Log> bossLogs)> Logs = [];

    private int RoundCount = Boss.Rarity == enums.RarityType.Archon ? 3 : 2;
    private BossAbilities Buffs = BossAbilities.None;
    private int Health = Boss.Health;

    private Loot Loot = new()
    {
        Baps = (int)Boss.Rarity * (700 + 70 * (4 - Boss.ParticipantSlots)) + 50 * Boss.Level
    };

    public void Handle()
    {
        for (int i = 0; i < RoundCount; i++)
        {
            Logs.Add(HandleRound());
            if (Health <= 0)
            {
                break;
            }
        }
    }

    private (List<Log> userLogs, List<Log> Logs) HandleRound()
    {
        var userLogs = HandleUserTurn();

        if (Health < 0)
        {
            return (userLogs, null);
        }

        var Logs = HandleBossTurn();

        return (userLogs, Logs);
    }

    private List<Log> HandleBossTurn()
    {
        var buffs = new HashSet<BossAbilities>() { BossAbilities.Harden, BossAbilities.Dodge, BossAbilities.Dismantle };
        var debuffs = new HashSet<BossAbilities>() { BossAbilities.Paralyze };
        var instant = new Dictionary<BossAbilities, Func<Log>>() { { BossAbilities.Regeneration, HandleRegenerate } };

        var activeAbilities = Boss.Abilities.GetEnabled();

        var picked = activeAbilities.Any()
            ? activeAbilities[new Random().Next(activeAbilities.Count)]
            : BossAbilities.None;

        Log log = null;

        if (buffs.Contains(picked))
        {
            Buffs = Buffs | picked;
            log = new Log
            {
                Actor = BossBases.Bases[Boss.BossBaseIndex].Name,
                Content = $"used **{picked}**"
            };
        }
        else if (debuffs.Contains(picked))
        {
            var unaffectedUsers = Users.Where(e => !e.Debuffs.HasFlag(picked)).ToList();
            var pickedUser = unaffectedUsers[Random.Shared.Next(0, unaffectedUsers.Count)];
            pickedUser.Debuffs = pickedUser.Debuffs | picked;

            log = new Log
            {
                Actor = BossBases.Bases[Boss.BossBaseIndex].Name,
                Content = $"used **{picked}** on **{pickedUser.UserName}**"
            };
        }
        else if (instant.ContainsKey(picked))
        {
            log = instant[picked].Invoke();
        }

        return [log];
    }

    private Log HandleRegenerate()
    {
        var range = BossBases.GetAbilityEffectRange(BossAbilities.Regeneration);
        var percent = Random.Shared.Next(range.min.Value, range.max.Value + 1);

        double multiplier = (double)percent / 100;
        var heal = (double)(Boss.Health - Health) * multiplier;
        Health += (int)heal;

        return new Log
        {
            Actor = BossBases.Bases[Boss.BossBaseIndex].Name,
            Content = $"healed for **{heal}**"
        };
    }

    private List<Log> HandleUserTurn()
    {
        List<Log> logs = [];

        int totalDmg = 0;
        foreach (var user in Users)
        {
            int dmg = GetUserDamage(user);
            bool missed = false;

            if (Buffs.HasFlag(BossAbilities.Dodge))
            {
                missed = Random.Shared.Next(1, 101) <= GetAbilityEffectiveness(BossAbilities.Dodge);
            }

            if (Buffs.HasFlag(BossAbilities.Harden))
            {
                var reduction = (double)GetAbilityEffectiveness(BossAbilities.Harden) / 100;
                dmg = (int)(dmg * (1 - reduction));
            }

            var result = GetUserAttackResult(dmg, missed, user.Debuffs);

            logs.Add(new Log
            {
                Actor = user.UserName,
                Content = result.content,
            });
            totalDmg += dmg;
        }

        Health -= totalDmg;

        Buffs &= ~BossAbilities.Harden;
        Buffs &= ~BossAbilities.Dismantle;

        return logs;
    }

    private int GetAbilityEffectiveness(BossAbilities bossAbility)
    {
        var range = BossBases.GetAbilityEffectRange(bossAbility);
        return Random.Shared.Next(range.min.Value, range.max.Value + 1);
    }

    private (int dmg, string content) GetUserAttackResult(int dmg, bool missed, BossAbilities debuffs) =>
        debuffs switch
        {
            _ when missed => (0, "swings and misses"),
            _ when debuffs.HasFlag(BossAbilities.Paralyze) => (0, "is paralyzed"),
            _ => (dmg, $"dealt {dmg} dmg")
        };

    private int GetUserDamage(ParticipatingUser user)
    {
        (int min, int max) = user.User.GetDamageRange();

        if (Buffs.HasFlag(BossAbilities.Dismantle))
        {
            //Include buff?
            int dmgReduction = user.User.Pets.TotalRawTier + (int)user.User.Items.Equipped.GetStatTypeBonus(ItemStatType.BossDmg);
            min -= dmgReduction / 2;
            max -= dmgReduction;
        }

        return Random.Shared.Next(min, max + 1);
    }
}
