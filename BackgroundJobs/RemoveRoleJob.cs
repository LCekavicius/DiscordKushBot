using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;

namespace KushBot.BackgroundJobs;

[DisallowConcurrentExecution]
public class RemoveRoleJob(DiscordSocketClient client) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var userId = (ulong)context.Trigger.JobDataMap.GetLong("UserId");
        var roleId = (ulong)context.Trigger.JobDataMap.GetLong("RoleId");
        var guildId = (ulong)context.Trigger.JobDataMap.GetLong("GuildId");

        var guild = client.GetGuild(guildId);
        var user = guild.GetUser(userId);

        await user.RemoveRoleAsync(roleId);
    }
}
