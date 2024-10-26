using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;
using System;
using KushBot.Resources.Database;
using KushBot.Global;
using KushBot.DataClasses;

namespace KushBot.Modules;

public partial class InventoryDestroy : Inventory
{
    public InventoryDestroy(SqliteDbContext dbContext, PortraitManager portraitManager) : base(dbContext, portraitManager) { }

    [Command("destroy"), Alias("unequip")]
    public async Task DestroyItem([Remainder] string input)
    {
        var user = await _dbContext.GetKushBotUserAsync(Context.User.Id, Data.UserDtoFeatures.Items);

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

        int cheems = 0;

        cheems += (int)selectedItem.Rarity * 40;

        for (int i = 0; i < (int)selectedItem.Rarity + 1; i++)
        {
            cheems += Random.Shared.Next(15, 36);
        }

        cheems += (selectedItem.Level - 1) * Random.Shared.Next(65, 75);
        cheems += GetUpgradeCost(selectedItem) / 4;
        user.Cheems += cheems;
        user.Items.RemoveAll(e => e.Id == selectedItem.Id);


        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();

        if (selectedItem.IsEquipped)
        {
            _portraitManager.GeneratePortrait(user);
        }

        await ReplyAsync($"{Context.User.Mention} You destroyed {selectedItem.Name} and got {cheems} cheems! {CustomEmojis.Cheems}");
    }
}
