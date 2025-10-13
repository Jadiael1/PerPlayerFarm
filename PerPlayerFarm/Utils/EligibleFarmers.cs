using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace PerPlayerFarm.Utils
{
    public static class EligibleFarmers
    {

        public static HashSet<long>? Get(IModHelper helper)
        {
            var eligible = new HashSet<long>();
            foreach (var peer in helper.Multiplayer.GetConnectedPlayers())
                eligible.Add(peer.PlayerID);

            var farm = Game1.getFarm();

            if (farm != null)
            {
                foreach (var building in farm.buildings)
                {
                    var indoors = building?.indoors.Value;
                    if (indoors is FarmHouse house)
                    {
                        var owner = house.owner;
                        if (owner is not null && !owner.IsMainPlayer)
                            eligible.Add(owner.UniqueMultiplayerID);
                    }
                }
            }
            return eligible.Count > 0 ? eligible : null;
        }
    }
}