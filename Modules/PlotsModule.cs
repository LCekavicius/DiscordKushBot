using Discord;
using Discord.Commands;
using KushBot.DataClasses;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KushBot.Modules
{
    [Group("plots"), Alias("plot")]
    public class PlotsModule : ModuleBase<SocketCommandContext>
    {
        [Command("")]
        public async Task ShowPlots(IUser targetUser = null)
        {
            IUser user = targetUser ?? Context.User;

            EmbedBuilder builder = new EmbedBuilder();
            builder.WithColor(Color.DarkGreen);

            PlotsManager userPlotsManager = Data.Data.GetUserPlotsManager(user.Id);

            if (!userPlotsManager.Plots.Any())
            {
                await ReplyAsync($"{Context.User.Mention} You got no plots nigga XD, first plot price: **1000** baps");
                return;
            }

            await userPlotsManager.UpdateStateAsync();

            builder.WithAuthor($"{user.Username}'s Plots", user.GetAvatarUrl());
            int userFriendlyPlotIndex = 1;
            foreach (var item in userPlotsManager.Plots)
            {
                builder.AddField($"Plot {userFriendlyPlotIndex}\n{item.GetLevelText()}\n{item.GetDataText()}", item.GetPlotIcon(), true);
                userFriendlyPlotIndex++;
            }

            if (userPlotsManager.Plots.Count < 9)
            {
                builder.AddField("Price for next Plot:", $"{userPlotsManager.NextPlotPrice()} baps");
            }

            await ReplyAsync(embed: builder.Build());

        }

        [Command("buy")]
        public async Task BuyPlot()
        {
            PlotsManager userPlotsManager = Data.Data.GetUserPlotsManager(Context.User.Id);
            int cost = userPlotsManager.NextPlotPrice();

            if (userPlotsManager.Plots.Count >= DiscordBotService.MaxPlots)
            {
                await ReplyAsync($"{Context.User.Mention} too many plots faggot");
                return;
            }

            if (Data.Data.GetBalance(Context.User.Id) < cost)
            {
                await ReplyAsync($"{Context.User.Mention} fuck outta here gay");
                return;
            }

            await Data.Data.SaveBalance(Context.User.Id, -1 * cost, false);
            await Data.Data.CreatePlotForUserAsync(Context.User.Id);

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

            if (type == PlotType.Hatchery && !Data.Data.GetUserPets(Context.User.Id).Any())
            {
                await ReplyAsync($"{Context.User.Mention} Cant get hatchery with no pets (Hatchery can only hatch owned pets)");
                return;
            }

            if (Data.Data.GetBalance(Context.User.Id) < transformCost)
            {
                await ReplyAsync($"{Context.User.Mention} 0 bitches 0 paper, transforming a plot costs {transformCost} baps");
                return;
            }

            PlotsManager plotsManager = Data.Data.GetUserPlotsManager(Context.User.Id);
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

            await Data.Data.SaveBalance(Context.User.Id, -1 * transformCost, false);
            await Data.Data.TransformPlotAsync(plotsManager.Plots[userFriendlyIndex - 1].Id, type);
            await ReplyAsync($"{Context.User.Mention} Plot #{userFriendlyIndex} was transformed from **{plotsManager.Plots[userFriendlyIndex - 1].Type}** into **{type}** for the cost of {transformCost} baps");
        }

        [Command("upgrade")]
        public async Task UpgradeQuery(int userFriendlyIndex)
        {
            PlotsManager plotsManager = Data.Data.GetUserPlotsManager(Context.User.Id);
            Plot plot = plotsManager.Plots[userFriendlyIndex - 1];

            if (plot.Level == 3)
            {
                await ReplyAsync($"{Context.User.Mention} That plot is already max level (3), kike");
                return;
            }

            int cost = plot.Level == 1 ? 2500 : 10000;


            if (Data.Data.GetBalance(Context.User.Id) < cost)
            {
                await ReplyAsync($"{Context.User.Mention} 0 bitches 0 paper, upgrading a plot to level {plot.Level + 1} costs {cost} baps");
                return;
            }

            if (plotsManager.Plots.Count < userFriendlyIndex)
            {
                await ReplyAsync($"{Context.User.Mention} You dont have that many plots make kebab");
                return;
            }

            await Data.Data.SaveBalance(Context.User.Id, -1 * cost, false);
            await ReplyAsync($"{Context.User.Mention} Successfully upgraded plot to level {plot.Level + 1}");
            plot.Upgrade();
            await Data.Data.UpdatePlotAsync(plot);
        }


        [Command("collect")]
        public async Task Collect(string input)
        {
            PlotsManager manager = Data.Data.GetUserPlotsManager(Context.User.Id);

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
                response = await manager.CollectAsync(userFriendlyPlotIndex - 1);
            }
            else
            {
                response = await manager.CollectAsync(input);
            }

            await ReplyAsync($"{Context.User.Mention} {(!string.IsNullOrEmpty(response) ? response : "Nothing to collect retard")}");
        }


        [Command("fill")]
        public async Task FillHatcheries(string input)
        {
            PlotsManager manager = Data.Data.GetUserPlotsManager(Context.User.Id);
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

            bool hasEgg = Data.Data.GetEgg(Context.User.Id);
            int maxFill = Data.Data.GetBalance(Context.User.Id) / 350;
            maxFill += hasEgg ? 1 : 0;

            if (maxFill == 0)
            {
                await ReplyAsync($"{Context.User.Mention} POOR AHAHAHAH");
                return;
            }

            int eggsFilled = await manager.FillHatcheriesAsync(maxFill, intParsed ? userFriendlyPlotIndex - 1 : null);

            eggsFilled = Math.Min(maxFill, eggsFilled);

            if (eggsFilled == 0)
            {
                await ReplyAsync($"{Context.User.Mention} hatchery's already full");
                return;
            }

            if (hasEgg)
            {
                await Data.Data.SaveEgg(Context.User.Id, 1);
                eggsFilled -= 1;
            }

            int cost = eggsFilled * 350;

            await ReplyAsync($"{Context.User.Mention} Spent {cost} baps{(hasEgg ? " and an egg" : "")} to fill his hatchery");
            await Data.Data.SaveBalance(Context.User.Id, -1 * cost, false);
        }
    }
}
