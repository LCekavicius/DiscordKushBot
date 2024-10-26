using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using KushBot.Data;
using System.Linq;
using KushBot.DataClasses;
using KushBot.Global;
using KushBot.DataClasses.enums;
using KushBot.Resources.Database;
using Microsoft.EntityFrameworkCore;

namespace KushBot.Modules;

public class Inventory : ModuleBase<SocketCommandContext>
{
    protected readonly SqliteDbContext _dbContext;
    protected readonly PortraitManager _portraitManager;

    public Inventory(SqliteDbContext dbContext, PortraitManager portraitManager)
    {
        _dbContext = dbContext;
        _portraitManager = portraitManager;
    }

    public int GetUpgradeCost(Item item)
    {
        int ret = 0;

        ret += 200 + 22 * (int)item.Rarity;

        for (int i = 0; i < item.Level; i++)
        {
            ret += 50 + (item.Level * 13);
        }

        return ret;
    }

    public string GetLevelSubString(Item item)
    {

        if (item.Level > 1)
        {
            return $"({item.Level})";
        }
        return "";
    }

    public string GetRarityEmote(RarityType rarity) => rarity switch
    {
        RarityType.Common => CustomEmojis.RarityCommon,
        RarityType.Uncommon => CustomEmojis.RarityUncommon,
        RarityType.Rare => CustomEmojis.RarityRare,
        RarityType.Epic => CustomEmojis.RarityEpic,
        RarityType.Legendary => CustomEmojis.RarityLegendary,
        RarityType.Archon => CustomEmojis.RarityArchon,
        _ => throw new Exception($"Unsupported rarity provided: {rarity}")
    };

    [Command("improve"), Alias("levelup", "upgrade")]
    public async Task UpgradeItem([Remainder] string input)
    {
        var user = await _dbContext.GetKushBotUserAsync(Context.User.Id, UserDtoFeatures.Items);

        var selected = user.Items.GetItemsByString(input);

        if (selected == null || !selected.Any())
        {
            await ReplyAsync($"{Context.User.Mention} You dont have that item dumb fucking bitch");
            return;
        }

        if (selected.Count > 1)
        {
            await ReplyAsync($"{Context.User.Mention} You have 2 or more items of the same name, use the id instead");
            return;
        }

        var selectedItem = selected[0];
        int upgradeCost = GetUpgradeCost(selectedItem);

        if (user.Cheems < upgradeCost)
        {
            await ReplyAsync($"{Context.User.Mention} Not enough cheems pepew");
            return;
        }

        user.Cheems -= upgradeCost;
        selectedItem.Level += 1;

        var builder = new ItemBuilder(selectedItem);
        selectedItem = builder
            .WithRandomStat()
            .Build();

        await _dbContext.SaveChangesAsync();

        await ReplyAsync($"{Context.User.Mention} You upgraded your {selectedItem.Name} for {upgradeCost} cheems");
    }

    [Command("Inventory"), Alias("Inv")]
    public async Task ShowInv(IUser user = null)
    {
        user ??= Context.User;

        var userData = await _dbContext.Users
            .AsNoTracking()
            .Where(e => e.Id == user.Id)
            .Include(e => e.Items)
                .ThenInclude(e => e.ItemStats)
            .Select(e => new { e.Cheems, e.Items })
            .FirstOrDefaultAsync();

        var cheems = userData?.Cheems;

        var items = userData?.Items
            .OrderByDescending(e => e.IsEquipped)
            .ThenBy(e => e.Name)
            .ToList();

        EmbedBuilder builder = new EmbedBuilder();

        builder.WithTitle($"{user.Username}'s Inventory: {items.Count}/{DiscordBotService.ItemCap}");

        foreach (var item in items)
        {
            string equipText = "";
            string name = $"{GetLevelSubString(item)}{GetRarityEmote(item.Rarity)}{char.ToUpper(item.Name[0])}{item.Name.Substring(1)}";

            name += $" (id:{item.Id})";

            if (item.IsEquipped)
            {
                await TutorialManager.AttemptSubmitStepCompleteAsync(user.Id, 3, 1, Context.Channel);
                equipText += $"*Equipped* :shield:\n";
            }


            builder.AddField($"**{name}**", equipText + item.GetItemDescription() + $"\nUpgrade cost: **{GetUpgradeCost(item)}**", true);
        }

        builder.WithColor(Discord.Color.Gold);
        builder.AddField($"Cheems {CustomEmojis.Cheems}", $"{cheems} cheems");

        await ReplyAsync("", false, builder.Build());
    }


    [Command("equip")]
    public async Task EquipItem([Remainder] string input)
    {
        var user = await _dbContext.GetKushBotUserAsync(Context.User.Id, UserDtoFeatures.Items);

        var selected = user.Items.GetItemsByString(input);

        if (selected == null || !selected.Any())
        {
            await ReplyAsync($"{Context.User.Mention} You dont have that item dumb fucking bitch");
            return;
        }

        if (selected.Count > 1)
        {
            await ReplyAsync($"{Context.User.Mention} You have 2 or more items of the same name, use the id instead");
            return;
        }

        var item = selected[0];
        await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 3, 1, Context.Channel);

        if (user.Items.Equipped.Any(e => e.Id == item.Id))
        {
            await ReplyAsync($"{Context.User.Mention} You already have that equipped. dubm fbyhudsadaslkjn");
            return;
        }

        if (user.Items.Equipped.Count >= Items.EquipLimit)
        {
            await ReplyAsync($"{Context.User.Mention} All of your item slots are equipped." +
                $" type 'kush destroy *slotNumber*' to destroy an equipped item.");
            return;
        }


        item.IsEquipped = true;
        await _dbContext.SaveChangesAsync();

        _portraitManager.GeneratePortrait(user);

        await ReplyAsync($"{Context.User.Mention} Successfully equipped {item.Name}");
    }
}
