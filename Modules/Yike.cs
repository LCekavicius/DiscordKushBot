using Discord.Commands;
using Discord;
using System.Threading.Tasks;
using System;
using KushBot.Resources.Database;
using KushBot.Global;

namespace KushBot.Modules;

public class Yike : ModuleBase<SocketCommandContext>
{
    private readonly SqliteDbContext _context;

    public Yike(SqliteDbContext context)
    {
        _context = context;
    }

    [Command("Yike")]
    public async Task yikes(IGuildUser user)
    {
        var botUser = await _context.GetKushBotUserAsync(Context.User.Id);

        if (botUser.YikeDate.AddHours(2) > DateTime.Now)
        {
            TimeSpan ts = botUser.YikeDate.AddHours(2) - DateTime.Now;
            await ReplyAsync($"{Context.User.Mention} your yike is on cooldown, time left: {ts.Hours}:{ts.Minutes}:{ts.Seconds}");
            return;
        }

        var target = await _context.GetKushBotUserAsync(user.Id);
        target.Yiked += 1;
        botUser.YikeDate = TimeHelper.Now;

        await _context.SaveChangesAsync();

        await ReplyAsync($"{user.Mention} you were yiked by {Context.User.Mention} {CustomEmojis.Omega}");
    }
}
