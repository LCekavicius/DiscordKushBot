using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KushBot.Data;

namespace KushBot.Modules
{
    public class Drop : ModuleBase<SocketCommandContext>
    {
        [Command("drop")]
        public async Task PingAsync(int n, IUser user)
        {
            if (Context.User.Id != 192642414215692300 && Context.User.Id != 187483265865613312)
                return;

            await Data.Data.SaveBalance(user.Id, n, false);
        }
    }
}
