using Discord;
using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using KushBot.Data;
using System.Threading.Tasks;

namespace KushBot
{
    public class BossNameImageRarityDesc
    {
        public string Name { get; set; }
        public string Rarity { get; set; }
        public string Desc { get; set; }
        public string ImageUrl { get; set; }

        public BossNameImageRarityDesc(string name, string rarity, string desc, string imageUrl)
        {
            Name = name;
            Rarity = rarity;
            Desc = desc;
            ImageUrl = imageUrl;
        }
    }

    public class Boss
    {
        public string Name { get; set; }
        public int HP { get; set; }
        public int Level { get; set; }
        public string Desc { get; set; }
        public int MaxParticipants { get; set; }
        public string ImageUrl { get; set; }
        public string Rarity { get; set; }

        //public Boss(string name, string rarity, string desc, string imageUrl, int maxParti = 6)
        public Boss(BossNameImageRarityDesc Info, int maxParti = 6)
        {
            Random rnd = new Random();
            Name = Info.Name;
            Desc = Info.Desc;
            MaxParticipants = maxParti + rnd.Next(-2, 3);
            ImageUrl = Info.ImageUrl;

            Rarity = Info.Rarity;

            int cHp = 67; //75
            int ucHp = 140; //160
            int rHp = 220; //260
            int eHp = 315; //380
            int lHp = 420; //515

            if(Rarity == "Common")
            {
                Level = rnd.Next(1, 4);
                cHp += Level * 7;
                HP = cHp + rnd.Next(-1 * (cHp / 8), cHp / 8 + 1);
            }
            else if(Rarity == "Uncommon")
            {
                Level = rnd.Next(4, 7);
                ucHp += Level % 4 * 9;
                HP = ucHp + rnd.Next(-1 * (ucHp / 9), ucHp / 9 + 1);
            }
            else if (Rarity == "Rare")
            {
                Level = rnd.Next(7, 10);
                rHp += Level % 4 * 11;
                HP = rHp + rnd.Next(-1 * (rHp / 10), rHp / 10 + 1);
            }
            else if (Rarity == "Epic")
            {
                Level = rnd.Next(10, 13);
                eHp += Level % 4 * 13;
                HP = eHp + rnd.Next(-1 * (eHp / 11), eHp / 11 + 1);
            }
            else
            {
                Level = rnd.Next(13, 15);
                lHp += Level % 4 * 15;
                HP = lHp + rnd.Next(-1 * (lHp / 12), lHp / 12 + 1);
            }
        }

        public Color GetColor()
        {
            switch (Rarity)
            {
                case "Common":
                    return Color.LightGrey;
                case "Uncommon":
                    return Color.Green;
                case "Rare":
                    return Color.Blue;
                case "Epic":
                    return Color.Purple;
                default:
                    return Color.Orange;
            }
        }

        public int GetBapsReward(int participantCount)
        {
            int r = (Level - 1) / 3;
            int baps = ((700 - 70 * (MaxParticipants - participantCount)) + (r * (675 - 35 * (MaxParticipants - participantCount)))) + 150 * Level;
            baps *= 6;
            return baps;
        }

        //public int GetMaxParticipants
        //{
        //    get
        //    {
        //        Random rnd = new Random();
        //        int diversity = rnd.Next(-2, 3);
        //        return MaxParticipants + diversity;
        //    }
        //}
        //public int GetHp
        //{
        //    get
        //    {
        //        Random rnd = new Random();
        //        int diversity = rnd.Next(-1 * (HP / 10), HP / 10 + 1);
        //        return HP + diversity;
        //    }
        //}
    }
}
