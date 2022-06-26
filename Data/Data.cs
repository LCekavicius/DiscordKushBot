using KushBot.Resources.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System.IO;
using System.Data;
using Microsoft.Data.Sqlite;
using KushBot.DataClasses;

namespace KushBot.Data
{
    public static class Data
    {
        public static async Task AddFollowRarity(ulong UserId, string rarity)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if(DbContext.RarityFollow.Any(x => x.fk_UserId == UserId && x.Rarity.Equals(rarity)))
                    return;

                DbContext.RarityFollow.Add(new RarityFollow(UserId, rarity));
                await DbContext.SaveChangesAsync();
            }
        }

        public static async Task RemoveFollowRarity(ulong UserId, string rarity)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if(!DbContext.RarityFollow.Any(x => x.fk_UserId == UserId && x.Rarity.Equals(rarity)))
                    return;

                DbContext.RarityFollow.Remove(DbContext.RarityFollow.Where(x => x.fk_UserId == UserId && x.Rarity.Equals(rarity)).FirstOrDefault());
                await DbContext.SaveChangesAsync();
            }
        }


        public static List<ulong> GetFollowingByRarity(string rarity)
        {
            using (var DbContext = new SqliteDbContext())
            {
                List<RarityFollow> list = DbContext.RarityFollow.Where(x => x.Rarity.Equals(rarity)).ToList();
                return list.Select(x => x.fk_UserId).ToList();
            }
        }


        public static SUser GetJew(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    DbContext.Jews.Add(new SUser(UserId, 30, false));


                return DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
            }
        }

        public static int GetTicketMultiplier(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    DbContext.Jews.Add(new SUser(UserId, 30, false));


                return DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault().TicketMultiplier;
            }
        }

        public static async Task IncrementTicketMultiplier(ulong UserId, int increase = 1)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    DbContext.Jews.Add(new SUser(UserId, 30, false));


                SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();

                Current.TicketMultiplier += increase;
                DbContext.Jews.Update(Current);
                await DbContext.SaveChangesAsync();
            }
        }

        public static async Task ResetTicketMultiplier(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    DbContext.Jews.Add(new SUser(UserId, 30, false));


                SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();

                Current.TicketMultiplier = 1;
                DbContext.Jews.Update(Current);
                await DbContext.SaveChangesAsync();
            }
        }

        public static string GetNyaMarry(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    DbContext.Jews.Add(new SUser(UserId, 30, false));


                return DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault().NyaMarry;
            }
        }

        public static async Task SaveNyaMarry(ulong UserId, string filePath)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    DbContext.Jews.Add(new SUser(UserId, 30, false));


                SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();

                Current.NyaMarry = filePath;
                DbContext.Jews.Update(Current);
                await DbContext.SaveChangesAsync();
            }
        }

        public static DateTime GetNyaMarryDate(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    DbContext.Jews.Add(new SUser(UserId, 30, false));


                return DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault().NyaMarryDate;
            }
        }

        public static async Task AddToNyaMarryDate(ulong UserId, int hours)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    DbContext.Jews.Add(new SUser(UserId, 30, false));


                SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();

                Current.NyaMarryDate = DateTime.Now.AddHours(hours);
                DbContext.Jews.Update(Current);
                await DbContext.SaveChangesAsync();
            }
        }


        public static int GetUserCheems(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    DbContext.Jews.Add(new SUser(UserId, 30, false));


                return DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault().Cheems;
            }
        }

        public static async Task AddUserCheems(ulong UserId, int amount)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    DbContext.Jews.Add(new SUser(UserId, 30, false));


                SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();

                Current.Cheems += amount;
                DbContext.Jews.Update(Current);
                await DbContext.SaveChangesAsync();
            }
        }

        public static async Task SaveEquipedItem(ulong UserId, int itemSlot, int itemId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    switch (itemSlot)
                    {
                        case 1:
                            Current.FirstItemId = itemId;
                            break;

                        case 2:
                            Current.SecondItemId = itemId;
                            break;
                        case 3:
                            Current.ThirdItemId = itemId;
                            break;
                        default:
                            Current.FourthItemId = itemId;
                            break;
                    }
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public static async Task DestroyItem(int itemId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                Item Current = DbContext.Item.Where(x => x.Id == itemId).FirstOrDefault();

                List<ItemPetConn> petcons = DbContext.ItemPetBonus.Where(x => x.ItemId == Current.Id).ToList();
                foreach (var item in petcons)
                {
                    DbContext.ItemPetBonus.Remove(item);
                }

                DbContext.Item.Remove(Current);
             
                await DbContext.SaveChangesAsync();
            }
        }

        public static int GetEquipedItem(ulong UserId, int itemSlot)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                switch (itemSlot)
                {
                    case 1:
                        return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.FirstItemId).FirstOrDefault();

                    case 2:
                        return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.SecondItemId).FirstOrDefault();

                   case 3:
                        return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.ThirdItemId).FirstOrDefault();

                    default:
                        return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.FourthItemId).FirstOrDefault();

                }
            }
        }

        public static List<string> ReadItems()
        {
            string path = @"Data/Items";

            if (Program.BotTesting)
                path = @"C:\Users\Laurel\Desktop\KushBot\KushBot\Data\Items";

            string[] files = Directory.GetFiles(path);

            List<string> ret = new List<string>();

            foreach (var item in files)
            {
                int end = 0;
                int start = 0;
                for (int i = item.Length - 1; i > 0; i--)
                {
                    if (end != 0 && start != 0)
                    {
                        continue;
                    }
                        

                    if (item[i] == '.')
                    {
                        end = i;
                    }
                    if (item[i] == '/' || item[i] == '\\')
                    {
                        start = i;
                    }
                }

                ret.Add(item.Substring(start+1, end - start - 1));
            }

            return ret;

        }

        private static string GetConnectionString()
        {
            if (Program.BotTesting)
                return $@"Data Source= D:/KushBot/Kush Bot/KushBot/KushBot/Data/Database.sqlite";
            else
                return $@"Data Source= Data/Database.sqlite";
        }

        public static List<Item> GetUserItems(ulong ownerId)
        {
            List<Item> items = new List<Item>();

            var conn = new SqliteConnection(GetConnectionString());
            conn.Open();
            SqliteCommand cmd = new SqliteCommand($"SELECT item.Id, item.OwnerId, item.name, item.BossDmg, item.AirdropFlat," +
                $" item.AirdropPercent, item.QuestSlot, item.QuestBapsFlat, item.QuestBapsPercent," +
                $" COALESCE(ItemPetBonus.PetId, 0) AS PetId, COALESCE(ItemPetBonus.LvlBonus, 0) AS LvlBonus, item.Rarity, item.Level, ItemPetBonus.Id "+
                $"FROM Item LEFT JOIN ItemPetBonus ON Item.Id = ItemPetBonus.ItemId Where Item.OwnerId = {ownerId}");

            cmd.Connection = conn;
            var result = cmd.ExecuteReader();



            bool res;

            Item temp = new Item();

            while (res = result.Read())
            {
                temp = new Item(ownerId,
                result[2].ToString(),
                int.Parse(result[3].ToString()),
                int.Parse(result[4].ToString()),
                double.Parse(result[5].ToString()),
                int.Parse(result[6].ToString()),
                int.Parse(result[7].ToString()),
                double.Parse(result[8].ToString()),
                int.Parse(result[11].ToString()),
                int.Parse(result[12].ToString()),
                int.Parse(result[0].ToString())
                );


                

                if (int.Parse(result[10].ToString()) != 0)
                {
                    temp.ItemPetConns.Add(new ItemPetConn(
                        int.Parse(result[9].ToString()),
                        int.Parse(result[10].ToString()),
                        int.Parse(result[0].ToString()),
                        int.Parse(result[13].ToString())
                    ));
                }

                if (items.Exists(x => x.Id == temp.Id))
                {
                    items.Where(x => x.Id == temp.Id).FirstOrDefault().ItemPetConns.Add(new ItemPetConn(
                         int.Parse(result[9].ToString()),
                        int.Parse(result[10].ToString()),
                        int.Parse(result[0].ToString()),
                        int.Parse(result[13].ToString())
                        ));
                }
                else
                {
                    items.Add(temp);
                }
            }

            conn.Close();

            return items;

            
        }

        public static async Task UpgradeItem(Item item)
        {
            Random rnd = new Random();

            int itemStat = GetItemStat();
            int itemStatBonus = GetItemStatBonus(itemStat, item.Rarity);

            if (itemStat < 7)
            {
                if (item.ItemPetConns.Where(x => x.PetId == itemStat - 1).Count() > 0)
                {
                    item.ItemPetConns.Where(x => x.PetId == itemStat - 1).FirstOrDefault().LvlBonus += itemStatBonus;
                }
                else
                {
                    var petcon = new ItemPetConn(itemStat - 1, itemStatBonus);
                    petcon.ItemId = item.Id;
                    item.ItemPetConns.Add(petcon);
                }

            }
            else
            {
                switch (itemStat)
                {
                    case 7:
                        item.BossDmg += itemStatBonus;
                        break;
                    case 8:
                        item.AirDropFlat += itemStatBonus;
                        break;
                    case 9:
                        item.AirDropPercent += itemStatBonus;
                        break;
                    case 10:
                        item.QuestSlot += itemStatBonus;
                        break;
                    case 11:
                        item.QuestBapsFlat += itemStatBonus;
                        break;
                    case 12:
                        item.QuestBapsPercent += itemStatBonus;
                        break;

                }
            }


            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Item.Where(x => x.Id == item.Id).Count() < 1)
                    return;
              
                Console.WriteLine(item.ItemPetConns.Count);

                foreach (var petc in item.ItemPetConns)
                {

                    DbContext.ItemPetBonus.Update(petc);
                }

                item.ItemPetConns.RemoveAll(x => true);

                DbContext.Item.Update(item);
                await DbContext.SaveChangesAsync();
            }
        }

        public static Item GenerateItem(ulong ownerId, int rarity = 1)
        {
            Random rnd = new Random();

            int pick = rnd.Next(0, Program.ItemPaths.Count);

            string chosenItem = Program.ItemPaths[pick];

            int rolls = rarity + 1;

            Item item = new Item(ownerId, chosenItem);

            item.Rarity = rarity;

            for (int i = 0; i < rolls; i++)
            {
                int itemStat = GetItemStat();
                int itemStatBonus = GetItemStatBonus(itemStat, rarity);

                if(itemStat < 7)
                {
                    if(item.ItemPetConns.Where(x=> x.PetId == itemStat - 1).Count() > 0)
                    {
                        item.ItemPetConns.Where(x => x.PetId == itemStat - 1).FirstOrDefault().LvlBonus += itemStatBonus;
                    }
                    else
                    {
                        var petcon = new ItemPetConn(itemStat - 1, itemStatBonus);
                        item.ItemPetConns.Add(petcon);
                    }
                    
                }
                else
                {
                    switch (itemStat)
                    {
                        case 7:
                            item.BossDmg += itemStatBonus;
                            break;
                        case 8:
                            item.AirDropFlat += itemStatBonus;
                            break;
                        case 9:
                            item.AirDropPercent += itemStatBonus;
                            break;
                        case 10:
                            item.QuestSlot += itemStatBonus;
                            break;
                        case 11:
                            item.QuestBapsFlat += itemStatBonus;
                            break;
                        case 12:
                            item.QuestBapsPercent += itemStatBonus;
                            break;

                    }
                }
                
            }


            var conn = new SqliteConnection(GetConnectionString());
            conn.Open();
            SqliteCommand cmd = new SqliteCommand(item.BuildQuery());

            cmd.Connection = conn;
            cmd.ExecuteNonQuery();

            cmd.CommandText = "select last_insert_rowid()";
            Int64 id64 = (Int64)cmd.ExecuteScalar();
            int id = (int)id64;

            conn.Close();


            string petConCmd = item.BuildPetConQuery(id);

            if (petConCmd.Length == 0)
                return item;

            conn.Open();
            cmd = new SqliteCommand(petConCmd);

            cmd.Connection = conn;
            cmd.ExecuteNonQuery();
            conn.Close();

            return item;

        }

        private static int GetItemStatBonus(int pickedStat, int rarity)
        {
            Random rnd = new Random();
            int ret = 0;
            if (pickedStat <= 7)
            {
                ret += rnd.Next(1 + rarity / 4, 4 +  rarity / 3);
                if (pickedStat == 7)
                    ret += 2 + rarity / 4;
            }
            else if(pickedStat == 8 || pickedStat == 11)
            {
                for (int i = 0; i < rarity; i++)
                {
                    ret += rnd.Next(25 / (i+1), 45 / (i + 1));
                }
            }
            else if(pickedStat == 10)
            {
                for (int i = 0; i < rarity; i++)
                {
                    ret += rnd.Next(15 / (i + 1), 25 / (i + 1));
                }
            }
            else if(pickedStat == 9 || pickedStat == 12)
            {
                for (int i = 0; i < rarity; i++)
                {
                    ret += rnd.Next(10 / (i+1), 20 / (i + 1));
                }
            }

            return ret;
        }
        //1 - petId 0
        //2 - petId 1
        //3 - petId 2
        //4 - petId 3
        //5 - petId 4
        //6 - petId 5
        //7 - BossDmg
        //8 - AirDropFlat
        //9 - AirDropPercent
        //10 - QuestSlot
        //11 - QuestBapsFlat
        //12 - QuestBapsPercent
        private static int GetItemStat()
        {
            Random rnd = new Random();
            //12
            return rnd.Next(1, 13);
        }
        public async static Task HandleAbuseChamber(ulong id)
        {
            List<string> plots = GetPlots(id);
            bool needsChange = false;

            for (int i = 0; i < plots.Count; i++)
            {
                if (plots[i].Equals("0"))
                {
                    break;
                }

                string plot = plots[i];
                if(plot[0] == 'a')
                {
                    //found abuse
                    DateTime plotDt = DateTime.Parse(plot.Substring(3));

                    if(plotDt < DateTime.Now && plot[2] != 'f' && plot[2] != 'x')
                    {
                        plots.RemoveAt(i);
                        plots.Insert(i, $"{plot[0]}{plot[1]}x{plotDt.AddHours(6)}");
                        //plots[i] = ;
                        needsChange = true;
                    }
                    else if (plotDt < DateTime.Now && plot[2] == 'x')
                    {
                        plots.RemoveAt(i);
                        plots.Insert(i, $"{plot[0]}{plot[1]}f{DateTime.Now}");
                        needsChange = true;
                    }
                }

            }

            if (needsChange)
                await SavePlots(id, plots);
        }

        public static int GetPetAbuseStrength(ulong id, int petId)
        {
            List<string> plots = GetPlots(id);
            int plotid = 0;
            bool isFound = false;
            foreach (var item in plots)
            {
                if (item[0] == 'a' && item[2] == char.Parse(petId.ToString()))
                {
                    isFound = true;
                    break;
                }
                plotid++;
            }
            if (!isFound)
                return 0;

            return int.Parse(plots[plotid][1].ToString());
        }

        public static int GetPetAbuseSupernedStrength(ulong id, int petId)
        {
            List<string> plots = GetPlots(id);
            int plotid = 0;
            bool isFound = false;
            foreach (var item in plots)
            {
                if (item[0] == 'a' && item[2] == '0')
                {
                    isFound = true;
                    break;
                }
                plotid++;
            }

            if (!isFound)
                return 0;

            if (plots[plotid][2].ToString().Equals(petId.ToString()))
            {
                if (plots[plotid][1] == '1')
                    return 13;
                else if (plots[plotid][1] == '2')
                    return 26;
                else
                    return 39;
                
            }
            return 0;
        }

        public static async Task ResetWeeklyStuff(List<SUser> Jews)
        {
            using (var cnn = new SqliteDbContext())
            {
                var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                SqliteCommand cmd = new SqliteCommand("Update jews set LostBapsWeekly=0, WonBapsWeekly=0, LostFlipsWeekly = 0," +
                    " WonFlipsWeekly = 0, WonBetsWeekly = 0, LostBetsWeekly = 0," +
                    " WonRisksWeekly = 0, LostRisksWeekly = 0," +
                    " BegsWeekly = 0, CompletedWeeklies = '0,0'");

                cmd.Connection = conn;
                cmd.ExecuteNonQuery();
                conn.Close();
                Random rad = new Random();
                conn.Open();
            }
        }

        public static void ResetDailyStuff(List<SUser> Jews)
        {
            using (var cnn = new SqliteDbContext())
            {
                var conn = new SqliteConnection(GetConnectionString());
                conn.Open();
                SqliteCommand cmd = new SqliteCommand("Update jews set LostBapsMN=0, WonBapsMN=0, LostFlipsMN = 0," +
                    " WonFlipsMN = 0, WonBetsMN = 0, LostBetsMN = 0," +
                    " WonRisksMN = 0, LostRisksMN = 0," +
                    " BegsMN = 0, FailedYoinks = 0, SuccesfulYoinks = 0," +
                    " WonFlipChainOverFifty = 0, WonDuelsMN = 0, DailyGive = 3000");

                cmd.Connection = conn;
                cmd.ExecuteNonQuery();
                conn.Close();
                Random rad = new Random();
                conn.Open();
                foreach (var item in Jews)
                {
                    int dupe;
                    if (!char.IsDigit(item.PetDupes[6]))
                        dupe = 0;
                    else
                        dupe = int.Parse(item.PetDupes.Split(',')[3]);

                    int GuaranteedExtras = 25 * GetPetTier(dupe);

                    List<Item> items = GetUserItems(item.Id);
                    //items

                    List<int> equiped = new List<int>();
                    for (int i = 0; i < 4; i++)
                    {
                        equiped.Add(item.FirstItemId);
                        equiped.Add(item.SecondItemId);
                        equiped.Add(item.ThirdItemId);
                        equiped.Add(item.FourthItemId);

                        if (equiped[i] != 0)
                        {
                            Item tempItem = items.Where(x => x.Id == equiped[i]).FirstOrDefault();
                            if (tempItem.QuestSlot != 0)
                            {
                                GuaranteedExtras += tempItem.QuestSlot;
                            }
                        }
                    }

                    int add = GuaranteedExtras / 100;
                    int r = rad.Next(1, 101);
                    if (r < GuaranteedExtras % 100)
                        add++;

                    int QuestsForPlayer = 3 + add;
                    char[] pets = item.Pets.ToCharArray();
                    List<int> options = new List<int>();
                    for (int i = 0; i < Program.Quests.Count; i++)
                    {

                        if(((i == 15 || i == 16) && !pets.Contains('4')) || (i == 13 && pets.Length == 0))
                        {

                        }
                        else
                        {
                            options.Add(i);
                        }
                           
                    }

                    string temp = "";

                    List<int> picks = options.OrderBy(x => rad.Next()).Take(QuestsForPlayer).ToList();

                    temp = string.Join(',', picks);

                    SqliteCommand cmdtemp = new SqliteCommand($"update Jews set QuestIndexes = '{temp}' where id={item.Id}");

                    cmdtemp.Connection = conn;

                    cmdtemp.ExecuteNonQuery();

                }


                
                conn.Close();
            }
        }

        public static List<string> ReadWeebShit()
        {
            string path = @"Data/Kemonos";

            string[] files = Directory.GetFiles(path);

            return files.ToList();

        }

        public static List<string> ReadCarShit()
        {
            string path = @"Data/Cars";

            string[] files = Directory.GetFiles(path);

            return files.ToList();

        }

        public static async Task MakeRowForJew(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    SUser newJew = new SUser(UserId, 30, false);
                    DbContext.Jews.Add(newJew);

                    string path = @"D:\KushBot\Kush Bot\KushBot\KushBot\Data\";
                    char seperator = '\\';

                    if (!Program.BotTesting)
                    {
                        seperator = '/';
                        path = @"Data/";
                    }
                    try
                    {
                        System.IO.File.Copy($@"{path}Pictures{seperator}{newJew.SelectedPicture}.jpg", $@"{path}Portraits{seperator}{newJew.Id}.png");
                    }
                    catch { }




                    await DbContext.SaveChangesAsync();
                }
            }
        }

        public static void RaceFinished()
        {
            using(StreamReader reader = new StreamReader(@"WeeklyQuests.txt"))
            {
                string line = reader.ReadLine();
                Console.WriteLine(line);

                string[] values = line.Split(',');

                values[2] = "-1";
                string newvalue = string.Join(",", values);

                reader.Close();

                File.WriteAllText(@"WeeklyQuests.txt", newvalue);

            }
        }

        public static List<int> GetWeeklyQuest()
        {
            using(StreamReader reader = new StreamReader(@"WeeklyQuests.txt"))
            {
                string[] values = reader.ReadLine().Split(',');
                List<int> ret = new List<int>();

                for (int i = 0; i < 3; i++)
                {
                    ret.Add(int.Parse(values[i]));
                }
                reader.Close();

                return ret;
            }
        }

        public static async Task SaveDailyGiveBaps(ulong UserId, int subtraction)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();

                    Current.DailyGive -= subtraction;

                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public static int GetRemainingDailyGiveBaps(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.DailyGive).FirstOrDefault();

            }
        }

        public static int GetCompletedWeekly(ulong UserId, int qInd)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                string weeklies = DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.CompletedWeeklies).FirstOrDefault();
                weeklies = weeklies.Replace(",", "");
                return int.Parse(weeklies[qInd].ToString());

            }
        }

        public static bool CompletedAllWeeklies(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return false;

                string weeklies = DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.CompletedWeeklies).FirstOrDefault();

                List<string> weeklyList = weeklies.Split(',').ToList();

                bool ret = weeklyList.All(x => x == "1");

                return ret;
            }

            //List<int> Weeklies = GetWeeklyQuest();
            //Weeklies.RemoveAt(2);

            //bool ret = false;
            //for (int i = 0; i < 2; i++)
            //{
            //    if (!Program.WeeklyQuests[Weeklies[i]].HasCompleted(UserId))
            //    {
            //        Console.WriteLine($"{Weeklies[i]} not completed");
            //        ret = !ret;
            //    }
            //}
            //return ret;
        }

        public static async Task ResetCompletedWeekly(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();

                    string save = $"0,0";

                    Current.CompletedWeeklies = save;

                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public static async Task SaveCompletedWeekly(ulong UserId, int id)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    string[] weeklies = Current.CompletedWeeklies.Split(',');
                    weeklies[id] = "1";

                    string save = $"{weeklies[0]},{weeklies[1]}";

                    Current.CompletedWeeklies = save;

                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public static void SetWeeklyQuests()
        {
            using (StreamWriter writer = new StreamWriter(@"WeeklyQuests.txt"))
            {

                List<int> qs = new List<int>();
                Random rad = new Random();

                qs.Add(rad.Next(0, Program.WeeklyQuests.Count));

                int temp = rad.Next(0, Program.WeeklyQuests.Count);

                while (qs.Contains(temp))
                {
                    temp = rad.Next(0, Program.WeeklyQuests.Count);
                }
                qs.Add(temp);
                while (qs.Contains(temp))
                {
                    temp = rad.Next(0, Program.WeeklyQuests.Count);
                }
                qs.Add(temp);
                writer.Write($"{qs[0]},");
                writer.Write($"{qs[1]},");
                writer.Write($"{qs[2]}");
                writer.Close();
            }
        }

        


        public static int GetRageDuration(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.RageDuration).FirstOrDefault();
            }
        }

        public static int GetRageCash(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.RageCash).FirstOrDefault();
            }
        }

        public static DateTime GetLastRage(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return DateTime.Now.AddHours(-9);

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.LastTylerRage).FirstOrDefault();
            }
        }

        public static DateTime GetLastYoink(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return DateTime.Now.AddHours(-9);

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.LastYoink).FirstOrDefault();
            }
        }

        public static DateTime GetLastDestroy(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return DateTime.Now.AddHours(-9);

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.LastDestroy).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets random pet id, owned by a user and not capped on level
        /// </summary>
        public static int GetRandomPetId(ulong userId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == userId).Count() < 1)
                    return -1;

                string temp = DbContext.Jews.Where(x => x.Id == userId).Select(x => x.PetLevels).FirstOrDefault();
                List<string> values = temp.Split(',').ToList();

                List<int> BYBYNAXUIDEJAUANTSITOSXUJNIOSKRW = new List<int>();

                for (int i = 0; i < Program.Pets.Count; i++)
                {
                    BYBYNAXUIDEJAUANTSITOSXUJNIOSKRW.Add(int.Parse(values[i]));
                }
                BYBYNAXUIDEJAUANTSITOSXUJNIOSKRW = BYBYNAXUIDEJAUANTSITOSXUJNIOSKRW.OrderBy(x => x).ToList();

                for (int i = 0; i < 6; i++)
                {
                    if(BYBYNAXUIDEJAUANTSITOSXUJNIOSKRW[i] != 99 && BYBYNAXUIDEJAUANTSITOSXUJNIOSKRW[i] != 0)
                    {
                        return values.IndexOf(BYBYNAXUIDEJAUANTSITOSXUJNIOSKRW[i].ToString());
                    }
                }
                return -1;

            }

        }

        public static int GetItemPetLevel(ulong UserId, int PetIndex)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                SUser jew = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();

                List<int> Equipped = new List<int>();
                Equipped.Add(jew.FirstItemId);
                Equipped.Add(jew.SecondItemId);
                Equipped.Add(jew.ThirdItemId);
                Equipped.Add(jew.FourthItemId);

                List<Item> Items = GetUserItems(UserId);

                int ret = 0;

                for (int i = 0; i < 4; i++)
                {
                    if(Equipped[i] != 0)
                    {
                        Item temp = Items.Where(x => x.Id == Equipped[i]).FirstOrDefault();

                        foreach (var item in temp.ItemPetConns)
                        {
                            if(item.PetId == PetIndex)
                            {
                                ret += item.LvlBonus;
                            }
                        }

                    }
                }

                return ret;
            }
        }

        public static int GetPetLevel(ulong UserId, int PetIndex)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                string temp = DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.PetLevels).FirstOrDefault();
                string[] values = temp.Split(',');

                return int.Parse(values[PetIndex]) + GetItemPetLevel(UserId, PetIndex);
            }
        }
        public static int GetPetDupe(ulong UserId, int PetIndex)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                string temp = DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.PetDupes).FirstOrDefault();
                string[] values = temp.Split(',');

                return int.Parse(values[PetIndex]);
            }
        }

        public static string GetPets(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return "";

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.Pets).FirstOrDefault();
            }
        }

        public static bool GetEgg(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return false;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.HasEgg).FirstOrDefault();
            }
        } 
        //balance
        public static int GetBalance(ulong UserId)
        {
            using(var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.Balance).FirstOrDefault();
            }
        }
        public static DateTime GetLastBeg(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return DateTime.Now.AddHours(-9);

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.LastBeg).FirstOrDefault();
            }
        }

        public static int GetTotalYikes(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.Yiked).FirstOrDefault();
            }
        }

        public static DateTime GetYikeDate(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return DateTime.Now;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.YikeDate).FirstOrDefault();
            }
        }

        public static DateTime GetRedeemDate(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return DateTime.Now;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.RedeemDate).FirstOrDefault();
            }
        }

        public static async Task AddYike(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.Yiked += 1;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public static async Task SaveYikeDate(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.YikeDate = DateTime.Now;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public static async Task SaveRedeemDate(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.RedeemDate = DateTime.Now;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }
        //Saving
        /// <summary>
        /// Saving Starts here
        /// </summary>awa
        /// <param name="UserId"></param>
        /// <param name="rageDuration"></param>
        /// <returns></returns>
        public static async Task SaveRageDuration(ulong UserId, int rageDuration)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.RageDuration += rageDuration;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public static async Task SaveRageCash(ulong UserId, int rageCash)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.RageCash += rageCash;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public static async Task SaveLastRage(ulong UserId, DateTime lastRage)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.LastTylerRage = lastRage;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public static async Task SaveLastYoink(ulong UserId, DateTime lastYoink)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.LastYoink = lastYoink;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public static async Task SaveLastDestroy(ulong UserId, DateTime lastDestroy)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.LastDestroy = lastDestroy;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public static async Task SavePetLevels(ulong UserId, int PetIndex, int PetLevel, bool NewPet)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();

                    string temp = DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.PetLevels).FirstOrDefault();
                    string[] values = temp.Split(',');

                    values[PetIndex] = PetLevel.ToString();

                    string result = String.Join(",", values);


                    Current.PetLevels = result;

                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public static async Task SavePetDupes(ulong UserId, int PetIndex, int PetDupe)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();

                    string temp = DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.PetDupes).FirstOrDefault();
                    string[] values = temp.Split(',');

                    values[PetIndex] = PetDupe.ToString();

                    string result = String.Join(",", values);

                    Current.PetDupes = result;

                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public static List<string> GetPlots(ulong userId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == userId).Count() < 1)
                    return new List<string>();

                string temp = DbContext.Jews.Where(x => x.Id == userId).Select(x => x.Plots).FirstOrDefault();
                return temp.Split(',').ToList();
            }
        }

        public static int GetPlotPrice(ulong userId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == userId).Count() < 1)
                    return 0;

                string temp = DbContext.Jews.Where(x => x.Id == userId).Select(x => x.Plots).FirstOrDefault();
                List<string> plots = temp.Split(',').ToList();
                int ret = 1000;
                int c = 0;
                foreach (string item in plots)
                {
                    if(item != "0")
                    {
                        c++;
                    }
                }
                if(c !=0)
                     ret = 1000 + 500 * (int)Math.Pow(2, c);

                return ret;
            }
        }

        public static async Task SavePlots(ulong UserId, List<string> newPlots)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    Console.WriteLine("bug");
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    string temp = string.Join(',', newPlots);
                    Current.Plots = temp;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public static async Task SavePets(ulong UserId, int PetIndex)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.Pets += PetIndex;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public static async Task SaveEgg(ulong UserId, bool HasEgg)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.HasEgg = HasEgg;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }
        /// <summary>
        /// Balance saving
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="Amount"></param>
        /// <param name="Gambling"></param>
        /// <returns></returns>
        public static async Task SaveBalance(ulong UserId, int Amount, bool Gambling)
        {
            using(var DbContext = new SqliteDbContext())
            {
                if(DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.Balance += Amount;
                    if (Gambling && GetRageDuration(UserId) > 0)
                    {
                        double RageCashDbl = 0;
                        int RageCash = 0;
                        if (Amount > 0)
                        {
                            double petAddition = Math.Pow(GetPetLevel(UserId, 5), 0.85);
                            RageCashDbl = (Amount / 1.33) * (5 + petAddition) / (68 + (GetPetLevel(UserId,5) / 1.5));
                            RageCash = (int)Math.Round(RageCashDbl);

                        }
                        else
                        {
                            RageCashDbl = (int)Math.Round(Amount / 100.5); ;
                            RageCash = (int)Math.Round(RageCashDbl);
                            if(RageCash < 0)
                            {
                                RageCash *= -1;
                            }
                        }
                        if (RageCash < 6 && Amount > 0)
                        {
                            RageCash = 5;
                        }

                        Current.RageCash += RageCash;
                        //Console.WriteLine(RageCash);
                        double chanceNotToConsume = (double)GetPetTier(UserId, 5) * 1.5;
                        Random rnd = new Random();
                        if(rnd.NextDouble() > chanceNotToConsume / 100)
                             Current.RageDuration -= 1;

                        if(Current.RageDuration == 0)
                        {
                            int temp = Current.RageCash;
                            if(temp < 0)
                            {
                                temp = temp * -1;
                            }
                            
                            await Program.EndRage(UserId,Current.RageCash);
                            Current.Balance += temp;
                            Current.RageCash = 0;

                        }
                    }
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }
        public static async Task SaveLastBeg(ulong UserId, DateTime lastBeg)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.LastBeg = lastBeg;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }
        /// <summary>
        /// Quests Start here
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        /// 

        public static List<int> GetPictures(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return new List<int>();

                string text = DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.Pictures).FirstOrDefault();

                List<int> pictures = new List<int>();

                if (text == "")
                {
                    return pictures;
                }

                foreach (string item in text.Split(','))
                {
                    pictures.Add(int.Parse(item));
                }

                return pictures;
            }
        }

        public static async Task UpdatePictures(ulong UserId, int picture)
        {
            using (var DbContext = new SqliteDbContext())
            {
                SUser current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();

                current.Pictures += $",{picture.ToString()}";
                DbContext.Jews.Update(current);

                await DbContext.SaveChangesAsync();
            }       
        }

        public static int GetSelectedPicture(ulong userId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                return DbContext.Jews.Where(x => x.Id == userId).Select(x => x.SelectedPicture).FirstOrDefault();
            }
        }

        public static async Task UpdateSelectedPicture(ulong UserId, int picture)
        {
            using (var DbContext = new SqliteDbContext())
            {
                SUser current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();

                current.SelectedPicture = picture;
                DbContext.Jews.Update(current);

                await DbContext.SaveChangesAsync();
            }
        }

        public static int GetLostBapsMN(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.LostBapsMN).FirstOrDefault();
            }
        }
        public static async Task SaveLostBapsMN(ulong UserId, int lostBapsMn)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.LostBapsMN += lostBapsMn;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }


        public static int GetWonBapsMN(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.WonBapsMN).FirstOrDefault();
            }
        }
        public static async Task SaveWonBapsMN(ulong UserId, int wonBapsMN)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.WonBapsMN += wonBapsMN;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }


        public static int GetWonFlipsMN(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.WonFlipsMN).FirstOrDefault();
            }
        }
        public static async Task SaveWonFlipsMN(ulong UserId, int wonFlipsMN)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.WonFlipsMN += wonFlipsMN;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }


        public static int GetLostFlipsMN(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.LostFlipsMN).FirstOrDefault();
            }
        }
        public static async Task SaveLostFlipsMN(ulong UserId, int lostFlipsMN)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.LostFlipsMN += lostFlipsMN;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }



        public static int GetSuccessfulYoinks(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.SuccesfulYoinks).FirstOrDefault();
            }
        }
        public static async Task SaveSuccessfulYoinks(ulong UserId, int succesfulYoinks)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.SuccesfulYoinks += succesfulYoinks;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }



        public static int GetFailedYoinks(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.FailedYoinks).FirstOrDefault();
            }
        }
        public static async Task SaveFailedYoinks(ulong UserId, int failedYoinks)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.FailedYoinks += failedYoinks;
                    DbContext.Jews.Update(Current);
                    
                }
                await DbContext.SaveChangesAsync();
            }
        }



        public static int GetWonFlipsChain(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.WonFlipChainOverFifty).FirstOrDefault();
            }
        }
        public static async Task SaveWonFlipsChains(ulong UserId, int wonFlipsChain)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.WonFlipChainOverFifty += wonFlipsChain;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }



        public static int GetLostBetsMN(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.LostBetsMN).FirstOrDefault();
            }
        }
        public static async Task SaveLostBetsMN(ulong UserId, int lostBetsMN)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.LostBetsMN += lostBetsMN;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }



        public static int GetWonBetsMN(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.WonBetsMN).FirstOrDefault();
            }
        }
        public static async Task SaveWonBetsMN(ulong UserId, int wonBetsMN)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.WonBetsMN += wonBetsMN;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }



        public static int GetLostRisksMN(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.LostRisksMN).FirstOrDefault();
            }
        }
        public static async Task SaveLostRisksMN(ulong UserId, int lostRisksMN)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.LostRisksMN += lostRisksMN;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }


        public static int GetWonRisksMN(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.WonRisksMN).FirstOrDefault();
            }
        }
        public static async Task SaveWonRisksMN(ulong UserId, int wonRisksMN)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.WonRisksMN += wonRisksMN;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public static int GetBegsMN(ulong UserId)
        {
            using(var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.BegsMN).FirstOrDefault();
            }
        }

        public static async Task SaveBegsMN(ulong UserId, int begsMN)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.BegsMN += begsMN;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }


        public static string GetQuestIndexes(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return "";

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.QuestIndexes).FirstOrDefault();
            }
        }

        public static async Task SaveQuestIndexes(ulong UserId, string questIndexes)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.QuestIndexes = questIndexes;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }


        public static DateTime GetSetDigger(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return DateTime.Now;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.SetDigger).FirstOrDefault();
            }
        }

        public static async Task SaveSetDigger(ulong UserId, DateTime setDigger)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.SetDigger = setDigger;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }



        public static DateTime GetLootedDigger(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return DateTime.Now;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.LootedDigger).FirstOrDefault();
            }
        }

        public static async Task SaveLootedDigger(ulong UserId, DateTime lootedDigger)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.LootedDigger = lootedDigger;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }



        public static int GetDiggerState(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.DiggerState).FirstOrDefault();
            }
        }
        public static async Task SaveDiggerState(ulong UserId, int diggerState)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.DiggerState = diggerState;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public static int GetWonDuelsMn(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.WonDuelsMN).FirstOrDefault();
            }
        }
        public static async Task SaveWonDuelsMn(ulong UserId, int wonDuelsMn)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.WonDuelsMN += wonDuelsMn;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public static int GetLostBapsWeekly(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.LostBapsWeekly).FirstOrDefault();
            }
        }
        public static async Task SaveLostBapsWeekly(ulong UserId, int lostBapsMn)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.LostBapsWeekly += lostBapsMn;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }


        public static int GetWonBapsWeekly(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.WonBapsWeekly).FirstOrDefault();
            }
        }
        public static async Task SaveWonBapsWeekly(ulong UserId, int wonBapsMN)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.WonBapsWeekly += wonBapsMN;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }


        public static int GetWonFlipsWeekly(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.WonFlipsWeekly).FirstOrDefault();
            }
        }
        public static async Task SaveWonFlipsWeekly(ulong UserId, int wonFlipsMN)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.WonFlipsWeekly += wonFlipsMN;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }


        public static int GetLostFlipsWeekly(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.LostFlipsWeekly).FirstOrDefault();
            }
        }
        public static async Task SaveLostFlipsWeekly(ulong UserId, int lostFlipsMN)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.LostFlipsWeekly += lostFlipsMN;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }
        public static int GetWonBetsWeekly(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.WonBetsWeekly).FirstOrDefault();
            }
        }
        public static async Task SaveWonBetsWeekly(ulong UserId, int wonBetsMN)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.WonBetsWeekly += wonBetsMN;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public static int GetLostBetsWeekly(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.LostBetsWeekly).FirstOrDefault();
            }
        }
        public static async Task SaveLostBetsWeekly(ulong UserId, int wonBetsMN)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.LostBetsWeekly += wonBetsMN;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public static int GetLostRisksWeekly(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.LostRisksWeekly).FirstOrDefault();
            }
        }
        public static async Task SaveLostRisksWeekly(ulong UserId, int lostRisksMN)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.LostRisksWeekly += lostRisksMN;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }


        public static int GetWonRisksWeekly(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.WonRisksWeekly).FirstOrDefault();
            }
        }
        public static async Task SaveWonRisksWeekly(ulong UserId, int wonRisksMN)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.WonRisksWeekly += wonRisksMN;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public static int GetBegsWeekly(ulong UserId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.BegsWeekly).FirstOrDefault();
            }
        }

        public static int GetNextPetTierReq(ulong UserId, int petId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                int TierCount = 12;
                int[] DupeReq = new int[TierCount];
                DupeReq[0] = 0;
                DupeReq[1] = 1;

                int petDupe = GetPetDupe(UserId, petId);

                if (petDupe == 0)
                    return 1;
                if (petDupe == 1)
                    return 4;

                for (int i = 2; i < TierCount; i++)
                {
                    DupeReq[i] = (int)Math.Round((2 + (1.2 * DupeReq[i - 1])), 0);
                    if (DupeReq[i] > petDupe)
                        return DupeReq[i];
                }
                return DupeReq[TierCount-1];

            }
        }

        public static int GetPetTier(int dupeCount)
        {
            int TierCount = 12;
            int[] DupeReq = new int[TierCount];
            DupeReq[0] = 0;
            DupeReq[1] = 1;

            int petDupe = dupeCount;

            if (petDupe == 0)
                return 0;
            if (petDupe == 1)
                return 1;

            for (int i = 2; i < TierCount; i++)
            {
                DupeReq[i] = (int)Math.Round((2 + (1.2 * DupeReq[i - 1])));
                if (DupeReq[i] > petDupe)
                    return i - 1; ;
            }
            return TierCount;
        }

        public static int GetPetTier(ulong UserId, int petId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                    return 0;

                int TierCount = 12;
                int[] DupeReq = new int[TierCount];
                DupeReq[0] = 0;
                DupeReq[1] = 1;

                int petDupe = GetPetDupe(UserId, petId);

                if (petDupe == 0)
                    return 0;
                if (petDupe == 1)
                    return 1;

                for (int i = 2; i < TierCount; i++)
                {
                    DupeReq[i] = (int)Math.Round((2 + (1.2 * DupeReq[i - 1])));
                    if (DupeReq[i] > petDupe)
                        return i - 1; ;
                }
                return TierCount;

            }
        }

        public static async Task SaveBegsWeekly(ulong UserId, int begsMN)
        {
            using (var DbContext = new SqliteDbContext())
            {
                if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                {
                    //no row for user, create one
                    DbContext.Jews.Add(new SUser(UserId, 30, false));
                }
                else
                {
                    SUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                    Current.BegsWeekly += begsMN;
                    DbContext.Jews.Update(Current);
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public static int GetTicketCount(ulong userId)
        {
            using (var DbContext = new SqliteDbContext())
            {
                return DbContext.Jews.Where(x => x.Id == userId).Select(x => x.Tickets).FirstOrDefault();
            }
        }

        public static async Task SaveTicket(ulong UserId, bool addition)
        {
            using (var DbContext = new SqliteDbContext())
            {
                SUser current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();

                if (addition)
                {
                    if(GetTicketCount(UserId) < 3)
                       current.Tickets += 1;
                }
                else
                {
                    current.Tickets -= 1;
                }
                DbContext.Jews.Update(current);

                await DbContext.SaveChangesAsync();
            }
        }

    }
}
