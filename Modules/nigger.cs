using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KushBot.Modules
{
    public class Nigger : ModuleBase<SocketCommandContext>
    {
        [Command("moteris")]
        public async Task PingAsync()
        {
            womens.Add("Is a fucking woman");
            womens.Add("i'll turn a case of jack into a case of domestic pretty fucking quick if you don't get cleaning fast");
            womens.Add("smell that? no? exactly, get cooking");
            womens.Add("Driving.");
            womens.Add("money sink");
            womens.Add("probably polish or sth");
            womens.Add("not worth it, become gay for aniki instead");
            womens.Add("femboys are better anyway");
            womens.Add("2D>3D");
            womens.Add("do they even exist?");
            womens.Add("Agota.");
            womens.Add("<:Cheems:945704650378707015>");

            int index = rad.Next(0, womens.Count);

            await ReplyAsync($"😅 {Context.User.Mention} {womens[index]} 📉");

            List<int> QuestIndexes = new List<int>();

            string hold = Data.Data.GetQuestIndexes(Context.User.Id);
            string[] values = hold.Split(',');
            for (int i = 0; i < values.Length; i++)
            {
                QuestIndexes.Add(int.Parse(values[i]));
            }

            if (QuestIndexes.Contains(9))
            {
                await Program.CompleteQuest(9, QuestIndexes, Context.Channel, Context.User);
            }
        }

        Random rad = new Random();

        List<string> womens = new List<string>();
        
        
    
    }
}
