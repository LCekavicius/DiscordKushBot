using System;
using System.Collections.Generic;
using System.Text;

namespace KushBot.DataClasses
{
    public class RarityFollow
    {
        public int Id { get; set; }
        public ulong fk_UserId { get; set; }
        public string Rarity { get; set; }

        public RarityFollow(ulong fk_UserId, string rarity)
        {
            this.Id = 0;
            this.fk_UserId = fk_UserId;
            Rarity = rarity;
        }

    }

}
