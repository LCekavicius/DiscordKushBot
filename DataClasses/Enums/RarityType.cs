using KushBot.Global;

namespace KushBot.DataClasses.enums;

public enum RarityType
{
    None,
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Archon,
}

public static class RarityTypeExtensions
{
    public static string GetRarityEmote(this RarityType rarity) =>
            rarity switch
            {
                RarityType.Uncommon => CustomEmojis.RarityUncommon,
                RarityType.Rare => CustomEmojis.RarityRare,
                RarityType.Epic => CustomEmojis.RarityEpic,
                RarityType.Legendary => CustomEmojis.RarityLegendary,
                _ => CustomEmojis.RarityCommon,
            };
}
