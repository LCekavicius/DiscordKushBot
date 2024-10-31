using Discord.Commands;
using System.Threading.Tasks;
using Discord;
using KushBot.DataClasses;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace KushBot.Modules;

[RequirePermissions(Permissions.Core)]
public class TutoBuilder
{
    private ulong UserId { get; set; }
    public int Page { get; set; }
    private EmbedBuilder builder { get; set; } = new();
    private int currentStepId = 0;
    public TutoBuilder(ulong userId, string avatarUrl, int? selectedPage)
    {
        UserId = userId;
        Page = selectedPage ?? Math.Min(TutorialManager.GetCurrentUserPage(userId), TutorialManager.PageCount);
        builder.WithColor(Color.DarkOrange);
        builder.WithThumbnailUrl(avatarUrl);
    }

    public TutoBuilder AddField(string label, string description, bool includeEmoji = true)
    {
        string strike = (includeEmoji || !TutorialManager.IsPageCompletedForDisplay(UserId, Page)) ? "" : "~~";
        builder.AddField($"{(includeEmoji ? GetProgressEmoji(Page, currentStepId) : "")} **{label}**", $"{strike}{description}{strike}");
        currentStepId++;
        return this;
    }


    public TutoBuilder WithTitle(string title)
    {
        builder.WithTitle(title);
        return this;
    }

    public TutoBuilder WithFooter(string footer)
    {
        builder.WithFooter(footer);
        return this;
    }

    public Embed Build()
    {
        return builder.Build();
    }

    private string GetStrikeText(int page, int stepIndex)
    {
        return TutorialManager.IsStepComplete(UserId, stepIndex, page) ? "~~" : "";
    }

    private string GetProgressEmoji(int page, int stepIndex)
    {
        return page > Math.Min(TutorialManager.GetCurrentUserPage(UserId), TutorialManager.PageCount)
            ? ":x:"
            : TutorialManager.IsStepComplete(UserId, stepIndex, page) ? ":white_check_mark:" : ":purple_circle:";
    }

}

public class Tutorial : ModuleBase<SocketCommandContext>
{
    Dictionary<int, Action<TutoBuilder>> actionDict = new();

    [Command("Tutorial"), Alias("tuto")]
    public async Task Tuto(int? selectedPage = null)
    {
        TutoBuilder builder = new(Context.User.Id, Context.User.GetAvatarUrl(), selectedPage);
        builder.WithTitle($"{builder.Page}/{TutorialManager.PageCount} - Tutorial");

        string methodName = "Page" + builder.Page;
        object[] arguments = new object[] { builder };
        MethodInfo method = typeof(Tutorial).GetMethod(methodName);

        if (method == null)
        {
            await ReplyAsync($"{Context.User.Mention} Smth went wrong, try again later");
            return;
        }

        method.Invoke(this, arguments);

        await ReplyAsync(embed: builder.Build());
    }



    public void Page1(TutoBuilder builder)
    {
        builder.AddField("Type kush stats", "'kush stats' is a command used to display the current status of your account such as: current currency amount, cooldowns, pet levels.")
               .AddField("Type kush beg", "'kush beg' is a command which can be used every hour to gain some baps (The bot's currency)")
               .AddField("Type kush bal", "'kush bal' is a command which ONLY display your baps without providing additional clutter as seen in the stats command.")
               .AddField("Type kush icons", $"The bot features {DiscordBotService.PictureCount} icons which can be unlocked and displayed on your stats embed, find out more when you type 'kush icons'")
               .AddField("\u200b", $"**Reward: {TutorialManager.GetPageReward(1)} **", false)
               .WithFooter("Type 'kush help' to see a detailed list of all commands");
    }

    public void Page2(TutoBuilder builder)
    {
        builder.AddField("Gamble with kush flip", "'kush flip *baps*' e.g. *'kush flip 20'* can be used to flip a coin for a provided baps sum")
               .AddField("Gamble with kush risk", "'Kush risk *baps modifier*' e.g. *'kush risk 10 7'* is similar to kush flip but is customizable. Higher modifier = bigger reward, lower success chance")
               .AddField("Gamble with kush bet", "'kush bet *baps*' A random modifier will be generated and multiplied against the specified baps amount. **Minimum bet is 100 baps**")
               .AddField("Gamble with kush slots", "use with 'kush slots' cost varies with TPL (Total Pet Level). They function as typical gambling types but also offer items and Cheems rewards. find out more in 'kush slots help'")
               .AddField("Complete a quest", "Quests are another key function of this bot, type 'kush quests' to see them, then complete one of them.")
               .AddField("\u200b", $"**Reward: {TutorialManager.GetPageReward(2)} **", false)
               .WithFooter("Type 'kush help' to see a detailed list of all commands");
    }

    public void Page3(TutoBuilder builder)
    {
        builder.AddField("Complete weekly quests", "Complete the weekly quests and acquire your first item")
               .AddField("Equip an item", "use 'kush inv' to check your inventory and 'kush equip *itemName*' to equip an item. **ITEMS CANT BE UNEQUIPPED, ONLY DESTROYED**. Pet levels from items only work if you have that pet.")
               .AddField("hatch your first egg", "Buy an egg with 'kush egg' for **350 baps**, then hatch it with 'kush hatch *baps*'. More baps increase hatch success, guaranteed at **700 baps**. use the commands 'kush pets *petname*' to get info about the pet.")
               .AddField("\u200b", $"**Reward: {TutorialManager.GetPageReward(3)} **", false)
               .WithFooter("Type 'kush items' to see a detailed list of inventory related commands\n'kush pets help' to see commands surrounding pets");
    }

    public void Page4(TutoBuilder builder)
    {
        builder.AddField("Feed a pet", "Pets can be fed and leveled up to level 99 (not accounting for levels gained from items) with the command 'kush feed *petName*'")
               .AddField("Spot an airdrop", "Occasionally, an airdrop may land in one of the designated channels, keep a lookout for it and loot it by reacting to it.")
               .AddField("Get a pet to Tier 1", "In addition to levels, pets also have tiers, which can be increased by hatching pets you already have. Tiers go up to Tier 12. Use 'kush pets progress' to see pet tier progress")
               .AddField("\u200b", $"**Reward: {TutorialManager.GetPageReward(4)} **", false)
               .WithFooter("'kush pets help' to see commands surrounding pets");
    }

    public void Page5(TutoBuilder builder)
    {
        builder.AddField("Collect from a plot", "Plots are yet another part of this bot, you can purchase a plot for **1000** baps with the command 'kush plots buy', review your plots with 'kush plots' and collect them with 'kush plots collect all', there are a variety of plots but the basic type is the 'Garden'.")
               .AddField("Kill a boss", $"Keep an eye out for bosses spawning in <#{DiscordBotService.BossChannelId}>, you can use a boss ticket to attempt to kill a boss for great rewards (baps, items, pet food, pet dupes etc.) Usually, to kill a boss, you need multiple people.")
               .AddField("Redeem something", $"Bot also supports some retarded shit via redeeming, see 'kush redeem'")
               .AddField("\u200b", $"**Reward: {TutorialManager.GetPageReward(5)} **", false)
               .WithFooter("'kush plots help' to see commands surrounding plots\n'kush bosses' to see info regarding bosses.");
    }
}
