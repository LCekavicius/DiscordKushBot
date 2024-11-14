using KushBot.DataClasses;
using KushBot.DataClasses.enums;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace KushBot.Global;

public class BossBase(string name, RarityType rarity, string description, string imageUrl)
{
    public string Name { get; set; } = name;
    public RarityType Rarity { get; set; } = rarity;
    public string Description { get; set; } = description;
    public string ImageUrl { get; set; } = imageUrl;
}

public static class BossBases
{
    public static List<BossBase> Bases = new List<BossBase>();

    public static Dictionary<BossAbilities, string> AbilityDescriptions = new()
    {
        { BossAbilities.Regeneration, "Regenerate % of missing hp" },
        { BossAbilities.Harden, "Absorb % of damage taken next round" },
        { BossAbilities.Paralyze, "Paralyze a participant, making them useless for the remainder of the combat" },
        { BossAbilities.Dismantle, "Ignore damage from items and pet tiers for the remainder of the combat" },
        //{ BossAbilities.Demoralize, "Make all participants attack with their weakest pets next round" },
        { BossAbilities.Dodge, "Obtain a chance to dodge user attacks for the next round" },
    };

    public static Dictionary<BossAbilities, string> AbilityEmojis = new()
    {
        { BossAbilities.Regeneration, ":heartpulse:" },
        { BossAbilities.Harden, ":shield:" },
        { BossAbilities.Paralyze, ":syringe:" },
        { BossAbilities.Dismantle, ":screwdriver:" },
        //{ { BossAbilities.Demoralize, ":speaking_head:" },
        { BossAbilities.Dodge, ":dash:" },
    };

    public static (int? min, int? max) GetAbilityEffectRange(BossAbilities ability) =>
        ability switch
        {
            BossAbilities.Regeneration => (14,24),
            BossAbilities.Harden => (14,24),
            BossAbilities.Dodge => (14,24),
            _ => (null, null)
        };

    public static void InitializeBosses()
    {
        //#region Archons
        //ArchonList.Add(new BossDetails(
        //    name: "Abyssal Archon",
        //    rarity: "Epic",
        //    desc: "Subject ZYL has been lost. Possible containment breach.  Specifications are as follows:\r\nthe parasitic entity incubates, hatches, and matures all within the host. Detectable only by those at the Precipice. Chances of finding the host in case of escape are nearly non existent with the current Lords at the peak. If our analysis is correct, the subject, at maturity, is instantly capable destruction theorized only ascended being can wield. If our analysis is correct, it is related to the Archon that appeared in the distant past. If our analysis is correct, there is no god that could save us. Not this time, at least.",
        //    imageUrl: "https://cdn.discordapp.com/attachments/263345049486622721/1224467436963889183/ezgif.com-animated-gif-maker.gif?ex=661d992a&is=660b242a&hm=9fecb03b5fb58c04aab64a06b56aead04617ab76193b5c53241bdac5e1419f2c&"));
        //#endregion

        var text = File.ReadAllText("Data/Bosses.json");

        Bases = JsonConvert.DeserializeObject<List<BossBase>>(text);
    }
}
