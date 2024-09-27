using System.Collections.Generic;
using System.Linq;

namespace KushBot.DataClasses;

public class ModifierXQuestRequirement : QuestRequirement
{
    public override bool Validate(List<UserEvent> events)
    {
        if (int.TryParse(Value, out var value))
        {
            events.RemoveAll(e => e.Modifier < value);
            return events.Any();
        }
        else
        {
            return false;
        }
    }
}
