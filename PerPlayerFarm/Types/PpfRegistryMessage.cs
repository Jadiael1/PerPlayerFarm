namespace PerPlayerFarm.Types
{
    public sealed class PpfRegistryMessage
    {
        public List<PpfFarmEntry> Farms { get; set; } = new();
    }
}