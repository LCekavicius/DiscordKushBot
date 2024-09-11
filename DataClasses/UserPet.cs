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
    public int ItemLevel { get; init; }
    [NotMapped]
    public int CombinedLevel { get => Level + ItemLevel; }
    [NotMapped]
    public string Name { get => Global.Pets.Dictionary[PetType].Name; }
    [NotMapped]
    public Rarity Rarity { get => GetPetRarity(); }
    [NotMapped]
    public int Tier { get => Pets.GetPetTier(Dupes); }

    private Rarity GetPetRarity() =>
    this.PetType switch
    {
        PetType.SuperNed or PetType.Pinata => Rarity.Common,
        PetType.Goran or PetType.Maybich => Rarity.Rare,
        PetType.Jew or PetType.TylerJuan => Rarity.Epic,
        _ => Rarity.None
    };
}
