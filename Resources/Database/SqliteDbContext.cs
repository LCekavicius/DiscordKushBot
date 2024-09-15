using Microsoft.EntityFrameworkCore;
using System.Reflection;
using KushBot.DataClasses;

namespace KushBot.Resources.Database
{
    public class SqliteDbContext : DbContext
    {
        public DbSet<KushBotUser> Jews { get; set; }
        public DbSet<Item> Item { get; set; }
        public DbSet<ItemPetConn> ItemPetBonus { get; set; }
        public DbSet<RarityFollow> RarityFollow { get; set; }
        public DbSet<Infection> UserInfections { get; set; }
        public DbSet<UserTutoProgress> UserTutoProgress { get; set; }
        public DbSet<Plot> Plots { get; set; }
        public DbSet<ConsumableBuff> ConsumableBuffs { get; set; }
        public DbSet<NyaClaim> NyaClaims { get; set; }
        public DbSet<UserPet> UserPets { get; set; }
        public DbSet<UserEvent> UserEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KushBotUser>(e =>
            {
                e.HasMany(e => e.NyaClaims)
                .WithOne(e => e.Owner)
                .HasForeignKey(e => e.OwnerId);

                e.HasMany(e => e.Items)
                .WithOne(e => e.Owner)
                .HasForeignKey(e => e.OwnerId);

                e.HasMany(e => e.UserEvents)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId);
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder Options)
        {
            string DbLocation = Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\netcoreapp2.0", @"Data/");

            Options.UseSqlite($@"Data Source= Data/Database.sqlite");
        }

    }
}
