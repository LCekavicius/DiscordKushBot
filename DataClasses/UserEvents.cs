using KushBot.DataClasses.Enums;
using System;
using System.Collections.Generic;

namespace KushBot.DataClasses;

public sealed class UserEvents : List<UserEvent>
{
    public UserEvents(IEnumerable<UserEvent> quests) : base(quests) { }
    public UserEvents() : base() { }

    public int GetLongestSequence(UserEventType type, int threshHold = 0)
    {
        if (Count == 0)
        {
            return 0;
        }
        //TODO filter list to only related events and check last X entries
        int maxConsecutiveCount = 0;
        int currentConsecutiveCount = 0;

        for (int i = Count - 1; i >= 0; i--)
        {
            if (this[i].Type == type && this[i].Amount >= threshHold)
            {
                currentConsecutiveCount++;
                maxConsecutiveCount = Math.Max(maxConsecutiveCount, currentConsecutiveCount);
            }
            else
            {
                currentConsecutiveCount = 0;
            }
        }

        return maxConsecutiveCount;
    }
}
