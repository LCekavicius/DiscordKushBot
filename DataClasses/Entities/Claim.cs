using System;
using System.ComponentModel.DataAnnotations;

namespace KushBot.DataClasses;

public class NyaClaim
{
    [Key]
    public int Id { get; set; }
    public ulong OwnerId { get; set; }
    public string Url { get; set; }
    public string FileName { get; set; }
    public int Keys { get; set; }
    public int SortIndex { get; set; }
    public DateTime ClaimDate { get; set; }
    public KushBotUser Owner { get; set; }
}
