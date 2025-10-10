using PerPlayerFarm.Types;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace PerPlayerFarm.Events.Saving
{
    public sealed class Saving
    {
        private readonly IModHelper _helper;

        public Saving(IModHelper helper)
        {
            _helper = helper;
            helper.Events.GameLoop.Saving += OnSaving;
        }

        public void OnSaving(object? sender, SavingEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            // rewrites the list from what is in the world (idempotent)
            PpfSaveData? data = new ();
            foreach (GameLocation? loc in Game1.locations)
            {
                if (loc?.Name is string name && name.StartsWith("PPF_", StringComparison.Ordinal))
                {
                    if (long.TryParse(name.Substring(4), out long uid))
                        data.OwnerUids.Add(uid);
                }
            }
            _helper.Data.WriteSaveData(Utils.Constants.SaveKey, data);
        }
    }
}