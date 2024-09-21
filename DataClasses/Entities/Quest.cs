using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KushBot.DataClasses;

public class Quest
{
    [Key]
    public int Id { get; init; }
    public int Pos { get; init; }
    public QuestType Type { get; init; }
    public int BaseBapsReward { get; init; }

    public ulong UserId { get; init; }
    public KushBotUser User { get; init; }
    public List<QuestRequirement> Requirements { get; init; } = new();
}
