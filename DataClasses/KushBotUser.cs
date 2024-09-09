using KushBot.DataClasses;
using System;
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
    public string Pets { get; set; }
    public string PetLevels { get; set; }
    public string PetDupes { get; set; }
    public DateTime LastDestroy { get; set; }
    public DateTime LastYoink { get; set; }
    public DateTime LastTylerRage { get; set; }

    public int RageCash { get; set; }
    public int RageDuration { get; set; }

    //Quests
    public string QuestIndexes { get; set; }
    public int LostBapsMN { get; set; }
    public int WonBapsMN { get; set; }
    public int LostFlipsMN { get; set; }
    public int WonFlipsMN { get; set; }
    public int LostBetsMN { get; set; }
    public int WonBetsMN { get; set; }
    public int LostRisksMN { get; set; }
    public int WonRisksMN { get; set; }
    public int BegsMN { get; set; }
    public int SuccesfulYoinks { get; set; }
    public int FailedYoinks { get; set; }
    public int WonFlipChainOverFifty { get; set; }

    //digger nigger
    public DateTime SetDigger { get; set; }
    public DateTime LootedDigger { get; set; }
    public int DiggerState { get; set; }

    public int WonDuelsMN { get; set; }
    public string Pictures { get; set; }
    public int SelectedPicture { get; set; }

    public int Yiked { get; set; }
    public DateTime YikeDate { get; set; }
    public DateTime RedeemDate { get; set; }

    public int LostBapsWeekly { get; set; }
    public int WonBapsWeekly { get; set; }
    public int LostFlipsWeekly { get; set; }
    public int WonFlipsWeekly { get; set; }
    public int LostBetsWeekly { get; set; }
    public int WonBetsWeekly { get; set; }
    public int LostRisksWeekly { get; set; }
    public int WonRisksWeekly { get; set; }
    public int BegsWeekly { get; set; }

    public int Tickets { get; set; }

    public string CompletedWeeklies { get; set; }

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
    [NotMapped]
    public UserPets Pets2 { get; set; }

    public KushBotUser(ulong id, int balance, bool hasEgg)
    {
        Id = id;
        Balance = balance;
        LastBeg = DateTime.Now.AddHours(-9);
        LastDestroy = DateTime.Now.AddHours(-25);
        HasEgg = hasEgg;
        Pets = "";
        PetLevels = "";
        PetDupes = "";
        for (int i = 0; i < Program.Pets.Count - 1; i++)
        {
            PetLevels += "0,";
            PetDupes += "0,";
        }
        PetLevels += "0";
        PetDupes += "0";

        LastYoink = DateTime.Now.AddHours(-9);
        LastTylerRage = DateTime.Now.AddHours(-9);
        RageCash = 0;
        RageDuration = 0;
        Random rad = new Random();

        QuestIndexes = "11,0,2";
        LostBapsMN = 0;
        WonBapsMN = 0;
        LostFlipsMN = 0;
        WonFlipsMN = 0;
        LostBetsMN = 0;
        WonBetsMN = 0;
        LostRisksMN = 0;
        WonRisksMN = 0;
        BegsMN = 0;
        SuccesfulYoinks = 0;
        FailedYoinks = 0;
        WonFlipChainOverFifty = 0;
        SetDigger = DateTime.Now.AddHours(-9);
        LootedDigger = DateTime.Now.AddHours(-9);
        WonDuelsMN = 0;

        Pictures = "1,2,3";

        SelectedPicture = rad.Next(1, 4);

        Yiked = 0;
        RedeemDate = DateTime.Now.AddHours(-8);
        YikeDate = DateTime.Now.AddHours(-2);

        LostBapsMN = 0;
        WonBapsMN = 0;
        LostFlipsMN = 0;
        WonFlipsMN = 0;
        LostBetsMN = 0;
        WonBetsMN = 0;
        LostRisksMN = 0;
        WonRisksMN = 0;
        BegsMN = 0;

        Tickets = 0;

        CompletedWeeklies = "0,0";

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
