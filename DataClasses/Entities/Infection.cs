using Discord;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KushBot.DataClasses;

public enum InfectionState
{
    Egg,
    Hatchling,
    Juvenile,
    Tyrant,
    [Description("Necrotic sovereign")]
    NecroticSovereign,
    [Description("Eldritch patriarch")]
    EldritchPatriarch,
    [Description("Abyssal archon")]
    AbyssalArchon,
}

public class Infection
{
    [Key]
    public Guid Id { get; set; }
    public ulong OwnerId { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime KillAttemptDate { get; set; }
    public int BapsDrained { get; set; }
    public KushBotUser Owner { get; set; }

    [NotMapped]
    public InfectionState State
    {
        get => GetInfectionState();
    }

    private InfectionState GetInfectionState()
    {
        TimeSpan ts = DateTime.Now - CreationDate;
        if (ts.TotalHours < 4)
            return InfectionState.Egg;
        if (ts.TotalHours < 16)
            return InfectionState.Hatchling;
        if (ts.TotalHours < 32)
            return InfectionState.Juvenile;
        if (BapsDrained >= 6000 && ts.TotalHours > 96)
            return InfectionState.AbyssalArchon;
        if (BapsDrained >= 3000 && ts.TotalHours > 72)
            return InfectionState.EldritchPatriarch;
        if (BapsDrained >= 1000)
            return InfectionState.NecroticSovereign;

        return InfectionState.Tyrant;
    }

    public int GetRequiredHoursForGrowth()
        => State switch
        {
            InfectionState.Egg => 4,
            InfectionState.Hatchling => 16,
            InfectionState.Juvenile => 32,
            InfectionState.Tyrant => 32,
            InfectionState.NecroticSovereign => 72,
            InfectionState.EldritchPatriarch => 96,
            _ => 96,
        };

    public double GetInfectionKillChance()
        => State switch
        {
            InfectionState.Tyrant => 0.7,
            InfectionState.NecroticSovereign => 0.5,
            InfectionState.EldritchPatriarch => 0.3,
            InfectionState.AbyssalArchon => 0,
            _ => 1
        };

    public ButtonStyle GetButtonStyle()
        => State switch
        {
            InfectionState.Egg => ButtonStyle.Secondary,
            InfectionState.Hatchling => ButtonStyle.Success,
            InfectionState.Juvenile => ButtonStyle.Primary,
            _ => ButtonStyle.Danger,
        };


    public Emote GetEmote()
        => State switch
        {
            InfectionState.Egg => Emote.Parse("<:p1:1224001339085029386>"),
            InfectionState.Hatchling => Emote.Parse("<:p2:1224001341169602601>"),
            InfectionState.Juvenile => Emote.Parse("<:p3:1224001343354830878>"),
            InfectionState.Tyrant => Emote.Parse("<:p4:1224001346961932359>"),
            InfectionState.NecroticSovereign => Emote.Parse("<:p5:1224001350036361279>"),
            InfectionState.EldritchPatriarch => Emote.Parse("<:p6:1224001353056260227>"),
            _ => Emote.Parse("<:p7:1224001356650774578>"),
        };

    public int GetBapsForKill(int petLvl)
    {
        Random rnd = new Random();
        if (State == InfectionState.Egg)
        {
            return rnd.Next(2, 10);
        }
        else if (State == InfectionState.Hatchling)
        {
            var rangeClamp = Math.Clamp(petLvl, 50, 200);
            return rnd.Next(-1 * ((int)(rangeClamp / 2)), rangeClamp);
        }
        else if (State == InfectionState.Juvenile)
        {
            int rangeClamp = (int)Math.Clamp(petLvl * 2, 125, 350);
            return rnd.Next(-1 * ((int)(rangeClamp / 2)), rangeClamp);
        }
        else if (State == InfectionState.Tyrant)
        {
            int rangeClamp = (int)Math.Clamp(petLvl * 3, 200, 500);
            int baps = rnd.Next(-1 * ((int)(rangeClamp / 2)), rangeClamp);
            return baps + BapsDrained;
        }
        else if (State == InfectionState.NecroticSovereign)
        {
            int rangeClamp = (int)Math.Clamp(petLvl * 4, 300, 800);
            int baps = rnd.Next(-1 * ((int)(rangeClamp / 2)), rangeClamp);
            return baps + BapsDrained;
        }
        else if (State == InfectionState.EldritchPatriarch)
        {
            int rangeClamp = (int)Math.Clamp(petLvl * 5, 500, 1_200);
            int baps = rnd.Next(-1 * ((int)(rangeClamp / 2)), rangeClamp);
            return baps + BapsDrained;
        }

        return 0;
    }
}
