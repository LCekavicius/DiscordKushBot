using System.Collections.Generic;
using System.Linq;

namespace KushBot.DataClasses.Entities;

public class ItemStats : List<ItemStat>
{
    public ItemStats() { }

    public void Add(ItemStat stat)
    {
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
