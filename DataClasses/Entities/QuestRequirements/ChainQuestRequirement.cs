using System.Collections.Generic;
using System.Linq;

namespace KushBot.DataClasses;

public class ChainQuestRequirement : QuestRequirement
{
    public override bool Validate(List<UserEvent> events)
    {
        if (int.TryParse(Value, out var value))
        {
            if (events.Count < value)
            {
                return false;
            }

            return events.Skip(events.Count - value)
                         .All(e => e.Type == Quest.GetMatchingEventType());
        }
        else
        {
            return false;
        }
    }
}
