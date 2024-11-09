using KushBot.DataClasses.enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace KushBot.DataClasses;

public class Item
{
    public int Id { get; set; }
    public ulong OwnerId { get; set; }
    public string Name { get; set; }
    public ItemStats ItemStats { get; set; }
    public RarityType Rarity { get; set; }
    public int Level { get; set; }
    public bool IsEquipped { get; set; }
    public KushBotUser Owner { get; set; }

    [NotMapped] public string FilePath { get => $"Data/{(Rarity == RarityType.Archon ? "ArchonItems" : "Items")}/{Name}.png"; }

    public Item()
    {
        ItemStats = new();
    }

    public Item(ulong ownerId, string name)
    {
        Name = name;
        OwnerId = ownerId;
        Rarity = 0;
        Level = 1;

        ItemStats = new();
    }

    public string GetItemDescription()
    {
        return string.Join("\n", ItemStats.Select(e => e.GetStatDescription()));
    }
}
