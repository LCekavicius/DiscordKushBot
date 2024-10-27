using KushBot.DataClasses.enums;

namespace KushBot.DataClasses;

public class ItemManager
{
    public static int ItemCap = 15;
    public static int EquipLimit = 4;

    public Item GenerateRandomItem(KushBotUser user, RarityType? rarity = null, int level = 1)
    {
        rarity ??= GetRarityByProgress(user);

        var builder = new ItemBuilder(rarity.Value)
            .WithOwner(user)
            .WithName()
            .WithLevel(level);

        int rolls = (int)rarity.Value + 1;

        for (int i = 0; i < rolls; i++)
        {
            builder.WithRandomStat();
        }

        return builder.Build();
    }

    public Item GenerateRandomItem(ulong userId, RarityType rarity, int level = 1)
    {
        var builder = new ItemBuilder(rarity)
            .WithOwner(userId)
            .WithName()
            .WithLevel(level);

        int rolls = (int)rarity + 1;

        for (int i = 0; i < rolls; i++)
        {
            builder.WithRandomStat();
        }

        return builder.Build();
    }

    public RarityType GetRarityByProgress(KushBotUser user)
    {
        var pets = user.Pets;
        if (pets is null)
        {
            pets = Data.Data.GetUserPets(user.Id);
        }

        return (RarityType)(1 + pets.TotalRawPetLevel / 60);
    }
}
