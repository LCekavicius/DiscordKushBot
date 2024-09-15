using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace KushBot.DataClasses;

public sealed class UserItems : List<Item>
{
    [NotMapped] public UserItems Equipped { get => this.Where(e => e.IsEquipped).ToList() as UserItems; }
    [NotMapped] public double AirDropFlatSum { get => Equipped.Sum(e => e.AirDropFlat); }
    [NotMapped] public double AirDropPercentSum { get => Equipped.Sum(e => e.AirDropPercent); }
    [NotMapped] public double QuestSlotSum { get => Equipped.Sum(e => e.QuestSlot); }
    [NotMapped] public double QuestBapsFlatSum { get => Equipped.Sum(e => e.QuestBapsFlat); }
    [NotMapped] public double QuestBapsPercentSum { get => Equipped.Sum(e => e.QuestBapsPercent); }
    [NotMapped] public double BossDmgSum { get => Equipped.Sum(e => e.BossDmg); }

    public int GetPetLevelBoost(PetType petType)
    {
        return Equipped
            .SelectMany(e => e.ItemPetConns)
            .Where(e => e.PetType == petType)
            .Sum(e => e.LvlBonus);
    }

    public List<Item> GetItemsByString(string input)
    {
        return this.Where(e => e.Id.ToString() == input || e.Name.Equals(input, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}
