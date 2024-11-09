using KushBot.DataClasses.enums;
using System.ComponentModel.DataAnnotations;

namespace KushBot.DataClasses;

public class SeasonParameters
{
    [Key]
    public int Id { get; init; }
    public RarityType BossProgress { get; init; } = RarityType.Common;
    public ulong BlueRoleId { get; init; }
    public ulong OrangeRoleId { get; init; }
    public ulong MutedRoleId { get; init; }
}
