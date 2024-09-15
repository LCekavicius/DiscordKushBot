using KushBot.DataClasses.enums;
using KushBot.Global;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KushBot.DataClasses;

public class UserPet
{
    [Key]
    public int Id { get; set; }
    public ulong UserId { get; init; }
    public PetType PetType { get; init; }
    public int Level { get; set; }
    public int Dupes { get; set; }
    public KushBotUser User { get; set; }


    [NotMapped]
    public int ItemLevel { get; set; }
    [NotMapped]
    public int CombinedLevel { get => Level + ItemLevel; }
    [NotMapped]
    public string Name { get => Global.Pets.Dictionary[PetType].Name; }
    [NotMapped]
    public RarityType Rarity { get => GetPetRarity(); }
    [NotMapped]
    public int Tier { get => Pets.GetPetTier(Dupes); }

    private RarityType GetPetRarity() =>
        this.PetType switch
        {
            PetType.SuperNed or PetType.Pinata => RarityType.Common,
            PetType.Goran or PetType.Maybich => RarityType.Rare,
            PetType.Jew or PetType.TylerJuan => RarityType.Epic,
            _ => RarityType.None
        };
}
