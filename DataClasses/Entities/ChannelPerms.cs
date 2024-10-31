using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KushBot.DataClasses;

[Flags]
public enum Permissions : long
{
    None = 0,
    Airdrop = 1 << 1,
    Core = 1 << 2,
    Misc = 1 << 3,
    Nya = 1 << 4,
    Boss = 1 << 5,
    Vendor = 1 << 6,
    All = -1L
}

public class ChannelPerms
{
    [Key]
    public ulong Id { get; init; }
    public long PermissionsValue { get; private set; }

    [NotMapped]
    public Permissions Permissions
    {
        get => (Permissions)PermissionsValue;
        set => PermissionsValue = (long)value;
    }
}
