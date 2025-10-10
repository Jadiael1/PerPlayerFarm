namespace PerPlayerFarm.Configuration
{
    public sealed class ModConfig
    {
        public TeleporterConfig Teleporter { get; set; } = new();
    }

    public sealed class TeleporterConfig
    {
        public int PreferredTileX { get; set; } = 74;
        public int PreferredTileY { get; set; } = 15;
    }
}
