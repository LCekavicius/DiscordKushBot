using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KushBot.DataClasses;

public abstract class QuestRequirement
{
    [Key]
    public int Id { get; init; }
    public QuestRequirementType Type { get; init; }
    public string Value { get; init; }
    public int QuestId { get; init; }
    public Quest Quest { get; init; }

    public abstract bool Validate(List<UserEvent> events);
}
