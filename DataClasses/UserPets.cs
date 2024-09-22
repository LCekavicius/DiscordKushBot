using System.Collections.Generic;

namespace KushBot.DataClasses;

public sealed class UserPets : Dictionary<PetType, UserPet>
{
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
