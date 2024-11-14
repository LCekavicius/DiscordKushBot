using System.ComponentModel.DataAnnotations;

namespace KushBot.DataClasses;

public class BossParticipant
{
    [Key]
    public int Id { get; init; }
    public required ulong UserId { get; init; }
    public required int BossId { get; init; }
    public Boss Boss { get; init; }
}
