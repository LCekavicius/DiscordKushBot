using System.Collections.Generic;
using System.Linq;

namespace KushBot.DataClasses;

public class LoseQuestRequirement : QuestRequirement
{
    public override bool Validate(List<UserEvent> events)
    {
        if (int.TryParse(Value, out var value))
        {
            return events.Sum(e => (long)e.BapsChange) >= value;
        }
        else
        {
            return false;
        }
    }
}
