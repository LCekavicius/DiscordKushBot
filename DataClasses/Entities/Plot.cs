using KushBot.Global;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace KushBot.DataClasses;

public enum PlotType
{
    None,
    Garden,
    Quarry,
    Hatchery,
}

public class PlotFactory
{
    public Plot CreatePlot(Plot plot)
    {
        Plot instance = CreatePlotInstance(plot.Type, plot.AdditionalData);

        Type classType = plot.GetType();
        PropertyInfo[] properties = classType.GetProperties();

        foreach (PropertyInfo property in properties)
        {
            if (property.CanRead && property.CanWrite)
            {
                object value = property.GetValue(plot);
                property.SetValue(instance, value);
            }
        }

        return instance;
    }

    private Plot CreatePlotInstance(PlotType type, string additionalData)
        => type switch
        {
            PlotType.Garden => new Garden(),
            PlotType.Quarry => new Quarry(),
            PlotType.Hatchery => new Hatchery(additionalData),
            _ => new Garden(),
        };
}

public class PlotsManager
{
    public List<Plot> Plots { get; set; } = new();

    public PlotsManager(List<Plot> plots)
    {
        this.Plots = plots;
    }

    public async Task UpdateStateAsync(List<Plot> plots = null)
    {
        plots ??= Plots.Where(e => e is Hatchery).ToList();
        plots.ForEach(e => ((Hatchery)e).UpdateState());
        if (plots.Any())
            await Data.Data.UpdatePlotsAsync(plots);
    }

    public async Task<int> FillHatcheriesAsync(int maxFill, int? index = null)
    {
        List<Plot> plots = Plots
            .Where(e => e is Hatchery
               && (!index.HasValue || e.Id == Plots[index.Value].Id))
            .ToList();

        if (!plots.Any())
            return 0;

        int totalEggsFilled = 0;
        plots.ForEach(e => totalEggsFilled += maxFill > 0 ? ((Hatchery)e).Fill(ref maxFill) : 0);

        await Data.Data.UpdatePlotsAsync(plots);
        return totalEggsFilled;
    }

    public int NextPlotPrice()
    {
        if (Plots.Count == 0)
            return 1000;

        return 1000 + 500 * (int)Math.Pow(2, Plots.Count);
    }

    public async Task<string> CollectAsync(int plotIndex, UserPets userPets = null)
    {
        Plot plot = Plots[plotIndex];
        userPets ??= Data.Data.GetUserPets(plot.UserId);

        if (plot.IsReadyForCollecting())
        {
            return await Plots[plotIndex].CollectAsync(plotIndex, userPets);
        }

        return "That plot isn't ready to be collected, retard nigger";
    }

    public async Task ShiftTimeAsync(int minutes, int? plotIndex = null)
    {
        if (plotIndex != null)
            throw new NotImplementedException();

        foreach (var plot in Plots)
        {
            plot.ShiftTime(minutes);
        }

        await Data.Data.UpdatePlotsAsync(this.Plots);
    }

    public async Task<string> CollectAsync(string input)
    {
        string amalgamation = "";
        int index = 0;
        //string userPets = Data.Data.GetPets(Plots.FirstOrDefault()?.UserId ?? ulong.MinValue);
        var userPets = Data.Data.GetUserPets(Plots.FirstOrDefault()?.UserId ?? ulong.MinValue);

        foreach (var plot in Plots)
        {
            if ((input.ToLower() == "all"
                || (input.Length == 1 && plot.Type.ToString().StartsWith(char.ToUpper(input[0])))
                || plot.Type == EnumHelperV2Singleton.Instance.Helper.GetEnumByDescriptedValue<PlotType>(input, true))
                && plot.IsReadyForCollecting())
            {
                amalgamation += await CollectAsync(index, userPets) + (index + 1 == Plots.Count ? "" : "\n");
            }
            index++;
        }

        return amalgamation;
    }
}

public class Plot
{
    [Key]
    public Guid Id { get; set; }
    public ulong UserId { get; set; }
    public PlotType Type { get; set; }
    public int Level { get; set; }
    public DateTime? LastActionDate { get; set; }
    public string AdditionalData { get; set; }
    public virtual string GetPlotIcon()
    {
        throw new NotImplementedException();
    }

    public virtual async Task<string> CollectAsync(int slotId, UserPets userPets)
    {
        throw new NotImplementedException();
    }

    public virtual bool IsReadyForCollecting()
    {
        throw new NotImplementedException();
    }

    public virtual void Upgrade()
    {
        Level += 1;
    }

    public virtual void ShiftTime(int minutes)
    {
        if (!LastActionDate.HasValue)
            return;

        LastActionDate = LastActionDate.Value.AddMinutes(-1 * minutes);
    }

    public virtual string GetDataText()
    {
        throw new NotImplementedException();
    }

    public string GetLevelText()
    {
        return $"Lvl {Level} {Type.ToString()}";
    }

}

public class Garden : Plot
{
    public override string GetPlotIcon()
    {
        return ":green_square:";
    }

    public override bool IsReadyForCollecting()
    {
        int cd = (8 - Level + 1);
        return ((LastActionDate ?? DateTime.MinValue).AddHours(cd) < DateTime.Now);
    }

    public override string GetDataText()
    {
        int cd = (8 - Level + 1);
        if ((LastActionDate ?? DateTime.MinValue).AddHours(cd) < DateTime.Now)
            return "Ready in: Now";

        TimeSpan ts = LastActionDate.Value.AddHours(cd) - DateTime.Now;

        return $"Ready in: {ts.ToString(@"hh\:mm\:ss")}";
    }

    // OLD LOGIC FROM 2018, DONT LOOK AT IT D:
    public override async Task<string> CollectAsync(int plotIndex, UserPets userPets)
    {
        //try
        //{

        //    this.LastActionDate = DateTime.Now;
        //    await Data.Data.UpdatePlotAsync(this);

        //    Random rad = new Random();

        //    int effectRolled = rad.Next(0, 7);
        //    string effect = "";

        //    if (effectRolled == 0)
        //    {
        //        effect = "fishing rod";
        //    }
        //    else if (effectRolled == 1)
        //    {
        //        effect = "kush gym";
        //    }
        //    else if (effectRolled == 2)
        //    {
        //        effect = "adderal";
        //    }
        //    else if (effectRolled == 3)
        //    {
        //        if (Data.Data.GetPets(UserId).Length <= 0)
        //        {
        //            effect = "fishing rod";
        //        }

        //        effect = "roids";
        //        int tmp = rad.Next(1, 101);

        //        if (tmp < 78 - 7 * Level)
        //        {
        //            effect = "kush gym";
        //        }
        //    }
        //    else if (effectRolled == 4)
        //    {
        //        effect = "baps";
        //    }
        //    else if (effectRolled == 5)
        //    {
        //        effect = "icon";

        //        int tmp = rad.Next(1, 101);

        //        if (tmp < 65 - 10 * Level)
        //        {
        //            effect = "fishing rod";
        //        }
        //    }
        //    else if (effectRolled == 6)
        //    {
        //        effect = "wither";
        //    }

        //    if (effect != "adderal" && effect != "roids" && effect != "icon" && effect != "baps" && effect != "wither")
        //    {
        //        int duration = 4 + Level * 2;

        //        await Data.Data.CreateConsumableBuffAsync(UserId,
        //            effect == "fishing rod" ? BuffType.FishingRod : BuffType.KushGym,
        //            duration, Level * 2);

        //        return $"You grew and consumed a lvl {Level} **{effect}**, duration: {duration} gambles";

        //    }
        //    else if (effect == "adderal")
        //    {
        //        string text = $" You grew and consumed lvl {Level} **{effect}**.\n-Your beg CD got reset";

        //        await Data.Data.SaveLastBeg(UserId, DateTime.Now.AddHours(-2));

        //        if (Data.Data.GetPetLevel(UserId, 1) > 0)
        //        {
        //            if (rad.Next(0, 5 - Level) == 0)
        //            {
        //                DateTime pinateDate = Data.Data.GetLastDestroy(UserId);
        //                await Data.Data.SaveLastDestroy(UserId, pinateDate.AddHours(-2 + -1 * Level));
        //                text += $"\n-Your pinata's CD got reduced by {-2 + -1 * Level} hours";
        //            }
        //        }
        //        if (Data.Data.GetPetLevel(UserId, 4) > 0)
        //        {
        //            if (rad.Next(0, 5 - Level) == 0)
        //            {
        //                DateTime yoinkDate = Data.Data.GetLastYoink(UserId);
        //                await Data.Data.SaveLastYoink(UserId, yoinkDate.AddMinutes(-15 + -15 * Level));
        //                text += $"\n-Your Jew's CD got reduced by {-15 + -15 * Level} minutes";
        //            }
        //        }
        //        if (Data.Data.GetPetLevel(UserId, 5) > 0)
        //        {
        //            if (rad.Next(0, 5 - Level) == 0)
        //            {
        //                DateTime rageDate = Data.Data.GetLastRage(UserId);
        //                await Data.Data.SaveLastRage(UserId, rageDate.AddMinutes(-30 + -30 * Level));
        //                text += $"\n-Your Tyler's CD got reduced by {-30 + -30 * Level} minutes";
        //            }
        //        }
        //        return text;
        //    }
        //    else if (effect == "roids")
        //    {
        //        string pets = Data.Data.GetPets(UserId);

        //        Dictionary<int, int> petLevelMap = new Dictionary<int, int>();

        //        foreach (char petIndex in pets)
        //        {
        //            int tempPetId = Convert.ToInt32(petIndex.ToString());
        //            int petLvl = Data.Data.GetPetLevel(UserId, tempPetId);
        //            int itemPetLvl = Data.Data.GetItemPetLevel(UserId, tempPetId);
        //            petLevelMap.Add(tempPetId, petLvl - itemPetLvl);
        //        }

        //        int petId = petLevelMap.MinBy(e => e.Value).Key;

        //        for (int i = 1; i < pets.Length; i++)
        //        {
        //            int temp = int.Parse(char.GetNumericValue(pets[i]).ToString());

        //            if (Data.Data.GetPetLevel(UserId, petId) > Data.Data.GetPetLevel(UserId, temp))
        //            {
        //                petId = temp;
        //            }
        //        }

        //        if (petLevelMap[petId] >= 99)
        //        {
        //            return "your plot has withered.";
        //        }

        //        await Data.Data.SavePetLevels(UserId, petId, petLevelMap[petId] + 1, false);
        //        return $"You plot's yield was roids which was stolen and consumed by {Program.GetPetName(petId)}, his level has gone up.";
        //    }
        //    else if (effect == "baps")
        //    {
        //        int baps = rad.Next(30, 40 + Level * 40);

        //        await Data.Data.SaveBalance(UserId, baps, false);
        //        return $"You grew **{baps}** baps";

        //    }
        //    else if (effect == "icon")
        //    {
        //        int chosen = 4;

        //        List<int> allPictures = Data.Data.GetPictures(UserId);
        //        if (allPictures.Count >= Program.PictureCount - 1)
        //        {
        //            return "the plant has withered";
        //        }
        //        do
        //        {
        //            chosen = rad.Next(4, Program.PictureCount + 1);
        //        } while (allPictures.Contains(chosen));

        //        string text = $" You grew an icon #{chosen}";
        //        await Data.Data.UpdatePictures(UserId, chosen);

        //        if (Program.CompletedIconBlock(UserId, chosen))
        //        {
        //            List<int> newAllPictures = Data.Data.GetPictures(UserId);
        //            List<int> Specials = new List<int>();

        //            foreach (int item in newAllPictures)
        //            {
        //                if (item > 1000)
        //                {
        //                    Specials.Add(item);
        //                }
        //            }
        //            int tmp;

        //            do
        //            {
        //                tmp = 1000 + rad.Next(1, 8);
        //            } while (Specials.Contains(tmp));

        //            text += $"\nUpon completing a full icon page, you got a special icon #{tmp}, type 'kush icons specials'";

        //            await Data.Data.UpdatePictures(UserId, tmp);

        //        }
        //        return text;
        //    }
        //}
        //catch (Exception ex)
        //{

        //}
        return "Your plant has withered.";
    }
}

public class Hatchery : Plot
{
    public List<HatcheryLine> Lines { get; set; }
    public Hatchery(string additionalData)
    {
        Lines = JsonConvert.DeserializeObject<List<HatcheryLine>>(additionalData);
    }

    public override bool IsReadyForCollecting()
    {
        UpdateState();
        return Lines.Any(e => e.Progress == 10);
    }

    public override string GetPlotIcon()
    {
        return ":yellow_square:";
    }

    public override void ShiftTime(int minutes)
    {
        foreach (var line in Lines)
        {
            if (!line.LastCheckDate.HasValue)
                continue;

            line.LastCheckDate = line.LastCheckDate.Value.AddMinutes(-1 * minutes);
        }

        UpdateState();
    }

    public override async Task<string> CollectAsync(int index, UserPets userPets)
    {
        string text = $"Hatchery plot #{index + 1} yields: ";
        List<string> hatchedPetNames = new();
        
        foreach (var line in Lines.Where(e => e.Progress == 10))
        {
            var petRoll = GetPetRoll(userPets);
            userPets[petRoll.PetType].Dupes += 1;
            hatchedPetNames.Add(petRoll.Name);

            line.LastCheckDate = null;
            line.Progress = 0;
        }
        
        UpdateState();
        await Data.Data.SaveUserPetsAsync(userPets);
        await Data.Data.UpdatePlotAsync(this);
        text += string.Join(", ", hatchedPetNames);
        return text;
    }

    private UserPet GetPetRoll(UserPets userPets)
    {
        Random rnd = new Random();
        double roll = rnd.NextDouble();

        UserPet GetRandomPet(PetType pet1, PetType pet2)
        {
            if (userPets.ContainsKey(pet1) && userPets.ContainsKey(pet2))
            {
                return userPets[(PetType)rnd.Next((int)pet1, (int)pet2)];
            }
            else if (userPets.ContainsKey(pet1))
            {
                return userPets[pet1];
            }
            else if (userPets.ContainsKey(pet2))
            {
                return userPets[pet2];
            }
            else
            {
                return userPets[(PetType)rnd.Next(0, userPets.Count)];
            }
        }

        if (roll > 0.9)
        {
            return GetRandomPet(PetType.Jew, PetType.TylerJuan);
        }
        else if (roll >= 0.55)
        {
            return GetRandomPet(PetType.Goran, PetType.Maybich);
        }
        else
        {
            return GetRandomPet(PetType.SuperNed, PetType.Pinata);
        }
    }

    public int Fill(ref int maxFill)
    {
        int eggsFilled = 0;
        foreach (var line in Lines)
        {
            if (line.LastCheckDate != null)
                continue;

            if (maxFill == 0)
                break;

            eggsFilled++;
            maxFill--;
            line.LastCheckDate = DateTime.Now;
        }

        UpdateState();
        return eggsFilled;
    }

    public void UpdateState()
    {
        Random rnd = new Random();
        foreach (var line in Lines)
        {
            if (line.LastCheckDate == null)
                continue;

            if (line.Progress >= 10)
                continue;

            int hoursPassed = (int)(DateTime.Now - (line.LastCheckDate.Value)).TotalHours;

            for (int i = 0; i < hoursPassed; i++)
            {
                if (line.Progress < 10 && rnd.NextDouble() < 0.334)
                {
                    line.Progress += 1;
                }
            }

            line.LastCheckDate = line.LastCheckDate.Value.AddHours(hoursPassed);
        }

        AdditionalData = JsonConvert.SerializeObject(Lines);
    }

    public override string GetDataText()
    {
        string dataText = string.Join("\n", Lines);
        return dataText;

    }

    public override void Upgrade()
    {
        Lines.Add(new());
        AdditionalData = JsonConvert.SerializeObject(Lines);
        base.Upgrade();
    }
}

public class Quarry : Plot
{
    public override string GetPlotIcon()
    {
        return ":brown_square:";
    }

    public override string GetDataText()
    {
        return $"Baps mined: {BapsMined()}";
    }

    public override bool IsReadyForCollecting()
    {
        return BapsMined() > 0;
    }

    public int BapsMined()
    {
        TimeSpan ts = DateTime.Now - (LastActionDate ?? DateTime.Now);
        int baps = (int)ts.TotalMinutes / (9 - (3 * (Level - 1)));
        return baps;
    }

    public override async Task<string> CollectAsync(int plotIndex, UserPets userPets)
    {
        int baps = BapsMined();
        this.LastActionDate = DateTime.Now;

        await Data.Data.UpdatePlotAsync(this);
        await Data.Data.SaveBalance(UserId, baps, false);

        return $"collected {baps} baps from quarry plot #{plotIndex + 1}";
    }
}

public class AbuseChamber : Plot
{
    private int Cd => 6 - (Level - 1);
    private int AbuseTime => 6;

    public override string GetPlotIcon()
    {
        return ":orange_square:";
    }

    public override Task<string> CollectAsync(int slotId, UserPets userPets) => null;

    public override bool IsReadyForCollecting()
    {
        return false;
    }

    public override void ShiftTime(int minutes)
    {
        if (IsRepairing)
        {
            if (!LastActionDate.HasValue)
                return;

            LastActionDate = LastActionDate.Value.AddMinutes(-1 * minutes);
            return;
        }
        else if (!IsEmpty)
        {
            LastActionDate = LastActionDate.Value.AddMinutes(minutes);
        }

    }

    public override string GetDataText()
    {
        if (IsEmpty)
            return "Empty";

        if (IsRepairing)
        {
            TimeSpan cdTs = DateTime.Now - LastActionDate.Value.AddHours(AbuseTime).AddHours(Cd);
            return $"Repairing: {cdTs.ToString(@"hh\:mm\:ss")}";
        }

        TimeSpan abuseTs = LastActionDate.Value.AddHours(AbuseTime) - DateTime.Now;
        return $"{AdditionalData} {abuseTs.ToString(@"hh\:mm\:ss")}";
    }

    public async Task StartAbuseAsync(PetType petType)
    {
        string petName = Pets.Dictionary[petType].Name;
        AdditionalData = petName;
        LastActionDate = DateTime.Now;

        await Data.Data.UpdatePlotAsync(this);
    }

    public bool IsAbusing()
    {
        return !string.IsNullOrEmpty(AdditionalData) && LastActionDate.HasValue && (DateTime.Now - LastActionDate.Value).TotalHours < AbuseTime;
    }

    public bool IsReadyToAbuse()
    {
        if ((LastActionDate ?? DateTime.MinValue).AddHours(Cd).AddHours(AbuseTime) < DateTime.Now)
            return true;

        return false;
    }

    private bool IsRepairing => ((LastActionDate ?? DateTime.MinValue).AddHours(AbuseTime) < DateTime.Now);
    private bool IsEmpty => ((LastActionDate ?? DateTime.MinValue).AddHours(Cd).AddHours(AbuseTime) < DateTime.Now);
}

public class HatcheryLine
{
    public int Slot { get; set; }
    public int Progress { get; set; }
    public DateTime? LastCheckDate { get; set; }

    public override string ToString()
    {
        if (!LastCheckDate.HasValue)
            return $"Slot {Slot}: **Idle**";

        if (Progress == 10)
            return $"**Egg hatched**";

        return $"Progress: {Progress}/10";
    }
}