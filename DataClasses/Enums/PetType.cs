using KushBot.DataClasses.Enums;
using System.Diagnostics.Eventing.Reader;

namespace KushBot.DataClasses;

public enum PetType
{
    SuperNed = 0,
    Pinata = 1,
    Goran = 2,
    Maybich = 3,
    Jew = 4,
    TylerJuan = 5,
}

public static class PetTypeExtension
{
    public static ItemStatType ConvertToStat(this PetType type)
    {
        return (ItemStatType)((int)type + 1);
    }
}