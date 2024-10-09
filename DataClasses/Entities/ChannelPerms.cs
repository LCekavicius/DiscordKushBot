using System.ComponentModel.DataAnnotations;

namespace KushBot.DataClasses;

public class ChannelPerms
{
    [Key]
    public ulong Id { get; init; }
    public bool PermitsAirDrop { get; init; }
    public bool PermitsCore { get; init; } // Gamble, give etc
    public bool PermitsMisc { get; init; } // Help, search, top etc
    public bool PermitsNya { get; init; }
    public bool PermitsBoss { get; init; }
    public bool PermitsVendor { get; init; }
}
