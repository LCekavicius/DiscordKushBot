using KushBot.DataClasses;
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

        var users = _dbContext.Users
            .Include(e => e.Items.Where(e => e.IsEquipped))
            .ToList();

        var pets = _dbContext.UserPets.ToList();

        await _dbContext.Quests.Where(e => e.IsDaily).ExecuteDeleteAsync();

        foreach (var user in users)
        {
            user.Pets = new UserPets(pets.Where(e => e.UserId == user.Id).ToList());
            var quests = Data.Data.CreateQuestEntities(user);
            _dbContext.AddRange(quests);
        }

        await _dbContext.SaveChangesAsync();
    }
}
