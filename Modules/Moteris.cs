using Discord.Commands;
using KushBot.DataClasses.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.Modules;

public class Moteris : ModuleBase<SocketCommandContext>
{
    [Command("moteris")]
    public async Task PingAsync()
    {
        var user = Data.Data.GetKushBotUser(Context.User.Id, Data.UserDtoFeatures.Quests);

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
        womens.Add("<:Cheems:945704650378707015>");

        if(!user.UserEvents.Any(e => e.Type == UserEventType.Moteris))
        {
            Data.Data.AddUserEvent(user, UserEventType.Moteris);
        }

        var (completed, completedLast) = Data.Data.AttemptCompleteQuests(user);
        await Context.CompleteQuestsAsync(completed, completedLast);

        int index = Random.Shared.Next(0, womens.Count);

        await ReplyAsync($"😅 {Context.User.Mention} {womens[index]} 📉");
        if (completed.Any())
        {
            await Data.Data.SaveKushBotUserAsync(user);
        }
    }

    List<string> womens = new List<string>();
}
