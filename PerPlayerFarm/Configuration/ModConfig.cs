namespace PerPlayerFarm.Configuration
{
    public sealed class ModConfig
    {
        public TeleporterConfig Teleporter { get; set; } = new();
        public string FarmMapWarpProperty { get; set; } = "80 15 BusStop 11 23 80 18 BusStop 11 24 80 16 BusStop 11 23 80 17 BusStop 11 23 40 65 Forest 68 2 41 65 Forest 68 2 42 65 Forest 68 2 40 -1 Backwoods 14 38 41 -1 Backwoods 14 38 34 5 FarmCave 8 11";
    }

    public sealed class TeleporterConfig
    {
        public int PreferredTileX { get; set; } = 74;
        public int PreferredTileY { get; set; } = 15;
    }
}
