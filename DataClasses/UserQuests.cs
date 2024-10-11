using System.Collections.Generic;
using System.Linq;

namespace KushBot.DataClasses;

public sealed class UserQuests : List<Quest>
{
    public UserQuests InProgress { get => new UserQuests(this.Where(e => !e.IsCompleted)); }
    public UserQuests(IEnumerable<Quest> quests) : base(quests) { }
    public UserQuests() : base() { }
}
