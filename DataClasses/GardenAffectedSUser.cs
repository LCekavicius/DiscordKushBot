using System;
using System.Collections.Generic;
using System.Text;

namespace KushBot
{
    class GardenAffectedSUser
    {
        public GardenAffectedSUser(ulong userId, string effect, int duration, int level)
        {
            UserId = userId;
            Effect = effect;
            Duration = duration;
            Level = 2 + level;
        }

        public GardenAffectedSUser()
        {
            UserId = 0;
            Effect = "";
            Duration = 1;
            Level = 1;
        }

        public int GetEffictivines()
        {
            return (int)Level * 2;
        }

        public ulong UserId { get; set; }
        public string Effect { get; set; }
        public int Duration { get; set; }
        public double Level { get; set; }
    }
}
