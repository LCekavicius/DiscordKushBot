using System;

namespace KushBot.DataClasses.Enums;

public enum ItemStatType
{
    None,
    SupernedLevel,
    PinataLevel,
    GoranLevel,
    MaybichLevel,
    JewLevel,
    TylerLevel,
    BossDmg,
    AirDropFlat,
    AirDropPercent,
    QuestSlotChance,
    QuestBapsFlat,
    QuestBapsPercent,
}

public static class ItemStatTypeExtension
{
    public static PetType ConvertToPetType(this ItemStatType type)
    {
        int value = (int)type;
        if (value < 1 || value > 6)
        {
            throw new ArgumentException($"Attempting to convert stat {type} to a pet type failed");
        }

        return (PetType)((int)type - 1);
    }
}