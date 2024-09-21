using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace KushBot;

public class QuestOld
{
    [Key]
    public int Id { get; set; }
    public int CompleteReq { get; set; }
    public string Desc { get; set; }
    public bool CashReward { get; set; }
    public int Baps { get; set; }

    public QuestOld(int id,int completeReq, string desc, bool cashreward, int baps)
    {
        Id = id;
        Desc = desc;
        CompleteReq = completeReq;
        CashReward = cashreward;
        Baps = baps;
        
    }

    public string GetDesc(ulong id)
    {
        int petLvl = Program.GetTotalPetLvl(id);

        if(Desc.Contains("**Flip 60"))
        {
            int ret = 60 + (int)(4 * Math.Pow(petLvl, 1.08) * ((double)60 / 1400));
            string t = Regex.Replace(Desc, "60", ret.ToString());
            return t;
        }
        if (Desc.Contains("Beg") || Desc.Contains("Yoink") || Desc.Contains("begging"))
        {
            return Desc;
        }
        string temp = Regex.Replace(Desc, "[0-9]{2,}", GetCompleteReq(id).ToString());

        if(CompleteReq == 0)
        {
            Random rnd = new Random();
            string[] a = {"**?Nekenciu.**", "**?Sumusiu.**", "**?Avarija.**", "**?Tragedija.**"};
        }

        return temp;

    }

    //This is utterly fucking retarded but me from 3 years ago had like 10 iq so it'll be fine for now
    public bool HasCompleted(ulong id)
    {
        if (Desc.Contains("gambling"))
        {
            if (Desc.Contains("Win"))
            {
                if (Data.Data.GetWonBapsWeekly(id) >= GetCompleteReq(id))
                    return true;
                return false;
            }
            else
            {
                if (Data.Data.GetLostBapsWeekly(id) >= GetCompleteReq(id))
                    return true;
                return false;
            }
        }
        else if (Desc.Contains("flipping"))
        {
            if (Desc.Contains("Win"))
            {
                if (Data.Data.GetWonFlipsWeekly(id) >= GetCompleteReq(id))
                    return true;
                return false;
            }
            else
            {
                if (Data.Data.GetLostFlipsWeekly(id) >= GetCompleteReq(id))
                    return true;
                return false;
            }
        }
        else if (Desc.Contains("betting"))
        {
            if (Desc.Contains("Win"))
            {
                if (Data.Data.GetWonBetsWeekly(id) >= GetCompleteReq(id))
                    return true;
                return false;
            }
            else
            {
                if (Data.Data.GetLostBetsWeekly(id) >= GetCompleteReq(id))
                    return true;
                return false;
            }
        }
        else
        {
            if (Desc.Contains("Win"))
            {
                if (Data.Data.GetWonRisksWeekly(id) >= GetCompleteReq(id))
                    return true;
                return false;
            }
            else
            {
                if (Data.Data.GetLostRisksWeekly(id) >= GetCompleteReq(id))
                    return true;
                return false;
            }
        }
    }

    public int GetCompleteReq(ulong id)
    {
        int petlvl = Program.GetTotalPetLvl(id);
        if(Desc.Contains("Reach"))
        {
            int reachRet = (int)(13 * Math.Pow(petlvl, 1.15));
            return reachRet + CompleteReq;
        }
        if (Desc.Contains("Beg") || Desc.Contains("Yoink") || Desc.Contains("begging"))
        {
            return CompleteReq;
        }

        if(Desc.Contains("**Flip 60"))
        {
            return 3;
        }

        if (Desc.Contains("Duel"))
        {
            int temp = (int)(4 * Math.Pow(petlvl, 1.08) * ((double)CompleteReq / 1400));
            return temp + CompleteReq;
        }

        int ret = (int)(4 * Math.Pow(petlvl, 1.08) * ((double)CompleteReq / 1400));

        return ret + CompleteReq;
    }
    
}
