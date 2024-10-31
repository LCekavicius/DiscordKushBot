using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using KushBot.DataClasses;
using KushBot.Global;

namespace KushBot.Modules;

public class Balance : ModuleBase<SocketCommandContext>
{
    [Command("Balance"), Alias("Bal", "Baps")]
    [RequirePermissions(Permissions.Core)]
    public async Task PingAsync(IGuildUser _user = null)
    {
        await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 1, 2, Context.Channel);

        var user = _user ?? (Context.User as IGuildUser);

        int baps = Data.Data.GetBalance(user.Id);

        if (baps < 30)
        {
            await ReplyAsync($"{user.Mention} has {baps} Baps, fucking homeless {CustomEmojis.Hangg}");

        }else if(baps < 200)
        {
            await ReplyAsync($"{user.Mention} has {baps} Baps, what an eyesore {CustomEmojis.InchisStovi}");
        }
        else if (baps < 500)
        {
            await ReplyAsync($"{user.Mention} has {baps} Baps, Jewish aborigen {CustomEmojis.Kitadimensija}");
        }
        else
        {
            await ReplyAsync($"{user.Mention} has {baps} Baps, wtf {CustomEmojis.Kitadimensija}{CustomEmojis.Monkaw}{CustomEmojis.Kitadimensija}");
        }

    }
}
