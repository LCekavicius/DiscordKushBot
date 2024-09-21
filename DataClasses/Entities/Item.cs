using KushBot.Global;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace KushBot.DataClasses;

public class Item
{
    public int Id { get; set; }
    public ulong OwnerId { get; set; }
    public string Name { get; set; }
    public int BossDmg { get; set; }
    public int AirDropFlat { get; set; }
    public double AirDropPercent { get; set; }
    public int QuestSlot { get; set; }
    public int QuestBapsFlat { get; set; }
    public double QuestBapsPercent { get; set; }
    public List<ItemPetConn> ItemPetConns { get; set; }
    public int Rarity { get; set; }
    public int Level { get; set; }
    public bool IsEquipped { get; set; }
    public KushBotUser Owner { get; set; }
    [NotMapped]
    public bool IsArchon { get => Rarity == 6; }

    public Item()
    {
        ItemPetConns = new List<ItemPetConn>();
    }

    public Item(ulong ownerId, string name)
    {
        Name = name;
        OwnerId = ownerId;
        BossDmg = 0;
        AirDropFlat = 0;
        AirDropPercent = 0;
        QuestSlot = 0;
        QuestBapsFlat = 0;
        QuestBapsPercent = 0;
        Rarity = 0;
        Level = 1;

        ItemPetConns = new List<ItemPetConn>();
    }

    public Item(ulong ownerId, string name, int bossDmg, int airDropFlat, double airDropPercent,
        int questSlot, int questBapsFlat, double questBapsPercent, int rarity, int level, int id = 0)
    {
        Id = id;
        OwnerId = ownerId;
        Name = name;
        BossDmg = bossDmg;
        AirDropFlat = airDropFlat;
        AirDropPercent = airDropPercent;
        QuestSlot = questSlot;
        QuestBapsFlat = questBapsFlat;
        QuestBapsPercent = questBapsPercent;
        Rarity = rarity;
        Level = level;

        ItemPetConns = new List<ItemPetConn>();
    }

    public string GetItemDescription()
    {
        string ret = "";

        if (BossDmg != 0)
            ret += $"**+{BossDmg}** Boss damage\n";
        if (AirDropFlat != 0)
            ret += $"**+{AirDropFlat}** Air drop value\n";
        if (AirDropPercent != 0)
            ret += $"**+{AirDropPercent}%** Air drop Value\n";
        if (QuestSlot != 0)
            ret += $"**+{QuestSlot}%** Quest slot chance\n";
        if (QuestBapsFlat != 0)
            ret += $"**+{QuestBapsFlat}** Baps from quests\n";
        if (QuestBapsPercent != 0)
            ret += $"**+{QuestBapsPercent}%** Baps from quests\n";

        foreach (var item in ItemPetConns)
        {
            ret += $"**+{item.LvlBonus}** {Pets.Dictionary[item.PetType].Name} levels\n";
        }


        return ret;
    }

    public string BuildPetConQuery(int insertItemId)
    {
        if (ItemPetConns.Count == 0)
            return "";
        string ret = $"Insert into ItemPetBonus (PetId, itemId, LvlBonus) Values ";
        foreach (var item in this.ItemPetConns)
        {
            ret += $"({item.PetType}, {insertItemId}, {item.LvlBonus})";

            if (this.ItemPetConns.IndexOf(item) != this.ItemPetConns.Count - 1)
            {
                ret += ", ";
            }
        }
        return ret;
    }

    public string BuildQuery()
    {
        string ret = "Insert into Item (OwnerId, Name, BossDmg, AirDropFlat, AirDropPercent, QuestSlot, QuestBapsFlat, QuestBapsPercent, Rarity, Level)" +
            $" VALUES ('{OwnerId}','{Name}','{BossDmg}','{AirDropFlat}','{AirDropPercent}','{QuestSlot}','{QuestBapsFlat}','{QuestBapsPercent}', {Rarity}, '{Level}');";

        return ret;
    }
}

public class ItemPetConn
{
    public int Id { get; set; }
    public PetType PetType { get; set; }
    public int ItemId { get; set; }
    public int LvlBonus { get; set; }

    public ItemPetConn(PetType petType, int lvlBonus, int itemId = 0, int id = 0)
    {
        Id = id;
        PetType = petType;
        ItemId = itemId;
        LvlBonus = lvlBonus;
    }
}
