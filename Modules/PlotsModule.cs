using Discord;
using Discord.Commands;
using KushBot.DataClasses;
using KushBot.Global;
using KushBot.Resources.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.Modules;

[Group("plots"), Alias("plot")]
[RequirePermissions(Permissions.Core)]
public class PlotsModule(SqliteDbContext dbContext) : ModuleBase<SocketCommandContext>
{
    [Command("")]
    public async Task ShowPlots(IUser targetUser = null)
    {
        IUser user = targetUser ?? Context.User;

        EmbedBuilder builder = new EmbedBuilder();
        builder.WithColor(Color.DarkGreen);

        var botUser = await dbContext.GetKushBotUserAsync(Context.User.Id, Data.UserDtoFeatures.Plots | Data.UserDtoFeatures.Pets);
        var manager = new PlotsManager(botUser);

        if (!manager.Plots.Any())
        {
            await ReplyAsync($"{Context.User.Mention} You got no plots nigga XD, first plot price: **1000** baps");
            return;
        }

        manager.UpdateState();

        builder.WithAuthor($"{user.Username}'s Plots", user.GetAvatarUrl());
        int userFriendlyPlotIndex = 1;
        foreach (var item in manager.Plots)
        {
            builder.AddField($"Plot {userFriendlyPlotIndex}\n{item.GetLevelText()}\n{item.GetDataText()}", item.GetPlotIcon(), true);
            userFriendlyPlotIndex++;
        }

        if (manager.Plots.Count < 9)
        {
            builder.AddField("Price for next Plot:", $"{manager.NextPlotPrice()} baps");
        }

        await ReplyAsync(embed: builder.Build());

    }

    [Command("buy")]
    public async Task BuyPlot()
    {
        var botUser = await dbContext.GetKushBotUserAsync(Context.User.Id, Data.UserDtoFeatures.Plots | Data.UserDtoFeatures.Pets);
        var manager = new PlotsManager(botUser);

        int cost = manager.NextPlotPrice();

        if (manager.Plots.Count >= DiscordBotService.MaxPlots)
        {
            await ReplyAsync($"{Context.User.Mention} too many plots faggot");
            return;
        }

        if (botUser.Balance < cost)
        {
            await ReplyAsync($"{Context.User.Mention} fuck outta here gay");
            return;
        }

        botUser.Balance -= cost;
        botUser.UserPlots.Add(new Garden()
        {
            Type = PlotType.Garden,
            UserId = botUser.Id,
            Level = 1,
            LastActionDate = null,
            AdditionalData = "",
        });

        await dbContext.SaveChangesAsync();
        await ReplyAsync($"{Context.User.Mention} You bought a new plot for {cost} baps!. gz");
    }

    [Command("transform")]
    public async Task TransformPlot(int userFriendlyIndex, string typeStr)
    {
        int transformCost = 300;
        PlotType type = EnumHelperV2Singleton.Instance.Helper.GetEnumByDescriptedValue<PlotType>(typeStr, true);
        if (type == PlotType.None)
        {
            await ReplyAsync($"{Context.User.Mention} YOU ARE ARAB no such plot type");
            return;
        }

        var botUser = await dbContext.GetKushBotUserAsync(Context.User.Id, Data.UserDtoFeatures.Plots | Data.UserDtoFeatures.Pets);

        if (type == PlotType.Hatchery && !botUser.Pets.Any())
        {
            await ReplyAsync($"{Context.User.Mention} Cant get hatchery with no pets (Hatchery can only hatch owned pets)");
            return;
        }

        if (botUser.Balance < transformCost)
        {
            await ReplyAsync($"{Context.User.Mention} 0 bitches 0 paper, transforming a plot costs {transformCost} baps");
            return;
        }

        var plotsManager = new PlotsManager(botUser);

        if (plotsManager.Plots.Count < userFriendlyIndex)
        {
            await ReplyAsync($"{Context.User.Mention} You dont have that many plots make kebab");
            return;
        }

        if (plotsManager.Plots[userFriendlyIndex - 1].Type == type)
        {
            await ReplyAsync($"{Context.User.Mention} that plot is already a {type} retard dog");
            return;
        }

        botUser.Balance -= transformCost;

        var plot = botUser.UserPlots[userFriendlyIndex - 1];
        plot.Type = type;
        plot.LastActionDate = TimeHelper.Now;
        plot.AdditionalData = type == PlotType.Hatchery
            ? JsonConvert.SerializeObject(
                Enumerable.Range(1, plot.Level)
                    .Select(slot => new HatcheryLine { Slot = slot })
                    .ToList())
            : "";

        await dbContext.SaveChangesAsync();

        await ReplyAsync($"{Context.User.Mention} Plot #{userFriendlyIndex} was transformed from **{plotsManager.Plots[userFriendlyIndex - 1].Type}** into **{type}** for the cost of {transformCost} baps");
    }

    [Command("upgrade")]
    public async Task UpgradeQuery(int userFriendlyIndex)
    {
        var botUser = await dbContext.GetKushBotUserAsync(Context.User.Id, Data.UserDtoFeatures.Plots | Data.UserDtoFeatures.Pets);
        var manager = new PlotsManager(botUser);
        Plot plot = manager.Plots[userFriendlyIndex - 1];

        if (plot.Level == 3)
        {
            await ReplyAsync($"{Context.User.Mention} That plot is already max level (3), kike");
            return;
        }

        int cost = plot.Level == 1 ? 2500 : 10000;


        if (botUser.Balance < cost)
        {
            await ReplyAsync($"{Context.User.Mention} 0 bitches 0 paper, upgrading a plot to level {plot.Level + 1} costs {cost} baps");
            return;
        }

        if (botUser.UserPlots.Count < userFriendlyIndex)
        {
            await ReplyAsync($"{Context.User.Mention} You dont have that many plots make kebab");
            return;
        }

        botUser.Balance -= cost;
        plot.Upgrade();
        await dbContext.SaveChangesAsync();
        await ReplyAsync($"{Context.User.Mention} Successfully upgraded plot to level {plot.Level} for {cost} baps");
    }


    [Command("collect")]
    public async Task Collect(string input)
    {
        var botUser = await dbContext.GetKushBotUserAsync(Context.User.Id, Data.UserDtoFeatures.Plots | Data.UserDtoFeatures.Pets);
        var manager = new PlotsManager(botUser);

        if (!manager.Plots.Any())
        {
            await ReplyAsync($"{Context.User.Mention} 0 bitches 0 plots");
            return;
        }

        bool intParsed = int.TryParse(input, out var userFriendlyPlotIndex);

        string loweredInput = input.ToLower();
        HashSet<string> allowedinputs = new() { "all", "hatchery", "hatcheries", "garden", "gardens", "quarry", "quarries", "g", "h", "q" };


        if (!intParsed && !allowedinputs.Contains(loweredInput))
        {
            await ReplyAsync($"{Context.User.Mention} your arab fuck you sand mirage player");
            return;
        }

        if (intParsed && manager.Plots.Count <= userFriendlyPlotIndex - 1 || userFriendlyPlotIndex < 0)
        {
            await ReplyAsync($"{Context.User.Mention} You dont have that many plots make kebab");
            return;
        }

        await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 5, 0, Context.Channel);

        string response = "";
        if (intParsed)
        {
            response = manager.Collect(userFriendlyPlotIndex - 1);
        }
        else
        {
            response = manager.Collect(input);
        }

        await ReplyAsync($"{Context.User.Mention} {(!string.IsNullOrEmpty(response) ? response : "Nothing to collect retard")}");
        await dbContext.SaveChangesAsync();
    }


    [Command("fill")]
    public async Task FillHatcheries(string input)
    {
        var botUser = await dbContext.GetKushBotUserAsync(Context.User.Id, Data.UserDtoFeatures.Plots | Data.UserDtoFeatures.Pets);
        var manager = new PlotsManager(botUser);

        if (!manager.Plots.Any(e => e is Hatchery))
        {
            await ReplyAsync($"{Context.User.Mention} 0 bitches 0 plots");
            return;
        }

        bool intParsed = int.TryParse(input, out int userFriendlyPlotIndex);

        if (!intParsed && input.ToLower() != "all")
        {
            await ReplyAsync($"{Context.User.Mention} your arab fuck you sand mirage player");
            return;
        }


        if (intParsed && (manager.Plots.Count <= userFriendlyPlotIndex - 1 || userFriendlyPlotIndex < 0))
        {
            await ReplyAsync($"{Context.User.Mention} You dont have that many plots make kebab");
            return;
        }

        if (intParsed && manager.Plots[userFriendlyPlotIndex - 1] is not Hatchery)
        {
            await ReplyAsync($"{Context.User.Mention} NT NIGGER XD");
            return;
        }

        int ownedEggs = botUser.Eggs;
        int maxFill = botUser.Balance / 350;
        maxFill += ownedEggs;

        if (maxFill == 0)
        {
            await ReplyAsync($"{Context.User.Mention} POOR AHAHAHAH");
            return;
        }

        int eggsFilled = manager.FillHatcheries(maxFill, intParsed ? userFriendlyPlotIndex - 1 : null);

        eggsFilled = Math.Min(maxFill, eggsFilled);

        if (eggsFilled == 0)
        {
            await ReplyAsync($"{Context.User.Mention} hatchery's already full");
            return;
        }

        if (ownedEggs > 0)
        {
            botUser.Eggs -= ownedEggs;
            eggsFilled -= ownedEggs;
        }

        int cost = eggsFilled * 350;

        await ReplyAsync($"{Context.User.Mention} Spent {cost} baps{(ownedEggs > 0 ? " and an egg" : "")} to fill his hatchery");
        botUser.Balance -= cost;

        await dbContext.SaveChangesAsync();
    }
}
