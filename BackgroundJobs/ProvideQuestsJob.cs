using KushBot.DataClasses;
using KushBot.Global;
using KushBot.Resources.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.BackgroundJobs;

[DisallowConcurrentExecution]
public class ProvideQuestsJob : IJob
{
    private readonly SqliteDbContext _dbContext;
    private readonly ILogger<ProvideQuestsJob> _logger;
    public ProvideQuestsJob(SqliteDbContext dbContext, ILogger<ProvideQuestsJob> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation($"{DateTime.Now} Providing quests");

        var users = _dbContext.Jews
            .Include(e => e.Items.Where(e => e.IsEquipped))
            .ToList();

        var pets = _dbContext.UserPets.ToList();

        await _dbContext.Quests.ExecuteDeleteAsync();

        foreach (var user in users)
        {
            user.Pets = new UserPets(pets.Where(e => e.UserId == user.Id).ToList());
            var quests = CreateQuestEntities(user).ToList();
            _dbContext.AddRange(quests);
        }

        await _dbContext.SaveChangesAsync();
    }

    private IEnumerable<Quest> CreateQuestEntities(KushBotUser user)
    {
        var additionalCount = (user.Pets[PetType.Maybich]?.Tier ?? 0) * 25 + user?.Items?.QuestSlotSum;
        int count = 3 + (int)(additionalCount / 100) + (Random.Shared.Next(1, 101) < (additionalCount % 100) ? 1 : 0);

        //TODO prevent certain quests based on conditions (no PP quest if no jew pet, No Reach quest if already past the bar etc.)
        var selectedQuests = QuestBases.QuestBaseList.OrderBy(e => Random.Shared.NextDouble()).Take(count);

        foreach (var questBase in selectedQuests)
        {
            yield return new Quest
            {
                Type = questBase.Type,
                UserId = user.Id,
                IsCompleted = false,
                IsDaily = true,
                Requirements = questBase.RequirementRewardMap.Select(e => new QuestRequirement
                {
                    Type = e.Key,
                    Value = GetRequiredValue(user, e.Value.From, e.Key).ToString(),
                }).ToList(),
            };
        }
    }

    private int GetRequiredValue(KushBotUser user, int requiredValue, QuestRequirementType type)
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
