using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using KushBot.Resources.Database;
using System.Linq;
using KushBot.DataClasses.Enums;
using KushBot.DataClasses;

namespace KushBot.Modules;

[RequirePermissions(Permissions.Core)]
public class Leaderboard(SqliteDbContext dbContext) : ModuleBase<SocketCommandContext>
{
    public static int GetMinimumDmg(KushBotUser user, bool isMinimum = true)
    {
        return 0;
        //var pets = Data.Data.GetUserPets(user.Id);
        //var items = Data.Data.GetUserItems(user.Id);

        //int itemdmg = (int)items.Equipped.GetStatTypeBonus(ItemStatType.BossDmg);

        //if(isMinimum)
        //{
        //    return 2 * pets.Select(e => e.Value.CombinedLevel).Min() + itemdmg;
        //}
        //else
        //{
        //    return 2 * pets.Select(e => e.Value.CombinedLevel).Max() + itemdmg;
        //}
    }

    [Command("dmg top")]
    public async Task ShowPetTop(int input = 1)
    {
        List<KushBotUser> users = new List<KushBotUser>();

        using (var DbContext = new SqliteDbContext())
        {
            users = DbContext.Users.ToList();
        }

        List<KushBotUser> sorted = users.OrderByDescending(x => GetMinimumDmg(x)).ThenByDescending(x => GetMinimumDmg(x, false)).Skip((input-1) * 10).Take(10).ToList();

        string print = "";
        int i = 1;
        foreach (var item in sorted)
        {
            try
            {
                print += $"{i}. {Context.Guild.GetUser(item.Id).Username,20} Dmg: **{GetMinimumDmg(item)}**-**{GetMinimumDmg(item,false)}**," +
                    $" Tickets: **{item.Tickets}**\n";
            }
            catch { }
            i++;
        }

        EmbedBuilder builder = new EmbedBuilder();
        builder.WithTitle("LeaderBoards");
        builder.WithColor(Color.Magenta);
        builder.AddField("Jews", $"{print}");
        builder.WithFooter("Type 'kush dmg top n' to view other pages. (e.g. kush dmg top 2)");

        await ReplyAsync("", false, builder.Build());
    }

    [Command("top")]
    public async Task ShowTop()
    {
        List<KushBotUser> Jews = new List<KushBotUser>();

        using (var DbContext = new SqliteDbContext())
        {
            foreach(var jew in DbContext.Users)
            {
                Jews.Add(jew);
            }
        }

        Jews = Jews.OrderByDescending(x => x.Balance).ToList();

        EmbedBuilder builder = new EmbedBuilder();
        string print = "";

        for(int i = 0; i < Jews.Count; i++)
        {
            try
            {
                var user = Context.Guild.GetUser(Jews[i].Id);
                print += $"{i + 1}. {user.Username}  {Jews[i].Balance} Baps\n";
            }
            catch
            {
                try
                {
                    //var user = DiscordBotService._client.GetUser(Jews[i].Id);
                    //print += $"{i + 1}. {user.Username}  {Jews[i].Balance} Baps\n";
                }
                catch
                {

                }                 
            }

            if (i == 9)
                break;
        }


        builder.WithTitle("LeaderBoards");
        builder.WithColor(Color.Magenta);
        builder.AddField("Jews",$"{print}");

       await ReplyAsync("",false,builder.Build());
        

    }

}
