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
    [NotMapped]
    public UserPets Pets { get; set; }

    public KushBotUser(ulong id, int balance, bool hasEgg)
    {
        Id = id;
        Balance = balance;
        LastBeg = DateTime.Now.AddHours(-9);
        LastDestroy = DateTime.Now.AddHours(-25);
        HasEgg = hasEgg;

        LastYoink = DateTime.Now.AddHours(-9);
        LastTylerRage = DateTime.Now.AddHours(-9);
        RageCash = 0;
        RageDuration = 0;
        Random rad = new Random();

        SetDigger = DateTime.Now.AddHours(-9);
        LootedDigger = DateTime.Now.AddHours(-9);

        Pictures = "1,2,3";

        SelectedPicture = rad.Next(1, 4);

        Yiked = 0;
        RedeemDate = DateTime.Now.AddHours(-8);
        YikeDate = DateTime.Now.AddHours(-2);

        Tickets = 0;

        DailyGive = 3000;

        FirstItemId = 0;
        SecondItemId = 0;
        ThirdItemId = 0;
        FourthItemId = 0;

        Cheems = 0;

        NyaMarryDate = DateTime.Now;
        NyaMarry = "";
    }

    public static bool operator >(KushBotUser lhs, KushBotUser rhs)
    {
        if (lhs.Balance > rhs.Balance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static bool operator <(KushBotUser lhs, KushBotUser rhs)
    {
        if (lhs.Balance < rhs.Balance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}
