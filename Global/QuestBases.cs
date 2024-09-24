using KushBot.DataClasses;
using KushBot.DataClasses.Enums;
using System.Collections.Generic;
using System.Linq;

namespace KushBot.Global;

public class QuestBase
{
    public required QuestType Type { get; init; }
    public required int BaseBapsReward { get; init; }
    public required string Text { get; init; }
    public UserEventType? MatchCondition { get; init; } = null;
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
    };

    public static List<QuestBase> QuestBaseList = QuestsBaseDict.SelectMany(e => e.Value).ToList();

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
                BaseBapsReward = 240,
                Text = "**Get >= {0} bet modifier** when betting >= **{1}** baps",
                RelevantEventTypes = [UserEventType.BetWin],
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.ModifierX, new RequirementData(3) },
                    { QuestRequirementType.BapsX, new RequirementData(75) },
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
                BaseBapsReward = 275,
                RelevantEventTypes = [UserEventType.FlipWin],
                Text = "**Win {0} baps** from **flipping**",
                RequirementRewardMap = new Dictionary<QuestRequirementType, RequirementData>
                {
                    { QuestRequirementType.Win, new RequirementData(1500) },
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
}
