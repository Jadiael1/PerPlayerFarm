using PerPlayerFarm.Config;
using PerPlayerFarm.Helpers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace PerPlayerFarm.Events.ButtonPressed
{
    public static class ButtonPressed
    {
        private static SButton TravelKey = SButton.K;

        public static void OnButtonPressed(
            IModHelper helper,
            IMonitor monitor,
            ModConfig config,
            object? s,
            ButtonPressedEventArgs e
        )
        {
            TravelKey = config.OpenTravelMenuKey;

            if (PpfMailboxHelper.TryHandlePpfMailboxAction(helper, e))
                return;

            OpenTravelMenu(helper, monitor, s, e);
            SetupFarmHousePPFDoorWarp(helper, monitor, s, e);
        }

        private static void SetupFarmHousePPFDoorWarp(
            IModHelper helper,
            IMonitor monitor,
            object? s,
            ButtonPressedEventArgs e
        )
        {
            if (!Context.IsWorldReady)
                return;

            if (Game1.activeClickableMenu != null)
                return;

            if (!e.Button.IsActionButton())
                return;

            WarpHelper.SetupFarmHousePPFDoorWarp(helper, monitor, s, e);
        }

        private static void OpenTravelMenu(
            IModHelper helper,
            IMonitor monitor,
            object? s,
            ButtonPressedEventArgs e
            )
        {
            if (e.Button != TravelKey)
                return;

            if (!Context.IsWorldReady || !Context.IsPlayerFree)
                return;

            if (Game1.activeClickableMenu is not null || Game1.eventUp || Game1.CurrentEvent is not null)
                return;

            var loc = Game1.currentLocation;

            string name = loc?.NameOrUniqueName ?? string.Empty;

            bool isFarmContext = name.Equals("Farm", StringComparison.OrdinalIgnoreCase) || name.StartsWith("PPF_", StringComparison.OrdinalIgnoreCase);
            if (!isFarmContext)
                return;

            helper.Input.Suppress(e.Button);

            TravelMenuManager.OpenTravelMenu(helper, monitor);
        }
    }
}