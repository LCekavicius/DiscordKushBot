using System;
using System.Collections.Generic;
using System.Linq;

namespace KushBot.DataClasses;

[Flags]
public enum BossAbilities : long
{
    None = 0,
    Regeneration = 1 << 1,
    Harden = 1 << 2,
    Paralyze = 1 << 3,
    Dismantle = 1 << 4,
    //Demoralize = 1 << 5,
    Dodge = 1 << 5,
    All = -1L
}

public static class BossAbilitiesExtensions
{
    public static List<BossAbilities> GetEnabled(this BossAbilities bossAbilities)
    {
        return Enum.GetValues(typeof(BossAbilities))
            .Cast<BossAbilities>()
            .Where(ability => ability != BossAbilities.None && bossAbilities.HasFlag(ability))
            .ToList();
    }
}
