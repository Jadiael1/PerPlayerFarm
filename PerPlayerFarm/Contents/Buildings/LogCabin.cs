using Microsoft.Xna.Framework;
using StardewValley.GameData.Buildings;

namespace PerPlayerFarm.Contents.Buildings
{
    public class PPFLogCabin : BuildingData
    {
        public PPFLogCabin()
        {
            this.Texture = "Buildings/Log Cabin";
            this.SourceRect = new Rectangle(0, 0, 80, 112);
            this.Size = new Point(5, 3);
            this.HumanDoor = new Point(2, 1);
            this.MaxOccupants = 0;
            this.Builder = "None";
            this.FadeWhenBehind = true;
            this.SortTileOffset = 1f;
            this.DrawShadow = true;
            this.AllowsFlooringUnderneath = true;
            this.CollisionMap = "XXXXX\nXXXXX\nOOOOX";
            this.ActionTiles = new()
            {
                new()
                {
                    Id = "Default_OpenMailbox",
                    Tile = new Point(4, 2),
                    Action = "Mailbox"
                },
                new()
                {
                    Id = "Door_CustomAction",
                    Tile = new Point(2, 1),
                    Action = "PPF_Door"
                }
            };
            this.TileProperties = new()
            {
                new()
                {
                    Id = "Default_Porch_NoSpawn",
                    Layer = "Back",
                    Name = "NoSpawn",
                    Value = "All",
                    TileArea = new Rectangle(0, 2, 5, 1)
                },
                new()
                {
                    Id = "Default_Porch_NotBuildable",
                    Layer = "Back",
                    Name = "Buildable",
                    Value = "f",
                    TileArea = new Rectangle(0, 2, 5, 1)
                }
            };
        }
    }
}