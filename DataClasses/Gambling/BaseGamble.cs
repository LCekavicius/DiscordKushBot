using Discord;
using Discord.Commands;
using KushBot.DataClasses.Enums;
using KushBot.Global;
using KushBot.Resources.Database;
using KushBot.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KushBot.DataClasses;

public abstract class BaseGamble(SqliteDbContext dbContext, TutorialManager tutorialManager, SocketCommandContext context)
{
    public const int GambleDelay = 500;
    protected SocketCommandContext Context { get; init; } = context;
    protected SqliteDbContext DbContext { get; init; } = dbContext;
    protected TutorialManager TutorialManager { get; init; } = tutorialManager;

    public class GambleResults
    {
        public int Baps { get; set; }
        public bool IsWin { get; init; }
        //TODO move to dictionary?
        public bool GymProc { get; set; }
        public bool RodProc { get; set; }
        public bool SlotsTokenUsed { get; set; }

        public GambleResults(int baps, bool isWin)
        {
            Baps = baps;
            IsWin = isWin;
        }
    }

    protected class DataForEvent(UserEventType type, double? modifier = null)
    {
        public UserEventType Type { get; init; } = type;
        public double? Modifier { get; init; } = modifier;
    }

    public int Amount { get; set; }
    protected KushBotUser BotUser { get; set; }
    protected Random Rnd { get; init; } = new();
    protected string OriginalInput { get; set; }

    public virtual async Task Start(string input)
    {
        if (DiscordBotService.IgnoredUsers.ContainsKey(Context.User.Id))
        {
            return;
        }

        OriginalInput = input;

        BotUser = await DbContext.GetKushBotUserAsync(Context.User.Id, Data.UserDtoFeatures.Buffs | Data.UserDtoFeatures.Quests);

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

        DiscordBotService.IgnoredUsers.Add(Context.User.Id, DateTime.Now.AddMilliseconds(GambleDelay));

        await HandleGambleAsync();
    }

    public abstract GambleResults Calculate();
    public abstract Task SendReplyAsync(GambleResults result);
    protected abstract DataForEvent GetUserEventType(GambleResults result);
    public void AddUserEvent(GambleResults result)
    {
        var dataForEvent = GetUserEventType(result);
        BotUser.UserEvents.Add(new()
        {
            UserId = BotUser.Id,
            Type = dataForEvent.Type,
            CreationTime = DateTime.Now,
            BapsChange = result.Baps,
            BapsInput = Amount,
            Modifier = dataForEvent.Modifier
        });
    }

    protected async Task HandleGambleAsync()
    {
        var result = HandleBuffs(Calculate());
        AddUserEvent(result);
        await HandleQuestsAsync();
        await HandleTutorialAsync();
        await HandleGambleResultAsync(result);
        await SendReplyAsync(result);
    }

    private async Task HandleGambleResultAsync(GambleResults result)
    {
        BotUser.Balance += (result.IsWin ? result.Baps : -result.Baps);
        await DbContext.SaveChangesAsync();
    }

    public async Task HandleQuestsAsync()
    {
        var result = BotUser.AttemptCompleteQuests();
        await TutorialManager.AttemptCompleteQuestSteps(BotUser, Context.Channel, result);
        await Context.CompleteQuestsAsync(result.freshCompleted, result.lastDailyCompleted, result.lastWeeklyCompleted);
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

    protected abstract Task HandleTutorialAsync();

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
