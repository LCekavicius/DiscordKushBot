using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using KushBot.DataClasses;

namespace KushBot.Modules;

public class Balance : ModuleBase<SocketCommandContext>
{

    [Command("Balance"), Alias("Bal", "Baps")]
    public async Task PingAsync()
    {
        await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 1, 2, Context.Channel);

        int baps = Data.Data.GetBalance(Context.User.Id);

        if (baps < 30)
        {
            await ReplyAsync($"{Context.User.Mention} has {baps} Baps, fucking homeless <:hangg:945706193358295052>");

        }else if(baps < 200)
        {
            await ReplyAsync($"{Context.User.Mention} has {baps} Baps, what an eyesore <:inchisstovi:945780209121103922>");
        }
        else if (baps < 500)
        {
            await ReplyAsync($"{Context.User.Mention} has {baps} Baps, Jewish aborigen <:kitadimensija:945779895164895252>");
        }
        else
        {
            await ReplyAsync($"{Context.User.Mention} has {baps} Baps, wtf <:kitadimensija:945779895164895252><:monkaw:945780720796835960><:kitadimensija:945779895164895252>");
        }

    }
    [Command("Balance"), Alias("Bal", "Baps")]
    public async Task PingAsync(IGuildUser user)
    {
        int baps = Data.Data.GetBalance(user.Id);

        if (baps < 30)
        {
            await ReplyAsync($"{user.Mention} has {baps} Baps, fucking homeless <:hangg:945706193358295052>");

        }
        else if (baps < 200)
        {
            await ReplyAsync($"{user.Mention} has {baps} Baps, what an eyesore <:inchisstovi:945780209121103922>");
        }
        else if (baps < 500)
        {
            await ReplyAsync($"{user.Mention} has {baps} Baps, cancer jew <:kitadimensija:945779895164895252>");
        }
        else if (baps < 1000)
        {
            await ReplyAsync($"{user.Mention} has {baps} Baps, wtf <:kitadimensija:945779895164895252><:monkaw:945780720796835960><:kitadimensija:945779895164895252>");
        }
        else
        {
            await ReplyAsync($"{user.Mention} has {baps} Baps, ?  <:zylpray:703125026353709096><:zylpray:703125026353709096><:kitadimensija:945779895164895252><:monkaw:945780720796835960><:kitadimensija:945779895164895252><:zylpray:703125026353709096><:zylpray:703125026353709096>");
        }
    }



}
