using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace KushBot.DataClasses;

public class ItemStats : List<ItemStat>
{
    [NotMapped] public List<ItemStat> RecentlyAdded { get; set; } = [];
    public ItemStats() { }

    public void Add(ItemStat stat)
    {
        RecentlyAdded.Add(stat);
        var existingStat = this.FirstOrDefault(e => e.StatType == stat.StatType);
        if (existingStat != null)
        {
            existingStat.Bonus += stat.Bonus;
        }
        else
        {
            base.Add(stat);
        }
    }
}
