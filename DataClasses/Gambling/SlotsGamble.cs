using Discord;
using Discord.Commands;
using KushBot.DataClasses.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.DataClasses;

public sealed class SlotsGamble : BaseGamble
{
    enum RewardType { Baps, Item, Cheems }

    private static SlotsContainer BapsSlots = GetEmojiList();
    //private static SlotsContainer LootSlots = GetEmojiList();

    private List<Slot> Rolled = new();

    public SlotsGamble(SocketCommandContext context) : base(context) { }

    public override GambleResults Calculate()
    {
        //TODO Handle slots tokens
        Rolled = Roll().ToList();

        double total = 0;

        for (int i = 0; i < Rolled.Count; i += 3)
        {
            if (Rolled[i].Emoji.Equals(Rolled[i + 1].Emoji) && Rolled[i].Emoji.Equals(Rolled[i + 2].Emoji))
            {
                total += Rolled[i].PayoutRatio * Amount;
                if (i == 3)
                {
                    total *= 2;
                }
            }
        }

        return new(total != 0 ? ((int)total - Amount) : Amount, total != 0);
    }

    protected override UserEventType GetUserEventType(GambleResults result)
    {
        return result.IsWin ? UserEventType.SlotsWin : UserEventType.SlotsLose;
    }

    protected override GambleResults HandleBuffs(GambleResults result)
    {
        var slotsBuff = BotUser.UserBuffs.Get(BuffType.SlotTokens);

        result.SlotsTokenUsed = slotsBuff != null;
        result.Baps = slotsBuff != null ? 0 : result.Baps;

        slotsBuff?.ReduceDuration();

        return base.HandleBuffs(result);
    }

    public override async Task SendReplyAsync(GambleResults result)
    {
        await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 2, 3, Context.Channel);

        int baps = result.IsWin ? result.Baps : 0;

        string costString = result.SlotsTokenUsed
            ? "1 slots token"
            : result.RodProc
                ? $":fishing_pole_and_fish: 0 :fishing_pole_and_fish:"
                : $"{OriginalInput} baps";

        string wonString = result.GymProc ? $"💪**{baps}**💪" : $"**{baps}**";

        EmbedBuilder builder = new EmbedBuilder();
        builder.WithTitle($"{Context.User.Username}'s slot machine");
        builder.AddField("Slots", $"{GetSlotsDisplayGrid(result)}");
        builder.AddField("Result", $"You won {wonString} baps and now have {BotUser.Balance}\nSlot machine cost: {costString}");
        builder.WithColor(result.IsWin ? Color.Green : Color.Red);

        await Context.Message.ReplyAsync(embed: builder.Build());
    }

    private string GetSlotsDisplayGrid(GambleResults result)
    {
        string grid = "";
        for (int i = 0; i < Rolled.Count; i += 3)
        {
            grid += Rolled[i].Emoji + " ";
            grid += Rolled[i + 1].Emoji + " ";
            grid += Rolled[i + 2].Emoji + " ";

            if (result.IsWin && Rolled[i].Emoji.Equals(Rolled[i + 1].Emoji) && Rolled[i].Emoji.Equals(Rolled[i + 2].Emoji))
            {
                grid += "🔔";
            }
            else
            {
                grid += "❌";
            }

            grid += "\n";
        }
        return grid;
    }

    private static SlotsContainer GetEmojiList()
    {
        SlotsContainer slots = new();

        slots.Add(new Slot("<:Omega:945781765899952199>", 62.9));
        slots.Add(new Slot("<:widepep:945703091876020245>", 55));
        slots.Add(new Slot("<:Pepejam:945806412049702972>", 48.99));
        slots.Add(new Slot("<:rieda:945781493291184168>", 40.73));
        slots.Add(new Slot("<:stovi:945780098332774441>", 37.17));
        slots.Add(new Slot("<:kitadimensija:945779895164895252>", 34.43));
        slots.Add(new Slot("<:Booba:944937036702441554>", 32.19));

        slots.Calculate();
        return slots;
    }

    private IEnumerable<Slot> Roll()
    {
        for (int i = 0; i < 9; i++)
        {
            yield return RollWithWeight();
        }
    }

    private Slot RollWithWeight()
    {
        double roll = Rnd.NextDouble();

        double span = 0;

        foreach (var item in BapsSlots)
        {
            if (roll < span + item.Probability)
            {
                return item;
            }
            else
            {
                span += item.Probability;
            }
        }

        return null;
    }

    private sealed class Slot(string emoji, double weight, RewardType rewardType = RewardType.Baps)
    {
        public string Emoji { get; set; } = emoji;
        public RewardType RewardType { get; set; } = rewardType;
        public double Weight { get; set; } = weight;

        public double Probability { get; set; }
        public double PayoutRatio { get; set; }

        public void Calculate(double totalWeight)
        {
            Probability = Weight / totalWeight;
            var singleRowProbability = (Math.Pow(Probability, 3));
            PayoutRatio = Probability / (singleRowProbability * 2 + (Math.Pow(singleRowProbability, 2) * 2) + (1 - Math.Pow(1 - singleRowProbability, 2)));
            PayoutRatio /= 1.00535;
        }
    }

    private sealed class SlotsContainer : List<Slot>
    {
        public void Calculate()
        {
            var totalWeight = this.Sum(e => e.Weight);
            foreach (var item in this)
            {
                item.Calculate(totalWeight);
            }
        }

    }
}
