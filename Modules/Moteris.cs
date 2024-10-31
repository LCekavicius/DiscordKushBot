using Discord.Commands;
using KushBot.DataClasses;
using KushBot.DataClasses.Enums;
using KushBot.Global;
using KushBot.Resources.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.Modules;

[RequirePermissions(Permissions.Nya, true)]
public class Moteris(SqliteDbContext dbContext) : ModuleBase<SocketCommandContext>
{
    [Command("moteris")]
    public async Task PingAsync()
    {
        var user = await dbContext.GetKushBotUserAsync(Context.User.Id, Data.UserDtoFeatures.Quests);

        womens.Add("Is a fucking woman");
        womens.Add("i'll turn a case of jack into a case of domestic pretty fucking quick if you don't get cleaning fast");
        womens.Add("smell that? no? exactly, get cooking");
        womens.Add("Driving.");
        womens.Add("money sink");
        womens.Add("probably polish or sth");
        womens.Add("not worth it, become gay for aniki instead");
        womens.Add("femboys are better anyway");
        womens.Add("kush nya");
        womens.Add("Agota.");
        womens.Add(CustomEmojis.Cheems);

        if(!user.UserEvents.Any(e => e.Type == UserEventType.Moteris))
        {
            user.AddUserEvent(UserEventType.Moteris);
        }

        var result = user.AttemptCompleteQuests();
        await Context.CompleteQuestsAsync(result.freshCompleted, result.lastDailyCompleted, false);

        int index = Random.Shared.Next(0, womens.Count);

        await ReplyAsync($"😅 {Context.User.Mention} {womens[index]} 📉");
        if (result.freshCompleted.Any())
        {
            await dbContext.SaveChangesAsync();
        }
    }

    List<string> womens = new List<string>();
}
