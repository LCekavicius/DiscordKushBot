using KushBot.DataClasses.enums;
using System.ComponentModel.DataAnnotations;

namespace KushBot.DataClasses;

public class RarityFollow
{
    [Key]
    public int Id { get; init; }
    public ulong UserId { get; init; }
    public RarityType Rarity { get; init; }
}
