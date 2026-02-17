namespace PerPlayerFarm.Types
{
    [Serializable]
    public class PpfSaveData
    {
        public bool IsDisabled { get; set; }
        public HashSet<long> OwnerUids { get; set; } = new();
    }
}
