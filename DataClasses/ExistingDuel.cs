namespace KushBot;

public class ExistingDuel(ulong challenger, ulong challenged, int baps)
{
    public ulong Challenger { get; set; } = challenger;
    public ulong Challenged { get; set; } = challenged;
    public int Baps { get; set; } = baps;
}
