using KushBot.DataClasses.enums;
using System.ComponentModel.DataAnnotations;

namespace KushBot.DataClasses.Entities;

public class SeasonParameters
{
    [Key]
    public int Id { get; init; }
    public RarityType BossProgress { get; init; }
    public ulong? RaceQuestFinisherId { get; init; }
}
