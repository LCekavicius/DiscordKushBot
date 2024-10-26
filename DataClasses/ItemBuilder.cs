using KushBot.DataClasses.enums;
using System.IO;
using System;
using KushBot.DataClasses.Enums;

namespace KushBot.DataClasses;

public class ItemBuilder
{
    private const string ItemsPath = "Data/Items";
    private const string ArchonItemsPath = "Data/ArchonItems";

    private Item Item { get; set; }

    public ItemBuilder(RarityType rarity)
    {
        Item = new Item();
        Item.Rarity = rarity;
    }

    public ItemBuilder(Item item)
    {
        Item = item;
    }

    public ItemBuilder WithName(string name = null)
    {
        if (name == null)
        {
            var directory = Item.Rarity == RarityType.Archon ? ArchonItemsPath : ItemsPath;
            var files = Directory.GetFiles(directory); // Maybe utilize enumeration to lazily load this instead of reading it into the memory every time?
            var file = files[Random.Shared.Next(0, files.Length)];

            name = Path.GetFileNameWithoutExtension(file);
        }

        Item.Name = name;
        return this;
    }

    public ItemBuilder WithOwner(ulong ownerId)
    {
        Item.OwnerId = ownerId;
        return this;
    }

    public ItemBuilder WithOwner(KushBotUser owner)
    {
        Item.Owner = owner;
        return this;
    }

    public ItemBuilder WithLevel(int level)
    {
        Item.Level = level;
        return this;
    }

    public ItemBuilder WithRandomStat()
    {
        var values = Enum.GetValues<ItemStatType>();
        var stat = values[Random.Shared.Next(1, values.Length)];
        return ApplyStat(stat);
    }

    private ItemBuilder ApplyStat(ItemStatType stat) => stat switch
    {
        ItemStatType when (int)stat < 7 => WithPetLevel(stat.ConvertToPetType()),
        ItemStatType.BossDmg => WithBossDmg(),
        ItemStatType.AirDropFlat => WithAirDropFlat(),
        ItemStatType.AirDropPercent => WithAirDropPercent(),
        ItemStatType.QuestBapsFlat => WithQuestBapsFlat(),
        ItemStatType.QuestBapsPercent => WithQuestBapsPercent(),
        ItemStatType.QuestSlotChance => WithQuestSlotChance(),
        _ => throw new Exception($"Stat {stat.ToString()} unsupported"),
    };

    public ItemBuilder WithPetLevel(PetType petType)
    {
        var statType = petType.ConvertToStat();
        int min = 1 + (int)Item.Rarity / 4;
        int max = 4 + (int)Item.Rarity / 3;

        Item.ItemStats.Add(new()
        {
            Item = Item,
            StatType = statType,
            Bonus = Random.Shared.Next(min, max)
        });
        return this;
    }

    public ItemBuilder WithBossDmg()
    {
        int min = 2 + (int)Item.Rarity / 4;
        int max = 5 + (int)Item.Rarity / 3;

        Item.ItemStats.Add(new()
        {
            Item = Item,
            StatType = ItemStatType.BossDmg,
            Bonus = Random.Shared.Next(min, max)
        });
        return this;
    }

    public ItemBuilder WithAirDropFlat()
    {
        int bonus = 0;
        for (int i = 0; i < (int)Item.Rarity; i++)
        {
            bonus += Random.Shared.Next(25 / (i + 1), 45 / (i + 1));
        }

        Item.ItemStats.Add(new()
        {
            Item = Item,
            StatType = ItemStatType.AirDropFlat,
            Bonus = bonus,
        });

        return this;
    }

    public ItemBuilder WithAirDropPercent()
    {
        int bonus = 0;
        for (int i = 0; i < (int)Item.Rarity; i++)
        {
            bonus += Random.Shared.Next(10 / (i + 1), 20 / (i + 1));
        }

        Item.ItemStats.Add(new()
        {
            Item = Item,
            StatType = ItemStatType.AirDropPercent,
            Bonus = bonus,
        });

        return this;
    }

    public ItemBuilder WithQuestSlotChance()
    {
        int bonus = 0;
        for (int i = 0; i < (int)Item.Rarity; i++)
        {
            bonus += Random.Shared.Next(15 / (i + 1), 25 / (i + 1));
        }

        Item.ItemStats.Add(new()
        {
            Item = Item,
            StatType = ItemStatType.QuestSlotChance,
            Bonus = bonus,
        });

        return this;
    }

    public ItemBuilder WithQuestBapsFlat()
    {
        int bonus = 0;
        for (int i = 0; i < (int)Item.Rarity; i++)
        {
            bonus += Random.Shared.Next(25 / (i + 1), 45 / (i + 1));
        }

        Item.ItemStats.Add(new()
        {
            Item = Item,
            StatType = ItemStatType.QuestBapsFlat,
            Bonus = bonus,
        });

        return this;
    }

    public ItemBuilder WithQuestBapsPercent()
    {
        int bonus = 0;
        for (int i = 0; i < (int)Item.Rarity; i++)
        {
            bonus += Random.Shared.Next(10 / (i + 1), 20 / (i + 1));
        }

        Item.ItemStats.Add(new()
        {
            Item = Item,
            StatType = ItemStatType.QuestBapsPercent,
            Bonus = bonus,
        });

        return this;
    }

    public Item Build()
    {
        return Item;
    }
}