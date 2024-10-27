using Discord.Interactions;
using KushBot.Data;
using KushBot.DataClasses.Vendor;
using KushBot.Global;
using KushBot.Resources.Database;
using KushBot.Services;
using System;
using System.Threading.Tasks;

namespace KushBot.Modules.Interactions;

public class PurchaseVendorWare : InteractionModuleBase<SocketInteractionContext>
{
    private readonly VendorService _vendorService;
    private readonly SqliteDbContext _context;

    public PurchaseVendorWare(VendorService vendorService, SqliteDbContext context)
    {
        _vendorService = vendorService;
        _context = context;
    }

    [ComponentInteraction($"{nameof(PurchaseVendorWare)}_*")]
    public async Task Purchase(string wareId)
    {
        if (!int.TryParse(wareId, out var index) || index < 0 || index >= _vendorService.Properties.Wares.Count)
        {
            await FollowupAsync("An error has occured", ephemeral: true);
            return;
        }

        var properties = _vendorService.Properties;
        Ware boughtWare = properties.Wares[index];

        var user = await _context.GetKushBotUserAsync(Context.User.Id, Data.UserDtoFeatures.Pets | Data.UserDtoFeatures.Pictures | UserDtoFeatures.Buffs);

        var validationResult = await Validate(user, boughtWare);
        if (!validationResult.success)
        {
            await FollowupAsync($":musical_note: {validationResult.validationMessage} :musical_note:", ephemeral: true);
            return;
        }

        if (_vendorService.UserPurchases.ContainsKey(Context.User.Id) && _vendorService.UserPurchases[Context.User.Id].AddSeconds(10) > DateTime.Now)
        {
            await FollowupAsync("Vendor overworked, try again in a few seconds", ephemeral: true);
            return;
        }
        else if (_vendorService.UserPurchases.ContainsKey(Context.User.Id))
        {
            _vendorService.UserPurchases[Context.User.Id] = DateTime.Now;
        }
        else
        {
            _vendorService.UserPurchases.Add(Context.User.Id, DateTime.Now);
        }

        var result = await boughtWare.PurchaseAsync(user, Context.User.Id);
        await Context.Interaction.FollowupAsync($":musical_note: {result.message} :musical_note:", ephemeral: true);

        if (result.isSuccess)
        {
            user.LastVendorPurchase = TimeHelper.Now;
            user.Balance -= (int)validationResult.price;
            _vendorService.UserPurchases.Remove(Context.User.Id);
            await _context.SaveChangesAsync();
        }
    }

    private async Task<(bool success, string validationMessage, int? price)> Validate(KushBotUser user, Ware ware)
    {
        var lastPurchaseDate = user.LastVendorPurchase;
        if (lastPurchaseDate > _vendorService.Properties.LastRestockDateTime)
        {
            return (false, "You can buy only one ware a day", null);
        }

        int warePrice = await ware.GetPriceAsync(user);
        if (user.Balance < warePrice)
        {
            return (false, $"{ware.EnumDisplayName} costs {warePrice} baps, but you only have {user.Balance}", null);
        }

        return (true, "", warePrice);
    }
}
