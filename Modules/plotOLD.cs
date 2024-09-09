//using Discord;
//using Discord.Commands;
//using Discord.WebSocket;
//using KushBot.DataClasses;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace KushBot.Modules
//{
    
//    [Group("plotsold"),Alias("plotold")]
//    public class plotOLD : ModuleBase<SocketCommandContext>
//    {
//        const int PlantGrowthLength = 8;
//        const int RaidCd = 3;

//        [Command("set")]
//        public async Task set([Remainder]string buff)
//        {
//            Program.GardenAffectedPlayers.Add(new GardenAffectedSUser(Context.User.Id, buff, 5, 1));
//        }

//        [Command("")]
//        public async Task ShowPlots()
//        {
//            EmbedBuilder builder = new EmbedBuilder();
//            builder.WithColor(Color.DarkGreen);
//            builder.WithTitle($"{Context.User.Username}'s Plots:");

//            List<string> plots = Data.Data.GetPlots(Context.User.Id);
//            List<string> newPlots = new List<string>();

//            foreach (string item in plots)
//            {
//                newPlots.Add(item);
//            }

//            plots.CopyTo(newPlots.ToArray());

//            if(plots.All(x => x == "0"))
//            {
//                await ReplyAsync($"{Context.User.Mention} You have no plots, dog\n" +
//                    $"Type 'kush plot buy' to buy a plot for: **{Data.Data.GetPlotPrice(Context.User.Id)}** baps");
//                return;
//            }

//            int c = 1;
//            foreach (string item in plots)
//            {
//                if(item == "0")
//                {
//                    break;
//                }
//                DateTime collectionDate;
//                if (item[0] != 'h' && item[0] != 'a')
//                    collectionDate = DateTime.Parse(item.Substring(2));      
//                else if (item[0] == 'h')
//                    collectionDate = DateTime.Parse(item.Substring(2 + int.Parse(item[1].ToString())));
//                else
//                    collectionDate = DateTime.Parse(item.Substring(3));
                   

//                if(item[0] == 'g')
//                {
//                    TimeSpan ts = collectionDate.AddHours(PlantGrowthLength) - DateTime.Now;

//                    string date = "";
//                    if (collectionDate.Year.ToString()[3] == '8')
//                    {
//                        date += $"**No seed**";
//                    }
//                    else if (collectionDate.AddHours(PlantGrowthLength) > DateTime.Now)
//                    {
//                        date = $"Ready in:{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
//                    }
//                    else
//                    {
//                        date = $"Ready in: **Now**";
//                    }
//                    builder.AddField($"Plot {c}\nLvl {item[1]} garden\n{date}", ":green_square:", true);
//                }
//                else if(item[0] == 'q')
//                {
//                    TimeSpan ts = DateTime.Now - collectionDate;
//                    int baps = (int)ts.TotalMinutes / (9 - 3 * (int.Parse(item[1].ToString()) - 1));

//                    builder.AddField($"Plot {c}\nLvl {item[1]} quarry\nMined: **{baps}** baps", ":brown_square:", true);
//                }
//                else if (item[0] == 'h')
//                {
//                    int eggCount = int.Parse(item[1].ToString());

//                    string text = "";

//                    TimeSpan ts = DateTime.Now - collectionDate;

//                    int fullHours = (int)ts.TotalHours;

//                    double chance = 0.33;
//                    Random rnd = new Random();

//                    bool changed = false;

//                    int index = plots.IndexOf(item);

//                    char[] newChars = new char[eggCount];

//                    for (int i = 0; i < eggCount; i++)
//                    {
//                        newChars[i] = item[2 + i];
//                    }

//                    for (int i = 0; i < fullHours; i++)
//                    {
//                        changed = true;
//                        for (int j = 0; j < eggCount; j++)
//                        {
//                            if(item[j] == 'f' || item[j] == 't')
//                            {
//                                continue;
//                            }
//                            if(chance > rnd.NextDouble())
//                            {
//                                string progressStr = newChars[j].ToString();
//                                int progress = 0;
//                                if(progressStr != "f" && progressStr != "t")
//                                {
//                                    progress = int.Parse(progressStr);
//                                    progress++;

//                                    if(progress >= 10)
//                                    {
//                                        newChars[j] = 't';
//                                    }
//                                    else
//                                    {
//                                        newChars[j] = progress.ToString()[0];
//                                    }
//                                }


//                            }
//                        }
//                    }
//                    string temp;
//                    if (changed)
//                    {
//                        temp = $"{item[0]}{item[1]}";
//                        for (int i = 0; i < eggCount; i++)
//                        {
//                            temp += newChars[i];
//                        }
//                        temp += collectionDate.AddHours(fullHours);
//                        newPlots[index] = temp;
//                        //newPlots.RemoveAt(index);
//                        //newPlots.Insert(index, temp);
//                    }
//                    else
//                    {
//                        temp = item;
//                    }

//                    for (int i = 0; i < eggCount; i++)
//                    {
//                        if(temp[2 + i] == 'f')
//                        {
//                            text += $"Slot {i + 1}: **Idle**\n";
//                        }
//                        else if(temp[2 + i] == 't')
//                        {
//                            text += $"**Egg hatched**\n";
//                        }
//                        else
//                        {
//                            text += $"Progress: **{temp[2 + i]}/10**\n";
//                        }
//                    }
                    
//                    builder.AddField($"Plot {c}\nLvl {item[1]} Hatchery\n{text}", ":yellow_square:", true);
//                }
//                else if(item[0] == PlotType.Abuse.ToString().ToLower()[0])
//                {
//                    string abusing = "empty";
//                    //f - empty; x - broken
//                    if (item[2] != 'f' && item[2] != 'x')
//                    {
//                        abusing = Program.GetPetName(int.Parse(item[2].ToString()));
//                    }
//                    else if(item[2] != 'f')
//                    {
//                        abusing = "broken";
//                    }
//                    TimeSpan ts = collectionDate - DateTime.Now;
//                    if(abusing.Equals("empty"))
//                        builder.AddField($"Plot {c}\nLvl {item[1]} abuse\n**{abusing}**", ":orange_square:", true);
//                    else if(abusing.Equals("broken") && ts.TotalSeconds < 0)
//                        builder.AddField($"Plot {c}\nLvl {item[1]} abuse\n**empty**", ":orange_square:", true);
//                    else if(abusing.Equals("broken"))
//                        builder.AddField($"Plot {c}\nLvl {item[1]} abuse\n**Repairing {ts.Hours}:{ts.Minutes}:{ts.Seconds}**", ":orange_square:", true);
//                    else
//                        builder.AddField($"Plot {c}\nLvl {item[1]} abuse\n**{abusing}** {ts.Hours}:{ts.Minutes}:{ts.Seconds}", ":orange_square:", true);

//                }
//                c++;
//            }
//            builder.AddField("Price for next Plot:",$"{Data.Data.GetPlotPrice(Context.User.Id)} baps");
//            await Data.Data.SavePlots(Context.User.Id, newPlots);
//            await ReplyAsync("", false, builder.Build());
//        }

//        public async Task LowerCds(int pet, int abuseStr, ulong userId)
//        {
//            int abuseCdr = 12;
//            if(pet == 1)
//            {
//                double pinataAbuseStr = 0;
//                if (abuseStr == 1) { pinataAbuseStr = 1.5; }
//                else if (abuseStr == 2) { pinataAbuseStr = 1.75; }
//                else { pinataAbuseStr = 2.25; }


//                TimeSpan ts = Data.Data.GetLastDestroy(userId).AddHours(24) - DateTime.Now;
//                int mins = (int)(ts.TotalMinutes / pinataAbuseStr);

//                await Data.Data.SaveLastDestroy(userId, Data.Data.GetLastDestroy(userId).AddMinutes(-1 * mins));
//            }
//            else if(pet == 0)
//            {
//                DateTime lastBeg = Data.Data.GetLastBeg(userId);
//                await Data.Data.SaveLastBeg(userId, lastBeg.AddMinutes(-12 * abuseStr));
//            }
//            else if(pet == 4)
//            {
//                DateTime lastYoink = Data.Data.GetLastYoink(userId);
//                await Data.Data.SaveLastYoink(userId,lastYoink.AddMinutes(-12 * abuseStr));
//            }
//            else if (pet == 5)
//            {
//                DateTime lastRage = Data.Data.GetLastRage(userId);
//                await Data.Data.SaveLastRage(userId, lastRage.AddMinutes(-12 * abuseStr));
//            }
//            else if (pet == 2)
//            {
//                DateTime lastDig = Data.Data.GetLootedDigger(userId);
//                await Data.Data.SaveLootedDigger(userId, lastDig.AddMinutes(-12 * abuseStr));
//            }
//        }

//            /// <summary>
//            /// _PetIndex - string input, could be: "superned", etc
//            /// </summary>
//            /// <param name="_PetIndex"></param>
//            /// <returns></returns>
//        [Command("abuse")]
//        public async Task Abuse(string _PetIndex)
//        {
//            int petIndex = Program.ParsePetIndex(_PetIndex);


//            if(petIndex == -1)
//            {
//                await ReplyAsync($"{Context.User.Mention} No such pet exists, are you fucking dyslexic or some shit?");
//                return;
//            }

//            string pets = Data.Data.GetPets(Context.User.Id);
//            if(!pets.Contains(petIndex.ToString()))
//            {
//                await ReplyAsync($"{Context.User.Mention} you dont have that pet MAN.");
//                return;
//            }

//            List<string> plots = Data.Data.GetPlots(Context.User.Id);
//            int plotId = 0;
//            bool isFound = false;
//            foreach (var item in plots)
//            {
//                if (item[0].ToString() == "a" && item != "0" && item[2].ToString() == $"{petIndex}")
//                {
//                    await ReplyAsync($"{Context.User.Mention} That pet's already being abused.");
//                    return;
//                }
//            }
//            foreach (var item in plots)
//            {
//                if(item[0] == PlotType.Abuse.ToString().ToLower()[0])
//                {
//                    if(item[2] == 'f')
//                    {
//                        isFound = true;
//                        break;
//                    }
                         
//                }
//                plotId++;
//            }
//            if (!isFound)
//            {
//                await ReplyAsync($"{Context.User.Mention} All abuse chambers busy");
//                return;
//            }
//            string selectedPlot = plots[plotId];

//            if(selectedPlot[2] != 'f')
//            {
//                await ReplyAsync($"{Context.User.Mention} You're already abusing someone, fucking die");
//                return;
//            }
//            int abuseTime = 6 + 2 * (int.Parse(selectedPlot[1].ToString()) - 1);

//            string newPlotString = $"{selectedPlot.Substring(0,2)}{petIndex}{DateTime.Now.AddHours(abuseTime)}";
            
//            plots[plotId] = newPlotString;
//            await Data.Data.SavePlots(Context.User.Id, plots);
//            await ReplyAsync($"{Context.User.Mention} you condemned {Program.GetPetName(petIndex)} to {abuseTime} hours of abuse.");
//            await LowerCds(petIndex, int.Parse(selectedPlot[1].ToString()), Context.User.Id);

//        }

//        [Command("fill")]
//        public async Task FillWithEggs(int input)
//        {
//            if(input > 9 || input < 1)
//            {
//                await ReplyAsync($"{Context.User.Mention} u funny guy!");
//                return;
//            }

//            if(Data.Data.GetBalance(Context.User.Id) < 350 && !Data.Data.GetEgg(Context.User.Id))
//            {
//                await ReplyAsync($"{Context.User.Mention} YYYYdiot");
//                return;
//            }

//            List<string> plots = Data.Data.GetPlots(Context.User.Id);

//            string plot = plots[input - 1];

//            if(plot[0] != 'h' || !plot.Contains("f"))
//            {
//                await ReplyAsync($"{Context.User.Mention} SICK IN HEAD");
//                return;
//            }

//            string newPlot = plot.Substring(0,2);

//            int lvl = int.Parse(plot[1].ToString());
//            int spent = 0;
//            bool usedEgg = false;
//            for (int i = 0; i < lvl; i++)
//            {

//                if(plot[2 + i] == 'f')
//                {
//                    if (Data.Data.GetEgg(Context.User.Id))
//                    {
//                        await Data.Data.SaveEgg(Context.User.Id, false);
//                        newPlot += "0";
//                        usedEgg = true;
//                    }
//                    else if (Data.Data.GetBalance(Context.User.Id) >= 350)
//                    {
//                        await Data.Data.SaveBalance(Context.User.Id, -350, false);
//                        newPlot += "0";
//                        spent += 350;
//                    }
//                }
//                else
//                {
//                    newPlot += plot[2 + i];
//                }
//            }

//            newPlot += plot.Substring(newPlot.Length);
//            string text = $"{Context.User.Mention} Spent";
//            if (usedEgg)
//                text += " an egg and";
//            text += $" {spent} baps to fill his hatchery";

//            plots[input - 1] = newPlot;
//            await Data.Data.SavePlots(Context.User.Id, plots);
//            await ReplyAsync(text);

//        }

//        [Command("upgrade")]
//        public async Task UpgradePlot(int plotId)
//        {
//            if(plotId <= 0 || plotId > 9)
//            {
//                await ReplyAsync($"{Context.User.Mention} <:pepew:638345326620704778><:pepew:638345326620704778><:pepew:638345326620704778><:pepew:638345326620704778>");
//                return;
//            }

//            List<string> plots = Data.Data.GetPlots(Context.User.Id);

//            string plot = plots[plotId - 1];
//            int lvl = int.Parse(plot[1].ToString());

//            if(lvl == 3)
//            {
//                await ReplyAsync($"{Context.User.Mention} level 3 is the max level");
//                return;
//            }

//            int cost = 2500;

//            if(lvl == 2)
//            {
//                cost = 10000;
//            }

//            if(Data.Data.GetBalance(Context.User.Id) < cost)
//            {
//                await ReplyAsync($"{Context.User.Mention} you can't afford an upgrade, poor bich");
//                return;
//            }

//            await Data.Data.SaveBalance(Context.User.Id, -1 * cost, false);

//            string newPlot;
//            if(plot[0] != 'h')
//                 newPlot = $"{plot[0]}{lvl+1}{plot.Substring(2)}";
//            else
//                newPlot = $"{plot[0]}{lvl + 1}f{plot.Substring(2)}";

//            plots[plotId - 1] = newPlot;

//            await Data.Data.SavePlots(Context.User.Id, plots);
//            await ReplyAsync($"{Context.User.Mention} You upgraded your plot to lvl {lvl + 1} for {cost} baps! <:pepejam:612682596757012500>");
//        }

//        [Command("help")]
//        public async Task showHelp()
//        {
//            EmbedBuilder builder = new EmbedBuilder();

//            builder.WithColor(Color.DarkGreen);

//            builder.AddField($"Plots", "Plots are landscapes that you can purchase and build something on it");
//            builder.AddField($"Buy a plot", "type 'kush plots buy' to buy a plot, you can only have 9 plots, the price for a plot increases everytime you buy one");
//            builder.AddField("Your plots", "type 'kush plots' to see your plots, their progress as well as the price for the next plot");
//            builder.AddField("Building stuff","Upon purchasing a plot you get a simple **garden** plot, you can change the plot type by typing 'kush plot transform *plotId* *type*' (e.g." +
//                " kush plot transform 3 quarry) this would transform a plot into a quarry. The cost is **300** Baps");
//            builder.AddField($"Types of plots", "Currently there are 4 types of plots: Garden, quarry, hatchery and abuse");
//            builder.AddField("Garden", "In the garden you can plant seeds by typing 'kush plots plant *plotId*' (e.g. kush plots plant 1) (you can also " +
//                "use the command 'kush plots plant all' to plant seeds in all gardens at once) this will buy and plant a seed in the plot, after " +
//                "+-8 hours you can come back and collect the yield which will give you a random buff.");
//            builder.AddField("Quarry", "Baps quarry is a passive baps income, depending on it's level it'll keep mining baps which you can loot later");
//            builder.AddField("Hatchery", "Egg hatchery is a passive egg hatcher, input your eggs into the hatchery by typing 'kush plots fill plotId' (e.g. " +
//                "kush plots fill 3), it'll use your egg if you have one, else it'll buy the egg/eggs automatically. The hatchery can hatch an amount of eggs equal to its level, " +
//                "in other words a lvl 2 hatchery can hatch 2 eggs at the same time. The hatchery can progress the hatch bar by 1 every hour at a rate of 33%. **You can " +
//                "only get the pets that you already have**");
//            //builder.AddField("Barracks", "the barracks are an aggresive aproach, you can recruit mercenaries to raid someone else's plot, type 'kush raid *@User* *enemyPlotId*' " +
//            //  "(e.g. 'kush raid @taboBanda 1'. Raiding a quarry might take a portion of the user's mined baps, raiding a garden might steal the effect and raiding the barracks might destroy them and turn them into a garden");
//            builder.AddField("Abuse", "*Abuse* will abuse your pets to make them more effective for a few hours, after the few hours " +
//                "are up, abuse chamber crumbles to pieces and you'll have to wait for it to repair. Abuse your pets by " +
//                "typing 'kush plots abuse *petName*'");

//            builder.AddField("Upgrading a plot", "to upgrade a plot type 'kush plots upgrade *plotId*', the max level is 3, upgrading the plot to lvl 2 costs **2500** baps, upgrading it to lvl 3 " +
//                "costs *10000* baps");
//            builder.AddField("Collecting", "Type 'kush plots collect *plotId* (e.g. kush plots collect 2) to collect a single plot, you can also use 'kush plots loot' to" +
//                " collect all garden plots at once **(the gambling buffs DO NOT stack)** " +
//                " 'kush plots collect q' will loot all quarries, 'kush plots collect h' will loot all of your hatcheries");

//            await ReplyAsync("",false,builder.Build());
//        }

//        [Command("Loot")]
//        public async Task CollectAll()
//        {
//            List<string> plots = Data.Data.GetPlots(Context.User.Id);

//            for (int i = 0; i < plots.Count; i++)
//            {
//                if(plots[i][0] == 'g')
//                {
//                    await this.collectShit((i + 1).ToString());
//                }
//            }
//        }

//        public int GetPetIndexRoll(string _pets)
//        {
//            List<int> ownedPets = new List<int>();

//            for (int i = 0; i < _pets.Length; i++)
//            {
//                ownedPets.Add(int.Parse(_pets[i].ToString()));
//            }

//            Random rnd = new Random();

//            double chance = rnd.NextDouble();

//            if(chance >= 0.9)
//            {
//                if(ownedPets.Contains(4) && ownedPets.Contains(5))
//                {
//                    return rnd.Next(4, 6);
//                }
//                else if(ownedPets.Contains(4))
//                {
//                    return 4;
//                }
//                else if (ownedPets.Contains(5))
//                {
//                    return 5;
//                }
//            }
//            if(chance >= 0.55)
//            {
//                if (ownedPets.Contains(2) && ownedPets.Contains(3))
//                {
//                    return rnd.Next(2, 4);
//                }
//                else if (ownedPets.Contains(2))
//                {
//                    return 2;
//                }
//                else if (ownedPets.Contains(3))
//                {
//                    return 3;
//                }
//            }
//            if(chance > 0)
//            {
//                if (ownedPets.Contains(0) && ownedPets.Contains(1))
//                {
//                    return rnd.Next(0, 2);
//                }
//                else if (ownedPets.Contains(1))
//                {
//                    return 1;
//                }
//                else if (ownedPets.Contains(0))
//                {
//                    return 0;
//                }
//            }

//            return ownedPets[rnd.Next(0, ownedPets.Count)];

//        }

//        [Command("collect")]
//        public async Task collectShit(string input)
//        {
//            if(input == "quarries" || input == "q")
//            {
//                int baps = 0;
//                List<string> plots = Data.Data.GetPlots(Context.User.Id);
//                List<string> newPlots = new List<string>();

//                foreach(string item in plots)
//                {
//                    if(item[0] == 'q')
//                    {
//                        DateTime collectionDate = DateTime.Parse(item.Substring(2));
//                        TimeSpan ts = DateTime.Now - collectionDate;
//                        int bapsFromSingleMine = (int)ts.TotalMinutes / (9 - 3 * (int.Parse(item[1].ToString()) - 1));

//                        baps += bapsFromSingleMine;

//                        string newPlot = $"{item[0]}{item[1]}{DateTime.Now}";
//                        newPlots.Add(newPlot);
//                    }
//                    else
//                    {
//                        newPlots.Add(item);
//                    }
//                }

//                await ReplyAsync($"{Context.User.Mention} you have collected {baps} from all of your quarries");
//                await Data.Data.SaveBalance(Context.User.Id, baps, false);
//                await Data.Data.SavePlots(Context.User.Id, newPlots);
//                return;
//            }
//            else if (input == "hatchery" || input == "hatcheries" || input == "h")
//            {
//                List<string> plots = Data.Data.GetPlots(Context.User.Id);
//                int loots = 0;
//                for (int i = 0; i < plots.Count; i++)
//                {

//                    if (plots[i][0] == 'h')
//                    {
//                        int lvl = int.Parse(plots[i][1].ToString());

//                        string newplot = plots[i].Substring(0, 2);

//                        for (int j = 0; j < lvl; j++)
//                        {
//                            if(plots[i][2 + j] == 't')
//                            {
//                                newplot += 'f';
//                                loots++;
//                            }
//                            else
//                            {
//                                newplot += plots[i][2 + j];
//                            }
//                        }

//                        newplot += plots[i].Substring(newplot.Length);
//                        plots[i] = newplot;
//                    }
//                }
//                string text = $"{Context.User.Mention} collected his hatcheries and got: ";
//                string pets = Data.Data.GetPets(Context.User.Id);
//                await Data.Data.SavePlots(Context.User.Id, plots);
//                Random rnd = new Random();
//                for (int i = 0; i < loots; i++)
//                {
//                    //int p = rnd.Next(0, pets.Length);
//                    //int petIndex = int.Parse(pets[p].ToString());

//                    int petIndex = GetPetIndexRoll(pets);

//                    await Data.Data.SavePetDupes(Context.User.Id, petIndex, Data.Data.GetPetDupe(Context.User.Id, petIndex) + 1);
//                    text += Program.Pets[petIndex].Name;
//                    if (i < loots - 1)
//                    {
//                        text += ", ";
//                    }
//                }

//                if (loots != 0)
//                {
//                    await ReplyAsync(text);
//                }
//                else
//                {
//                    await ReplyAsync($"{Context.User.Mention} tried looting hatcheries and failed miserably...");
//                }

//            }
//            else if(input.Length == 1)
//            {
//                int id = -1;
//                try
//                {
//                    id = int.Parse(input);
//                }
//                catch
//                {
//                    await ReplyAsync($"{Context.User.Mention} reTARD");
//                    return;
//                }

//                if(id <= 0 || id > 9)
//                {
//                    await ReplyAsync($"{Context.User.Mention} reTARD");
//                    return;
//                }

//                List<string> plots = Data.Data.GetPlots(Context.User.Id);

//                string plot = plots[id - 1];

//                if(plot[0] == 'q')
//                {
//                    DateTime collectionDate = DateTime.Parse(plot.Substring(2));
//                    TimeSpan ts = DateTime.Now - collectionDate;
//                    int bapsFromSingleMine = (int)ts.TotalMinutes / (9 - 3 * (int.Parse(plot[1].ToString()) - 1));

//                    plot = $"{plot[0]}{plot[1]}{DateTime.Now}";
//                    plots[id - 1] = plot;
//                    await Data.Data.SaveBalance(Context.User.Id, bapsFromSingleMine, false);
//                    await Data.Data.SavePlots(Context.User.Id, plots);

//                    await ReplyAsync($"{Context.User.Mention} you collected **{bapsFromSingleMine}** baps from plot #{id}");
//                    return;
//                }
//                else if (plot[0] == 'g')
//                {
//                    if(plot[11] == '8')
//                    {
//                        await ReplyAsync($"{Context.User.Mention} try planting a seed first, retarded woman");
//                        return;
//                    }
//                    DateTime collectionDate = DateTime.Parse(plot.Substring(2));
//                    TimeSpan ts = collectionDate.AddHours(PlantGrowthLength) - DateTime.Now;

//                    if(collectionDate.AddHours(PlantGrowthLength) > DateTime.Now)
//                    {
//                        await ReplyAsync($"{Context.User.Mention} your seed is not ready for collecting! RETARD!");
//                        return;
//                    }


//                    plot = plot.Substring(0, 11) + '8' + plot.Substring(12);

//                    plots[id - 1] = plot;

//                    await Data.Data.SavePlots(Context.User.Id, plots);

//                    Random rad = new Random();

//                    int effectRolled = rad.Next(0, 7);
//                    string effect = "";

//                    if(effectRolled == 0)
//                    {
//                        effect = "fishing rod";
//                    }
//                    else if(effectRolled == 1)
//                    {
//                        effect = "kush gym";
//                    }
//                    else if(effectRolled == 2)
//                    {
//                        effect = "adderal";
//                    }
//                    else if (effectRolled == 3)
//                    {
//                        if(Data.Data.GetPets(Context.User.Id).Length <= 0)
//                        {
//                            effect = "fishing rod";
//                        }
//                        int lvl = int.Parse(plot[1].ToString());

//                        effect = "roids";
//                        int tmp = rad.Next(1, 101);

//                        if (tmp < 78 - 7 * lvl)
//                        {
//                            effect = "kush gym";
//                        }
//                    }
//                    else if (effectRolled == 4)
//                    {
//                        effect = "baps";
//                    }
//                    else if (effectRolled == 5)
//                    {
//                        effect = "icon";

//                        int tmp = rad.Next(1, 101);
//                        int lvl = int.Parse(plot[1].ToString());

//                        if (tmp < 65 - 10 * lvl)
//                        {
//                            effect = "fishing rod";
//                        }
//                    }
//                    else if (effectRolled == 6)
//                    {
//                        effect = "wither";
//                    }

//                    if (effect != "adderal" && effect != "roids" && effect != "icon" && effect != "baps" && effect != "wither")
//                    {
//                        int lvl = int.Parse(plot[1].ToString());
//                        int duration = 4 + lvl * 2;

//                        await ReplyAsync($"{Context.User.Mention} You grew and consumed a lvl {lvl} **{effect}**, duration: {duration} gambles");
//                        GardenAffectedSUser temp = new GardenAffectedSUser(Context.User.Id, effect, duration, lvl);

//                        if(Program.GardenAffectedPlayers.Where(x => x.UserId == Context.User.Id).Count() >= 1)
//                        {
//                            Program.GardenAffectedPlayers.Remove(Program.GardenAffectedPlayers.Where(x => x.UserId == Context.User.Id).FirstOrDefault());
//                        }

//                        Program.GardenAffectedPlayers.Add(temp);

//                    }
//                    else if(effect == "adderal")
//                    {
//                        int lvl = int.Parse(plot[1].ToString());
//                        string text = $" You grew and consumed lvl {lvl} **{effect}**.\nYour beg CD got reset";

//                        await Data.Data.SaveLastBeg(Context.User.Id, DateTime.Now.AddHours(-2));

//                        if(Data.Data.GetPetLevel(Context.User.Id, 1) > 0)
//                        {
//                            if(rad.Next(0, 5 - lvl) == 0)
//                            {
//                                DateTime pinateDate = Data.Data.GetLastDestroy(Context.User.Id);
//                                await Data.Data.SaveLastDestroy(Context.User.Id, pinateDate.AddHours(-1 + -1 * lvl));
//                                text += $"\nYour pinata's CD got reduced by {-1 + -1 * lvl} hours";
//                            }
//                        }
//                        if (Data.Data.GetPetLevel(Context.User.Id, 4) > 0)
//                        {
//                            if (rad.Next(0, 5 - lvl) == 0)
//                            {
//                                DateTime yoinkDate = Data.Data.GetLastYoink(Context.User.Id);
//                                await Data.Data.SaveLastYoink(Context.User.Id, yoinkDate.AddMinutes(-15 + -10 * lvl));
//                                text += $"\nYour Jew's CD got reduced by {-15 + -15 * lvl} minutes";
//                            }
//                        }
//                        if (Data.Data.GetPetLevel(Context.User.Id, 5) > 0)
//                        {
//                            if (rad.Next(0, 5 - lvl) == 0)
//                            {
//                                DateTime rageDate = Data.Data.GetLastRage(Context.User.Id);
//                                await Data.Data.SaveLastRage(Context.User.Id, rageDate.AddMinutes(-30 + -20 * lvl));
//                                text += $"\nYour Tyler's CD got reduced by {-40 + -25 * lvl} minutes";
//                            }
//                        }
//                        await ReplyAsync($"{Context.User.Mention} + {text}");
//                    }
//                    else if (effect == "roids")
//                    {
//                        string pets = Data.Data.GetPets(Context.User.Id);
//                        int petId = int.Parse(char.GetNumericValue(pets[0]).ToString());

//                        for (int i = 1; i < pets.Length; i++)
//                        {
//                            int temp = int.Parse(char.GetNumericValue(pets[i]).ToString());

//                            if (Data.Data.GetPetLevel(Context.User.Id, petId) > Data.Data.GetPetLevel(Context.User.Id, temp))
//                            {
//                                petId = temp;
//                            }
//                        }

//                        if(Data.Data.GetPetLevel(Context.User.Id, petId) == 99)
//                        {
//                            await ReplyAsync($"{Context.User.Mention} your plot has withered.");
//                            return;
//                        }

//                        await Data.Data.SavePetLevels(Context.User.Id, petId, Data.Data.GetPetLevel(Context.User.Id, petId) + 1, false);
//                        await ReplyAsync($"{Context.User.Mention} You plot's yield was roids which was stolen and consumed by {Program.GetPetName(petId)}, his level has gone up.");
//                    }
//                    else if(effect == "baps")
//                    {
//                        int lvl = int.Parse(plot[1].ToString());

//                        int baps = rad.Next(30, 40 + lvl * 40);
//                        await ReplyAsync($"{Context.User.Mention} You grew **{baps}** baps");

//                        await Data.Data.SaveBalance(Context.User.Id, baps, false);

//                    }
//                    else if(effect == "icon")
//                    {
//                        int chosen = 4;

//                        List<int> allPictures = Data.Data.GetPictures(Context.User.Id);
//                        if (allPictures.Count >= Program.PictureCount - 1)
//                        {
//                            await ReplyAsync($"{Context.User.Mention} the plant has withered");
//                            return;
//                        }
//                        do
//                        {
//                            chosen = rad.Next(4, Program.PictureCount + 1);
//                        } while (allPictures.Contains(chosen));

//                        await ReplyAsync($"{Context.User.Mention} You grew an icon #{chosen}");
//                        await Data.Data.UpdatePictures(Context.User.Id, chosen);

//                        if (Program.CompletedIconBlock(Context.User.Id, chosen))
//                        {
//                            List<int> newAllPictures = Data.Data.GetPictures(Context.User.Id);
//                            List<int> Specials = new List<int>();

//                            foreach (int item in newAllPictures)
//                            {
//                                if (item > 1000)
//                                {
//                                    Specials.Add(item);
//                                }
//                            }
//                            int tmp;

//                            do
//                            {
//                                tmp = 1000 + rad.Next(1, 8);
//                            } while (Specials.Contains(tmp));

//                            await ReplyAsync($"Upon completing a full icon page, you got a special icon #{tmp}, type 'kush icons specials'");

//                            await Data.Data.UpdatePictures(Context.User.Id, tmp);
//                        }
//                    }
//                    else if(effect == "wither")
//                    {
//                        await ReplyAsync($"{Context.User.Mention} your plant has withered :(");
//                    }

//                }

//            }
//            else
//            {
//                await ReplyAsync($"{Context.User.Mention} SERIOUS reTARd");
//                return;
//            }

//        }

//        [Command("plant")]
//        public async Task PlantSeed(int id)
//        {
//            if(id <= 0 || id > 9)
//            {
//                await ReplyAsync($"{Context.User.Mention} That's not a valid plot #, DAUN");
//                return;
//            }
//            List<string> plots = Data.Data.GetPlots(Context.User.Id);
//            string plot = plots[id - 1];

//            if(plot[0] != 'g')
//            {
//                await ReplyAsync($"{Context.User.Mention} The fuck are you trying to do??");
//                return;
//            }

//            if(plot[11] != '8')
//            {
//                await ReplyAsync($"{Context.User.Mention} There's already a seed in there...");
//                return;
//            }

//            int lvl = int.Parse(plot[1].ToString());
//            int seedCost = 10 + lvl * 20;

//            if(Data.Data.GetBalance(Context.User.Id) < seedCost)
//            {
//                await ReplyAsync($"{Context.User.Mention} too poor for seed... smh...");
//                return;
//            }

//            string newPlot = plot.Substring(0, 2) + $"{DateTime.Now.AddHours(-1 * (lvl - 1))}";
//            plots[id - 1] = newPlot;

//            await Data.Data.SavePlots(Context.User.Id, plots);
//            await Data.Data.SaveBalance(Context.User.Id, seedCost * -1, false);
//            await ReplyAsync($"{Context.User.Mention} you succesfully bought a seed for {seedCost} baps");
//        }

//        [Command("Plant all")]
//        public async Task PlantSeed()
//        {
//            List<string> plots = Data.Data.GetPlots(Context.User.Id);

//            int c = 0;

//            foreach (var item in plots)
//            {
//                if(item[0] == 'g')
//                {
//                    await PlantSeed(c + 1);
//                    if(Data.Data.GetBalance(Context.User.Id) < 30)
//                    {
//                        break;
//                    }
//                }
//                c++;
//            }
//        }

//        [Command("buy")]
//        public async Task BuyPlot()
//        {
//            int plotPrice = Data.Data.GetPlotPrice(Context.User.Id);

//            if(Data.Data.GetBalance(Context.User.Id) < plotPrice)
//            {
//                await ReplyAsync($"{Context.User.Mention} fuck outta here gay");
//                return;
//            }

//            List<string> plots = Data.Data.GetPlots(Context.User.Id);
//            List<string> newPlots = new List<string>();

//            bool found = false;

//            foreach (string item in plots)
//            {
//                if(item == "0" && !found)
//                {
//                    found = true;
//                    newPlots.Add($"g1{DateTime.Now}");
//                }
//                else
//                {
//                    newPlots.Add(item);
//                }
//            }


//            await Data.Data.SaveBalance(Context.User.Id, -1 * plotPrice, false);
//            await Data.Data.SavePlots(Context.User.Id, newPlots);

//            await ReplyAsync($"{Context.User.Mention} you bought a new plot for {plotPrice} baps! gz.");
//        }

//        [Command("transform")]
//        public async Task TransformPlot(int plotNumber, [Remainder]string type) 
//        {
//            type = type.ToLower();
//            //if(type != "garden" && type != "quarry" && type != "barracks")
//            if (type != "garden" && type != "quarry" && type != "hatchery" && type != PlotType.Abuse.ToString().ToLower())
//            {
//                await ReplyAsync($"{Context.User.Mention} That's not a valid type, mongoloid");
//                return;
//            }
//            int transCost = 300;

//            if(Data.Data.GetBalance(Context.User.Id) < transCost)
//            {
//                await ReplyAsync($"{Context.User.Mention} THE COST IS {transCost} RETARD");
//                return;
//            }

//            if(Data.Data.GetPets(Context.User.Id).Length < 1)
//            {
//                await ReplyAsync($"{Context.User.Mention} You should probably get atleast 1 pet");
//                return;
//            }

//            List<string> plots = Data.Data.GetPlots(Context.User.Id);

//            if(plots[plotNumber - 1][0] == type[0])
//            {
//                await ReplyAsync($"{Context.User.Mention} cant transform into something it already IS, APE");
//                return;
//            }
//            if (plots[plotNumber - 1][0] == '0')
//            {
//                await ReplyAsync($"{Context.User.Mention} YOU DONT EVEN HAVE THAT MANY PLOTS DUMB BITCH");
//                return;
//            }
//            if (plots[plotNumber - 1][0] == 'a' && plots[plotNumber - 1][2] != 'f' && plots[plotNumber - 1][2] != 'x')
//            {
//                await ReplyAsync($"{Context.User.Mention} Cant transform plot while pet is being abused L");
//                return;
//            }

//            string transformedPlot = $"{type[0]}{plots[plotNumber-1][1]}";
//            if(type[0] == 'h')
//            {
//                transformedPlot += "f";            }
//            else if(type[0] == 'a')
//            {
//                transformedPlot += "x";
//                transformedPlot += DateTime.Now.AddHours(4);
//            }

//            if(type[0] != 'a')
//                transformedPlot += DateTime.Now;

//            plots[plotNumber - 1] = transformedPlot;

//            await Data.Data.SavePlots(Context.User.Id, plots);

//            await Data.Data.SaveBalance(Context.User.Id, -1 * transCost, false);

//            await ReplyAsync($"{Context.User.Mention} Your plot #{plotNumber} was transfomed into a {type} for {transCost} baps");

//        }
//    }
//}
