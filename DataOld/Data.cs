using KushBot.Resources.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using System.IO;
using System.Data;
using Microsoft.Data.Sqlite;
using KushBot.DataClasses;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using KushBot.Global;
using KushBot.DataClasses.Enums;

namespace KushBot.Data;

[Flags]
public enum UserDtoFeatures : long
{
    None = 0,
    Pets = 1 << 0,
    Items = 1 << 1,
    Plots = 1 << 2,
    Claims = 1 << 3,
    Buffs = 1 << 4,
    All = -1L
}

public static class Data
{
    public static async Task AddFollowRarity(ulong UserId, string rarity)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.RarityFollow.Any(x => x.fk_UserId == UserId && x.Rarity.Equals(rarity)))
                return;

            DbContext.RarityFollow.Add(new RarityFollow(UserId, rarity));
            await DbContext.SaveChangesAsync();
        }
    }

    public static async Task RemoveFollowRarity(ulong UserId, string rarity)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (!DbContext.RarityFollow.Any(x => x.fk_UserId == UserId && x.Rarity.Equals(rarity)))
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


    public static KushBotUser GetKushBotUser(ulong userId, UserDtoFeatures features = UserDtoFeatures.None)
    {
        using var dbContext = new SqliteDbContext();

        var query = dbContext.Jews.Where(e => e.Id == userId);

        if (features.HasFlag(UserDtoFeatures.Items))
        {
            query = query
                .Include(e => e.Items)
                    .ThenInclude(e => e.ItemPetConns);
        }

        if (features.HasFlag(UserDtoFeatures.Buffs))
        {
            query = query.Include(e => e.UserBuffs);
        }

        var user = query.FirstOrDefault();

        if (features.HasFlag(UserDtoFeatures.Pets))
        {
            user.Pets = GetUserPetsInternal(dbContext, userId);
        }

        return dbContext.Jews.FirstOrDefault(e => e.Id == userId);
    }

    public static UserPets GetUserPets(ulong userId)
    {
        using var dbContext = new SqliteDbContext();
        return GetUserPetsInternal(dbContext, userId);
    }

    private static UserPets GetUserPetsInternal(SqliteDbContext dbContext, ulong userId)
    {
        var pets = new UserPets(dbContext.UserPets.Where(e => e.UserId == userId).ToList());
        var equippedItems = GetUserItemsInternal(dbContext, userId, true);

        foreach (var petConn in equippedItems.SelectMany(e => e.ItemPetConns))
        {
            if (pets.ContainsKey(petConn.PetType))
            {
                pets[petConn.PetType].ItemLevel += petConn.LvlBonus;
            }
        }

        return pets;
    }

    public static UserItems GetUserItemsInternal(SqliteDbContext dbContext, ulong userId, bool? isEquipped = null)
    {
        return (dbContext.Item
            .Include(e => e.ItemPetConns)
            .Where(e => e.OwnerId == userId && (!isEquipped.HasValue || e.IsEquipped == isEquipped.Value))
            .ToList() as UserItems) ?? new();
    }

    public static UserItems GetUserItems(ulong userId, bool? isEquipped = null)
    {
        using var dbContext = new SqliteDbContext();
        return GetUserItemsInternal(dbContext, userId, isEquipped);
    }

    public static async Task SaveKushBotUserAsync(KushBotUser user, UserDtoFeatures features = UserDtoFeatures.None)
    {
        using var dbContext = new SqliteDbContext();

        dbContext.Jews.Update(user);

        if (user.Items != null && user.Items.Any() && features.HasFlag(UserDtoFeatures.Items))
        {
            dbContext.Item.UpdateRange(user.Items);
        }

        if (user.UserBuffs != null && user.UserBuffs.Any() && features.HasFlag(UserDtoFeatures.Buffs))
        {
            SaveUserBuffsInternal(dbContext, user.UserBuffs);
        }

        if (user.Pets != null && user.Pets.Any() && features.HasFlag(UserDtoFeatures.Pets))
        {
            dbContext.UserPets.UpdateRange(user.Pets.Select(e => e.Value));
        }

        await dbContext.SaveChangesAsync();
    }

    public static void SaveUserBuffsInternal(SqliteDbContext context, UserBuffs buffs)
    {
        context.ConsumableBuffs.UpdateRange(buffs.NotDepleted);
        context.ConsumableBuffs.RemoveRange(buffs.Depleted);
    }

    public static async Task SaveUserPetsAsync(UserPets userPets)
    {
        using var dbContext = new SqliteDbContext();
        dbContext.UserPets.UpdateRange(userPets.Select(e => e.Value));
        await dbContext.SaveChangesAsync();
    }

    public static async Task CreateUserEventAsync(ulong userId, UserEventType type, int amount)
    {
        using var dbContext = new SqliteDbContext();
        dbContext.UserEvents.Add(new() { UserId = userId, Type = type, CreationTime = DateTime.Now, Amount = amount });
        await dbContext.SaveChangesAsync();
    }

    public static int GetTicketMultiplier(ulong UserId)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));


            return DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault().TicketMultiplier;
        }
    }

    public static async Task IncrementTicketMultiplier(ulong UserId, int increase = 1)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));


            KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();

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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));


            KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();

            Current.TicketMultiplier = 1;
            DbContext.Jews.Update(Current);
            await DbContext.SaveChangesAsync();
        }
    }

    public static async Task SaveNyaMarryDate(ulong userId, DateTime date)
    {
        using var DbContext = new SqliteDbContext();

        var user = DbContext.Jews.FirstOrDefault(e => e.Id == userId);

        user.NyaMarryDate = date;
        DbContext.Jews.Update(user);
        await DbContext.SaveChangesAsync();
    }

    public static async Task SaveNyaMarry(ulong UserId, string filePath)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));


            KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();

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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));


            return DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault().NyaMarryDate;
        }
    }

    public static async Task AddToNyaMarryDate(ulong UserId, int hours)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));


            KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();

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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));


            return DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault().Cheems;
        }
    }

    public static async Task AddUserCheems(ulong UserId, int amount)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));


            KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();

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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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

    public static async Task DeleteUser(ulong id)
    {
        using var DbContext = new SqliteDbContext();
        var user = DbContext.Jews.FirstOrDefault(e => e.Id == id);
        DbContext.Jews.Remove(user);
        await DbContext.SaveChangesAsync();
    }

    public static async Task RefreshLastVendorPurchaseAsync(ulong id)
    {
        using var DbContext = new SqliteDbContext();
        var user = DbContext.Jews.FirstOrDefault(e => e.Id == id);
        user.LastVendorPurchase = DateTime.MinValue;
        DbContext.Jews.Update(user);
        await DbContext.SaveChangesAsync();
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

    public static List<string> ReadItems(string itemFolderName)
    {
        string path = $@"Data/{itemFolderName}";

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

            ret.Add(item.Substring(start + 1, end - start - 1));
        }

        return ret;

    }

    private static string GetConnectionString()
    {
        return $@"Data Source= ./Data/Database.sqlite";
    }


    public static async Task UpgradeItem(Item item)
    {
        Random rnd = new Random();

        int itemStat = GetItemStat();
        int itemStatBonus = GetItemStatBonus(itemStat, item.Rarity);

        if (itemStat < 7)
        {
            if (item.ItemPetConns.Where(x => (int)x.PetType == itemStat - 1).Count() > 0)
            {
                item.ItemPetConns.Where(x => (int)x.PetType == itemStat - 1).FirstOrDefault().LvlBonus += itemStatBonus;
            }
            else
            {
                var petcon = new ItemPetConn((PetType)(itemStat - 1), itemStatBonus);
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

        List<string> itemPaths = Program.GetItemPathsByRarity(rarity);

        string chosenItem = itemPaths[rnd.Next(0, itemPaths.Count)];

        int rolls = rarity + 1;

        Item item = new Item(ownerId, chosenItem);

        item.Rarity = rarity;

        for (int i = 0; i < rolls; i++)
        {
            int itemStat = GetItemStat();
            int itemStatBonus = GetItemStatBonus(itemStat, rarity);

            if (itemStat < 7)
            {
                if (item.ItemPetConns.Where(x => (int)x.PetType == itemStat - 1).Count() > 0)
                {
                    item.ItemPetConns.Where(x => (int)x.PetType == itemStat - 1).FirstOrDefault().LvlBonus += itemStatBonus;
                }
                else
                {
                    var petcon = new ItemPetConn((PetType)(itemStat - 1), itemStatBonus);
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
            ret += rnd.Next(1 + rarity / 4, 4 + rarity / 3);
            if (pickedStat == 7)
                ret += 2 + rarity / 4;
        }
        else if (pickedStat == 8 || pickedStat == 11)
        {
            for (int i = 0; i < rarity; i++)
            {
                ret += rnd.Next(25 / (i + 1), 45 / (i + 1));
            }
        }
        else if (pickedStat == 10)
        {
            for (int i = 0; i < rarity; i++)
            {
                ret += rnd.Next(15 / (i + 1), 25 / (i + 1));
            }
        }
        else if (pickedStat == 9 || pickedStat == 12)
        {
            for (int i = 0; i < rarity; i++)
            {
                ret += rnd.Next(10 / (i + 1), 20 / (i + 1));
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

    public static int GetPetAbuseStrength(ulong id, int petId)
    {
        PlotsManager plots = GetUserPlotsManager(id);
        var abusingPlot = plots.Plots.FirstOrDefault(e =>
                        e.Type == PlotType.Abuse
                        && ((AbuseChamber)e).IsAbusing()
                        && e.UserId == id
                        && e.AdditionalData.ToLower() == Global.Pets.Dictionary[(PetType)petId].Name.ToLower());
        return abusingPlot?.Level ?? 0;

    }

    public static int GetPetAbuseSupernedStrength(ulong id, int petId)
    {
        PlotsManager plots = GetUserPlotsManager(id);
        var abusingPlot = plots.Plots.FirstOrDefault(e =>
                        e.Type == PlotType.Abuse
                        && ((AbuseChamber)e).IsAbusing()
                        && e.UserId == id
                        && e.AdditionalData.ToLower() == Global.Pets.Dictionary[(PetType)petId].Name.ToLower());
        return (abusingPlot?.Level * 8) ?? 0;
    }

    public static async Task ResetWeeklyStuff(List<KushBotUser> Jews)
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

    public static void ResetDailyStuff(List<KushBotUser> Jews)
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
                int GuaranteedExtras = 25 * (item.Pets[PetType.Maybich]?.Tier ?? 0);

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
                var pets = item.Pets;
                List<int> options = new List<int>();
                for (int i = 0; i < Program.Quests.Count; i++)
                {

                    if (((i == 15 || i == 16) && !pets.ContainsKey(PetType.Jew)) || (i == 13 && pets.Any()))
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

    public static async Task<bool> MakeRowForJew(ulong UserId)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
            {
                KushBotUser newJew = new KushBotUser(UserId, 30, false);
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
                return true;
            }
        }
        return false;
    }

    public static void RaceFinished()
    {
        using (StreamReader reader = new StreamReader(@"Data/WeeklyQuests.txt"))
        {
            string line = reader.ReadLine();
            Console.WriteLine(line);

            string[] values = line.Split(',');

            values[2] = "-1";
            string newvalue = string.Join(",", values);

            reader.Close();

            File.WriteAllText(@"Data/WeeklyQuests.txt", newvalue);

        }
    }

    public static List<int> GetWeeklyQuest()
    {
        using (StreamReader reader = new StreamReader(@"Data/WeeklyQuests.txt"))
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();

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
    }

    public static async Task ResetCompletedWeekly(ulong UserId)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
            {
                //no row for user, create one
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();

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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
        using (StreamWriter writer = new StreamWriter(@"Data/WeeklyQuests.txt"))
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

    public static int GetItemPetLevel(ulong UserId, int PetIndex)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                return 0;

            KushBotUser jew = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();

            List<int> Equipped = new List<int>();
            Equipped.Add(jew.FirstItemId);
            Equipped.Add(jew.SecondItemId);
            Equipped.Add(jew.ThirdItemId);
            Equipped.Add(jew.FourthItemId);

            List<Item> Items = GetUserItems(UserId);

            int ret = 0;

            for (int i = 0; i < 4; i++)
            {
                if (Equipped[i] != 0)
                {
                    Item temp = Items.Where(x => x.Id == Equipped[i]).FirstOrDefault();

                    foreach (var item in temp.ItemPetConns)
                    {
                        if ((int)item.PetType == PetIndex)
                        {
                            ret += item.LvlBonus;
                        }
                    }

                }
            }

            return ret;
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
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
                return 0;

            return DbContext.Jews.Where(x => x.Id == UserId).Select(x => x.Balance).FirstOrDefault();
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

    public static async Task AddYike(ulong userId)
    {
        using var DbContext = new SqliteDbContext();

        if (DbContext.Jews.Where(x => x.Id == userId).Count() < 1)
        {
            DbContext.Jews.Add(new KushBotUser(userId, 30, false));
        }

        KushBotUser Current = DbContext.Jews.FirstOrDefault(e => e.Id == userId);
        Current.Yiked += 1;
        DbContext.Jews.Update(Current);

        await DbContext.SaveChangesAsync();
    }

    public static async Task SaveYikeDate(ulong UserId, DateTime? date = null)
    {
        using var DbContext = new SqliteDbContext();

        KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
        Current.YikeDate = (date ?? DateTime.Now);
        DbContext.Jews.Update(Current);

        await DbContext.SaveChangesAsync();
    }

    public static async Task SaveRedeemDate(ulong UserId, DateTime? date = null)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
            {
                //no row for user, create one
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                Current.RedeemDate = (date ?? DateTime.Now);
                DbContext.Jews.Update(Current);
            }
            await DbContext.SaveChangesAsync();
        }
    }

    public static async Task SaveRageDuration(ulong UserId, int rageDuration)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
            {
                //no row for user, create one
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                Current.LastDestroy = lastDestroy;
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                Current.HasEgg = HasEgg;
                DbContext.Jews.Update(Current);
            }
            await DbContext.SaveChangesAsync();
        }
    }

    public static async Task SaveBalance(ulong UserId, int Amount, bool Gambling, IMessageChannel channelForRage = null, int gambleAmount = 0)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
            {
                //no row for user, create one
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = GetKushBotUser(UserId, UserDtoFeatures.Pets);
                Current.Balance += Amount;
                if (Gambling && GetRageDuration(UserId) > 0)
                {
                    double RageCashDbl = 0;
                    int RageCash = 0;

                    const double A = 2;
                    const double B = 5;
                    const double D = 0.9;

                    if (Amount > 0)
                    {
                        int lvl = Current.Pets[PetType.TylerJuan].CombinedLevel;
                        //(a*Baps-(Baps^2/(b+Lv+Baps/c)))*d 
                        //RageCashDbl = (A * Amount - ((Math.Pow(Amount, 2)) / (B + lvl + (Amount / A)))) * D;
                        RageCashDbl = (A * gambleAmount - ((Math.Pow(gambleAmount, 2)) / (B + lvl + (gambleAmount / A)))) * D;
                        double coefficient = RageCashDbl / gambleAmount;
                        RageCashDbl = coefficient * Amount;

                    }
                    else
                    {
                        Random rnd = new();
                        RageCashDbl = rnd.Next(2, 7);
                    }

                    RageCash = (int)Math.Round(RageCashDbl);
                    Current.RageDuration -= 1;


                    Current.RageCash += RageCash;

                    if (Current.RageDuration == 0)
                    {
                        int temp = Current.RageCash;
                        if (temp < 0)
                        {
                            temp = temp * -1;
                        }

                        await Program.EndRage(UserId, Current.RageCash, channelForRage);
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                Current.LastBeg = lastBeg;
                DbContext.Jews.Update(Current);
            }
            await DbContext.SaveChangesAsync();
        }
    }

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
            KushBotUser current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();

            current.Pictures += $",{picture.ToString()}";
            DbContext.Jews.Update(current);

            await DbContext.SaveChangesAsync();
        }
    }

    public static async Task UpdateSelectedPicture(ulong UserId, int picture)
    {
        using (var DbContext = new SqliteDbContext())
        {
            KushBotUser current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();

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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                Current.WonRisksMN += wonRisksMN;
                DbContext.Jews.Update(Current);
            }
            await DbContext.SaveChangesAsync();
        }
    }

    public static int GetBegsMN(ulong UserId)
    {
        using (var DbContext = new SqliteDbContext())
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
                Current.QuestIndexes = questIndexes;
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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

    public static async Task SaveBegsWeekly(ulong UserId, int begsMN)
    {
        using (var DbContext = new SqliteDbContext())
        {
            if (DbContext.Jews.Where(x => x.Id == UserId).Count() < 1)
            {
                //no row for user, create one
                DbContext.Jews.Add(new KushBotUser(UserId, 30, false));
            }
            else
            {
                KushBotUser Current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();
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
            KushBotUser current = DbContext.Jews.Where(x => x.Id == UserId).FirstOrDefault();

            if (addition)
            {
                if (GetTicketCount(UserId) < 3)
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

    public static async Task InfestUserAsync(ulong userId, Infection infection = null, bool ignoreTime = false)
    {
        using var DbContext = new SqliteDbContext();

        KushBotUser current = DbContext.Jews.FirstOrDefault(e => e.Id == userId);

        if (current == null)
            return;

        if (!ignoreTime && DbContext.UserInfections.Any(e => e.OwnerId == userId && e.CreationDate.AddMinutes(10) > DateTime.Now))
            return;

        if (DbContext.UserInfections.Count(e => e.OwnerId == userId) >= 8)
            return;

        infection ??= new()
        {
            OwnerId = userId,
            CreationDate = DateTime.Now,
            KillAttemptDate = DateTime.MinValue
        };

        DbContext.UserInfections.Add(infection);
        await DbContext.SaveChangesAsync();

    }

    public static async Task RemoveInfection(Guid id)
    {
        using var DbContext = new SqliteDbContext();
        var infection = DbContext.UserInfections.FirstOrDefault(e => e.Id == id);

        try
        {
            DbContext.UserInfections.Remove(infection);
            await DbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERR] Error {ex.Message} when trying to delete infection id: {id}");
        }
    }

    public static async Task<List<Infection>> GetUserInfectionsAsync(ulong userId)
    {
        using var DbContext = new SqliteDbContext();

        var result = await DbContext.UserInfections
            .Where(e => e.OwnerId == userId)
            .ToListAsync();

        return result;
    }

    public static async Task<(int? baps, bool isCd)> KillInfectionAsync(Guid id)
    {
        using var DbContext = new SqliteDbContext();

        Infection infection = DbContext.UserInfections.FirstOrDefault(e => e.Id == id);

        if (infection == null)
            return (null, true);

        if (infection.KillAttemptDate.AddHours(2) > DateTime.Now)
            return (null, true);

        if (infection is null)
            return (null, false);

        bool isKilled = AttemptKillInfection(DbContext, infection);
        await DbContext.SaveChangesAsync();
        if (!isKilled)
            return (null, false);

        int petLvl = Program.GetTotalPetLvl(infection.OwnerId);

        int bapsForKill = infection.GetBapsForKill(petLvl);

        await SaveBalance(infection.OwnerId, bapsForKill, false);

        return (bapsForKill, false);
    }

    private static bool AttemptKillInfection(SqliteDbContext context, Infection infection)
    {
        if (infection.KillAttemptDate.AddHours(2) > DateTime.Now)
        {
            return false;
        }

        Random rnd = new Random();

        //Kill failed
        if (rnd.NextDouble() > infection.GetInfectionKillChance())
        {
            infection.KillAttemptDate = DateTime.Now;
            context.UserInfections.Update(infection);
            return false;
        }

        context.UserInfections.Remove(infection);
        return true;
    }

    public static async Task<int> InfectionConsumeBapsAsync(ulong userId)
    {
        using var DbContext = new SqliteDbContext();
        List<Infection> qualifiedInfections = await DbContext.UserInfections.Where(e => e.OwnerId == userId).ToListAsync();

        qualifiedInfections = qualifiedInfections
            .Where(e => (e.State == InfectionState.Tyrant || e.State == InfectionState.NecroticSovereign || e.State == InfectionState.EldritchPatriarch)
                        && e.BapsDrained <= 6000)
            .ToList();

        if (!qualifiedInfections.Any())
            return 0;

        int bapsConsumed = 0;

        Random rnd = new Random();

        Infection infection = qualifiedInfections[rnd.Next(0, qualifiedInfections.Count)];

        switch (infection.State)
        {
            case InfectionState.Tyrant:
                bapsConsumed += rnd.Next(80, 141);
                break;
            case InfectionState.NecroticSovereign:
                bapsConsumed += rnd.Next(180, 281);
                break;
            case InfectionState.EldritchPatriarch:
                bapsConsumed += rnd.Next(300, 561);
                break;
            default:
                break;
        }


        int userBaps = Data.GetBalance(userId);
        bapsConsumed = Math.Min(bapsConsumed, userBaps);
        bapsConsumed = bapsConsumed < 0 ? 0 : bapsConsumed;
        infection.BapsDrained += bapsConsumed;
        DbContext.UserInfections.Update(infection);

        await DbContext.SaveChangesAsync();

        await Data.SaveBalance(userId, -1 * bapsConsumed, false);

        return bapsConsumed;
    }

    public static Dictionary<ulong, List<UserTutoProgress>> LoadAllUsersTutorialProgress()
    {
        using var DbContext = new SqliteDbContext();
        var allData = DbContext.UserTutoProgress.ToList();

        var ret = allData.GroupBy(e => e.UserId).ToDictionary(e => e.Key, e => e.ToList());
        return ret;
    }

    public static async Task<UserTutoProgress> InsertTutoStepCompletedAsync(ulong userId, int page, int stepIndex)
    {
        using var DbContext = new SqliteDbContext();

        UserTutoProgress step = new()
        {
            Id = Guid.NewGuid(),
            Page = page,
            UserId = userId,
            TaskIndex = stepIndex,
        };

        DbContext.UserTutoProgress.Add(step);

        await DbContext.SaveChangesAsync();

        return step;
    }


    public static async Task RemoveDeprecatedTutoStepsAsync(ulong userId, int maxPage)
    {
        using (var cnn = new SqliteDbContext())
        {
            var conn = new SqliteConnection(GetConnectionString());
            conn.Open();
            SqliteCommand cmd = new SqliteCommand(
                $"""
                DELETE
                FROM UserTutoProgress
                WHERE [Page] <= {maxPage} AND [UserId] = {userId} 
                """
                );

            cmd.Connection = conn;
            await cmd.ExecuteNonQueryAsync();
            conn.Close();
        }
    }

    public static PlotsManager GetUserPlotsManager(ulong userId)
    {
        using var DbContext = new SqliteDbContext();

        PlotFactory factory = new();

        List<Plot> plots = DbContext.Plots
            .Where(e => e.UserId == userId)
            .Select(e => factory.CreatePlot(e))
            .ToList();

        return new PlotsManager(plots);
    }

    public static async Task CreatePlotForUserAsync(ulong userId)
    {
        using var DbContext = new SqliteDbContext();

        Plot plot = new Garden()
        {
            Type = PlotType.Garden,
            UserId = userId,
            Level = 1,
            LastActionDate = null,
            AdditionalData = "",
        };

        DbContext.Plots.Add(plot);
        await DbContext.SaveChangesAsync();
    }

    public static async Task UpdatePlotAsync(Plot plot)
    {
        using var DbContext = new SqliteDbContext();
        DbContext.Plots.Update(plot);
        await DbContext.SaveChangesAsync();
    }

    public static async Task UpdatePlotsAsync(List<Plot> plots)
    {
        using var DbContext = new SqliteDbContext();
        plots.ForEach(e => DbContext.Update(e));
        await DbContext.SaveChangesAsync();
    }

    public static async Task TransformPlotAsync(Guid plotId, PlotType type)
    {
        using var DbContext = new SqliteDbContext();
        var plot = DbContext.Plots.FirstOrDefault(e => e.Id == plotId);
        plot.Type = type;
        plot.LastActionDate = type == PlotType.Abuse ? DateTime.Now.AddHours(-6) : DateTime.Now;
        if (type == PlotType.Hatchery)
        {
            List<HatcheryLine> list = new();

            for (int i = 0; i < plot.Level; i++)
            {
                HatcheryLine line = new();
                line.Slot = i + 1;
                list.Add(line);
            }


            plot.AdditionalData = JsonConvert.SerializeObject(list);
        }
        else
        {
            plot.AdditionalData = "";
        }

        DbContext.Plots.Update(plot);
        await DbContext.SaveChangesAsync();
    }

    public static async Task SaveUserVendorPurchaseDateAsync(ulong userId)
    {
        using var dbContext = new SqliteDbContext();
        var jew = await dbContext.Jews.FirstOrDefaultAsync(e => e.Id == userId);

        jew.LastVendorPurchase = DateTime.Now;
        await dbContext.SaveChangesAsync();
    }

    public static async Task<DateTime> GetUserLastVendorPurchaseDateAsync(ulong userId)
    {
        using var dbContext = new SqliteDbContext();
        var dt = await dbContext.Jews.Where(e => e.Id == userId).Select(e => new { e.Id, e.LastVendorPurchase }).FirstOrDefaultAsync();
        return dt.LastVendorPurchase;
    }

    public static async Task<bool> UserHasBuffsAsync(ulong userId)
    {
        using var dbContext = new SqliteDbContext();
        return await dbContext.ConsumableBuffs.AnyAsync(e => e.OwnerId == userId);
    }

    public static async Task<bool> UserHasBuffAsync(ulong userId, BuffType type)
    {
        using var dbContext = new SqliteDbContext();
        return await dbContext.ConsumableBuffs.AnyAsync(e => e.OwnerId == userId && e.Type == type);
    }

    public static async Task CreateConsumableBuffAsync(ulong userId, BuffType type, int duration, double potency)
    {
        using var dbContext = new SqliteDbContext();

        if (dbContext.ConsumableBuffs.Where(e => e.OwnerId == userId).Count() >= 15)
            return;

        ConsumableBuff buff = new()
        {
            Type = type,
            Duration = duration,
            TotalDuration = duration,
            OwnerId = userId,
            Id = Guid.NewGuid(),
            Potency = potency,
        };

        dbContext.ConsumableBuffs.Add(buff);
        await dbContext.SaveChangesAsync();
    }

    public static List<ConsumableBuff> GetConsumableBuffList(ulong userId)
    {
        using var dbContext = new SqliteDbContext();
        return dbContext.ConsumableBuffs.Where(e => e.OwnerId == userId).ToList();
    }

    public static ConsumableBuff GetConsumableBuff(ulong userId, BuffType buffType)
    {
        using var dbContext = new SqliteDbContext();
        ConsumableBuff buff = dbContext.ConsumableBuffs.FirstOrDefault(e => e.OwnerId == userId && e.Type == buffType);
        return buff;
    }

    public static async Task CreateConsumableBuffAsync(ulong userId, BuffType type, DateTime expirationDate, double potency)
    {
        using var dbContext = new SqliteDbContext();
        ConsumableBuff buff = new()
        {
            Type = type,
            OwnerId = userId,
            Id = Guid.NewGuid(),
            Potency = potency,
            ExpirationDate = expirationDate,
        };

        dbContext.ConsumableBuffs.Add(buff);
        await dbContext.SaveChangesAsync();
    }

    public static async Task ReduceOrRemoveBuffAsync(ulong userId, BuffType type)
    {
        using var dbContext = new SqliteDbContext();

        var buff = await dbContext.ConsumableBuffs.FirstOrDefaultAsync(e => e.OwnerId == userId && e.Type == type);
        if (buff is null)
            return;

        buff.Duration -= 1;
        if (buff.Duration <= 0)
        {
            dbContext.ConsumableBuffs.Remove(buff);
        }
        else
        {
            dbContext.ConsumableBuffs.Update(buff);
        }

        await dbContext.SaveChangesAsync();
    }

    public static async Task ReduceOrRemoveBuffAsync(Guid buffId)
    {
        using var dbContext = new SqliteDbContext();

        var buff = await dbContext.ConsumableBuffs.FirstOrDefaultAsync(e => e.Id == buffId);
        if (buff is null)
            return;

        buff.Duration -= 1;
        if (buff.Duration <= 0)
        {
            dbContext.ConsumableBuffs.Remove(buff);
        }
        else
        {
            dbContext.ConsumableBuffs.Update(buff);
        }

        await dbContext.SaveChangesAsync();
    }

    public static async Task IncrementKeysForClaimAsync(int claimId)
    {
        using var dbContext = new SqliteDbContext();
        var claim = dbContext.NyaClaims.FirstOrDefault(e => e.Id == claimId);
        claim.Keys += 1;
        await dbContext.SaveChangesAsync();
    }

    public static NyaClaim GetClaimByPath(string path)
    {
        using var dbContext = new SqliteDbContext();
        return dbContext.NyaClaims.FirstOrDefault(e => e.FileName == path);
    }

    public static HashSet<string> GetClaimedImgPaths()
    {
        using var dbContext = new SqliteDbContext();
        return dbContext.NyaClaims.Select(e => e.FileName).ToHashSet();
    }

    public static DateTime GetLastClaimDate(ulong userId)
    {
        using var dbContext = new SqliteDbContext();
        return dbContext.Jews.FirstOrDefault(e => e.Id == userId).LastNyaClaim;
    }

    public static async Task SaveClaimAsync(ulong userId, NyaClaimEvent claimEvent)
    {
        using var dbContext = new SqliteDbContext();
        var userClaims = dbContext.NyaClaims.Where(e => e.OwnerId == userId).ToList();

        int nextSortIndex = userClaims.Any() ? userClaims.Max(e => e.SortIndex) + 1 : 0;

        NyaClaim claim = new()
        {
            FileName = claimEvent.FileName,
            OwnerId = userId,
            Url = claimEvent.ImageMessage.Attachments.FirstOrDefault()?.Url,
            SortIndex = nextSortIndex,
            ClaimDate = DateTime.Now,
        };

        dbContext.NyaClaims.Add(claim);
        await dbContext.SaveChangesAsync();
    }

    public static NyaClaim GetClaimBySortIndex(ulong userId, int sortIndex)
    {
        using var dbContext = new SqliteDbContext();
        return dbContext.NyaClaims.FirstOrDefault(e => e.OwnerId == userId && e.SortIndex == sortIndex);
    }

    public static async Task SaveLastClaimDate(ulong userId, DateTime? dateTime = null)
    {
        using var dbContext = new SqliteDbContext();
        var user = dbContext.Jews.FirstOrDefault(e => e.Id == userId);
        dateTime ??= DateTime.Now;
        user.LastNyaClaim = dateTime.Value;
        dbContext.Jews.Update(user);
        await dbContext.SaveChangesAsync();
    }

    public static List<NyaClaim> GetUserNyaClaims(ulong userId)
    {
        using var dbContext = new SqliteDbContext();
        return dbContext.NyaClaims.Where(e => e.OwnerId == userId).ToList();
    }

    public static async Task DismissNyaClaims(ulong userId, int sortIndex)
    {
        using var dbContext = new SqliteDbContext();
        var nyaClaims = dbContext.NyaClaims.Where(e => e.OwnerId == userId).OrderBy(e => e.SortIndex).ToList();

        bool sortIndexPassed = false;

        foreach (var item in nyaClaims)
        {
            if (sortIndexPassed)
            {
                item.SortIndex -= 1;
                dbContext.NyaClaims.Update(item);
            }
            if (item.SortIndex == sortIndex && !sortIndexPassed)
            {
                dbContext.NyaClaims.Remove(item);
                sortIndexPassed = true;
            }
        }

        await dbContext.SaveChangesAsync();
    }

    public static void FixSortIndex(List<NyaClaim> nyaClaims)
    {
        nyaClaims = nyaClaims.OrderBy(e => e.SortIndex).ToList();
        for (int i = 0; i < nyaClaims.Count; i++)
        {
            nyaClaims[i].SortIndex = i;
        }
    }

    public static async Task FixUserClaimSortIndexes(ulong userId)
    {
        using var dbContext = new SqliteDbContext();
        var nyaClaims = dbContext.NyaClaims.Where(e => e.OwnerId == userId).ToList();
        FixSortIndex(nyaClaims);
        await dbContext.SaveChangesAsync();
    }

    public static async Task ConcludeNyaTradeAsync(NyaClaimTrade trade)
    {
        using var dbContext = new SqliteDbContext();

        if (trade.Suggester.NyaClaim != null)
        {
            var trackedSugesteeClaim = dbContext.NyaClaims.FirstOrDefault(e => e.Id == trade.Suggester.NyaClaim.Id);
            trackedSugesteeClaim.OwnerId = trade.Respondee.UserId;
        }


        if (trade.Respondee.NyaClaim != null)
        {
            var trackedRespondeeClaim = dbContext.NyaClaims.FirstOrDefault(e => e.Id == trade.Respondee.NyaClaim.Id);
            trackedRespondeeClaim.OwnerId = trade.Suggester.UserId;
        }



        await dbContext.SaveChangesAsync();
        NyaClaimGlobals.NyaTrades.RemoveAll(e => e.Suggester.UserId == trade.Suggester.UserId);

        await FixUserClaimSortIndexes(trade.Suggester.UserId);
        await FixUserClaimSortIndexes(trade.Respondee.UserId);
    }

    //Claimsorder represent where current sort positions should be, e.g.: 3, 6, 1 would move the 3rd position to the first position. etc
    public static async Task SortClaims(ulong userId, List<int> claimsOrder)
    {
        using var dbContext = new SqliteDbContext();

        var claims = dbContext.NyaClaims.Where(e => e.OwnerId == userId).ToList();

        for (int i = 0; i < claimsOrder.Count; i++)
        {
            claims.FirstOrDefault(e => e.SortIndex == claimsOrder[i] - 1).SortIndex = -1000 + i;
        }

        FixSortIndex(claims);
        dbContext.NyaClaims.UpdateRange(claims);
        await dbContext.SaveChangesAsync();
    }

    public static async Task SaveUserExtraClaimsAsync(ulong userId, int increment = 1)
    {
        using var dbContext = new SqliteDbContext();
        var jew = dbContext.Jews.FirstOrDefault(e => e.Id == userId);
        jew.ExtraClaimSlots += increment;
        dbContext.Update(jew);
        await dbContext.SaveChangesAsync();
    }

    public static int GetUserExtraClaims(ulong userId)
    {
        using var dbContext = new SqliteDbContext();
        return dbContext.Jews.FirstOrDefault(e => e.Id == userId).ExtraClaimSlots;
    }
}
