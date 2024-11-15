﻿using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using KushBot.DataClasses;
using KushBot.Global;
using KushBot.Resources.Database;
using KushBot.Services;

namespace KushBot.Modules;

public class Balance(SqliteDbContext dbContext, TutorialManager tutorialManager) : ModuleBase<SocketCommandContext>
{
    [Command("Balance"), Alias("Bal", "Baps")]
    [RequirePermissions(Permissions.Core)]
    public async Task PingAsync(IGuildUser _user = null)
    {
        var user = _user ?? (Context.User as IGuildUser);
        var botUser = await dbContext.GetKushBotUserAsync(user.Id);

        if (user.Id == Context.User.Id && await tutorialManager.AttemptSubmitStepCompleteAsync(botUser, 1, 2, Context.Channel))
        {
            await dbContext.SaveChangesAsync();
        }

        string response = botUser.Balance switch
        {
            < 30 => $"{user.Mention} has {botUser.Balance} Baps, fucking homeless {CustomEmojis.Hangg}",
            < 200 => $"{user.Mention} has {botUser.Balance} Baps, what an eyesore {CustomEmojis.InchisStovi}",
            < 500 => $"{user.Mention} has {botUser.Balance} Baps, cancer kike {CustomEmojis.Kitadimensija}",
            _ => $"{user.Mention} has {botUser.Balance} Baps, wtf {CustomEmojis.Kitadimensija}{CustomEmojis.Monkaw}{CustomEmojis.Kitadimensija}"
        };

        await ReplyAsync(response);
    }
}
