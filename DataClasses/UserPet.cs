using KushBot.Global;
using System.ComponentModel.DataAnnotations;

namespace KushBot.DataClasses;

public class UserPet
{
    [Key]
    public int Id { get; set; }
    public ulong UserId { get; init; }
    public PetType PetType { get; init; }
    public int Level { get; set; }
    public int Dupes { get; set; }
}
