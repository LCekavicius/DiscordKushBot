using KushBot.DataClasses.enums;
using KushBot.Global;
using KushBot.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.DataClasses.Vendor;

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
}

public abstract class Ware
{
    public VendorWare Type { get; set; }
    public string DisplayName => $"{VendorService.LeftSideVendorWareEmojiMap[Type]} " +
        $"{EnumHelperV2Singleton.Instance.Helper.ToString<VendorWare>(Type)}{(Amount > 1 ? $" **({Amount})**" : "")}";
    public string EnumDisplayName => $"{EnumHelperV2Singleton.Instance.Helper.ToString<VendorWare>(Type)}{(Amount > 1 ? $" ({Amount})" : "")}";

    public double Amount { get; set; }
    public int Price { get; set; }
    public double Rate { get; set; }

    public abstract string GetWareDescription();
    public abstract string GetWareString();
    public abstract (string message, bool isSuccess) Purchase(KushBotUser user, ulong userId);
    public abstract int GetPrice(KushBotUser user);

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
        Amount = Random.Shared.Next(80, 131);
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

    public override int GetPrice(KushBotUser user) => Price;

    public override (string message, bool isSuccess) Purchase(KushBotUser user, ulong userId)
    {
        user.Cheems += (int)Amount;
        return ($"You successfully bought {Amount} cheems for {Price} baps", true);
    }
}

public sealed class ItemWare : Ware
{
    public ItemWare()
    {
        Type = VendorWare.Item;

        Rate = RollRarity();
        Price = GetPrice();
    }

    public override string GetWareDescription()
    {
        return $"Obtain a {GetRarityEmote((RarityType)Rate)} {GetRarityString((RarityType)Rate)} item. See 'kush items'";
    }
    private int RollRarity()
    {
        double val = Random.Shared.NextDouble();
        return val switch
        {
            < 0.4 => 1,
            < 0.7 => 2,
            < 0.85 => 3,
            < 0.95 => 4,
            _ => 5
        };
    }

    public override int GetPrice(KushBotUser user) => Price;

    public override string GetWareString() => GetWareStringForStaticWares($"{GetRarityEmote((RarityType)Rate)} {GetRarityString((RarityType)Rate)}");
    public override (string message, bool isSuccess) Purchase(KushBotUser user, ulong userId)
    {
        var manager = new ItemManager();
        var item = manager.GenerateRandomItem(userId, (RarityType)Rate);
        user.Items ??= new();
        user.Items.Add(item);

        return ($"You successfully bought {EnumDisplayName} of rarity {GetRarityString((RarityType)Rate)} for {Price} baps.", true);
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

    private string GetRarityEmote(RarityType rarity) =>
        rarity switch
        {
            RarityType.Uncommon => CustomEmojis.RarityUncommon,
            RarityType.Rare => CustomEmojis.RarityRare,
            RarityType.Epic => CustomEmojis.RarityEpic,
            RarityType.Legendary => CustomEmojis.RarityLegendary,
            _ => CustomEmojis.RarityCommon,
        };

    private string GetRarityString(RarityType rarity) => rarity.ToString();
}

public sealed class FoodWare : Ware
{
    public PetType PetType { get; set; }
    private int PetLvl { get; set; }

    private string PetName { get => Pets.Dictionary[PetType].Name; }

    public FoodWare(int bottomEnd, int topEnd, VendorWare type)
    {
        int priceOffsetByRarity = bottomEnd + topEnd;
        Type = type;
        PetType = (PetType)Random.Shared.Next(bottomEnd, topEnd);
        Rate = Random.Shared.Next(45 + (priceOffsetByRarity * 2), 66 + (priceOffsetByRarity * 2));
    }

    public override string GetWareDescription()
    {
        return $"Obtain a level up for {PetName}. See 'kush pets help'";
    }

    public override int GetPrice(KushBotUser user)
    {
        var pets = user.Pets;

        PetLvl = pets[PetType]?.Level ?? 1;

        int ogPrice = Pets.GetNextFeedCost(PetLvl);
        Price = (int)((Rate / 100) * (double)ogPrice);
        return Price;
    }

    public override string GetWareString() => GetWareStringForVariableWares($"{PetName}");

    public override (string message, bool isSuccess) Purchase(KushBotUser user, ulong userId)
    {
        var userPets = user.Pets;
        if (!userPets.ContainsKey(PetType))
        {
            return ($"You don't have the pet {PetName} so you cant buy food for it.", false);
        }

        int petLvl = userPets[PetType].Level;

        if (petLvl >= 99)
        {
            return ($"You can't level your pets past level 99", false);
        }

        userPets[PetType].Level += 1;

        return ($"You succesfully bought {EnumDisplayName} for {Price} baps, for the pet {PetName}", true);
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

    public override int GetPrice(KushBotUser user) => Price;

    public override (string message, bool isSuccess) Purchase(KushBotUser user, ulong userId)
    {
        if (user.Tickets >= DiscordBotService.MaxTickets)
        {
            return ($"You already have {DiscordBotService.MaxTickets}/{DiscordBotService.MaxTickets} boss tickets", false);
        }

        if (user.Tickets >= 2)
        {
            return ($"You already have {DiscordBotService.MaxTickets}/{DiscordBotService.MaxTickets} boss tickets", false);
        }


        user.Tickets += 1;
        return ($"You succesfully bought a {EnumDisplayName} for {Price} baps", true);
    }
}

public sealed class IconWare : Ware
{
    public IconWare()
    {
        Type = VendorWare.Icon;
        Rate = Random.Shared.Next(45, 66);
    }

    public override string GetWareDescription()
    {
        return $"Obtain an icon for a discounted price. See 'kush icons'";
    }

    public override string GetWareString() => GetWareStringForVariableWares();

    public override int GetPrice(KushBotUser user)
    {
        int price = 325 + user.UserPictures.Count * 25;

        double modifier = ((double)Rate) / 100;

        Price = (int)((double)price * modifier);
        return Price;
    }

    public override (string message, bool isSuccess) Purchase(KushBotUser user, ulong userId)
    {
        var ownedPictures = user.UserPictures;
        if (ownedPictures.Count >= DiscordBotService.PictureCount)
        {
            return ($"You already have all {DiscordBotService.PictureCount} icons", false);
        }

        var pictureRange = Enumerable.Range(1, DiscordBotService.PictureCount).Select(e => $"{e}.jpg");
        var available = pictureRange.Except(user.UserPictures.Select(e => e.Path)).ToList();

        var chosen = available[Random.Shared.Next(0, available.Count)];

        var chosenPicture = new UserPicture(user.Id, chosen);

        user.UserPictures.Add(chosenPicture);

        return ($"You successfully purchased the icon #{System.IO.Path.GetFileNameWithoutExtension(chosen)} for {Price} baps", true);
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

    public override int GetPrice(KushBotUser user) => Price;

    public override (string message, bool isSuccess) Purchase(KushBotUser user, ulong userId)
    {
        user.LastNyaClaim = DateTime.MinValue;
        user.RedeemDate = DateTime.MinValue;
        user.YikeDate = DateTime.MinValue;
        user.LastNyaClaim = DateTime.MinValue;
        return ($"You bought {EnumDisplayName}, your yike, redeem and nya marry CDs have been reset", true);
    }
}

public sealed class EggWare : Ware
{
    public EggWare()
    {
        Type = VendorWare.Egg;
        Price = Random.Shared.Next(125, 225);
    }

    public override string GetWareDescription()
    {
        return $"Obtain an egg for a discounted price. See 'kush pets'";
    }

    public override string GetWareString() => GetWareStringForStaticWares("");

    public override int GetPrice(KushBotUser user) => Price;

    public override (string message, bool isSuccess) Purchase(KushBotUser user, ulong userId)
    {
        user.Eggs += 1;
        return ($"You bought an egg for {Price} baps", true);
    }
}

public sealed class PetDupeWare : Ware
{
    public PetType PetType { get; set; }

    private string PetName { get => Pets.Dictionary[PetType].Name; }

    public PetDupeWare(int bottomEnd, int topEnd, VendorWare type)
    {
        int priceOffsetByRarity = bottomEnd + topEnd;
        Type = type;
        Price = Random.Shared.Next(350 + (priceOffsetByRarity * 5), 451 + (priceOffsetByRarity * 7));
        PetType = (PetType)Random.Shared.Next(bottomEnd, topEnd);
    }

    public override string GetWareDescription()
    {
        return $"Obtain a dupe for **{PetName}**. See 'kush pets help'";
    }

    public override string GetWareString() => GetWareStringForStaticWares(PetName);

    public override int GetPrice(KushBotUser user) => Price;

    public override (string message, bool isSuccess) Purchase(KushBotUser user, ulong userId)
    {
        var pets = user.Pets;

        if (!pets.ContainsKey(PetType))
        {
            return ($"You dont have that pet so you cant buy {EnumDisplayName} for {PetName}", false);
        }

        pets[PetType].Dupes += 1;

        return ($"You successfully bought {EnumDisplayName} for {PetName}", true);
    }
}

public sealed class PlotBoostWare : Ware
{
    public PlotBoostWare()
    {
        Type = VendorWare.PlotBoost;
        Amount = Random.Shared.Next(80, 141);
        Rate = Random.Shared.Next(15, 26);
    }

    public override string GetWareDescription()
    {
        return $"Obtain a plot boost, this will speed up your plots by {Amount} minutes, (but increase duration of currently abused pets in abuse chamber).";
    }

    public override string GetWareString() => GetWareStringForVariableWares("Denominated in minutes");

    public override int GetPrice(KushBotUser user)
    {
        var plots = user.UserPlots;
        Price = 0;
        foreach (var item in plots)
        {
            Price += (int)(Rate * item.Level);
        }

        return Price;
    }

    public override (string message, bool isSuccess) Purchase(KushBotUser user, ulong userId)
    {
        if (user.UserPlots.Count == 0)
        {
            return ("Maybe you should buy it when you have a plot 'kush plots help'", false);
        }

        var manager = new PlotsManager(user);

        manager.ShiftTime((int)Amount);
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
        Level = Random.Shared.Next(1, 4);
        Duration = Random.Shared.Next(6, 11);
        Price = Level * 100;
    }

    public override string GetWareDescription()
    {
        return $"Obtain a level **{Level}** duration **{Duration}** {EnumDisplayName} buff. This gives a chance (equal to 2% * Level) to get double baps while gambling.";
    }

    public override string GetWareString() => GetWareStringForStaticWares($" Level: **{Level}**\nDuration: **{Duration}**");

    public override int GetPrice(KushBotUser user) => Price;

    public override (string message, bool isSuccess) Purchase(KushBotUser user, ulong userId)
    {
        if (user.UserBuffs.Count >= 15)
        {
            return ($"You already have 15 buffs and can not have more", false);
        }

        ConsumableBuff buff = new()
        {
            Type = BuffType.KushGym,
            Duration = Duration,
            TotalDuration = Duration,
            OwnerId = userId,
            Potency = Level * 2,
        };

        user.UserBuffs.Add(buff);

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
        Level = Random.Shared.Next(1, 4);
        Duration = Random.Shared.Next(6, 11);
        Price = Level * 100;
    }

    public override string GetWareDescription()
    {
        return $"Obtain a level **{Level}** duration **{Duration}** {EnumDisplayName} buff. This gives a chance (equal to 2% * Level) to prevent the loss of baps when gambling.";
    }

    public override string GetWareString() => GetWareStringForStaticWares($" Level: **{Level}**\nDuration: **{Duration}**");

    public override int GetPrice(KushBotUser user)
    {
        return Price;
    }

    public override (string message, bool isSuccess) Purchase(KushBotUser user, ulong userId)
    {
        if (user.UserBuffs.Count >= 15)
        {
            return ($"You already have 15 buffs and can not have more", false);
        }

        ConsumableBuff buff = new()
        {
            Type = BuffType.KushGym,
            Duration = Duration,
            TotalDuration = Duration,
            OwnerId = userId,
            Potency = Level * 2,
        };

        user.UserBuffs.Add(buff);

        return ($"You succesfully bought the weed effect {EnumDisplayName} (Lvl {Level}, Duration: {Duration}) for {Price} baps", true);
    }
}

public sealed class ParasiteWare : Ware
{
    public Infection Infection { get; set; }

    public ParasiteWare()
    {
        Type = VendorWare.Parasite;
        List<InfectionState> states = new List<InfectionState>() { InfectionState.Egg, InfectionState.Hatchling, InfectionState.Juvenile, InfectionState.Tyrant };
        InfectionState state = states.OrderBy(e => Random.Shared.NextDouble()).FirstOrDefault();

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

    public override int GetPrice(KushBotUser user) => Price;

    public override (string message, bool isSuccess) Purchase(KushBotUser user, ulong userId)
    {
        //TODO fix
        var infections = user.UserInfections;

        if (infections.Count >= 8)
        {
            return ($"You already have 8 parasites and cant have more", false);
        }

        Infection clone = Infection.CloneObject() as Infection;
        clone.OwnerId = userId;
        clone.Id = Guid.NewGuid();
        user.UserInfections.Add(new()
        {
            CreationDate = TimeHelper.Now,
            KillAttemptDate = DateTime.MinValue,
            OwnerId = user.Id,
        });

        return ($"You successfully bought {EnumHelperV2Singleton.Instance.Helper.ToString<InfectionState>(Infection.State)} tier parasite for {Price} baps", true);
    }
}

public sealed class ArtilleryWare : Ware
{
    public ArtilleryWare()
    {
        Type = VendorWare.Artillery;
        Price = 100;
        Amount = Random.Shared.Next(3, 6);
    }

    public override string GetWareDescription()
    {
        return $"Get +{Amount} damage for the next boss fight (lost upon boss fight)";
    }

    public override string GetWareString() => GetWareStringForStaticWares($"Extra {Amount} boss dmg next fight");

    public override int GetPrice(KushBotUser user) => Price;
    public override (string message, bool isSuccess) Purchase(KushBotUser user, ulong userId)
    {
        if (user.UserBuffs.Count >= 15)
        {
            return ($"You already have 15 buffs and can not have more", false);
        }

        if (user.UserBuffs.Any(e => e.Type == BuffType.BossArtillery))
        {
            return ($"You already own {EnumDisplayName} and cant carry another one", false);
        }

        ConsumableBuff buff = new()
        {
            Type = BuffType.BossArtillery,
            Duration = 1,
            TotalDuration = 1,
            OwnerId = userId,
            Potency = Amount,
        };

        user.UserBuffs.Add(buff);

        return ($"You successfully bought {EnumDisplayName} for {Price} baps", true);
    }
}

public sealed class AdderalWare : Ware
{
    public int Level { get; set; }

    public AdderalWare()
    {
        Type = VendorWare.Adderal;
        Level = Random.Shared.Next(1, 4);
        Price = Level * 75;
    }

    public override string GetWareDescription()
    {
        return $"Consume some adderal and get a reset of your cooldowns (beg is certain, everything else comes with a chance based off of the level)";
    }

    public override string GetWareString() => GetWareStringForStaticWares($"Level: **{Level}**");

    public override int GetPrice(KushBotUser user) => Price;

    public override (string message, bool isSuccess) Purchase(KushBotUser user, ulong userId)
    {
        string text = $"You successfully bought Level {Level} adderal.\nYour beg CD got reset";

        user.LastBeg = user.LastBeg.AddHours(-2);

        if (user.Pets.ContainsKey(PetType.Pinata))
        {
            if (Random.Shared.Next(0, 5 - Level) == 0)
            {
                user.LastDestroy = user.LastDestroy.AddHours(-2 + -1 * Level);
                text += $"\nYour pinata's CD got reduced by {-2 + -1 * Level} hours";
            }
        }
        if (user.Pets.ContainsKey(PetType.Jew))
        {
            if (Random.Shared.Next(0, 5 - Level) == 0)
            {
                user.LastYoink = user.LastYoink.AddMinutes(-15 + -15 * Level);
                text += $"\nYour Jew's CD got reduced by {-15 + -15 * Level} minutes";
            }
        }
        if (user.Pets.ContainsKey(PetType.TylerJuan))
        {
            if (Random.Shared.Next(0, 5 - Level) == 0)
            {
                user.LastTylerRage = user.LastTylerRage.AddMinutes(-30 + -30 * Level);
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
        Amount = Random.Shared.Next(4, 7);
    }

    public override string GetWareDescription()
    {
        return $"Get {Amount} free uses of the slot machine";
    }

    public override string GetWareString() => GetWareStringForVariableWares($"");

    public override int GetPrice(KushBotUser user)
    {
        int amount = 40;

        var tpl = user.Pets.TotalCombinedPetLevel;

        if (tpl > 0)
        {
            var apl = user.Pets.TotalCombinedPetLevel / user.Pets.Count;
            amount += tpl + (5 * apl);
        }

        Price = amount;
        return Price;
    }

    public override (string message, bool isSuccess) Purchase(KushBotUser user, ulong userId)
    {
        if (user.UserBuffs.Count >= 15)
        {
            return ($"You already have 15 buffs and can not have more", false);
        }

        ConsumableBuff buff = new()
        {
            Type = BuffType.SlotTokens,
            Duration = (int)Amount,
            TotalDuration = (int)Amount,
            OwnerId = userId,
            Potency = default,
        };

        user.UserBuffs.Add(buff);

        return ($"You successfully bought {EnumDisplayName} for {Price}. The next {Amount} slots are free <3", true);
    }
}
