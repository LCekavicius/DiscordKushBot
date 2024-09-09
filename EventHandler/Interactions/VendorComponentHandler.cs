using Discord;
using Discord.WebSocket;
using KushBot.DataClasses.Vendor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KushBot.EventHandler.Interactions
{
    public class VendorComponentHandler : ComponentHandler
    {
        public VendorComponentHandler(string customData, SocketInteraction interaction, SocketMessageComponent component, ulong userId)
            : base(customData, interaction, component, userId)
        {

        }

        public override Task<MessageComponent> BuildMessageComponent(bool isDisabled)
        {
            throw new NotImplementedException();
        }

        public override async Task HandleClick()
        {
            if (!int.TryParse(Component.Data.CustomId.Split('_')[1], out int wareIndex))
            {
                await Interaction.RespondAsync("Something went wrong, @unholy57", ephemeral: true);
                return;
            }


            Vendor vendor = Program.VendorObj;
            Ware boughtWare = vendor.Wares[wareIndex];

            var validationResult = await Validate(Interaction.User.Id, vendor, boughtWare);
            if (!validationResult.success)
            {
                await Interaction.RespondAsync($":musical_note: {validationResult.validationMessage} :musical_note:", ephemeral: true);
                return;
            }

            if (vendor.UserPurchases.ContainsKey(Interaction.User.Id) && vendor.UserPurchases[Interaction.User.Id].AddSeconds(10) > DateTime.Now)
            {
                await Interaction.RespondAsync("Vendor overworked, try again in a few seconds", ephemeral: true);
                return;
            }
            else if (vendor.UserPurchases.ContainsKey(Interaction.User.Id))
            {
                vendor.UserPurchases[Interaction.User.Id] = DateTime.Now;
            }
            else
            {
                vendor.UserPurchases.Add(Interaction.User.Id, DateTime.Now);
            }

            var result = await boughtWare.PurchaseAsync(Interaction.User.Id);

            await Interaction.RespondAsync($":musical_note: {result.message} :musical_note:", ephemeral: true);

            if (result.isSuccess)
            {
                await Data.Data.SaveUserVendorPurchaseDateAsync(Interaction.User.Id);
                await Data.Data.SaveBalance(UserId, -1 * boughtWare.Price, false);
                vendor.UserPurchases.Remove(Interaction.User.Id);
            }
        }

        private async Task<(bool success, string validationMessage)> Validate(ulong userId, Vendor vendor, Ware ware)
        {
            DateTime lastPurchaseDate = await Data.Data.GetUserLastVendorPurchaseDateAsync(userId);
            if (lastPurchaseDate > vendor.LastRestockDateTime)
            {
                return (false, "You can buy only one ware a day");
            }

            if (ware.Price > 0)
            {
                int userBaps = Data.Data.GetBalance(userId);
                int warePrice = await ware.GetPriceAsync(userId);
                if (userBaps < warePrice)
                {
                    return (false, $"{ware.EnumDisplayName} costs {warePrice} baps, but you only have {userBaps}");
                }
            }

            return (true, "");
        }

    }
}
