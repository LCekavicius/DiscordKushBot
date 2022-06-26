using System;
using System.Collections.Generic;
using System.Text;

namespace KushBot
{
    public class CursedPlayer
    {
        public ulong ID { get; set; }
        public string CurseName { get; set; }
        public int Duration { get; set; }

        public List<string> lastMessages { get; set; }

        public CursedPlayer(ulong iD, string curseName, int duration)
        {
            ID = iD;
            CurseName = curseName;
            Duration = duration;
            lastMessages = new List<string>();
        }
    }
}
