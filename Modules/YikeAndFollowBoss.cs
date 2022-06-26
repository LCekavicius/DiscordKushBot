using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KushBot.Modules
{
    public class Yike : ModuleBase<SocketCommandContext>
    {
        [Command("Follow")]
        public async Task followBoss(string rarity)
        {
            string temp = "";
            try
            {
                temp = char.ToUpper(rarity[0]) + rarity.Substring(1);
            }
            catch
            {
                await ReplyAsync($"{Context.User.Mention} XD?");
                return;
            }

            if(!temp.Equals("Common") && !temp.Equals("Uncommon") && !temp.Equals("Rare") && !temp.Equals("Epic") && !temp.Equals("Legendary"))
            {
                await ReplyAsync($"{Context.User.Mention} XD?");
                return;
            }

            await Data.Data.AddFollowRarity(Context.User.Id, temp);
            await ReplyAsync($"{Context.User.Mention} you are now following {temp} boss rarity");
        }

        [Command("Unfollow")]
        public async Task unfollowBoss(string rarity)
        {
            string temp = "";
            try
            {
                temp = char.ToUpper(rarity[0]) + rarity.Substring(1);
            }
            catch
            {
                await ReplyAsync($"{Context.User.Mention} XD?");
                return;
            }

            if (!temp.Equals("Common") && !temp.Equals("Uncommon") && !temp.Equals("Rare") && !temp.Equals("Epic") && !temp.Equals("Legendary"))
            {
                await ReplyAsync($"{Context.User.Mention} XD?");
                return;
            }

            await Data.Data.RemoveFollowRarity(Context.User.Id, temp);
            await ReplyAsync($"{Context.User.Mention} you unfollowed {temp} boss rarity");
        }

        [Command("Yike")]
        public async Task yikes(IGuildUser user)
        {
            if(Data.Data.GetYikeDate(Context.User.Id).AddHours(2) > DateTime.Now)
            {
                TimeSpan ts = Data.Data.GetYikeDate(Context.User.Id).AddHours(2) - DateTime.Now;
                await ReplyAsync($"{Context.User.Mention} your yike is on cooldown, time left: {ts.Hours}:{ts.Minutes}:{ts.Seconds}");
                return;
            }

            await Data.Data.AddYike(user.Id);
            await Data.Data.SaveYikeDate(Context.User.Id);

            await ReplyAsync($"{user.Mention} you were yiked by {Context.User.Mention} <:Omega:945781765899952199>");
        }

    }
}
