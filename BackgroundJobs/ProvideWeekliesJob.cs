using KushBot.DataClasses;
using KushBot.Global;
using KushBot.Resources.Database;
using KushBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.BackgroundJobs;

[DisallowConcurrentExecution]
public class ProvideWeekliesJob(SqliteDbContext dbContext, ILogger<ProvideWeekliesJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation($"{DateTime.Now} Providing weekly quests");

        var users = await dbContext.Users
            .Include(e => e.Items.Where(e => e.IsEquipped))
            .ToListAsync();

        var pets = await dbContext.UserPets.ToListAsync();

        await dbContext.Quests.Where(e => !e.IsDaily).ExecuteDeleteAsync();

        foreach (var user in users)
        {
            user.Pets = new UserPets(pets.Where(e => e.UserId == user.Id).ToList());
            var quests = QuestHelper.CreateWeeklyQuestEntities(user);
            dbContext.AddRange(quests);
        }

        await dbContext.SaveChangesAsync();
    }
}
