using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace KushBot.DataClasses;

public class UserBuffs : List<ConsumableBuff>
{
    [NotMapped] public List<ConsumableBuff> Depleted { get => this.Where(e => e.Duration <= 0).ToList(); }
    [NotMapped] public List<ConsumableBuff> NotDepleted { get => this.Where(e => e.Duration > 0).ToList(); }

    public ConsumableBuff Get(BuffType type)
    {
        return this.Where(e => e.Type == type).OrderByDescending(e => e.Potency).FirstOrDefault();
    }
}
