using Discord;
using KushBot.DataClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KushBot;

public class KushBotUser
{
    [Key]
    public ulong Id { get; set; }
    public int Balance { get; set; }
    public DateTime LastBeg { get; set; }

    public bool HasEgg { get; set; }
    public DateTime LastDestroy { get; set; }
    public DateTime LastYoink { get; set; }
    public DateTime LastTylerRage { get; set; }

    public int RageCash { get; set; }
    public int RageDuration { get; set; }

    public DateTime SetDigger { get; set; }
    public DateTime LootedDigger { get; set; }
    public int DiggerState { get; set; }

    public string Pictures { get; set; }
    public int SelectedPicture { get; set; }

    public int Yiked { get; set; }
    public DateTime YikeDate { get; set; }
    public DateTime RedeemDate { get; set; }

    public int Tickets { get; set; }

    public int DailyGive { get; set; }

    public int FirstItemId { get; set; }
    public int SecondItemId { get; set; }
    public int ThirdItemId { get; set; }
    public int FourthItemId { get; set; }

    public int Cheems { get; set; }

    public DateTime NyaMarryDate { get; set; }
    public string NyaMarry { get; set; }
    public int TicketMultiplier { get; set; }
    public int? GoranMaxDigMinutes { get; set; }
    public DateTime LastVendorPurchase { get; set; }
    public DateTime LastNyaClaim { get; set; }

    public int PetPity { get; set; }
    public int ExtraClaimSlots { get; set; }
    public List<NyaClaim> NyaClaims { get; set; }
    public UserItems Items { get; set; }
    public UserBuffs UserBuffs { get; set; }
    public UserEvents UserEvents { get; set; }
    public UserQuests UserQuests { get; set; }
    [NotMapped] public UserPets Pets { get; set; }

    public KushBotUser(ulong id, int balance, bool hasEgg)
    {
        Id = id;
        Balance = balance;

        Pictures = "1,2,3";

        SelectedPicture = Random.Shared.Next(1, 4);

        DailyGive = 3000;

        NyaMarryDate = DateTime.Now;
        NyaMarry = "";
    }

    public int GetFullQuestCompleteReward()
    {
        int petLvl = Pets?[PetType.Maybich]?.CombinedLevel ?? 0;
        int bapsFromPet = (int)Math.Round(Math.Pow(petLvl, 1.3) + petLvl * 3);

        int baps = 113 + (int)DateTime.Today.DayOfWeek * 13;
        baps += (int)Items?.Equipped?.QuestBapsFlatSum;
        baps += (int)(bapsFromPet * 1.4);
        baps = (int)((double)baps * (1 + Items?.Equipped.QuestBapsPercentSum));

        return baps;
    }

    public static bool operator >(KushBotUser lhs, KushBotUser rhs) => lhs.Balance > rhs.Balance;
    public static bool operator <(KushBotUser lhs, KushBotUser rhs) => lhs.Balance < rhs.Balance;

}
