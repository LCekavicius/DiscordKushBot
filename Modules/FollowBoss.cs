using Discord.Commands;
using System.Threading.Tasks;

namespace KushBot.Modules;

public class FollowBoss : ModuleBase<SocketCommandContext>
{
    [Command("Follow")]
    public async Task followBoss(string rarity)
    {
        string temp = "";
        try
        {
            temp = char.ToUpper(rarity[0]) + rarity.ToLower().Substring(1);
        }
        catch
        {
            await ReplyAsync($"{Context.User.Mention} XD?");
            return;
        }

        if(!temp.Equals("Common") && !temp.Equals("Uncommon") && !temp.Equals("Rare") && !temp.Equals("Epic") && !temp.Equals("Legendary") && !temp.Equals("Archon"))
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

        if (!temp.Equals("Common") && !temp.Equals("Uncommon") && !temp.Equals("Rare") && !temp.Equals("Epic") && !temp.Equals("Legendary") && !temp.Equals("Archon"))
        {
            await ReplyAsync($"{Context.User.Mention} XD?");
            return;
        }

        await Data.Data.RemoveFollowRarity(Context.User.Id, temp);
        await ReplyAsync($"{Context.User.Mention} you unfollowed {temp} boss rarity");
    }
}
