using Microsoft.Xna.Framework;
using PerPlayerFarm.Configuration;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace PerPlayerFarm.Events.DayStarted
{
    public static class TeleportItem
    {
        private static readonly Vector2 DefaultPreferredAnchor = new(74, 15);
        private const string PlacedKey = Utils.Constants.TeleportPlacedKey;
        private const string PlacedMainFarmKey = Utils.Constants.TeleportPlacedMainFarmKey;
        private const string TeleportModDataKey = Utils.Constants.TeleportModDataKey;
        private const string TeleportQualifiedItemId = Utils.Constants.TeleportQualifiedItemId;
        private const string TeleportModDataMainFarm = Utils.Constants.TeleportModDataMainFarm;
        private const string TeleportModDataPpf = Utils.Constants.TeleportModDataPpf;

        private static Vector2 GetPreferredAnchor(GameLocation loc, ModConfig teleporterConfig)
        {
            int rawX = teleporterConfig?.Teleporter.PreferredTileX ?? (int)DefaultPreferredAnchor.X;
            int rawY = teleporterConfig?.Teleporter.PreferredTileY ?? (int)DefaultPreferredAnchor.Y;

            if (rawX < 0 || rawY < 0)
            {
                rawX = (int)DefaultPreferredAnchor.X;
                rawY = (int)DefaultPreferredAnchor.Y;
            }

            var configuredAnchor = new Vector2(rawX, rawY);
            var back = loc.Map?.GetLayer("Back");
            if (back == null) return configuredAnchor;

            int x = Math.Clamp((int)configuredAnchor.X, 0, back.LayerWidth - 1);
            int y = Math.Clamp((int)configuredAnchor.Y, 0, back.LayerHeight - 1);
            return new Vector2(x, y);
        }

        private static bool IsGood(GameLocation l, Vector2 p)
        {
            if (!l.isTileOnMap(p)) return false;
            if (!l.CanItemBePlacedHere(p)) return false; // follows vanilla placement rule
            if (l.isWaterTile((int)p.X, (int)p.Y)) return false;
            if (l.objects.ContainsKey(p)) return false;
            if (l.isObjectAtTile((int)p.X, (int)p.Y)) return false;

            var nf = l.doesTileHaveProperty((int)p.X, (int)p.Y, "NoFurniture", "Back");
            if (nf != null && nf.Equals("total", StringComparison.OrdinalIgnoreCase)) return false;

            return true;
        }

        private static bool IsGoodExact(GameLocation l, Vector2 p)
        {
            // "exact" version without fallback; uses the same validation rule
            return IsGood(l, p);
        }

        private static IEnumerable<Vector2> Spiral(Vector2 center, int maxRadius)
        {
            for (int r = 1; r <= maxRadius; r++)
            {
                int left = (int)center.X - r;
                int right = (int)center.X + r;
                int top = (int)center.Y - r;
                int bottom = (int)center.Y + r;

                for (int x = left; x <= right; x++)
                {
                    yield return new Vector2(x, top);
                    yield return new Vector2(x, bottom);
                }
                for (int y = top + 1; y <= bottom - 1; y++)
                {
                    yield return new Vector2(left, y);
                    yield return new Vector2(right, y);
                }
            }
        }

        private static Vector2? FindFirstPlaceableTile(GameLocation loc, Vector2 anchor, int maxRadius)
        {
            if (IsGood(loc, anchor))
                return anchor;

            foreach (var pos in Spiral(anchor, maxRadius))
            {
                if (IsGood(loc, pos))
                    return pos;
            }

            var back = loc.Map?.GetLayer("Back");
            if (back != null)
            {
                for (int y = 0; y < back.LayerHeight; y++)
                {
                    for (int x = 0; x < back.LayerWidth; x++)
                    {
                        var p = new Vector2(x, y);
                        if (IsGood(loc, p))
                            return p;
                    }
                }
            }

            return null;
        }

        private static bool TryPlaceBigCraftable(GameLocation loc, string qualifiedId, Vector2 anchor, IMonitor monitor, int maxRadius, ITranslationHelper translate)
        {
            Vector2? target = FindFirstPlaceableTile(loc, anchor, maxRadius);
            if (target is null)
                return false;

            try
            {
                var item = ItemRegistry.Create(qualifiedId);
                if (item is StardewValley.Object obj && obj.bigCraftable.Value)
                {
                    var tile = target.Value;

                    if (loc.objects.ContainsKey(tile))
                        return false;

                    obj.TileLocation = tile;
                    obj.modData[TeleportModDataKey] = loc.Name?.Equals("Farm", StringComparison.OrdinalIgnoreCase) == true
                        ? TeleportModDataMainFarm
                        : TeleportModDataPpf;

                    loc.objects[tile] = obj;
                    return true;
                }
            }
            catch (Exception ex)
            {
                monitor.Log(translate.Get(
                    "derexsv.ppf.log.error.teleporter_create_failed",
                    new { id = qualifiedId, error = ex.ToString() }
                ), LogLevel.Error);
            }
            return false;
        }

        private static bool TryPlaceExactlyAt(GameLocation loc, string qualifiedId, Vector2 tile, IMonitor monitor, ITranslationHelper translate)
        {
            if (!IsGoodExact(loc, tile))
                return false;

            try
            {
                var item = ItemRegistry.Create(qualifiedId);
                if (item is StardewValley.Object obj && obj.bigCraftable.Value)
                {
                    if (loc.objects.ContainsKey(tile))
                        return false;

                    obj.TileLocation = tile;
                    obj.modData[TeleportModDataKey] = loc.Name?.Equals("Farm", StringComparison.OrdinalIgnoreCase) == true
                        ? TeleportModDataMainFarm
                        : TeleportModDataPpf;

                    loc.objects[tile] = obj;
                    return true;
                }
            }
            catch (Exception ex)
            {
                monitor.Log(translate.Get(
                    "derexsv.ppf.log.error.teleporter_create_failed_at_tile",
                    new { id = qualifiedId, tile = tile.ToString(), error = ex.ToString() }
                ), LogLevel.Error);
            }
            return false;
        }

        public static void OnDayStartedRetag(object? sender, DayStartedEventArgs e, IMonitor monitor, ITranslationHelper translate)
        {
            try
            {
                if (!Context.IsWorldReady || !Context.IsMainPlayer)
                    return;

                foreach (var loc in Game1.locations)
                {
                    if (loc == null) continue;
                    var name = loc.NameOrUniqueName ?? loc.Name ?? string.Empty;
                    bool isFarm = name.Equals("Farm", StringComparison.OrdinalIgnoreCase);
                    bool isPpf = name.StartsWith("PPF_", StringComparison.OrdinalIgnoreCase);
                    if (!isFarm && !isPpf) continue;

                    RetagMiniObelisksIn(loc, onlyIfMissingTag: true, monitor, translate);
                }
            }
            catch (Exception ex)
            {
                monitor?.Log(translate.Get(
                    "derexsv.ppf.log.warn.retag_failed_start",
                    new { error = ex.ToString() }
                ), LogLevel.Warn);
            }
        }

        private static void RetagMiniObelisksIn(GameLocation loc, bool onlyIfMissingTag, IMonitor monitor, ITranslationHelper translate)
        {
            try
            {
                string name = loc.NameOrUniqueName ?? loc.Name ?? string.Empty;
                bool isFarm = name.Equals("Farm", StringComparison.OrdinalIgnoreCase);
                bool isPpf = name.StartsWith("PPF_", StringComparison.OrdinalIgnoreCase);
                if (!isFarm && !isPpf) return;

                foreach (var pair in loc.objects.Pairs)
                {
                    var obj = pair.Value;
                    if (obj == null || !obj.bigCraftable.Value) continue;

                    bool isMiniObelisk = string.Equals(obj.QualifiedItemId, TeleportQualifiedItemId, StringComparison.Ordinal)
                        || obj.ParentSheetIndex == 238;

                    if (!isMiniObelisk) continue;

                    if (onlyIfMissingTag && obj.modData != null && obj.modData.ContainsKey(TeleportModDataKey))
                        continue;

                    if (obj.modData != null)
                        obj.modData[TeleportModDataKey] = isFarm ? TeleportModDataMainFarm : TeleportModDataPpf;
                }
            }
            catch (Exception ex)
            {
                monitor.Log(translate.Get(
                    "derexsv.ppf.log.warn.retag_failed_location",
                    new { error = ex.ToString() }
                ), LogLevel.Warn);
            }
        }

        private static bool TryFindExistingTeleport(GameLocation loc, out Vector2 tile)
        {
            tile = default;
            if (loc.objects == null || loc.objects.Count() == 0)
                return false;

            foreach (var pair in loc.objects.Pairs)
            {
                var obj = pair.Value;
                if (obj is null || !obj.bigCraftable.Value) continue;

                bool hasTag = obj.modData.TryGetValue(TeleportModDataKey, out var tag) == true && !string.IsNullOrEmpty(tag);
                bool isOurBC = string.Equals(obj.QualifiedItemId, TeleportQualifiedItemId, StringComparison.Ordinal);


                if (hasTag || isOurBC)
                {
                    tile = pair.Key;
                    return true;
                }
            }
            return false;
        }

        private static Vector2 GetSafeAnchor(GameLocation loc)
        {
            var back = loc.Map?.GetLayer("Back");
            if (back == null)
                return new Vector2(10, 10);

            int cx = Math.Max(0, Math.Min(back.LayerWidth - 1, back.LayerWidth / 2));
            int cy = Math.Max(0, Math.Min(back.LayerHeight - 1, back.LayerHeight / 2));
            return new Vector2(cx, cy);
        }

        private static void EnsureInLocation(GameLocation loc, string flagKey, IMonitor monitor, ITranslationHelper translate, ModConfig config)
        {
            if (loc == null)
            {
                monitor.Log(translate.Get("derexsv.ppf.log.warn.ensure_location_null"), LogLevel.Warn);
                return;
            }

            // if there is already a teleport for this mod here, just ensure the flag
            if (TryFindExistingTeleport(loc, out _))
            {
                if (!loc.modData.TryGetValue(flagKey, out var v) || v != "1")
                    loc.modData[flagKey] = "1";

                // relabels any untagged Mini-Obelisk (old cases)
                RetagMiniObelisksIn(loc, onlyIfMissingTag: true, monitor, translate);
                monitor.Log(translate.Get(
                    "derexsv.ppf.log.trace.teleporter_already_present",
                    new { location = loc.Name ?? string.Empty }
                ), LogLevel.Trace);
                return;
            }

            // flag found but object missing => recreate
            if (loc.modData.TryGetValue(flagKey, out var placed) && placed == "1")
                monitor.Log(translate.Get(
                    "derexsv.ppf.log.trace.teleporter_flag_recreate",
                    new { location = loc.Name ?? string.Empty }
                ), LogLevel.Trace);

            // exact attempt at (76.19)
            var preferred = GetPreferredAnchor(loc, config);
            bool placedAtPreferred = TryPlaceExactlyAt(loc, TeleportQualifiedItemId, preferred, monitor, translate);

            // fallback
            bool placedOk = placedAtPreferred || TryPlaceBigCraftable(loc, TeleportQualifiedItemId, GetSafeAnchor(loc), monitor, maxRadius: 30, translate);

            if (placedOk)
            {
                loc.modData[flagKey] = "1";
                // ensures that any other existing Mini-Obelisks (without a tag) will also be tagged
                RetagMiniObelisksIn(loc, onlyIfMissingTag: true, monitor, translate);
                monitor.Log(translate.Get(
                    "derexsv.ppf.log.info.teleporter_added",
                    new { location = loc.Name ?? string.Empty }
                ), LogLevel.Info);
            }
            else
            {
                monitor.Log(translate.Get(
                    "derexsv.ppf.log.warn.teleporter_failed",
                    new { location = loc.Name ?? string.Empty }
                ), LogLevel.Warn);
            }
        }

        public static void Initializer(IMonitor monitor, ITranslationHelper translate, ModConfig config)
        {
            if (!Context.IsMainPlayer)
                return;

            bool anyPpf = false;

            foreach (var loc in Game1.locations)
            {
                if (loc == null) continue;
                var name = loc.Name ?? string.Empty;
                if (name.StartsWith("PPF_", StringComparison.Ordinal))
                {
                    anyPpf = true;
                    EnsureInLocation(loc, PlacedKey, monitor, translate, config);
                }
            }

            if (anyPpf)
            {
                var farm = Game1.getLocationFromName("Farm");
                if (farm != null)
                    EnsureInLocation(farm, PlacedMainFarmKey, monitor, translate, config);
            }

        }

        internal static void EnsureIn(GameLocation loc, IMonitor monitor, ITranslationHelper translate, ModConfig config)
        {
            if (!Context.IsMainPlayer)
            {
                monitor.Log(translate.Get("derexsv.ppf.log.warn.host_only_teleporters"), LogLevel.Warn);
                return;
            }

            if (loc == null)
            {
                monitor.Log(translate.Get("derexsv.ppf.log.warn.ensure_in_null_location"), LogLevel.Warn);
                return;
            }

            string name = loc.NameOrUniqueName ?? loc.Name ?? string.Empty;

            // decides the correct flag according to Farm vs PPF_*
            string flagKey =
                name.Equals("Farm", StringComparison.OrdinalIgnoreCase) ? PlacedMainFarmKey :
                (name.StartsWith("PPF_", StringComparison.OrdinalIgnoreCase) ? PlacedKey : string.Empty);

            if (string.IsNullOrEmpty(flagKey))
            {
                monitor.Log(translate.Get(
                    "derexsv.ppf.log.info.ensure_in_ignored",
                    new { location = name }
                ), LogLevel.Info);
                return;
            }

            EnsureInLocation(loc, flagKey, monitor, translate, config);
        }
    }
}
