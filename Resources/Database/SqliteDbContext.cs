using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using KushBot.DataClasses;

namespace KushBot.Resources.Database
{
    public class SqliteDbContext : DbContext
    {
        public DbSet<SUser> Jews { get; set; }
        public DbSet<Item> Item { get; set; }
        public DbSet<ItemPetConn> ItemPetBonus { get; set; }
        public DbSet<RarityFollow> RarityFollow { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder Options)
        {
            string DbLocation = Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\netcoreapp2.0", @"Data/");

            //Options.UseSqlite($@"Data Source= {DbLocation}/Database.sqlite");
            if (Program.BotTesting)
            {
                Options.UseSqlite($@"Data Source= D:/KushBot/Kush Bot/KushBot/KushBot/Data/Database.sqlite");
            }
            else
            {
                Options.UseSqlite($@"Data Source= Data/Database.sqlite");
            }

            //{DbLocation}
        }

    }
}
