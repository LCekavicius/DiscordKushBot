using Discord;
using Discord.Commands;
using KushBot.Global;
using System;
using System.Threading.Tasks;

namespace KushBot.DataClasses;

public abstract class BaseGamble
{
    public class GambleResults
    {
        public int Baps { get; set; }
        public bool IsWin { get; init; }
        public bool GymProc { get; set; }
        public bool RodProc { get; set; }
        public bool SlotsTokenUsed { get; set; }

        public GambleResults(int baps, bool isWin)
        {
            Baps = baps;
            IsWin = isWin;
        }
    }

    protected int Amount { get; set; }
    protected KushBotUser BotUser { get; set; }
    protected SocketCommandContext Context { get; init; }
    protected Random Rnd { get; init; } = new();
    protected string OriginalInput { get; set; }

    public BaseGamble(SocketCommandContext context)
    {
        Context = context;
    }

    public async Task Start(string input)
    {
        if (Program.IgnoredUsers.ContainsKey(Context.User.Id))
        {
            return;
        }

        OriginalInput = input;

        BotUser = Data.Data.GetKushBotUser(Context.User.Id, Data.UserDtoFeatures.Buffs);

        var amount = ParseInput(input);

        if (!amount.HasValue)
        {
            await Context.Message.ReplyAsync($"{Context.User.Mention} Is too poor to bet {CustomEmojis.Hangg}");
            return;
        }

        Amount = amount.Value;

        var validationMessage = Validate();

        if (!string.IsNullOrEmpty(validationMessage))
        {
            await Context.Message.ReplyAsync(validationMessage);
            return;
        }

        Program.IgnoredUsers.Add(Context.User.Id, DateTime.Now.AddMilliseconds(Program.GambleDelay + 150));

        await HandleGambleAsync();
    }

    public abstract GambleResults Calculate();
    public abstract Task SendReplyAsync(GambleResults result);
    public abstract Task CreateUserEventAsync(GambleResults result);

    //TODO Handle quests
    private async Task HandleGambleAsync()
    {
        var result = HandleBuffs(Calculate());
        await HandleGambleResultAsync(result);
        await CreateUserEventAsync(result);
        await SendReplyAsync(result);
    }

    private async Task HandleGambleResultAsync(GambleResults result)
    {
        BotUser.Balance += (result.IsWin ? result.Baps : -result.Baps);

        await Data.Data.SaveKushBotUserAsync(BotUser, Data.UserDtoFeatures.Buffs);
    }

    protected virtual GambleResults HandleBuffs(GambleResults result)
    {
        bool isProc(ConsumableBuff buff)
        {
            return Rnd.Next(1, 101) <= buff.Potency;
        }

        var rodBuff = BotUser.UserBuffs.Get(BuffType.FishingRod);
        var gymBuff = BotUser.UserBuffs.Get(BuffType.KushGym);

        if (result.IsWin && gymBuff != null && isProc(gymBuff))
        {
            result.GymProc = true;
            result.Baps *= 2;
        }
        else if (!result.IsWin && rodBuff != null && isProc(rodBuff))
        {
            result.RodProc = true;
            result.Baps = 0;
        }

        rodBuff?.ReduceDuration();
        gymBuff?.ReduceDuration();

        return result;
    }

    private int? ParseInput(string input)
    {
        if (input.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            return BotUser.Balance;
        }
        else if (int.TryParse(input, out int parsed))
        {
            return parsed;
        }
        else
        {
            return null;
        }
    }

    protected virtual string Validate()
    {
        if (BotUser.Balance < Amount || Amount <= 0)
        {
            return $"{Context.User.Mention} Is too poor to bet {CustomEmojis.Hangg}";
        }
        else
        {
            return null;
        }
    }
}
