using StardewModdingAPI;

namespace PerPlayerFarm.Config
{
    public sealed class ModConfig
    {
        public bool WarpToPPFOnHouseExit = true;
        public bool RedirectFarmhandsToPPF = true;
        public SButton OpenTravelMenuKey = SButton.K;
        // "Maps/Farm", "Maps/Farm_Fishing", "Maps/Farm_Foraging", "Maps/Farm_Mining", "Maps/Farm_Combat", "Maps/Farm_FourCorners", "Maps/Farm_Island", "Maps/Farm_Ranching"
        public string? DefaultPPFMapType { get; set; } = null;
    }
}