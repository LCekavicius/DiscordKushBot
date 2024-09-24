using KushBot.DataClasses.Enums;
using KushBot.Global;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;

namespace KushBot.DataClasses;

public class Quest
{
    [Key]
    public int? Id { get; init; }
    public QuestType Type { get; init; }
    public bool IsCompleted { get; set; }
    public bool IsDaily { get; init; }
    public ulong UserId { get; init; }
    public KushBotUser User { get; init; }
    public List<QuestRequirement> Requirements { get; init; } = new();

    public string GetQuestText()
    {
        var values = Requirements.Select(e => int.TryParse(e.Value, out var value) ? value : 0).ToList();
        var placeholder = GetMatchingQuestBase().Text;

        return Regex.Replace(placeholder, @"\{(\d+)\}", match =>
        {
            int index = int.Parse(match.Groups[1].Value);
            return index >= 0 && index < values.Count ? values[index].ToString() : match.Value;
        });
    }

    public int GetQuestReward(KushBotUser user)
    {
        int petLvl = user.Pets[PetType.Maybich]?.CombinedLevel ?? 0;
        int bapsFromPet = (int)Math.Round(Math.Pow(petLvl, 1.3) + petLvl * 3);

        int baps = GetMatchingQuestBase().BaseBapsReward;
        baps += (int)user.Items.Equipped.QuestBapsFlatSum;
        baps += bapsFromPet;
        baps = (int)((double)baps * (1 + user.Items.Equipped.QuestBapsPercentSum));

        return baps;
    }

    public List<UserEventType> GetRelevantEventTypes()
    {
        return GetMatchingQuestBase().RelevantEventTypes;
    }

    public UserEventType? GetMatchingEventType()
    {
        return GetMatchingQuestBase().MatchCondition;
    }

    private QuestBase GetMatchingQuestBase()
    {
        var types = Requirements.Select(e => e.Type);
        return QuestBases.QuestsBaseDict[Type].FirstOrDefault(quest => types.All(type => quest.RequirementRewardMap.Keys.Contains(type)));
    }
}
