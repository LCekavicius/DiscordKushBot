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
    public int QuestBaseIndex { get; init; }
    [NotMapped] public bool ProvidesEgg { get; set; }

    public string GetQuestText()
    {
        var values = Requirements.Select(e => int.TryParse(e.Value, out var value) ? value : 0).ToList();
        var questBase = GetMatchingQuestBase();
        var placeholder = questBase.Text;

        return Regex.Replace(placeholder, @"\{(\d+)\}", match =>
        {
            int index = int.Parse(match.Groups[1].Value);
            var baseReq = questBase.RequirementRewardMap.ToArray()[index];
            return Requirements.FirstOrDefault(e => e.Type == baseReq.Key).Value;
        });
    }

    public int GetQuestReward()
    {
        int petLvl = User.Pets[PetType.Maybich]?.CombinedLevel ?? 0;
        int bapsFromPet = (int)Math.Round(Math.Pow(petLvl, 1.3) + petLvl * 3);

        int baps = GetMatchingQuestBase().BaseBapsReward;
        baps += (int)User.Items.Equipped.GetStatTypeBonus(ItemStatType.QuestBapsFlat);
        baps += bapsFromPet;
        baps = (int)((double)baps * (1 + (User.Items.Equipped.GetStatTypeBonus(ItemStatType.QuestBapsPercent) / 100)));

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

    private QuestBase GetMatchingQuestBase() => IsDaily ? QuestBases.QuestBaseList[QuestBaseIndex] : QuestBases.WeeklyQuestBaseList[QuestBaseIndex];
}
