using Discord;
using Discord.Rest;
using KushBot.DataClasses;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KushBot.Global
{
    public class NyaClaimEvent
    {
        public ulong UserId { get; set; }
        public RestUserMessage ImageMessage { get; set; }
        public string FileName { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    public class NyaClaimTrade
    {
        public NyaClaimParty Suggester { get; set; }
        public NyaClaimParty Respondee { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class NyaClaimParty
    {
        public ulong UserId { get; set; }
        public NyaClaim NyaClaim { get; set; }
        public int Baps { get; set; }

        public bool HasResponded => NyaClaim != null || Baps > 0;
    }

    public class PaginatedEmbed
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public Func<ulong, int, int, Embed> GetPageEmbed { get; set; }
        public Func<ulong, int, int, Task<Embed>> GetPageEmbedAsync { get; set; }

        public int NextPage()
        {
            CurrentPage = (CurrentPage + 1) % TotalPages;
            return CurrentPage;
        }

        public int PrevPage()
        {
            if (CurrentPage == 0)
                CurrentPage = TotalPages - 1;
            else
                CurrentPage = CurrentPage - 1;
            return CurrentPage;
        }
    }

    public static class NyaClaimGlobals
    {
        public static HashSet<ulong> ClaimReadyUsers { get; set; } = new HashSet<ulong>();
        public static Dictionary<ulong, HashSet<string>> UserClaims { get; set; } = new();
        public static Dictionary<Guid, NyaClaimEvent> NyaClaimEvents { get; set; } = new();

        public static Dictionary<ulong, PaginatedEmbed> PaginatedEmbed { get; set; } = new();
        public static List<NyaClaimTrade> NyaTrades { get; set; } = new();

        public static int? ParseTradeInput(string input)
        {
            try
            {
                string pattern = @"\d+";
                Match match = Regex.Match(input, pattern);

                if (match.Success)
                {
                    return int.Parse(match.Value);
                }
                else
                {
                    return null;
                }
            }
            catch(Exception ex)
            {
                return null;
            }
        }
    }
}
