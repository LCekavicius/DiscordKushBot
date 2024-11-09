using Discord.Commands;
using KushBot.DataClasses;
using KushBot.Global;
using KushBot.Resources.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace KushBot.Modules;

public class Destroy(SqliteDbContext dbContext) : ModuleBase<SocketCommandContext>
{
    [Command("destroy"), Alias("pinata","d")]
    [RequirePermissions(Permissions.Core)]
    public async Task DestroyPinatac()
    {
        var user = await dbContext.GetKushBotUserAsync(Context.User.Id, Data.UserDtoFeatures.Pets);

        if (!user.Pets.ContainsKey(PetType.Pinata))
        {
            await ReplyAsync($"{Context.User.Mention} dipshit doesnt even have a pinata pet");
            return;
        }

        DateTime lastDestroy = user.LastDestroy;

        if (lastDestroy.AddHours(22) > DateTime.Now)
        {
            TimeSpan timeLeft = lastDestroy.AddHours(22) - DateTime.Now;
            await ReplyAsync($"{CustomEmojis.Egg} {Context.User.Mention} Your Pinata is still growing, you still need to wait: {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2} {CustomEmojis.Zltr}");
            return;
        }

        bool isCdReset = false;

        double tierFloat = (double)user.Pets[PetType.Pinata].Tier;

        Random rad = new Random();

        if(rad.NextDouble() < (tierFloat * 2) / 100)
        {
            isCdReset = true;
        }

        int sum = rad.Next(100,141);
        int petLvl = user.Pets[PetType.Pinata].CombinedLevel;

        for (int i = 0; i < petLvl; i++)
        {
            sum += rad.Next(18,29);
        }
        
        sum += petLvl * 14;

        sum += (int)(sum * (((double)petLvl) / 100));

        if (!isCdReset)
        {
            user.LastDestroy = DateTime.Now;
        }

        string cdResetText = isCdReset ? "\nWtf? The tier bonus proc'ed and the pinata restored itself immediately!" : "";

        await ReplyAsync($"{Context.User.Mention} You destroyed your pinata and got {sum} Baps, the pinata starts growing again.{cdResetText}");
        user.Balance += sum;

        await dbContext.SaveChangesAsync();
    }
}
