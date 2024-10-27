using Microsoft.EntityFrameworkCore;
using KushBot.DataClasses;

namespace KushBot.Resources.Database;

public class SqliteDbContext : DbContext
{
    public DbSet<KushBotUser> Users { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<ItemStat> ItemStats { get; set; }
    public DbSet<RarityFollow> RarityFollow { get; set; }
    public DbSet<Infection> UserInfections { get; set; }
    public DbSet<UserTutoProgress> UserTutoProgress { get; set; }
    public DbSet<Plot> Plots { get; set; }
    public DbSet<ConsumableBuff> ConsumableBuffs { get; set; }
    public DbSet<NyaClaim> NyaClaims { get; set; }
    public DbSet<UserPet> UserPets { get; set; }
    public DbSet<UserEvent> UserEvents { get; set; }
    public DbSet<Quest> Quests { get; set; }
    public DbSet<QuestRequirement> QuestRequirements { get; set; }
    public DbSet<ChannelPerms> ChannelPerms { get; set; }
    public DbSet<UserPicture> UserPictures { get; set; }

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

            e.HasMany(e => e.UserBuffs)
                .WithOne(e => e.Owner)
                .HasForeignKey(e => e.OwnerId);

            e.HasMany(e => e.UserQuests)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId);

            e.HasMany(e => e.UserPictures)
                .WithOne(e => e.Owner)
                .HasForeignKey(e => e.OwnerId);
        });

        modelBuilder.Entity<ChannelPerms>()
            .HasIndex(e => e.PermitsVendor)
            .IsUnique()
            .HasFilter("[PermitsVendor] = 1");

        modelBuilder.Entity<ChannelPerms>()
            .HasIndex(e => e.PermitsBoss)
            .IsUnique()
            .HasFilter("[PermitsBoss] = 1");

        modelBuilder.Entity<Quest>(e =>
        {
            e.HasMany(e => e.Requirements)
                .WithOne(e => e.Quest)
                .HasForeignKey(e => e.QuestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuestRequirement>()
            .HasDiscriminator<QuestRequirementType>("Type")
            .HasValue<WinQuestRequirement>(QuestRequirementType.Win)
            .HasValue<LoseQuestRequirement>(QuestRequirementType.Lose)
            .HasValue<BapsXQuestRequirement>(QuestRequirementType.BapsX)
            .HasValue<ModifierXQuestRequirement>(QuestRequirementType.ModifierX)
            .HasValue<CommandQuestRequirement>(QuestRequirementType.Command)
            .HasValue<ChainQuestRequirement>(QuestRequirementType.Chain)
            .HasValue<CountQuestRequirement>(QuestRequirementType.Count);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($@"Data Source= Data/Database.sqlite");
    }
}
