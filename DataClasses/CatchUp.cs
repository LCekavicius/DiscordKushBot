using System;
using System.Collections.Generic;
using System.Text;

namespace KushBot.DataClasses
{
    public class CatchUp
    {
        public ulong userId { get; set; }
        public int remainingBuffs { get; set; }

        public CatchUp(ulong userId, int remainingBuffs = 15)
        {
            this.userId = userId;
            this.remainingBuffs = remainingBuffs;
        }
    }
}
