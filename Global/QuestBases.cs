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
    public required Dictionary<QuestRequirementType, int> RequirementRewardMap { get; init; }

}

public static class QuestBases
{
    public static Dictionary<QuestType, List<QuestBase>> QuestsBaseDict = new Dictionary<QuestType, List<QuestBase>>()
    {
        { QuestType.Flip, GetFlippingQuests() }
    };

    public static List<QuestBase> QuestBaseList = QuestsBaseDict.SelectMany(e => e.Value).ToList();

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
                RequirementRewardMap = new Dictionary<QuestRequirementType, int>
                {
                    { QuestRequirementType.Win, 700 },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Flip,
                BaseBapsReward = 275,
                RelevantEventTypes = [UserEventType.FlipWin],
                Text = "**Win {0} baps** from **flipping**",
                RequirementRewardMap = new Dictionary<QuestRequirementType, int>
                {
                    { QuestRequirementType.Win, 1500 },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Flip,
                BaseBapsReward = 140,
                Text = "**Lose {0} baps** from **flipping**",
                RelevantEventTypes = [UserEventType.FlipLose],
                RequirementRewardMap = new Dictionary<QuestRequirementType, int>
                {
                    { QuestRequirementType.Lose, 700 },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Flip,
                BaseBapsReward = 240,
                Text = "**Flip {0} or more baps** in one flip",
                RelevantEventTypes = [UserEventType.FlipLose, UserEventType.FlipWin],
                RequirementRewardMap = new Dictionary<QuestRequirementType, int>
                {
                    { QuestRequirementType.BapsX, 750 },
                }
            },
            new QuestBase()
            {
                Type = QuestType.Flip,
                BaseBapsReward = 200,
                Text = "**Flip {0} or more baps** and win {1} times in a row",
                MatchCondition = UserEventType.FlipWin,
                RelevantEventTypes = [UserEventType.FlipLose, UserEventType.FlipWin],
                RequirementRewardMap = new Dictionary<QuestRequirementType, int>
                {
                    { QuestRequirementType.Win, 60 },
                    { QuestRequirementType.Chain, 3 },
                }
            },
        };
    }
}
