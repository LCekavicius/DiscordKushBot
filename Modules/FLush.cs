using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KushBot.Data;
using System.Linq;

namespace KushBot.Modules
{
    public class FLush : ModuleBase<SocketCommandContext>
    {
        [Command("Flush")]
        public async Task PingAsync()
        {
            Program.IgnoredUsers.Clear();
        }

        [Command("rngTest")]
        public async Task rtnfdfas()
        {
            Random rnd = new Random();

            List<int> ints = new List<int>();
            ints.Add(0);
            ints.Add(1);
            ints.Add(2);
            ints.Add(3);

            List<int> picks = ints.OrderBy(x => rnd.Next()).Take(3).ToList();
            string temp = string.Join(',', picks);
            Console.WriteLine(temp);
        }
    }
}
