using Discord.Commands;
using Discord;
using System.Threading.Tasks;
using System;
using KushBot.Resources.Database;
using KushBot.Global;
using KushBot.DataClasses;

namespace KushBot.Modules;

[RequirePermissions(Permissions.Core)]
public class Yike(SqliteDbContext dbContext) : ModuleBase<SocketCommandContext>
{
    [Command("Yike")]
    public async Task yikes(IGuildUser user)
    {
        var botUser = await dbContext.GetKushBotUserAsync(Context.User.Id);

        if (botUser.YikeDate.AddHours(2) > DateTime.Now)
        {
            TimeSpan ts = botUser.YikeDate.AddHours(2) - DateTime.Now;
            await ReplyAsync($"{Context.User.Mention} your yike is on cooldown, time left: {ts.Hours}:{ts.Minutes}:{ts.Seconds}");
            return;
        }

        var target = await dbContext.GetKushBotUserAsync(user.Id);
        target.Yiked += 1;
        botUser.YikeDate = TimeHelper.Now;

        await dbContext.SaveChangesAsync();

        await ReplyAsync($"{user.Mention} you were yiked by {Context.User.Mention} {CustomEmojis.Omega}");
    }
}
