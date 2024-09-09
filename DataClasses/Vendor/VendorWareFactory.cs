using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KushBot.DataClasses.Vendor
{
    public class VendorWareFactory
    {
        public List<Ware> GetRandomWares()
        {
            Random rnd = new Random();
            return Enum
                .GetValues(typeof(VendorWare))
                .Cast<VendorWare>()
                .OrderBy(e => rnd.NextDouble())
                .Take(3)
                .Select(e => GetWare(e))
                .ToList();
        }

        private Ware GetWare(VendorWare wareType) =>
            wareType switch
            {
                VendorWare.Cheems => new CheemsWare(),
                VendorWare.Item => new ItemWare(),
                //VendorWare.PetFood => new FoodWare(),
                VendorWare.PetFoodCommon => new FoodWare(0, 2, VendorWare.PetFoodCommon),
                VendorWare.PetFoodRare => new FoodWare(2, 4, VendorWare.PetFoodRare),
                VendorWare.PetFoodEpic => new FoodWare(4, 6, VendorWare.PetFoodEpic),
                VendorWare.BossTicket => new TicketWare(),
                VendorWare.Icon => new IconWare(),
                VendorWare.Rejuvenation => new RejuvenationWare(),
                VendorWare.Egg => new EggWare(),
                //VendorWare.PetDupe => new PetDupeWare(),
                VendorWare.PetDupeCommon => new PetDupeWare(0, 2, VendorWare.PetDupeCommon),
                VendorWare.PetDupeRare => new PetDupeWare(2, 4, VendorWare.PetDupeRare),
                VendorWare.PetDupeEpic => new PetDupeWare(4, 6, VendorWare.PetDupeEpic),
                VendorWare.PlotBoost => new PlotBoostWare(),
                VendorWare.KushGym => new KushGymWare(),
                VendorWare.FishingRod => new FishingRodWare(),
                VendorWare.Parasite => new ParasiteWare(),
                VendorWare.Artillery => new ArtilleryWare(),
                VendorWare.Adderal => new AdderalWare(),
                VendorWare.SlotsTokens => new SlotsTokenWare(),
                //VendorWare.Plot => new PlotWare(),
                _ => null
            };

    }
}
