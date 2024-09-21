//using Discord.Commands;
//using System;

//namespace KushBot.DataClasses;

//public sealed class GamblingHelper<T>
//{
//    private KushBotUser BotUser { get; init; }
//    private SocketCommandContext Context { get; init; }
//    private BaseGamble Gamble { get; set; }

//    public GamblingHelper(SocketCommandContext context)
//    {
//        Context = context;
//        BotUser = Data.Data.GetKushBotUser(context.User.Id);
//        Gamble = new();
//    }

//    private int ParseInput(string input)
//    {
//        if (input.Equals("all", StringComparison.OrdinalIgnoreCase))
//        {
//            return BotUser.Balance;
//        }
//        else
//        {
            
//        }
//    }

//    private bool Validate()
//    {
//        return BotUser.Balance >= Gamble.GetRequiredBaps();
//    }
//}
