using Discord.WebSocket;
using KushBot.Resources.Database;
using KushBot.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Threading.Tasks;

namespace KushBot.BackgroundJobs;

public class BossFightJob(
    DiscordSocketClient client,
    BossService bossService,
    SqliteDbContext dbContext) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var bossId = context.Trigger.JobDataMap.GetInt("Id");

        var boss = await dbContext.Bosses
            .Include(e => e.Participants)
            .FirstOrDefaultAsync(e => e.Id == bossId);

        await bossService.HandleBossFightAsync(boss);
    }
}
