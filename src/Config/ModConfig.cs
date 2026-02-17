using StardewModdingAPI;

namespace PerPlayerFarm.Config
{
    public sealed class ModConfig
    {
        public bool WarpToPPFOnHouseExit = true;
        public bool RedirectFarmhandsToPPF = true;
        public SButton OpenTravelMenuKey = SButton.K;
        public string? DefaultPPFMapType { get; set; } = null;
    }
}