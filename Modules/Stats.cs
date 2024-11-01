using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using System.Linq;
using KushBot.DataClasses;
using KushBot.Global;
using KushBot.Resources.Database;
using KushBot.Modules.Interactions;

namespace KushBot.Modules;

[RequirePermissions(Permissions.Core)]
public class Stats(SqliteDbContext dbContext, TutorialManager tutorialManager) : ModuleBase<SocketCommandContext>
{
    [Command("stats")]
    public async Task StatsTable(IUser user = null)
    {
        user ??= Context.User;


        var botUser = await dbContext.GetKushBotUserAsync(user.Id, Data.UserDtoFeatures.Pets | Data.UserDtoFeatures.Infections);
        var petText = await PetDesc(user, botUser);

        if (Context.User.Id == user.Id)
        {
            bool anyStepCompleted = false;
            anyStepCompleted = await tutorialManager.AttemptSubmitStepCompleteAsync(botUser, 1, 0, Context.Channel);
            if(botUser.Pets.Any(e => e.Value?.Tier >= 1))
            {
                anyStepCompleted = anyStepCompleted || await tutorialManager.AttemptSubmitStepCompleteAsync(botUser, 4, 2, Context.Channel);
            }

            if (anyStepCompleted)
            {
                await dbContext.SaveChangesAsync();
            }
        } 
        
        EmbedBuilder builder = new EmbedBuilder()
            .WithTitle($"{user.Username}'s Statistics :");

        builder.WithColor(Discord.Color.Orange);
        builder.AddField($"Balance: :four_leaf_clover:", $"{botUser.Balance} baps", true);
        builder.AddField($"Yiked: :grimacing:", $"{botUser.Yiked} times\n{GetPrefix(botUser.Yiked)}", true);
        builder.AddField("Pets", petText);
        builder.AddField($"Eggs", $"{botUser.Eggs}", true);
        builder.AddField("Boss tickets", $"{botUser.Tickets}/3", true);

        builder.AddField("Next Procs", $"{Proc(user, botUser)}");

        // TODO: Change to retrieve buffs with GetKushBotUser
        bool hasBuffs = await Data.Data.UserHasBuffsAsync(user.Id);

        if (hasBuffs || botUser.RageDuration > 0)
        {
            builder.AddField("User has active buffs", "See **'kush buffs'**");
        }

        string path = @"Data/Portraits";

        builder.AddField("To-give baps remaining:", $"{botUser.DailyGive}/{DiscordBotService.DailyGiveLimit}");

        string selectedPicture = user.Id.ToString() + (botUser.SelectedPicture.StartsWith("gif") ? ".gif" : ".png");

        if (!string.IsNullOrEmpty(botUser.NyaMarry))
        {
            builder.WithThumbnailUrl(botUser.NyaMarry);
        }

        builder.WithImageUrl($"attachment://{selectedPicture}");

        await Context.Channel.SendFileAsync($"{path}/{selectedPicture}", embed: builder.Build(), components: KillParasite.BuildMessageComponent(botUser));
    }

    public string Proc(IUser user, KushBotUser botUser, int skip = 0, int take = 500)
    {
        TimeSpan timeLeft;
        List<string> Procs = new List<string>();

        string Beg;
        string Pinata;
        string Digger;
        string Yoink;
        string Tyler = "Next rage in:";

        timeLeft = botUser.LastBeg.AddHours(1) - DateTime.Now;

        if (timeLeft.Ticks > 0)
        {
            Beg = $"Next Beg in: {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
        }
        else
        {
            Beg = "Next Beg in: **Ready**";
        }
        Procs.Add(Beg);

        if (botUser.Pets.Any(e => e.Key == PetType.Pinata))
        {
            timeLeft = (botUser.LastDestroy.AddHours(22) - DateTime.Now);
            if (timeLeft.Ticks > 0)
            {
                Pinata = $"Next Pinata in: {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
            }
            else
            {
                Pinata = "Next Pinata in: **Ready**";
            }
            Procs.Add(Pinata);
        }
        if (botUser.Pets.Any(e => e.Key == PetType.Goran))
        {
            timeLeft = botUser.LootedDigger - DateTime.Now;
            if ((botUser.DiggerState == 0 || botUser.DiggerState == -1) && timeLeft.Ticks > 0)
            {
                Digger = $"Next Digger in: {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
            }
            else if (botUser.DiggerState == 1)
            {
                var diggerData = (botUser.SetDigger, botUser.GoranMaxDigMinutes);
                TimeSpan ts = DateTime.Now - diggerData.SetDigger;
                int minutes = (int)Math.Round(ts.TotalMinutes) + 1;

                minutes = Math.Min(minutes, diggerData.GoranMaxDigMinutes ?? int.MaxValue);
                Digger = $"Next Digger in: **Digging, {minutes} minutes in**";
            }
            else
            {
                Digger = "Next Digger in: **Ready**";
            }
            Procs.Add(Digger);
        }

        if (botUser.Pets.Any(e => e.Key == PetType.Jew))
        {
            timeLeft = botUser.LastYoink.AddHours(1).AddMinutes(30 - ((botUser.Pets[PetType.Goran]?.CombinedLevel ?? 0) / 3)) - DateTime.Now;
            if (timeLeft.Ticks > 0)
            {
                Yoink = $"Next Yoink in: {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
            }
            else
            {
                Yoink = "Next Yoink in: **Ready**";
            }
            Procs.Add(Yoink);
        }
        if (botUser.Pets.Any(e => e.Key == PetType.TylerJuan))
        {
            timeLeft = botUser.LastTylerRage.AddHours(4).AddSeconds(-1 * Math.Pow((botUser.Pets[PetType.TylerJuan]?.CombinedLevel ?? 0), 1.5)) - DateTime.Now;
            if (timeLeft.Ticks > 0)
            {
                Tyler = $"Next Rage in: {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
            }
            else
            {
                Tyler = "Next Rage in: **Ready**";
            }
            Procs.Add(Tyler);
        }

        if (botUser.YikeDate.AddHours(2) > DateTime.Now)
        {
            TimeSpan ts = botUser.YikeDate.AddHours(2) - DateTime.Now;
            Procs.Add($"Next Yike in: {ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}");
        }
        else
        {
            Procs.Add($"Next Yike in: **Ready**");
        }

        if (botUser.RedeemDate.AddHours(3) > DateTime.Now)
        {
            TimeSpan ts = botUser.RedeemDate.AddHours(3) - DateTime.Now;
            Procs.Add($"Next Redeem in: {ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}");
        }
        else
        {
            Procs.Add($"Next Redeem in: **Ready**");
        }

        string procs = string.Join("\n", Procs.Skip(skip).Take(take));

        return procs;
    }

    public async Task<string> PetDesc(IUser user, KushBotUser botUser)
    {
        if (!botUser.Pets.Any())
        {
            return "No Pets";
        }

        string[] petLines = new string[Pets.All.Count];

        foreach (var petKvp in botUser.Pets)
        {
            var pet = petKvp.Value;

            int petTier = pet.Tier;

            int nextFeedCost = Pets.GetNextFeedCost(pet.Level);

            string realPetLvl = $"({pet.CombinedLevel})";

            petLines[(int)pet.PetType] = $"[{petTier}]{Pets.Dictionary[pet.PetType].Name} - Level {pet.CombinedLevel}/{99 + pet.ItemLevel}" +
                (pet.Level < 99 ? $" NLC: {nextFeedCost}" : "");
        }
        return string.Join("\n", petLines.Where(e => e != null));
    }

    public string GetPrefix(int yikes)
    {
        if (yikes >= 300)
        {
            return "Astro cringe";
        }
        else if (yikes >= 275)
        {
            return "Turbo cringe";
        }
        else if (yikes >= 250)
        {
            return "Nuclear cringe";
        }
        else if (yikes >= 225)
        {
            return "Astoundingly cringe";
        }
        else if (yikes >= 200)
        {
            return "Excessively cringe";
        }
        else if (yikes >= 175)
        {
            return "WAY too cringe";
        }
        else if (yikes >= 150)
        {
            return "Too cringe";
        }
        else if (yikes >= 125)
        {
            return "Very cringe";
        }
        else if (yikes >= 100)
        {
            return "Cringe+";
        }
        else if (yikes >= 75)
        {
            return "Cringe";
        }
        else if (yikes >= 50)
        {
            return "Kinda cringe";
        }
        else if (yikes >= 25)
        {
            return "a bit cringe";
        }
        else
        {
            return "";
        }

    }
}
