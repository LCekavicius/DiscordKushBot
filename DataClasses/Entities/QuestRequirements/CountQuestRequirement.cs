using System.Collections.Generic;

namespace KushBot.DataClasses;

public class CountQuestRequirement : QuestRequirement
{
    public override bool Validate(List<UserEvent> events)
    {
        if (int.TryParse(Value, out var value))
        {
            return events.Count >= value;
        }
        else
        {
            return false;
        }

    }
}