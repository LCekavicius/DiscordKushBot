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

    public class ArchonAbility
    {
        public string Name { get; set; }
        public double Effect { get; set; }
        public bool Prevent { get; set; }

        public ArchonAbility(string rolledAbility)
        {
            Random rnd = new();
            Name = rolledAbility;
            if (Name == "Regeneration" || Name == "Toughen hide" || Name == "Dodge")
            {
                Effect = rnd.Next(14, 24);
            }
            else
            {
                Effect = 100;
            }
        }

        public override string ToString()
        {
            return $"{Program.ArchonAbilityEmoji[Name]} {Name} {(Effect == 100 ? "" : $"{Effect}%")}";
        }
    }

    public class BossDetails
    {
        public string Name { get; set; }
        public string Rarity { get; set; }
        public string Desc { get; set; }
        public string ImageUrl { get; set; }

        public BossDetails(string name, string rarity, string desc, string imageUrl)
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
        public bool IsArchon { get; set; } = false;
        public List<ArchonAbility> ArchonAbilities { get; set; } = new();

        public Boss(BossDetails Info, int maxParti = 6, bool isArchon = false)
        {
            IsArchon = isArchon;
            Random rnd = new Random();
            Name = Info.Name;
            Desc = Info.Desc;
            ImageUrl = Info.ImageUrl;
            MaxParticipants = maxParti + rnd.Next(IsArchon ? 0 : -2, IsArchon ? 5 : 3);
            Rarity = Info.Rarity;

            if (IsArchon)
            {
                Rarity = "Epic";
                IEnumerable<string> rolledAbilities = Program.ArchonAbilityList.OrderBy(e => rnd.Next()).Take(2);
                foreach (var item in rolledAbilities)
                {
                    ArchonAbilities.Add(new ArchonAbility(item));
                }
            }


            int cHp = 67; //75
            int ucHp = 140; //160
            int rHp = 220; //260
            int eHp = 315; //380
            int lHp = 420; //515


            if (Rarity == "Common")
            {
                Level = rnd.Next(1, 4);
                cHp += Level * 7;
                HP = cHp + rnd.Next(-1 * (cHp / 8), cHp / 8 + 1);
            }
            else if (Rarity == "Uncommon")
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
                HP = eHp + rnd.Next(-1 * (eHp / 10), eHp / 10 + 1);
            }
            else
            {
                Level = rnd.Next(13, 15);
                lHp += Level % 4 * 15;
                HP = lHp + rnd.Next(-1 * (lHp / 10), lHp / 10 + 1);
            }

            if (isArchon)
            {
                HP = (int)(HP * 2.05);
            }
        }

        public Color GetColor()
        {
            if (IsArchon)
            {
                return Color.DarkRed;
            }

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
    }
}
