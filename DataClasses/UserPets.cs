using System.Collections.Generic;
using System.Linq;

namespace KushBot.DataClasses;

public sealed class UserPets : Dictionary<PetType, UserPet>
{
    public int TotalCombinedPetLevel => this.Sum(e => e.Value.CombinedLevel);
    public int TotalRawPetLevel => this.Sum(e => e.Value.Level);
    public int TotalRawTier => this.Sum(e => e.Value.Tier);

    public UserPets() : base() { }

    public UserPets(List<UserPet> pets) : base()
    {
        foreach (var pet in pets)
        {
            this[pet.PetType] = pet;
        }
    }

    public new UserPet this[PetType key]
    {
        get
        {
            base.TryGetValue(key, out UserPet pet);
            return pet;
        }
        set => base[key] = value;
    }
}
