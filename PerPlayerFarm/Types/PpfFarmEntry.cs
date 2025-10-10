namespace PerPlayerFarm.Types
{
    public sealed class PpfFarmEntry
    {
        public string InternalName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public long OwnerId { get; set; }
        public bool Available { get; set; }
        public bool OwnerOnline { get; set; }
    }
}