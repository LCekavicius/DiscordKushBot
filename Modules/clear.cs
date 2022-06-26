using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KushBot.Modules
{
    public class clearpp : ModuleBase<SocketCommandContext>
    {
        [Command("clear")]
        public async Task cl()
        {
            Program.Fail = 0;
            Program.Test = 0;
            Program.NerfUser = 0;
            Program.PetTest = 0;
        }
    }
}
