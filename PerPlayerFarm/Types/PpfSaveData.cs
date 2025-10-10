namespace PerPlayerFarm.Types
{
    [Serializable]
    public class PpfSaveData
    {
        public HashSet<long> OwnerUids { get; set; } = new();
    }
}