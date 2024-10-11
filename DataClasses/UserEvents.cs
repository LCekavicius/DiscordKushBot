using KushBot.DataClasses.Enums;
using System;
using System.Collections.Generic;

namespace KushBot.DataClasses;

public sealed class UserEvents : List<UserEvent>
{
    public UserEvents(IEnumerable<UserEvent> quests) : base(quests) { }
    public UserEvents() : base() { }

    public int GetChainLength(Quest quest)
    {
        int counter = 0;

        for (int i = this.Count - 1; i > 0; i--)
        {
            if (this[i].Type != quest.GetMatchingEventType())
            {
                return counter;
            }
            counter++;
        }

        return counter;

    }
}
