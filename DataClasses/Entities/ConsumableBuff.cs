using KushBot.Services;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KushBot.DataClasses;

public enum BuffType
{
    [Description("Boss artillery")]
    BossArtillery,
    [Description("Kush gym")]
    KushGym,
    [Description("Fishing rod")]
    FishingRod,
    TylerRage,
    [Description("Slot tokens")]
    SlotTokens,
}

public class ConsumableBuff
{
    [Key]
    public Guid Id { get; set; }
    public BuffType Type { get; set; }
    public ulong OwnerId { get; set; }
    public int Duration { get; set; }
    public double Potency { get; set; }
    public int TotalDuration { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public KushBotUser Owner { get; set; }

    [NotMapped]
    public string DisplayName => $"{EnumHelperV2Singleton.Instance.Helper.ToString<BuffType>(Type)} {GetEmojiByType()}";

    public void ReduceDuration()
    {
        Duration -= 1;
    }

    private string GetEmojiByType() =>
        Type switch
        {
            BuffType.BossArtillery => VendorService.LeftSideVendorWareEmojiMap[Vendor.VendorWare.Artillery],
            BuffType.KushGym => VendorService.LeftSideVendorWareEmojiMap[Vendor.VendorWare.KushGym],
            BuffType.FishingRod => VendorService.LeftSideVendorWareEmojiMap[Vendor.VendorWare.FishingRod],
            BuffType.SlotTokens => VendorService.LeftSideVendorWareEmojiMap[Vendor.VendorWare.SlotsTokens],
            BuffType.TylerRage => "<:fear:1231718238031712316>",
            _ => ""
        };

    public string GetDescriptionByType() =>
        Type switch
        {
            BuffType.BossArtillery => $"+{Potency} extra damage for next boss fight",
            BuffType.KushGym => $"Level: **{(int)Potency / 2}**\nRemaining duration: **{Duration}**\n**{((int)Potency)}%** Chance to gain double baps when gambling",
            BuffType.FishingRod => $"Level: **{(int)Potency / 2}**\nRemaining duration: **{Duration}**\n**{((int)Potency)}%** Chance to not lose baps when gambling",
            BuffType.TylerRage => "Generate rage baps, paid out when the rage ends",
            BuffType.SlotTokens => $"Next **{Duration}** slots are free",
            _ => ""
        };
}
