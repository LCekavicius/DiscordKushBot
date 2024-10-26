using KushBot.DataClasses.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace KushBot.DataClasses;

public sealed class UserItems : List<Item>
{
    [NotMapped] public UserItems Equipped { get => new UserItems(this.Where(e => e.IsEquipped).ToList()); }

    public UserItems() { }

    public UserItems(List<Item> items) : base(items) { }

    public double GetStatTypeBonus(ItemStatType statType)
    {
        return Equipped
            .SelectMany(e => e.ItemStats)
            .Where(e => e.StatType == statType)
            .Sum(e => e.Bonus);
    }

    public List<Item> GetItemsByString(string input)
    {
        return this.Where(e => e.Id.ToString() == input || e.Name.Equals(input, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public new Item this[int index]
    {
        get
        {
            if (index < 0 || index >= base.Count)
            {
                return null;
            }
            else
            {
                return base[index];
            }
        }
        set => base[index] = value;
    }
}
