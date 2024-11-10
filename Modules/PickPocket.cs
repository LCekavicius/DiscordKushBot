using Discord;
using Discord.Commands;
using KushBot.DataClasses;
using KushBot.DataClasses.Enums;
using KushBot.Global;
using KushBot.Resources.Database;
using System;
using System.Threading.Tasks;

namespace KushBot.Modules;

[RequirePermissions(Permissions.Core)]
public class PickPocket(SqliteDbContext dbContext) : ModuleBase<SocketCommandContext>
{
    [Command("Yoink"), Alias("Pickpocket", "PP")]
    public async Task PickTarget()
    {
        var user = await dbContext.GetKushBotUserAsync(Context.User.Id, UserDtoFeatures.Pets);

        if (!user.Pets.ContainsKey(PetType.Jew))
        {
            await ReplyAsync($"{Context.User.Mention} no pet jew");
            return;
        }

        double YoinkCd = 30 - (user.Pets[PetType.Jew].CombinedLevel / 3);

        if (user.LastYoink.AddHours(1).AddMinutes(YoinkCd) > DateTime.Now)
        {
            TimeSpan timeLeft = user.LastYoink.AddHours(1).AddMinutes(YoinkCd) - DateTime.Now;
            await ReplyAsync($"{CustomEmojis.Hangg} {Context.User.Mention} You still Have to wait {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2} to yoink again, you sadistic jew {CustomEmojis.Gana}");
            return;
        }
        else
        {
            await ReplyAsync($"{Context.User.Mention} your PP is ready");
        }
    }

    [Command("Yoink"), Alias("Pickpocket", "PP")]
    public async Task PickTarget(IUser user)
    {
        var botUser = await dbContext.GetKushBotUserAsync(Context.User.Id, UserDtoFeatures.Pets | UserDtoFeatures.Quests);
        var level = botUser.Pets[PetType.Jew].CombinedLevel;
        double cooldown = 30 - (level / 3);

        if (!botUser.Pets.ContainsKey(PetType.Jew))
        {
            await ReplyAsync($"{Context.User.Mention} You don't even have a pet {Pets.Jew.Name}, Dumbass cuck");
            return;
        }

        var targetUser = await dbContext.GetKushBotUserAsync(user.Id);

        if (targetUser == null)
        {
            await ReplyAsync($"?");
            return;
        }

        if (botUser.LastYoink.AddHours(1).AddMinutes(cooldown) > DateTime.Now)
        {
            TimeSpan timeLeft = botUser.LastYoink.AddHours(1).AddMinutes(cooldown) - DateTime.Now;
            await ReplyAsync($"{CustomEmojis.Hangg} {Context.User.Mention} You still Have to wait {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2} to yoink again, you sadistic jew <:gana:945781528699474053>");
            return;
        }

        if (targetUser.Balance < 50 + (19 + (int)level) * (1.12 + level / 100))
        {
            await ReplyAsync($"{Context.User.Mention} you tried yoinking {user.Mention} but he's too poor to even bother to");
            return;
        }

        double chance = 57 + (level / 3);

        if (DiscordBotService.Test == Context.User.Id)
        {
            chance = 100;
            DiscordBotService.Test = default;
        }

        if (Random.Shared.NextDouble() < (chance / 100))
        {
            var result = CalculateUserYoinkBaps(botUser, targetUser.Balance, out string tierBenefit);
            int totalBaps = result.yoinkedBaps + result.extraBaps;
            targetUser.Balance -= result.yoinkedBaps;
            botUser.Balance += totalBaps;
            await ReplyAsync($"{CustomEmojis.Ima} {Context.User.Mention} Yoinked {user.Mention} for {result.yoinkedBaps} Baps, on the way back he found some more and got **{totalBaps}** in total {CustomEmojis.Clueless}{tierBenefit}");

            botUser.AddUserEvent(UserEventType.YoinkSuccess);
            var questResult = botUser.AttemptCompleteQuests();

            await Context.CompleteQuestsAsync(questResult.freshCompleted, questResult.lastDailyCompleted, questResult.lastWeeklyCompleted);
            await dbContext.SaveChangesAsync();
        }
    }

    private (int yoinkedBaps, int extraBaps) CalculateUserYoinkBaps(KushBotUser user, int maxYoink, out string tierBenefit)
    {
        var pet = user.Pets[PetType.Jew];
        int petTier = pet.Tier;
        double JewLevel = pet.CombinedLevel;

        int lowYoink = (int)Math.Round(JewLevel / 1.2);
        double yoinkedBaps = Random.Shared.Next(13 + lowYoink, 19 + (int)JewLevel) * (1.12 + JewLevel / 100);

        yoinkedBaps = Math.Round(Math.Min(yoinkedBaps, maxYoink));

        double min = 0.65;
        double max = 0.85;
        double extraBaps = min + (Random.Shared.NextDouble() * (max - min));

        extraBaps = (0.55 + extraBaps) * yoinkedBaps;

        extraBaps += (int)(extraBaps * (((double)JewLevel) / 100));

        double TierBenefiteChance = petTier * 2;

        if (DiscordBotService.TierTest == Context.User.Id)
        {
            TierBenefiteChance = 100;
            DiscordBotService.TierTest = default;
        }

        tierBenefit = "";
        double roll = Random.Shared.NextDouble();

        if (roll > TierBenefiteChance / 100)
        {
            user.LastYoink = DateTime.Now;
        }
        else
        {
            tierBenefit = $"\nJew's tier reset his cooldown immediately. {CustomEmojis.Pepehap}";
        }

        return ((int)yoinkedBaps, (int)extraBaps);
    }

    [Command("Yoink"), Alias("Pickpocket", "PP")]
    public async Task PickTarget(string code)
    {
        var user = await dbContext.GetKushBotUserAsync(Context.User.Id, UserDtoFeatures.Pets);

        if (!user.Pets.ContainsKey(PetType.Jew))
        {
            await ReplyAsync($"{Context.User.Mention} You don't even have a pet {Pets.Jew.Name}, Dumbass cuck");
            return;
        }

        bool exist = false;
        int index = -1;

        for (int i = 0; i < Give.GivePackages.Count; i++)
        {
            if (Give.GivePackages[i].Code == code)
            {
                exist = true;
                index = i;
            }
        }

        if (!exist)
        {
            await ReplyAsync($"{Context.User.Mention}, the {code} package either doesn't exist or has already bean yoinked");
            return;
        }

        if (Give.GivePackages[index].Author == Context.User.Id)
        {
            await ReplyAsync($"{Context.User.Mention} You can't yoink your own package, feeling smart?");
            return;
        }

        if (Give.GivePackages[index].Recipient == Context.User.Id)
        {
            await ReplyAsync($"{Context.User.Mention} You can't yoink a package addressed to you, feeling smart?");
            return;
        }

        float StealMultiplier = Random.Shared.Next(23, 32 + user.Pets[PetType.Jew].CombinedLevel / 3);
        StealMultiplier /= 100;

        double stolen = Give.GivePackages[index].Baps * StealMultiplier;
        int _stolen = (int)Math.Round(stolen);

        await ReplyAsync($"{Context.User.Mention} has succesfully yoinked the package 'code **{code}**' and stole **{_stolen}/{Give.GivePackages[index].Baps}** baps!");

        Give.GivePackages[index].Baps -= _stolen;


        user.Balance += _stolen;

        Give.GivePackages.RemoveAt(index);

        user.LastYoink = user.LastYoink.AddMinutes(25);

        await dbContext.SaveChangesAsync();
    }
}
