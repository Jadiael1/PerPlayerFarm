using StardewValley.GameData.BigCraftables;

namespace PerPlayerFarm.Contents.Itens
{
    public class PPFTeleporter : BigCraftableData
    {
        private readonly string _ppfTeleporterId;
        public PPFTeleporter()
        {
            _ppfTeleporterId = Utils.Constants.PpfTeleporterId;
            this.Name = _ppfTeleporterId;
            this.DisplayName = "PPF Teleporter";
            this.Description = "Teleports between PPF_* and the main Farm via the menu.";
            this.CanBePlacedIndoors = true;
            this.CanBePlacedOutdoors = true;
            this.Fragility = 0;
            this.Texture = "TileSheets/Craftables";
            this.SpriteIndex = 238;
        }
    }
}