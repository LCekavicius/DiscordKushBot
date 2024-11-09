using KushBot.DataClasses.Enums;
using KushBot.DataClasses;
using KushBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KushBot.Global;

public static class QuestHelper
{
    public static IEnumerable<Quest> CreateQuestEntities(KushBotUser user)
    {
        QuestRequirementFactory factory = new();

        var additionalCount = (user.Pets?[PetType.Maybich]?.Tier ?? 0) * 25 + user?.Items?.GetStatTypeBonus(ItemStatType.QuestSlotChance) ?? 0;
        int count = 3 + (int)(additionalCount / 100) + (Random.Shared.Next(1, 101) < (additionalCount % 100) ? 1 : 0);

        var permittedQuests = QuestBases.QuestBaseList;

        if ((user.Pets?.All(e => e.Value.Level == 99)) ?? false)
        {
            permittedQuests = permittedQuests.Where(e => !e.Prerequisites.Contains(Prerequisite.AnyPetNotMaxLevel)).ToList();
        }

        if (!user.Pets?.Any() ?? true)
        {
            permittedQuests = permittedQuests.Where(e => !e.Prerequisites.Contains(Prerequisite.AnyPet)).ToList();
        }

        if (user.Pets?[PetType.Jew] == null)
        {
            permittedQuests = permittedQuests.Where(e => !e.Prerequisites.Contains(Prerequisite.JewPet)).ToList();
        }

        var selectedQuests = permittedQuests.OrderBy(e => Random.Shared.NextDouble()).Take(count).ToList();

        foreach (var questBase in selectedQuests)
        {
            yield return new Quest
            {
                Type = questBase.Type,
                UserId = user.Id,
                IsCompleted = false,
                IsDaily = true,
                QuestBaseIndex = QuestBases.QuestBaseList.IndexOf(questBase),
                Requirements = questBase.RequirementRewardMap
                    .Select(e => factory.Create(e.Key, GetQuestRequirementValue(user, e.Value.From, e.Key).ToString()))
                    .ToList()
            };
        }
    }

    public static IEnumerable<Quest> CreateWeeklyQuestEntities(KushBotUser user)
    {
        QuestRequirementFactory factory = new();

        int count = 2;

        var selectedQuests = QuestBases.WeeklyQuestBaseList.OrderBy(e => Random.Shared.NextDouble()).Take(count).ToList();

        foreach (var questBase in selectedQuests)
        {
            yield return new Quest
            {
                Type = questBase.Type,
                UserId = user.Id,
                IsCompleted = false,
                IsDaily = false,
                QuestBaseIndex = QuestBases.WeeklyQuestBaseList.IndexOf(questBase),
                Requirements = questBase.RequirementRewardMap
                    .Select(e => factory.Create(e.Key, GetQuestRequirementValue(user, e.Value.From, e.Key).ToString()))
                    .ToList()
            };
        }
    }

    private static int GetQuestRequirementValue(KushBotUser user, int requiredValue, QuestRequirementType type)
    {
        if (type == QuestRequirementType.Chain)
        {
            return requiredValue;
        }

        var TPL = user.Pets?.Sum(e => e.Value.CombinedLevel) ?? 0;

        return requiredValue + ((int)(4 * Math.Pow(TPL, 1.08) * ((double)requiredValue / 1400)));

        //if (Desc.Contains("Reach"))
        //{
        //    int reachRet = (int)(13 * Math.Pow(petlvl, 1.15));
        //    return reachRet + CompleteReq;
        //}
        //if (Desc.Contains("Beg") || Desc.Contains("Yoink") || Desc.Contains("begging"))
        //{
        //    return CompleteReq;
        //}

        //if (Desc.Contains("**Flip 60"))
        //{
        //    return 3;
        //}

        //if (Desc.Contains("Duel"))
        //{
        //    int temp = (int)(4 * Math.Pow(petlvl, 1.08) * ((double)CompleteReq / 1400));
        //    return temp + CompleteReq;
        //}
    }
}
