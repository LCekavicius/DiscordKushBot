using System.Collections.Generic;
using System.Linq;

namespace KushBot.DataClasses;

public class BapsXQuestRequirement : QuestRequirement
{
    public override bool Validate(List<UserEvent> events)
    {
        if (int.TryParse(Value, out var value))
        {
            events.RemoveAll(e => e.BapsInput < value);
            return events.Any();
        }
        else
        {
            return false;
        }
    }
}
