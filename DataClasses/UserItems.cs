using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace KushBot.DataClasses;

public sealed class UserItems : List<Item>
{
    [NotMapped] public UserItems Equipped { get => new UserItems(this.Where(e => e.IsEquipped).ToList()); }
    [NotMapped] public double AirDropFlatSum { get => Equipped?.Sum(e => e.AirDropFlat) ?? default; }
    [NotMapped] public double AirDropPercentSum { get => Equipped?.Sum(e => e.AirDropPercent) ?? default; }
    [NotMapped] public double QuestSlotSum { get => Equipped?.Sum(e => e.QuestSlot) ?? default; }
    [NotMapped] public double QuestBapsFlatSum { get => Equipped?.Sum(e => e.QuestBapsFlat) ?? default; }
    [NotMapped] public double QuestBapsPercentSum { get => Equipped?.Sum(e => e.QuestBapsPercent) ?? default; }
    [NotMapped] public double BossDmgSum { get => Equipped?.Sum(e => e.BossDmg) ?? default; }

    public UserItems() { }

    public UserItems(List<Item> items) : base(items) { }

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
