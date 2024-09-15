using KushBot.DataClasses.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace KushBot.DataClasses;

public sealed class UserEvent()
{
    [Key]
    public int Id { get; init; }
    public KushBotUser User { get; init; }
    public DateTime CreationTime { get; init; }
    public UserEventType Type { get; init; }
    public ulong UserId { get; init; }
}
