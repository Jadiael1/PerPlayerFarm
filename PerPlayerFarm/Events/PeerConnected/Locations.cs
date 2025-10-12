using System.Globalization;
using Microsoft.Xna.Framework;
using PerPlayerFarm.Types;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using xTile.ObjectModel;

namespace PerPlayerFarm.Events.Peerconnected
{
    public static class Locations
    {
        private static readonly string _facadeBuildingId = Utils.Constants.FacadeBuildingId;
        private static readonly string _facadeOwnerUid = Utils.Constants.FacadeOwnerUid;
        private static readonly string _saveKey = Utils.Constants.SaveKey;
        private static PpfSaveData? _cache;

        public static PpfSaveData ReadSaveData(IModHelper helper, IMonitor monitor, ITranslationHelper translate)
        {
            if (_cache is not null)
                return _cache;

            try
            {
                _cache = helper.Data.ReadSaveData<PpfSaveData>(_saveKey) ?? new PpfSaveData();
            }
            catch (Exception ex)
            {
                monitor.Log($"{translate.Get("derexsv.ppf.log.notice.failed_read_save_data", new { saveKey = _saveKey, message = ex.Message })}", LogLevel.Warn);
                _cache = new PpfSaveData();
            }
            return _cache;
        }

        private static void WriteSaveData(PpfSaveData data, IModHelper helper)
        {
            _cache = data;
            helper.Data.WriteSaveData(_saveKey, data);
        }

        public static void TrackOwner(long uid, IMonitor monitor, IModHelper helper, ITranslationHelper translate)
        {
            var data = ReadSaveData(helper, monitor, translate);
            if (data.OwnerUids.Add(uid))
                WriteSaveData(data, helper);
        }

        private static bool AlreadyHasPpfFor(long uid)
        {
            string ownerKey = Utils.Constants.OwnerKey;
            return Game1.locations
                .OfType<StardewValley.Farm>()
                .Any(loc =>
                    loc.Name.StartsWith("PPF_") &&
                    loc.modData.TryGetValue(ownerKey, out string? s) &&
                    long.TryParse(s, out long saved) &&
                    saved == uid
                );
        }

        public static void LoadInvitedPpfFarmsForHost(long uid, IMonitor monitor, ITranslationHelper translate, string? displayName = null)
        {
            if (AlreadyHasPpfFor(uid))
                return;
        
            string locName = $"PPF_{uid}";

            if (Game1.getLocationFromName(locName) is null)
            {
                var loc = new Farm($"Maps/{locName}", locName)
                {
                    IsOutdoors = true,
                    IsFarm = true
                };
                loc.isAlwaysActive.Value = true;
                loc.map.Properties["CanBuildHere"] = new PropertyValue("T");

                string finalDisplay = !string.IsNullOrWhiteSpace(displayName)
                    ? $"Farm · {displayName}"
                    : $"Farm · {uid}";
                loc.map.Properties["DisplayName"] = new PropertyValue(finalDisplay);

                loc.modData[Utils.Constants.OwnerKey] = uid.ToString(CultureInfo.InvariantCulture);

                Game1.locations.Add(loc);
                monitor.Log($"{translate.Get("derexsv.ppf.log.notice.invited_ppf_farms_were_loaded_to_the_host", new { PPF = locName })}", LogLevel.Info);
            }
        }

        public static void LoadFacadeCabinInPpfOfInvitedForHost(long uid, IMonitor monitor, ITranslationHelper translate)
        {
            string locName = $"PPF_{uid}";
            var loc = Game1.getLocationFromName(locName);
            if (loc is not Farm)
                return;

            bool alreadyExists = loc.buildings.Any(b =>
                b.buildingType?.Value == _facadeBuildingId &&
                b.modData.TryGetValue(_facadeOwnerUid, out string owner) &&
                owner == uid.ToString(CultureInfo.InvariantCulture)
            );

            if (alreadyExists)
                return;

            var place = new Vector2(64, 10);
            var facade = new Building(_facadeBuildingId, place);
            facade.daysOfConstructionLeft.Value = 0;
            facade.modData[_facadeOwnerUid] = uid.ToString(CultureInfo.InvariantCulture);
            loc.buildings.Add(facade);
            monitor.Log($"{translate.Get("derexsv.ppf.log.notice.facade_cabin_were_loaded_into_the_invited_farms_at_the_host", new { PPF = locName })}", LogLevel.Info);
        }
    }
}
