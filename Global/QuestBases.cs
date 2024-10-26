using KushBot.DataClasses;
using KushBot.DataClasses.Enums;
using System.Collections.Generic;
using System.Linq;

namespace KushBot.Global;

public enum Prerequisite
{
    JewPet, AnyPet, AnyPetNotMaxLevel
}

public class QuestBase
{
    public required QuestType Type { get; init; }
    public required int BaseBapsReward { get; init; }
    public required string Text { get; init; }
    public UserEventType? MatchCondition { get; init; } = null;
    public List<Prerequisite> Prerequisites { get; init; } = [];
    public required List<UserEventType> RelevantEventTypes { get; init; }
    public required Dictionary<QuestRequirementType, RequirementData> RequirementRewardMap { get; init; }
}

public class RequirementData(int from)
{
    public int From { get; init; } = from;
}

public static class QuestBases
{
    public static Dictionary<QuestType, List<QuestBase>> QuestsBaseDict = new Dictionary<QuestType, List<QuestBase>>()
    {
        { QuestType.Flip, GetFlippingQuests() },
        { QuestType.Bet, GetBettingQuests() },
        { QuestType.Command, GetCommandQuests() },
        { QuestType.Risk, GetRiskingQuests() },
        { QuestType.Slots, GetSlottingQuests() },
        { QuestType.Gamble, GetGamblingQuests() },
    };

    public static List<QuestBase> QuestBaseList = QuestsBaseDict.SelectMany(e => e.Value).ToList();

    public static Dictionary<QuestType, List<QuestBase>> WeeklyQuestsBaseDict = new Dictionary<QuestType, List<QuestBase>>()
    {
        { QuestType.Flip, GetWeeklyFlippingQuests() },
        { QuestType.Bet, GetWeeklyBettingQuests() },
        { QuestType.Risk, GetWeeklyRiskingQuests() },
        { QuestType.Slots, GetWeeklySlottingQuests() },

    };

    public static List<QuestBase> WeeklyQuestBaseList = WeeklyQuestsBaseDict.SelectMany(e => e.Value).ToList();

    private static List<QuestBase> GetGamblingQuests()
    {
        return new List<QuestBase>()
        {
            new QuestBase()
            {
                Type = QuestType.Gamble,
                BaseBapsReward = 120,
                Text = "**Win {0} baps** from **gambling**",
                RelevantEventTypes = [UserEventType.SlotsWin, UserEventType.FlipWin, UserEventType.BetWin, UserEventType.RiskWin],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.Win, new RequirementData(850) },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Gamble,
                BaseBapsReward = 130,
                Text = "**Lose {0} baps** from **gambling**",
                RelevantEventTypes = [UserEventType.SlotsLose, UserEventType.FlipLose, UserEventType.BetLose, UserEventType.RiskLose],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.Lose, new RequirementData(850) },
                }
            }
        };
    }

    private static List<QuestBase> GetSlottingQuests()
    {
        return new List<QuestBase>()
        {
            new QuestBase()
            {
                Type = QuestType.Slots,
                BaseBapsReward = 140,
                Text = "**Win** a **Slot {0} time(s)**",
                RelevantEventTypes = [UserEventType.SlotsWin],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.Count, new RequirementData(1) },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Slots,
                BaseBapsReward = 140,
                Text = "**Lose** a **Slot {0} times**",
                RelevantEventTypes = [UserEventType.SlotsLose],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.Count, new RequirementData(8) },
                }
            }
        };
    }

    private static List<QuestBase> GetRiskingQuests()
    {
        return new List<QuestBase>()
        {
            new QuestBase()
            {
                Type = QuestType.Risk,
                BaseBapsReward = 130,
                Text = "**Win {0} baps** from **risking**",
                RelevantEventTypes = [UserEventType.RiskWin],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.Win, new RequirementData(700) },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Risk,
                BaseBapsReward = 140,
                Text = "**Lose {0} baps** from **risking**",
                RelevantEventTypes = [UserEventType.RiskLose],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.Lose, new RequirementData(700) },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Risk,
                BaseBapsReward = 240,
                Text = "**Risk {0} or more baps** in one risk",
                RelevantEventTypes = [UserEventType.RiskLose, UserEventType.RiskWin],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.BapsX, new RequirementData(400) },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Risk,
                BaseBapsReward = 200,
                Text = "**Risk {0} or more baps** in one risk **{1} times**",
                RelevantEventTypes = [UserEventType.RiskLose, UserEventType.RiskWin],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.BapsX, new RequirementData(225) },
                    { QuestRequirementType.Count, new RequirementData(3) },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Risk,
                BaseBapsReward = 160,
                Text = "**Win** a **Risk** of 25 or more baps with a min modifier of **8**",
                RelevantEventTypes = [UserEventType.RiskWin],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.ModifierX, new RequirementData(8) },
                    { QuestRequirementType.BapsX, new RequirementData(25) },
                }
            }
        };
    }

    private static List<QuestBase> GetCommandQuests()
    {
        return new List<QuestBase>()
        {
            new QuestBase()
            {
                Type = QuestType.Command,
                BaseBapsReward = 135,
                Text = "**Beg {0}** times",
                RelevantEventTypes = [UserEventType.Beg],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.Count, new RequirementData(5) },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Command,
                BaseBapsReward = 75,
                RelevantEventTypes = [UserEventType.Moteris],
                Text = "**?nekenciu**",
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.Count, new RequirementData(1) },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Command,
                BaseBapsReward = 135,
                RelevantEventTypes = [UserEventType.YoinkSuccess],
                Prerequisites = [Prerequisite.JewPet],
                Text = "**Yoink** Succesfully {0} times",
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.Count, new RequirementData(3) },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Command,
                BaseBapsReward = 100,
                RelevantEventTypes = [UserEventType.Feed],
                Prerequisites = [Prerequisite.AnyPetNotMaxLevel, Prerequisite.AnyPet],
                Text = "**Feed** any pet once",
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.Count, new RequirementData(1) },
                }
            },
        };
    }

    private static List<QuestBase> GetBettingQuests()
    {
        return new List<QuestBase>()
        {
            new QuestBase()
            {
                Type = QuestType.Bet,
                BaseBapsReward = 130,
                Text = "**Win {0} baps** from **betting**",
                RelevantEventTypes = [UserEventType.BetWin],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.Win, new RequirementData(700) },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Bet,
                BaseBapsReward = 140,
                Text = "**Lose {0} baps** from **betting**",
                RelevantEventTypes = [UserEventType.BetLose],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.Lose, new RequirementData(700) },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Bet,
                BaseBapsReward = 240,
                Text = "**Bet {0} or more baps** in one bet",
                RelevantEventTypes = [UserEventType.BetLose, UserEventType.BetWin],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.BapsX, new RequirementData(850) },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Bet,
                BaseBapsReward = 200,
                Text = "**Bet {0} or more baps** in one bet **{1} times**",
                RelevantEventTypes = [UserEventType.BetLose, UserEventType.BetWin],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.BapsX, new RequirementData(450) },
                    { QuestRequirementType.Count, new RequirementData(3) },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Bet,
                BaseBapsReward = 220,
                Text = "**Get >= {0} bet modifier** when betting **>= {1}** baps",
                RelevantEventTypes = [UserEventType.BetWin],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.ModifierX, new RequirementData(3) },
                    { QuestRequirementType.BapsX, new RequirementData(75) },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Bet,
                BaseBapsReward = 200,
                Text = "**Get >= {0} bet modifier** when betting **>= {1}** baps **{2} times**",
                RelevantEventTypes = [UserEventType.BetWin],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.ModifierX, new RequirementData(2) },
                    { QuestRequirementType.BapsX, new RequirementData(75) },
                    { QuestRequirementType.Count, new RequirementData(3) },
                }
            }
        };
    }

    private static List<QuestBase> GetFlippingQuests()
    {
        return new List<QuestBase>()
        {
            new QuestBase()
            {
                Type = QuestType.Flip,
                BaseBapsReward = 130,
                Text = "**Win {0} baps** from **flipping**",
                RelevantEventTypes = [UserEventType.FlipWin],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.Win, new RequirementData(700) },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Flip,
                BaseBapsReward = 140,
                Text = "**Lose {0} baps** from **flipping**",
                RelevantEventTypes = [UserEventType.FlipLose],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.Lose, new RequirementData(700) },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Flip,
                BaseBapsReward = 240,
                Text = "**Flip {0} or more baps** in one flip",
                RelevantEventTypes = [UserEventType.FlipLose, UserEventType.FlipWin],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.BapsX, new RequirementData(750) },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Flip,
                BaseBapsReward = 240,
                Text = "**Flip {0} or more baps** in one flip **{1} times**",
                RelevantEventTypes = [UserEventType.FlipLose, UserEventType.FlipWin],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.BapsX, new RequirementData(375) },
                    { QuestRequirementType.Count, new RequirementData(3) },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Flip,
                BaseBapsReward = 200,
                Text = "**Flip {0} or more baps** and win **{1} times** in a row",
                MatchCondition = UserEventType.FlipWin,
                RelevantEventTypes = [UserEventType.FlipLose, UserEventType.FlipWin],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.BapsX, new RequirementData(60) },
                    { QuestRequirementType.Chain, new RequirementData(3) },
                }
            },
        };
    }

    private static List<QuestBase> GetWeeklyFlippingQuests()
    {
        return new List<QuestBase>()
        {
            new QuestBase()
            {
                Type = QuestType.Flip,
                BaseBapsReward = 910,
                Text = "**Win {0} baps** from **flipping**",
                RelevantEventTypes = [UserEventType.FlipWin],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.Win, new RequirementData(12000) },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Flip,
                BaseBapsReward = 900,
                Text = "**Lose {0} baps** from **flipping**",
                RelevantEventTypes = [UserEventType.FlipLose],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.Lose, new RequirementData(12000) },
                }
            }
        };
    }

    private static List<QuestBase> GetWeeklyBettingQuests()
    {
        return new List<QuestBase>()
        {
            new QuestBase()
            {
                Type = QuestType.Bet,
                BaseBapsReward = 900,
                Text = "**Win {0} baps** from **betting**",
                RelevantEventTypes = [UserEventType.BetWin],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.Win, new RequirementData(12000) },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Bet,
                BaseBapsReward = 910,
                Text = "**Lose {0} baps** from **betting**",
                RelevantEventTypes = [UserEventType.BetLose],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.Lose, new RequirementData(12000) },
                }
            }
        };
    }

    private static List<QuestBase> GetWeeklyRiskingQuests()
    {
        return new List<QuestBase>()
        {
            new QuestBase()
            {
                Type = QuestType.Risk,
                BaseBapsReward = 900,
                Text = "**Win {0} baps** from **risking**",
                RelevantEventTypes = [UserEventType.RiskWin],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.Win, new RequirementData(12000) },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Risk,
                BaseBapsReward = 910,
                Text = "**Lose {0} baps** from **risking**",
                RelevantEventTypes = [UserEventType.RiskLose],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.Lose, new RequirementData(12000) },
                }
            }
        };
    }

    private static List<QuestBase> GetWeeklySlottingQuests()
    {
        return new List<QuestBase>()
        {
            new QuestBase()
            {
                Type = QuestType.Slots,
                BaseBapsReward = 900,
                Text = "**Win {0} baps** from **slotting**",
                RelevantEventTypes = [UserEventType.SlotsWin],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.Win, new RequirementData(12000) },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Slots,
                BaseBapsReward = 910,
                Text = "**Lose {0} baps** from **slotting**",
                RelevantEventTypes = [UserEventType.SlotsLose],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.Lose, new RequirementData(12000) },
                }
            }
        };
    }
}
