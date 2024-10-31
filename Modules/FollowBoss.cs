using Discord.Commands;
using KushBot.DataClasses;
using KushBot.DataClasses.enums;
using KushBot.Resources.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.Modules;

public class FollowBoss(SqliteDbContext dbContext) : ModuleBase<SocketCommandContext>
{
    [Command("Follow")]
    public async Task followBoss(string rarity)
    {
        if (!Enum.TryParse(typeof(RarityType), rarity.ToLower(), true, out var parsed) || rarity.ToLower() == "none")
        {
            await ReplyAsync($"{Context.User.Mention} XD?");
            return;
        }

        var casted = (RarityType)parsed;
        await dbContext.RarityFollow.AddAsync(new RarityFollow { UserId = Context.User.Id, Rarity = casted });
        await dbContext.SaveChangesAsync();
        await ReplyAsync($"{Context.User.Mention} you are now following {casted} boss rarity");
    }

    [Command("Unfollow")]
    public async Task unfollowBoss(string rarity)
    {
        if (!Enum.TryParse(typeof(RarityType), rarity.ToLower(), true, out var parsed) || rarity.ToLower() == "none")
        {
            await ReplyAsync($"{Context.User.Mention} XD?");
            return;
        }

        var casted = (RarityType)parsed;
        await dbContext.RarityFollow.AddAsync(new RarityFollow { UserId = Context.User.Id, Rarity = casted });
        await dbContext.SaveChangesAsync();

        await dbContext.RarityFollow.Where(e => e.UserId == Context.User.Id && e.Rarity == casted).ExecuteDeleteAsync();
        await ReplyAsync($"{Context.User.Mention} you unfollowed {casted} boss rarity");
    }
}
