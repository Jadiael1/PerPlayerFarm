namespace PerPlayerFarm.Models
{
    public sealed class CharacterSetupFinishedPayload
    {
        public long UniqueMultiplayerId { get; set; }
        public string Name { get; set; } = "";
        public string FarmName { get; set; } = "";
        public bool IsFarmhand { get; set; }
    }
}