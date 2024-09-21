using System.ComponentModel.DataAnnotations;

namespace KushBot.DataClasses.Entities;

public class QuestRequirementType
{
    [Key]
    public int Id { get; init; }
    public int QuestId { get; init; }
    public Quest Quest { get; init; }
}
