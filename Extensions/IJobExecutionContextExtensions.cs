using Quartz;

namespace KushBot.Extensions;

public static class IJobExecutionContextExtensions
{
    public static ulong? GetGuildId(this IJobExecutionContext context)
    {
        ulong.TryParse(context.JobDetail.JobDataMap.GetString("GuildId"), out var id);
        return id;
    }
}
