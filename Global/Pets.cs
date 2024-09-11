using KushBot.DataClasses;
using System;
using System.Collections.Generic;

namespace KushBot.Global;

public sealed record PetInfo(string Name, string NormalizedName, PetType Type);

public static class Pets
{
    public static PetInfo SuperNed { get; } = new PetInfo("Superned", "superned", PetType.SuperNed);
    public static PetInfo Pinata { get; } = new PetInfo("Pinata", "pinata", PetType.Pinata);
    public static PetInfo Goran { get; } = new PetInfo("Goran Jelic", "goran-jelic", PetType.Goran);
    public static PetInfo Maybich { get; } = new PetInfo("Maybich", "maybich", PetType.Maybich);
    public static PetInfo Jew { get; } = new PetInfo("Jew", "jew", PetType.Jew);
    public static PetInfo TylerJuan { get; } = new PetInfo("TylerJuan", "tylerjuan", PetType.TylerJuan);

    public static List<PetInfo> All = new List<PetInfo>() { SuperNed, Pinata, Goran, Maybich, Jew, TylerJuan };

    public static Dictionary<PetType, PetInfo> Dictionary = new Dictionary<PetType, PetInfo>()
    {
        { PetType.SuperNed, SuperNed },
        { PetType.Pinata, Pinata},
        { PetType.Goran, Goran},
        { PetType.Maybich, Maybich},
        { PetType.Jew, Jew},
        { PetType.TylerJuan, TylerJuan},
    };

    public static int GetNextFeedCost(int level)
    {
        double negate = level < 15 ? ((double)level) / 100 : 0.14;

        int BapsFed = 100;

        if (level > 1)
        {
            double _BapsFed = Math.Pow(level, 1.14 - negate) * (70 + ((level) / 1.25));
            BapsFed = (int)Math.Round(_BapsFed);
        }

        return BapsFed;
    }

    public static int GetPetTier(int dupeCount)
    {
        return (int)((1 + Math.Sqrt(1 + 8 * dupeCount)) / 2) - 1;
    }

    public static int GetNextPetTierReq(int dupes)
    {
        int tier = GetPetTier(dupes) + 1;

        return (tier * (tier + 1)) / 2;
    }
}
