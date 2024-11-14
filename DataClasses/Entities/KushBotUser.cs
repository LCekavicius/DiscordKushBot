using Discord;
using KushBot.DataClasses;
using KushBot.DataClasses.Enums;
using KushBot.Global;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace KushBot;

public class KushBotUser
{
    [Key]
    public ulong Id { get; set; }
    public int Balance { get; set; }
    public DateTime LastBeg { get; set; }

    public int Eggs { get; set; }
    public DateTime LastDestroy { get; set; }
    public DateTime LastYoink { get; set; }
    public DateTime LastTylerRage { get; set; }

    public int RageCash { get; set; }
    public int RageDuration { get; set; }

    public DateTime SetDigger { get; set; }
    public DateTime LootedDigger { get; set; }
    public int DiggerState { get; set; }

    public string SelectedPicture { get; set; }

    public int Yiked { get; set; }
    public DateTime YikeDate { get; set; }
    public DateTime RedeemDate { get; set; }

    public int Tickets { get; set; }

    public int DailyGive { get; set; } = 3000;

    public int Cheems { get; set; }

    public DateTime NyaMarryDate { get; set; } = TimeHelper.Now;
    public string NyaMarry { get; set; } = "";
    public int TicketMultiplier { get; set; }
    public int? GoranMaxDigMinutes { get; set; }
    public DateTime LastVendorPurchase { get; set; }
    public DateTime LastNyaClaim { get; set; }

    public int PetPity { get; set; }
    public int ExtraClaimSlots { get; set; }
    public UserItems Items { get; set; }
    public UserBuffs UserBuffs { get; set; }
    public UserEvents UserEvents { get; set; }
    public UserQuests UserQuests { get; set; }
    public List<UserPicture> UserPictures { get; set; }
    public List<NyaClaim> NyaClaims { get; set; }
    public List<Infection> UserInfections { get; set; }
    public List<Plot> UserPlots { get; set; }
    [NotMapped] public UserPets Pets { get; set; }

    public KushBotUser() { }

    public KushBotUser(ulong id, int balance = 30)
    {
        Id = id;
        Balance = balance;
        UserPictures = [new(Id, "1.jpg"), new(Id, "2.jpg"), new(Id, "3.jpg")];
        SelectedPicture = UserPictures[Random.Shared.Next(0, UserPictures.Count)].Path;
    }

    public int GetDailiesCompleteReward()
    {
        int petLvl = Pets?[PetType.Maybich]?.CombinedLevel ?? 0;
        int bapsFromPet = (int)Math.Round(Math.Pow(petLvl, 1.3) + petLvl * 3);

        int baps = 113 + (int)DateTime.Today.DayOfWeek * 13;
        baps += (int)Items?.Equipped?.GetStatTypeBonus(ItemStatType.QuestBapsFlat);
        baps += (int)(bapsFromPet * 1.4);
        baps = (int)((double)baps * (1 + Items?.Equipped.GetStatTypeBonus(ItemStatType.QuestBapsPercent)));

        return baps;
    }

    public Item GetWeekliesCompleteReward()
    {
        return new ItemManager().GenerateRandomItem(this);
    }

    public void AddUserEvent(UserEventType type)
    {
        UserEvents.Add(new UserEvent
        {
            CreationTime = DateTime.Now,
            Type = type,
            UserId = Id,
            User = this,
        });
    }

    public (int minDmg, int maxDmg) GetDamageRange()
    {
        int min = Pets.Count
            + Pets.TotalRawTier
            + (int)(UserBuffs.Get(BuffType.BossArtillery)?.Potency ?? 0)
            + (int)Items.Equipped.GetStatTypeBonus(ItemStatType.BossDmg);

        int max = min + Pets.TotalCombinedPetLevel;

        return (min, max);
    }

    public (List<Quest> freshCompleted, bool lastDailyCompleted, bool lastWeeklyCompleted) AttemptCompleteQuests()
    {
        var relevantQuests = UserQuests.InProgress;

        if (!relevantQuests.Any())
            return ([], false, false);

        var freshCompletedQuests = new List<Quest>();

        int eggs = 0;

        foreach (var quest in relevantQuests)
        {
            var relevantEventTypes = quest.User.UserEvents.Where(e => quest.GetRelevantEventTypes().Contains(e.Type)).ToList();

            quest.IsCompleted = quest.Requirements.All(e => e.Validate(relevantEventTypes));

            if (quest.IsCompleted)
            {
                freshCompletedQuests.Add(quest);
                if (Random.Shared.NextDouble() > 0.97)
                {
                    eggs++;
                }
            }
        }

        var lastDailyCompleted = freshCompletedQuests.Any(e => e.IsDaily) && UserQuests.Where(e => e.IsDaily).All(e => e.IsCompleted);
        var lastWeeklyCompleted = freshCompletedQuests.Any(e => !e.IsDaily) && UserQuests.Where(e => !e.IsDaily).All(e => e.IsCompleted);

        Eggs += eggs;
        Balance += relevantQuests.Where(e => e.IsCompleted).Sum(e => e.GetQuestReward());

        if (lastDailyCompleted)
        {
            Balance += GetDailiesCompleteReward();
        }

        if (lastWeeklyCompleted)
        {
            Items.Add(GetWeekliesCompleteReward());
        }

        return (freshCompletedQuests, lastDailyCompleted, lastWeeklyCompleted);
    }

    public static bool operator >(KushBotUser lhs, KushBotUser rhs) => lhs.Balance > rhs.Balance;
    public static bool operator <(KushBotUser lhs, KushBotUser rhs) => lhs.Balance < rhs.Balance;

}
