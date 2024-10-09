using KushBot.DataClasses.enums;
using System.ComponentModel.DataAnnotations;

namespace KushBot.DataClasses.Entities;

public class Configuration
{
    [Key]
    public ulong GuildId { get; init; }
    public RarityType HighestRarityBossKilled { get; init; }


}
