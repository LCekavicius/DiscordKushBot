using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
namespace KushBot.Modules
{
    public class EasterEggs : ModuleBase<SocketCommandContext>
    {
        string source = @"Data/EasterEggs.txt";

       //treamReader reader = new StreamReader(@"Data/EasterEggs.txt");
       //treamWriter writer = new StreamWriter(@"Data/EasterEggs.txt");

        List<eggas> Eggai = new List<eggas>();

        struct eggas
        {
            public string KeyWord { get; set; }
            public string Hint { get; set; }
            public string Link { get; set; }
            public ulong Id { get; set; }

            public eggas(string keyword, string hint, string link, ulong id)
            {
                KeyWord = keyword;
                Hint = hint;
                Link = link;
                Id = id;
            }
        };

        public void Read()
        {
            return;
            using (StreamReader reader = new StreamReader(@"Data/EasterEggs.txt"))
            {
                // FileStream fs = File.OpenRead
                string line = reader.ReadLine();
                while (line != null)
                {
                    string[] values;
                    values = line.Split(';');
                    Eggai.Add(new eggas(values[0], values[1], values[2], ulong.Parse(values[3])));
                    line = reader.ReadLine();
                }
            }
        }

        [Command("hint"), Alias("hints","eastereggs", "secrets")]
        public async Task Eggs()
        {
            Read();

            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("Easter Egg hints!");
            foreach (eggas egg in Eggai)
            {
                if(egg.Id != 0)
                {
                    builder.AddField($"**{egg.KeyWord}**", $"**~~{egg.Hint}~~**\n This egg was found by: **{Program._client.GetUser(egg.Id).Username}**");
                }
                else
                {
                    builder.AddField("**-----------------**", $"**{egg.Hint}**");
                }
            }
            builder.AddField("**-----------------**", $"--use 'kush secret *word*' (e.g. kush secret autism). to look for easter eggs. use only 1 word\nGuessing correctly will give you baps ranging from 100 to 150.");

            await ReplyAsync("", false, builder.Build());
        }
        [Command("secret")]
        public async Task CompleteEgg(string input)
        {
            Read();

            for (int i = 0; i < Eggai.Count; i++)
            {
               // if (Eggai[i].KeyWord == input)
               if(input.Equals(Eggai[i].KeyWord, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (Eggai[i].Id == 0)
                    {
                        Random rad = new Random();
                        int baps = rad.Next(100, 151);
                        await ReplyAsync($"{Context.User.Mention} Was the first to find this easteregg and got **{baps}** baps !\n{Eggai[i].Link}");
                        await Data.Data.SaveBalance(Context.User.Id, baps, false);
                        Write(Eggai[i].KeyWord);
                    }
                    else
                    {
                        await ReplyAsync($"{Context.User.Mention} Has found an easteregg!\n{Eggai[i].Link}");
                    }
                }
            }        
        }
        public void Write(string keyword)
        {
            using(StreamWriter writer = new StreamWriter(@"Data/EasterEggs.txt"))
            {
                foreach(eggas egg in Eggai)
                {
                    string write = "";
                    if(egg.KeyWord == keyword)
                    {
                        write = string.Join(';', egg.KeyWord, $"{egg.Hint}", egg.Link, Context.User.Id);
                    }
                    else
                    {
                        write = string.Join(';', egg.KeyWord, $"{egg.Hint}", egg.Link, egg.Id);
                    }
                    writer.WriteLine(write);
                }
            }            
        }
    }
}
