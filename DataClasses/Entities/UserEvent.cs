using KushBot.DataClasses.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace KushBot.DataClasses;

[Index(nameof(UserId), nameof(Type), nameof(CreationTime))]
public sealed class UserEvent
{
    [Key]
    public int Id { get; init; }
    public KushBotUser User { get; init; }
    public DateTime CreationTime { get; init; }
    public UserEventType Type { get; init; }
    public ulong UserId { get; init; }
    public int Amount { get; init; }
}
