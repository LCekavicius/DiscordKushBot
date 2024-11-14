using Discord;
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
                RarityType.Common => CustomEmojis.RarityCommon,
                RarityType.Uncommon => CustomEmojis.RarityUncommon,
                RarityType.Rare => CustomEmojis.RarityRare,
                RarityType.Epic => CustomEmojis.RarityEpic,
                RarityType.Legendary => CustomEmojis.RarityLegendary,
                RarityType.Archon => CustomEmojis.RarityArchon,
                _ => ":question:"
            };

    public static Color GetRarityColor(this RarityType rarity) =>
            rarity switch
            {
                RarityType.Common => Color.LightGrey,
                RarityType.Uncommon => Color.Green,
                RarityType.Rare => Color.Blue,
                RarityType.Epic => Color.Purple,
                RarityType.Legendary => Color.Orange,
                RarityType.Archon => Color.Red,
                _ => Color.Default
            };
}
