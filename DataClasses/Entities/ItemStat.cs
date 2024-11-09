using KushBot.DataClasses.Enums;
using KushBot.Global;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KushBot.DataClasses;

public class ItemStat
{
    [Key]
    public int Id { get; set; }
    public int ItemId { get; set; }
    public ItemStatType StatType { get; set; }
    public double Bonus { get; set; }
    public Item Item {  get; set; }
    [NotMapped]
    public PetType? PetType { get => GetPetType(); }


    private PetType? GetPetType()
    {
        int statNumber = (int)StatType;
        if (statNumber > 7)
        {
            return null;
        }
        else
        {
            return (PetType)(statNumber - 1);
        }
    }

    public string GetStatDescription() =>
        StatType switch
        {
            ItemStatType.BossDmg => $"**+{Bonus}** Boss damage",
            ItemStatType.AirDropFlat => $"**+{Bonus}** Air drop value",
            ItemStatType.AirDropPercent => $"**+{Bonus}%** Air drop Value",
            ItemStatType.QuestSlotChance => $"**+{Bonus}%** Quest slot chance",
            ItemStatType.QuestBapsFlat => $"**+{Bonus}** Baps from quests",
            ItemStatType.QuestBapsPercent => $"**+{Bonus}%** Baps from quests",
            _ => PetType is null ? "" : $"**+{Bonus}** {Pets.Dictionary[PetType.Value].Name} levels",
        };
}
