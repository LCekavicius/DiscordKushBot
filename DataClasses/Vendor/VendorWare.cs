using Discord;
using KushBot.Modules;
using Microsoft.EntityFrameworkCore.Query.Internal;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Formats.Tar;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KushBot.DataClasses.Vendor
{
    public enum VendorWare
    {
        Cheems,
        Item,

        [Description("Pet food")]
        PetFoodCommon,
        [Description("Pet food")]
        PetFoodRare,
        [Description("Pet food")]
        PetFoodEpic,


        [Description("Boss ticket")]
        BossTicket,
        Icon,
        Rejuvenation, //Reset yike, nya marry and redeem CDs,
        Egg,

        //[Description("Pet dupe")]
        //PetDupe,
        [Description("Pet dupe")]
        PetDupeCommon,
        [Description("Pet dupe")]
        PetDupeRare,
        [Description("Pet dupe")]
        PetDupeEpic,

        [Description("Plot boost")]
        PlotBoost,
        [Description("Kush gym")]
        KushGym,
        [Description("Fishing rod")]
        FishingRod,
        Parasite,
        Artillery, // extra dmg to next boss, no stack
        Adderal,
        [Description("Slots tokens")]
        SlotsTokens,
        //Plot
    }


    public class WareData
    {
        public int Type { get; set; }
        public string DisplayName { get; set; }
        public string EnumDisplayName { get; set; }
        public double Amount { get; set; }
        public int Price { get; set; }
    }

    public abstract class Ware
    {
        public VendorWare Type { get; set; }
        public string DisplayName => $"{Program.LeftSideVendorWareEmojiMap[Type]} " +
            $"{EnumHelperV2Singleton.Instance.Helper.ToString<VendorWare>(Type)}{(Amount > 1 ? $" **({Amount})**" : "")}";
        public string EnumDisplayName => $"{EnumHelperV2Singleton.Instance.Helper.ToString<VendorWare>(Type)}{(Amount > 1 ? $" ({Amount})" : "")}";

        public double Amount { get; set; }
        public int Price { get; set; }
        public double Rate { get; set; }

        public abstract string GetWareDescription();
        public abstract string GetWareString();
        public abstract Task<(string message, bool isSuccess)> PurchaseAsync(ulong userId);
        public abstract Task<int> GetPriceAsync(ulong userId);

        protected (int userBaps, string message, bool validationSuccess) ValidatePrice(ulong userId, int warePrice)
        {
            int userBalance = Data.Data.GetBalance(userId);
            if (userBalance >= warePrice)
            {
                return (userBalance, "", true);
            }
            return (userBalance, $"{EnumDisplayName} costs {warePrice} baps, but you only have {userBalance}", false);
        }

        protected string GetWareStringForVariableWares(string insert = "")
        {
            return $"{(string.IsNullOrEmpty(insert) ? "" : insert + "\n")}Check details with **'kush vendor'**";
        }

        protected string GetWareStringForStaticWares(string insert)
        {
            return $"{(string.IsNullOrEmpty(insert) ? "" : insert + "\n")}**{Price}** baps";
        }
    }

    public sealed class CheemsWare : Ware
    {
        public CheemsWare()
        {
            Type = VendorWare.Cheems;
            Random rnd = new Random();
            Amount = rnd.Next(80, 131);
            Price = GetPrice();
        }

        public override string GetWareString() => GetWareStringForStaticWares($"");

        public override string GetWareDescription()
        {
            return $"Obtain {Amount} cheems. See 'kush items'";
        }

        private int GetPrice()
        {
            return (int)(Amount * 8);
        }

        public override async Task<int> GetPriceAsync(ulong userId)
        {
            return GetPrice();
        }

        public override async Task<(string message, bool isSuccess)> PurchaseAsync(ulong userId)
        {
            await Data.Data.AddUserCheems(userId, (int)Amount);
            return ($"You successfully bought {Amount} cheems for {Price} baps", true);
        }
    }

    public sealed class ItemWare : Ware
    {
        public ItemWare()
        {
            Random rnd = new();
            Type = VendorWare.Item;

            Rate = RollRarity();
            Price = GetPrice();
        }

        public override string GetWareDescription()
        {
            return $"Obtain a {GetRarityEmote((int)Rate)} {GetRarityString((int)Rate)} item. See 'kush items'";
        }
        private int RollRarity()
        {
            Random rnd = new Random();
            double val = rnd.NextDouble();
            if (val < 0.4)
            {
                return 1;
            }
            else if (val < 0.7)
            {
                return 2;
            }
            else if (val < 0.85)
            {
                return 3;
            }
            else if (val < 0.95)
            {
                return 4;
            }
            else
            {
                return 5;
            }
        }

        public override async Task<int> GetPriceAsync(ulong userId)
        {
            return Price;
        }

        public override string GetWareString() => GetWareStringForStaticWares($"{GetRarityEmote((int)Rate)} {GetRarityString((int)Rate)}");
        public override async Task<(string message, bool isSuccess)> PurchaseAsync(ulong userId)
        {
            Data.Data.GenerateItem(userId, (int)Rate);
            return ($"You successfully bought {EnumDisplayName} of rarity {GetRarityString((int)Rate)} for {Price} baps.", true);
        }

        private int GetPrice() =>
            Rate switch
            {
                1 => 1200,
                2 => 2200,
                3 => 4250,
                4 => 7950,
                _ => 18500
            };

        //kill me
        private string GetRarityEmote(int rarity)
        {
            switch (rarity)
            {
                case 2:
                    return ":green_square:";

                case 3:
                    return ":blue_square:";

                case 4:
                    return ":purple_square:";

                case 5:
                    return ":orange_square:";

                default:
                    return ":white_large_square:";
            }
        }

        private string GetRarityString(int rarity)
        {
            switch (rarity)
            {
                case 2:
                    return "Uncommon";

                case 3:
                    return "Rare";

                case 4:
                    return "Epic";

                case 5:
                    return "Legendary";

                default:
                    return "Common";
            }
        }
    }

    public sealed class FoodWare : Ware
    {
        public int PetId { get; set; }
        private int PetLvl { get; set; }
        public FoodWare(int bottomEnd, int topEnd, VendorWare type)
        {
            int priceOffsetByRarity = bottomEnd + topEnd;
            Type = type;
            Random rnd = new Random();
            PetId = rnd.Next(bottomEnd, topEnd);
            Rate = rnd.Next(45 + (priceOffsetByRarity * 2), 66 + (priceOffsetByRarity * 2));
        }

        public override string GetWareDescription()
        {
            return $"Obtain a level up for {Program.GetPetName(PetId)}. See 'kush pets help'";
        }

        public override async Task<int> GetPriceAsync(ulong userId)
        {
            int petLevel = Data.Data.GetPetLevel(userId, PetId);
            int itemPetLevel = Data.Data.GetItemPetLevel(userId, PetId);
            PetLvl = petLevel - itemPetLevel;

            int ogPrice = GetNLC(userId, PetLvl);
            Price = (int)((Rate / 100) * (double)ogPrice);
            return Price;
        }

        public override string GetWareString() => GetWareStringForVariableWares($"{Program.GetPetName(PetId)}");

        public override async Task<(string message, bool isSuccess)> PurchaseAsync(ulong userId)
        {
            string userPets = Data.Data.GetPets(userId);
            if (!userPets.Contains(PetId.ToString()))
                return ($"You don't have the pet {Program.GetPetName(PetId)} so you cant buy food for it.", false);


            await GetPriceAsync(userId);
            var priceValidation = ValidatePrice(userId, Price);

            if (!priceValidation.validationSuccess)
                return ($"the food costs {Price} baps, but you only have {priceValidation.userBaps}", false);


            int petLvl = Data.Data.GetPetLevel(userId, PetId) - Data.Data.GetItemPetLevel(userId, PetId);

            if (PetLvl >= 99)
            {
                return ($"You can't level your pets past level 99", false);
            }

            await Data.Data.SavePetLevels(userId, PetId, PetLvl + 1, false);

            return ($"You succesfully bought {EnumDisplayName} for {Price} baps, for the pet {Program.GetPetName(PetId)}", true);
        }

        private int GetNLC(ulong userId, int petLevel)
        {


            double negate = 0;
            if (petLevel < 15)
            {
                negate = (double)(petLevel) / 100;
            }
            else
            {
                negate = 0.14;
            }

            int BapsFed = 0;

            if (petLevel == 1)
            {
                BapsFed = 100;
            }

            else
            {
                double _BapsFed = Math.Pow(petLevel, 1.14 - negate) * (70 + ((petLevel) / 1.25));
                BapsFed = (int)Math.Round(_BapsFed);
            }

            return BapsFed;
        }

    }

    public sealed class TicketWare : Ware
    {
        public TicketWare()
        {
            Type = VendorWare.BossTicket;
            Price = 1000;
        }

        public override string GetWareDescription()
        {
            return $"Obtain a boss ticket. See 'kush bosses'";
        }
        public override string GetWareString() => GetWareStringForStaticWares("");

        public override async Task<int> GetPriceAsync(ulong userId)
        {
            return Price;
        }

        public override async Task<(string message, bool isSuccess)> PurchaseAsync(ulong userId)
        {
            int userTickets = Data.Data.GetTicketCount(userId);
            if (userTickets >= Program.MaxTickets)
            {
                return ($"You already have {Program.MaxTickets}/{Program.MaxTickets} boss tickets", false);
            }

            if (Program.BossObject != null && Program.BossObject.Participants.Contains(userId) && userTickets >= 2)
            {
                return ($"You already have {Program.MaxTickets}/{Program.MaxTickets} boss tickets", false);
            }

            if (Program.ArchonObject != null && Program.ArchonObject.Participants.Contains(userId) && userTickets >= 2)
            {
                return ($"You already have {Program.MaxTickets}/{Program.MaxTickets} boss tickets", false);
            }



            await Data.Data.SaveTicket(userId, true);
            return ($"You succesfully bought a {EnumDisplayName} for {Price} baps", true);
        }
    }

    public sealed class IconWare : Ware
    {
        public IconWare()
        {
            Random rnd = new();
            Type = VendorWare.Icon;
            Rate = rnd.Next(45, 66);
        }

        public override string GetWareDescription()
        {
            return $"Obtain an icon for a discounted price. See 'kush icons'";
        }

        public override string GetWareString() => GetWareStringForVariableWares();

        public override async Task<int> GetPriceAsync(ulong userId)
        {
            return GetPrice(userId);
        }

        private int GetPrice(ulong userId, List<int> providedPictures = null)
        {
            providedPictures ??= Data.Data.GetPictures(userId);
            int price = 325 + providedPictures.Count * 25;

            double modifier = ((double)Rate) / 100;

            Price = (int)((double)price * modifier);
            return Price;
        }

        public override async Task<(string message, bool isSuccess)> PurchaseAsync(ulong userId)
        {

            List<int> ownedPictures = Data.Data.GetPictures(userId);
            if (ownedPictures.Count >= Program.PictureCount)
            {
                return ($"You already have all {Program.PictureCount} icons", false);
            }

            int price = GetPrice(userId, ownedPictures);
            var priceValidationResult = ValidatePrice(userId, price);

            if (!priceValidationResult.validationSuccess)
            {
                return (priceValidationResult.message, false);
            }

            List<int> range = Enumerable.Range(1, 99).ToList();
            List<int> allowedIcons = range.Except(ownedPictures).ToList();

            Random rnd = new Random();

            int icon = allowedIcons[rnd.Next(0, allowedIcons.Count)];

            await Data.Data.UpdatePictures(userId, icon);

            return ($"You successfully purchased the icon #{icon} for {price} baps", true);
        }
    }

    public sealed class RejuvenationWare : Ware
    {
        public RejuvenationWare()
        {
            Type = VendorWare.Rejuvenation;
            Price = 50;
        }

        public override string GetWareDescription()
        {
            return $"Obtain a reset for the 'yike', 'redeem', 'nya marry', 'nya claim' commands.";
        }

        public override string GetWareString() => GetWareStringForStaticWares("");

        public override async Task<int> GetPriceAsync(ulong userId) => Price;

        public override async Task<(string message, bool isSuccess)> PurchaseAsync(ulong userId)
        {
            await Data.Data.AddToNyaMarryDate(userId, -12);
            await Data.Data.SaveRedeemDate(userId, DateTime.Now.AddHours(-12));
            await Data.Data.SaveYikeDate(userId, DateTime.Now.AddHours(-12));
            await Data.Data.SaveLastClaimDate(userId, DateTime.Now.AddHours(-12));
            return ($"You bought {EnumDisplayName}, your yike, redeem and nya marry CDs have been reset", true);
        }
    }

    public sealed class EggWare : Ware
    {
        public EggWare()
        {
            Type = VendorWare.Egg;
            Random rnd = new Random();
            Price = rnd.Next(125, 225);
        }

        public override string GetWareDescription()
        {
            return $"Obtain an egg for a discounted price. See 'kush pets'";
        }

        public override string GetWareString() => GetWareStringForStaticWares("");

        public override async Task<int> GetPriceAsync(ulong userId) => Price;

        public override async Task<(string message, bool isSuccess)> PurchaseAsync(ulong userId)
        {
            if (Data.Data.GetEgg(userId))
            {
                return ("You already have an egg uwu :3", false);
            }

            await Data.Data.SaveEgg(userId, true);
            return ($"You bought an egg for {Price} baps", true);
        }
    }

    public sealed class PetDupeWare : Ware
    {
        public int PetId { get; set; }
        //public PetDupeWare()
        //{
        //    Type = VendorWare.PetDupe;
        //    Random rnd = new Random();
        //    Price = rnd.Next(350, 451);
        //    PetId = rnd.Next(0, 6);
        //}

        public PetDupeWare(int bottomEnd, int topEnd, VendorWare type)
        {
            int priceOffsetByRarity = bottomEnd + topEnd;
            Type = type;
            Random rnd = new Random();
            Price = rnd.Next(350 + (priceOffsetByRarity * 5), 451 + (priceOffsetByRarity * 7));
            PetId = rnd.Next(bottomEnd, topEnd);
        }

        public override string GetWareDescription()
        {
            return $"Obtain a dupe for **{Program.GetPetName(PetId)}**. See 'kush pets help'";
        }

        public override string GetWareString() => GetWareStringForStaticWares(Program.GetPetName(PetId));

        public override async Task<int> GetPriceAsync(ulong userId) => Price;

        public override async Task<(string message, bool isSuccess)> PurchaseAsync(ulong userId)
        {
            if (!Data.Data.GetPets(userId).Contains(PetId.ToString()))
            {
                return ($"You dont have that pet so you cant buy {EnumDisplayName} for {Program.GetPetName(PetId)}", false);
            }
            await Data.Data.SavePetDupes(userId, PetId, Data.Data.GetPetDupe(userId, PetId) + 1);
            return ($"You successfully bought {EnumDisplayName} for {Program.GetPetName(PetId)}", true);
        }
    }

    public sealed class PlotBoostWare : Ware
    {
        //scale off plots #
        private PlotsManager plotsManager;
        public PlotBoostWare()
        {
            Random rnd = new();
            Type = VendorWare.PlotBoost;
            Amount = rnd.Next(80, 141);
            Rate = rnd.Next(15, 26);
        }

        public override string GetWareDescription()
        {
            return $"Obtain a plot boost, this will speed up your plots by {Amount} minutes, (but increase duration of currently abused pets in abuse chamber).";
        }

        public override string GetWareString() => GetWareStringForVariableWares("Denominated in minutes");

        public override async Task<int> GetPriceAsync(ulong userId)
        {
            plotsManager = Data.Data.GetUserPlotsManager(userId);
            Price = 0;
            foreach (var item in plotsManager.Plots)
            {
                if (item.Type == PlotType.Abuse)
                {
                    Price += (int)((Rate + 12) * item.Level);
                }
                else
                {
                    Price += (int)(Rate * item.Level);
                }
            }

            return Price;
        }

        public override async Task<(string message, bool isSuccess)> PurchaseAsync(ulong userId)
        {
            await GetPriceAsync(userId);

            if (plotsManager.Plots.Count == 0)
            {
                return ("Maybe you should buy it when you have a plot 'kush plots help'", false);
            }

            var priceValidation = ValidatePrice(userId, Price);
            if (!priceValidation.validationSuccess)
                return (priceValidation.message, false);

            await plotsManager.ShiftTimeAsync((int)Amount);
            return ($"You successfully bought {EnumDisplayName} for {Price} baps", true);
        }
    }

    public sealed class KushGymWare : Ware
    {
        public int Level { get; set; }
        public int Duration { get; set; }

        public KushGymWare()
        {
            Type = VendorWare.KushGym;
            Random rnd = new Random();
            Level = rnd.Next(1, 4);
            Duration = rnd.Next(6, 11);
            Price = Level * 100;
        }

        public override string GetWareDescription()
        {
            return $"Obtain a level **{Level}** duration **{Duration}** {EnumDisplayName} buff. This gives a chance (equal to 2% * Level) to get double baps while gambling.";
        }

        public override string GetWareString() => GetWareStringForStaticWares($" Level: **{Level}**\nDuration: **{Duration}**");

        public override async Task<int> GetPriceAsync(ulong userId) => Price;

        public override async Task<(string message, bool isSuccess)> PurchaseAsync(ulong userId)
        {
            await Data.Data.CreateConsumableBuffAsync(userId, BuffType.KushGym, Duration, Level * 2);
            return ($"You succesfully bought the weed effect {EnumDisplayName} (Lvl {Level}, Duration: {Duration}) for {Price} baps", true);
        }
    }

    public sealed class FishingRodWare : Ware
    {
        public int Level { get; set; }
        public int Duration { get; set; }

        public FishingRodWare()
        {
            Type = VendorWare.FishingRod;
            Random rnd = new Random();
            Level = rnd.Next(1, 4);
            Duration = rnd.Next(6, 11);
            Price = Level * 100;
        }

        public override string GetWareDescription()
        {
            return $"Obtain a level **{Level}** duration **{Duration}** {EnumDisplayName} buff. This gives a chance (equal to 2% * Level) to prevent the loss of baps when gambling.";
        }

        public override string GetWareString() => GetWareStringForStaticWares($" Level: **{Level}**\nDuration: **{Duration}**");

        public override async Task<int> GetPriceAsync(ulong userId)
        {
            return Price;
        }

        public override async Task<(string message, bool isSuccess)> PurchaseAsync(ulong userId)
        {
            await Data.Data.CreateConsumableBuffAsync(userId, BuffType.FishingRod, Duration, Level * 2);
            return ($"You succesfully bought the weed effect {EnumDisplayName} (Lvl {Level}, Duration: {Duration}) for {Price} baps", true);
        }
    }

    public sealed class ParasiteWare : Ware
    {
        public Infection Infection { get; set; }

        public ParasiteWare()
        {
            Type = VendorWare.Parasite;
            Random rnd = new Random();
            List<InfectionState> states = new List<InfectionState>() { InfectionState.Egg, InfectionState.Hatchling, InfectionState.Juvenile, InfectionState.Tyrant };
            InfectionState state = states.OrderBy(e => rnd.NextDouble()).FirstOrDefault();

            Infection = new()
            {
                CreationDate = DateTime.Now.AddHours(state == InfectionState.Egg
                ? 0
                : state == InfectionState.Hatchling
                    ? -4
                    : state == InfectionState.Juvenile
                        ? -16
                        : -32)
            };

            Price = state == InfectionState.Egg
                ? 30
                : state == InfectionState.Hatchling
                    ? 60
                    : state == InfectionState.Juvenile
                        ? 120
                        : 200;
        }

        public override string GetWareDescription()
        {
            return $"Get infected with a {EnumHelperV2Singleton.Instance.Helper.ToString<InfectionState>(Infection.State)} tier parasite.";
        }

        public override string GetWareString() => GetWareStringForStaticWares(GetStringByTier());

        private string GetStringByTier()
        {
            return $"{Infection.GetEmote()} " +
                $"{EnumHelperV2Singleton.Instance.Helper.ToString<InfectionState>(Infection.State)}";
        }

        public override async Task<int> GetPriceAsync(ulong userId)
        {
            return Price;
        }

        public override async Task<(string message, bool isSuccess)> PurchaseAsync(ulong userId)
        {
            var infections = await Data.Data.GetUserInfectionsAsync(userId);

            if (infections.Count >= 8)
            {
                return ($"You already have 8 parasites and cant have more", false);
            }

            Infection clone = Infection.CloneObject() as Infection;
            clone.OwnerId = userId;
            clone.Id = Guid.NewGuid();
            await Data.Data.InfestUserAsync(userId, clone, true);

            return ($"You successfully bought {EnumHelperV2Singleton.Instance.Helper.ToString<InfectionState>(Infection.State)} tier parasite for {Price} baps", true);
        }
    }

    public sealed class ArtilleryWare : Ware
    {
        public ArtilleryWare()
        {
            Type = VendorWare.Artillery;
            Random rnd = new();
            Price = 100;
            Amount = rnd.Next(3, 6);
        }

        public override string GetWareDescription()
        {
            return $"Get +{Amount} damage for the next boss fight (lost upon boss fight)";
        }

        public override string GetWareString() => GetWareStringForStaticWares($"Extra {Amount} boss dmg next fight");

        public override async Task<int> GetPriceAsync(ulong userId)
        {
            return Price;
        }

        public override async Task<(string message, bool isSuccess)> PurchaseAsync(ulong userId)
        {
            bool alreadyOwned = await Data.Data.UserHasBuffAsync(userId, BuffType.BossArtillery);
            if (alreadyOwned)
            {
                return ($"You already own {EnumDisplayName} and cant carry another one", false);
            }
            await Data.Data.CreateConsumableBuffAsync(userId, BuffType.BossArtillery, 1, Amount);

            return ($"You successfully bought {EnumDisplayName} for {Price} baps", true);
        }
    }

    public sealed class PlotWare : Ware
    {
        private PlotsManager plotsManager;

        public PlotWare()
        {
            Random rnd = new Random();
            //Type = VendorWare.Plot;
            Rate = rnd.Next(50, 76);
        }

        public override string GetWareDescription()
        {
            return $"Get a plot for a discounted price (will spawn as garden). see 'kush plots help'";
        }

        public override string GetWareString() => GetWareStringForVariableWares();

        public override async Task<(string message, bool isSuccess)> PurchaseAsync(ulong userId)
        {
            await GetPriceAsync(userId);

            var plotsManager = Data.Data.GetUserPlotsManager(userId);

            if (plotsManager.Plots.Count >= Program.MaxPlots)
            {
                return ($"You already more than enough plots", false);
            }

            var validatationResult = ValidatePrice(userId, Price);

            if (!validatationResult.validationSuccess)
            {
                return ($"The {EnumDisplayName} ware costs {Price} baps, but you only have {validatationResult.userBaps}", false);
            }

            await Data.Data.CreatePlotForUserAsync(userId);

            return ($"You successfully bought a {EnumDisplayName} for {Price} baps", true);
        }

        public override async Task<int> GetPriceAsync(ulong userId)
        {
            PlotsManager userPlotsManager = Data.Data.GetUserPlotsManager(userId);
            int ogPrice = userPlotsManager.NextPlotPrice();

            Price = (int)(ogPrice * (((double)Rate) / 100));
            return Price;
        }
    }

    //public sealed class ConcertaWare : Ware
    //{
    //    public int Duration { get; set; }
    //    public int Percentage { get; set; }

    //    public ConcertaWare()
    //    {
    //        Type = VendorWare.Concerta;
    //        Random rnd = new Random();
    //        Duration = rnd.Next(8, 11);
    //        Percentage = rnd.Next(8, 11);
    //        Price = 10 * (Duration + Percentage);
    //    }

    //    public override string GetWareString() => GetWareStringForStaticWares($" Duration: **{Duration}**\nPotency: **{Percentage}%**");

    //    public override async Task<int> GetPriceAsync(ulong userId)
    //    {
    //        return Price;
    //    }

    //    public override Task<(string message, bool isSuccess)> PurchaseAsync(ulong userId)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public sealed class AmbienWare : Ware
    //{
    //    public int Duration { get; set; }
    //    public int Percentage { get; set; }

    //    public AmbienWare()
    //    {
    //        Type = VendorWare.Ambien;
    //        Random rnd = new Random();
    //        Duration = rnd.Next(8, 11);
    //        Percentage = rnd.Next(8, 11);
    //        Price = 10 * (Duration + Percentage);
    //    }

    //    public override string GetWareString() => GetWareStringForStaticWares($"Duration: **{Duration}**\nPotency: **{Percentage}%**");

    //    public override async Task<int> GetPriceAsync(ulong userId)
    //    {
    //        return Price;
    //    }

    //    public override Task<(string message, bool isSuccess)> PurchaseAsync(ulong userId)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public sealed class AdderalWare : Ware
    {
        public int Level { get; set; }

        public AdderalWare()
        {
            Type = VendorWare.Adderal;
            Random rnd = new Random();
            Level = rnd.Next(1, 4);
            Price = Level * 75;
        }

        public override string GetWareDescription()
        {
            return $"Consume some adderal and get a reset of your cooldowns (beg is certain, everything else comes with a chance based off of the level)";
        }

        public override string GetWareString() => GetWareStringForStaticWares($"Level: **{Level}**");

        public override async Task<int> GetPriceAsync(ulong userId)
        {
            return Price;
        }

        public override async Task<(string message, bool isSuccess)> PurchaseAsync(ulong userId)
        {
            //Copied from plots.
            Random rnd = new();
            string text = $"You successfully bought Level {Level} adderal.\nYour beg CD got reset";

            await Data.Data.SaveLastBeg(userId, DateTime.Now.AddHours(-2));

            if (Data.Data.GetPetLevel(userId, 1) > 0)
            {
                if (rnd.Next(0, 5 - Level) == 0)
                {
                    DateTime pinateDate = Data.Data.GetLastDestroy(userId);
                    await Data.Data.SaveLastDestroy(userId, pinateDate.AddHours(-2 + -1 * Level));
                    text += $"\nYour pinata's CD got reduced by {-2 + -1 * Level} hours";
                }
            }
            if (Data.Data.GetPetLevel(userId, 4) > 0)
            {
                if (rnd.Next(0, 5 - Level) == 0)
                {
                    DateTime yoinkDate = Data.Data.GetLastYoink(userId);
                    await Data.Data.SaveLastYoink(userId, yoinkDate.AddMinutes(-15 + -15 * Level));
                    text += $"\nYour Jew's CD got reduced by {-15 + -15 * Level} minutes";
                }
            }
            if (Data.Data.GetPetLevel(userId, 5) > 0)
            {
                if (rnd.Next(0, 5 - Level) == 0)
                {
                    DateTime rageDate = Data.Data.GetLastRage(userId);
                    await Data.Data.SaveLastRage(userId, rageDate.AddMinutes(-30 + -30 * Level));
                    text += $"\nYour Tyler's CD got reduced by {-30 + -30 * Level} minutes";
                }
            }

            return (text, true);
        }
    }

    public sealed class SlotsTokenWare : Ware
    {

        public SlotsTokenWare()
        {
            Type = VendorWare.SlotsTokens;
            Random rnd = new Random();
            Amount = rnd.Next(4, 7);
        }

        public override string GetWareDescription()
        {
            return $"Get {Amount} free uses of the slot machine";
        }

        public override string GetWareString() => GetWareStringForVariableWares($"");

        public override async Task<int> GetPriceAsync(ulong userId)
        {
            int amount = 40;

            if (Program.GetTotalPetLvl(userId) > 0)
                amount += (Program.GetTotalPetLvl(userId)) + 5 * Program.GetAveragePetLvl(userId);

            Price = amount;
            return Price;
        }

        public override async Task<(string message, bool isSuccess)> PurchaseAsync(ulong userId)
        {
            await GetPriceAsync(userId);

            var validationResult = ValidatePrice(userId, Price);
            if (!validationResult.validationSuccess)
            {
                return ($"{EnumDisplayName} cost {Price} baps, but you only have {validationResult.userBaps}", false);
            }

            await Data.Data.CreateConsumableBuffAsync(userId, BuffType.SlotTokens, (int)Amount, default);

            return ($"You successfully bought {EnumDisplayName} for {Price}. The next {Amount} slots are free <3", true);
        }
    }
}
