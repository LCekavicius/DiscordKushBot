using System;
using System.Collections.Generic;
using System.Text;

namespace KushBot
{
    public class ExistingDuel
    {
        public ulong Challenger { get; set; }
        public ulong Challenged { get; set; }
        public int Baps { get; set; }

        public ExistingDuel(ulong challenger, ulong challenged, int baps)
        {
            Challenger = challenger;
            Challenged = challenged;
            Baps = baps;
        }

    }
}
