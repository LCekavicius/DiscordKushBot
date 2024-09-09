using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using KushBot.Resources.Database;
using System.Linq;
using KushBot.DataClasses;

namespace KushBot.Modules
{
    public class Leaderboard : ModuleBase<SocketCommandContext>
    {
        public static int GetMinimumDmg(SUser jew, bool isMinimum = true)
        {
            List<int> levels = jew.PetLevels.Split(',').Select(int.Parse).ToList();
            List<int> dupes = jew.PetDupes.Split(',').Select(int.Parse).ToList();
            List<Item> items = Data.Data.GetUserItems(jew.Id);

            for (int i = 0; i < levels.Count; i++)
            {
                levels[i] += Data.Data.GetPetTier(dupes[i]);
                if(levels[i] != 0)
                    levels[i] += Data.Data.GetItemPetLevel(jew.Id, i);
            }

            int itemdmg = 0;
            foreach (var item in items)
            {
                if(item.Id == jew.FirstItemId || item.Id == jew.SecondItemId || item.Id == jew.ThirdItemId || item.Id == jew.FourthItemId)
                {
                    itemdmg += item.BossDmg;
                }
            }

            if(isMinimum)
            {
                try
                {
                    return 2 * levels.Where(x => x != 0).Min() + itemdmg;
                } catch { return 0; }
            }
            else
            {
                try
                {
                    return 2 * levels.Where(x => x != 0).Max() + itemdmg;
                }
                catch { return 0; }
            }
        }

        [Command("dmg top")]
        public async Task ShowPetTop(int input = 1)
        {
            List<SUser> Jews = new List<SUser>();

            using (var DbContext = new SqliteDbContext())
            {
                Jews = DbContext.Jews.ToList();
            }

            List<SUser> sorted = Jews.OrderByDescending(x => GetMinimumDmg(x)).ThenByDescending(x => GetMinimumDmg(x, false)).Skip((input-1) * 10).Take(10).ToList();

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
            List<SUser> Jews = new List<SUser>();

            using (var DbContext = new SqliteDbContext())
            {
                foreach(var jew in DbContext.Jews)
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
                        var user = Program._client.GetUser(Jews[i].Id);
                        print += $"{i + 1}. {user.Username}  {Jews[i].Balance} Baps\n";
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
}
