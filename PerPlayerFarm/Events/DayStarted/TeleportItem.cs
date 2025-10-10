using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace PerPlayerFarm.Events.DayStarted
{
    public static class TeleportItem
    {
        private static readonly Vector2 PreferredAnchor = new(74, 15);
        private const string PlacedKey = Utils.Constants.TeleportPlacedKey;
        private const string PlacedMainFarmKey = Utils.Constants.TeleportPlacedMainFarmKey;
        private const string TeleportModDataKey = Utils.Constants.TeleportModDataKey;
        private const string TeleportQualifiedItemId = Utils.Constants.TeleportQualifiedItemId;
        private const string TeleportModDataMainFarm = Utils.Constants.TeleportModDataMainFarm;
        private const string TeleportModDataPpf = Utils.Constants.TeleportModDataPpf;


        private static Vector2 GetPreferredAnchor(GameLocation loc)
        {
            var back = loc.Map?.GetLayer("Back");
            if (back == null) return PreferredAnchor;

            int x = Math.Clamp((int)PreferredAnchor.X, 0, back.LayerWidth - 1);
            int y = Math.Clamp((int)PreferredAnchor.Y, 0, back.LayerHeight - 1);
            return new Vector2(x, y);
        }

        private static bool IsGood(GameLocation l, Vector2 p)
        {
            if (!l.isTileOnMap(p)) return false;
            if (!l.CanItemBePlacedHere(p)) return false; // regra oficial
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

        private static bool TryPlaceBigCraftable(GameLocation loc, string qualifiedId, Vector2 anchor, IMonitor monitor, int maxRadius)
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
                monitor.Log($"[PPF] Error creating '{qualifiedId}': {ex}", LogLevel.Error);
            }
            return false;
        }

        private static bool TryPlaceExactlyAt(GameLocation loc, string qualifiedId, Vector2 tile, IMonitor monitor)
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
                monitor.Log($"[PPF] Error creating '{qualifiedId}' in {tile}: {ex}", LogLevel.Error);
            }
            return false;
        }

        public static void OnDayStartedRetag(object? sender, DayStartedEventArgs e, IMonitor monitor)
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

                    RetagMiniObelisksIn(loc, onlyIfMissingTag: true, monitor);
                }
            }
            catch (Exception ex)
            {
                monitor?.Log($"[PPF] OnDayStartedRetag failed: {ex}", LogLevel.Warn);
            }
        }

        private static void RetagMiniObelisksIn(GameLocation loc, bool onlyIfMissingTag, IMonitor monitor)
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
                monitor.Log($"[PPF] RetagMiniObelisksIn failed: {ex}", LogLevel.Warn);
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


                if (obj.modData is not null || hasTag || isOurBC)
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

        private static void EnsureInLocation(GameLocation loc, string flagKey, IMonitor monitor)
        {
            if (loc == null)
            {
                monitor.Log("[PPF] EnsureInLocation: null location.", LogLevel.Warn);
                return;
            }

            // if there is already a teleport for this mod here, just ensure the flag
            if (TryFindExistingTeleport(loc, out _))
            {
                if (!loc.modData.TryGetValue(flagKey, out var v) || v != "1")
                    loc.modData[flagKey] = "1";

                // relabels any untagged Mini-Obelisk (old cases)
                RetagMiniObelisksIn(loc, onlyIfMissingTag: true, monitor);
                monitor.Log($"[PPF] Teleport already present in {loc.Name} (nothing to do).", LogLevel.Trace);
                return;
            }

            // flag found but object missing => recreate
            if (loc.modData.TryGetValue(flagKey, out var placed) && placed == "1")
                monitor.Log($"[PPF] Flag found, but object missing {loc.Name}. Recreating…", LogLevel.Trace);

            // exact attempt at (76.19)
            var preferred = GetPreferredAnchor(loc);
            bool placedAtPreferred = TryPlaceExactlyAt(loc, TeleportQualifiedItemId, preferred, monitor);

            // fallback
            bool placedOk = placedAtPreferred || TryPlaceBigCraftable(loc, TeleportQualifiedItemId, GetSafeAnchor(loc), monitor, maxRadius: 30);

            if (placedOk)
            {
                loc.modData[flagKey] = "1";
                // ensures that any other existing Mini-Obelisks (without a tag) will also be tagged
                RetagMiniObelisksIn(loc, onlyIfMissingTag: true, monitor);
                monitor.Log($"[PPF] Mini-Obelisk added in {loc.Name}.", LogLevel.Info);
            }
            else
            {
                monitor.Log($"[PPF] Failed to place Mini-Obelisk on {loc.Name} (no valid tiles found).", LogLevel.Warn);
            }
        }

        public static void Initializer(IMonitor monitor)
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
                    EnsureInLocation(loc, PlacedKey, monitor);
                }
            }

            if (anyPpf)
            {
                var farm = Game1.getLocationFromName("Farm");
                if (farm != null)
                    EnsureInLocation(farm, PlacedMainFarmKey, monitor);
            }

        }

        internal static void EnsureIn(GameLocation loc, IMonitor monitor)
        {
            if (!Context.IsMainPlayer)
            {
                monitor.Log("[PPF] Only the host can guarantee teleporters.", LogLevel.Warn);
                return;
            }

            if (loc == null)
            {
                monitor.Log("[PPF] EnsureIn: Null location", LogLevel.Warn);
                return;
            }

            string name = loc.NameOrUniqueName ?? loc.Name ?? string.Empty;

            // decides the correct flag according to Farm vs PPF_*
            string flagKey =
                name.Equals("Farm", StringComparison.OrdinalIgnoreCase) ? PlacedMainFarmKey :
                (name.StartsWith("PPF_", StringComparison.OrdinalIgnoreCase) ? PlacedKey : string.Empty);

            if (string.IsNullOrEmpty(flagKey))
            {
                monitor.Log($"[PPF] '{name}' is not Farm nor PPF_*; ignored.", LogLevel.Info);
                return;
            }

            EnsureInLocation(loc, flagKey, monitor);
        }
    }
}