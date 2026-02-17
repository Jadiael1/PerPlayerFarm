using PerPlayerFarm.Config;
using PerPlayerFarm.Interfaces;
using StardewModdingAPI;
using StardewModdingAPI.Events;


namespace PerPlayerFarm.Events.GameLaunched
{
    public static class GameLaunched
    {

        public static void OnGameLaunched(
            IModHelper helper,
            IMonitor monitor,
            ModConfig config,
            IManifest modManifest,
            object? s,
            GameLaunchedEventArgs e
        )
        {
            var configMenu = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            configMenu.Register(
                mod: modManifest,
                reset: () =>
                {
                    var defaults = new ModConfig();
                    config.WarpToPPFOnHouseExit = defaults.WarpToPPFOnHouseExit;
                    config.RedirectFarmhandsToPPF = defaults.RedirectFarmhandsToPPF;
                    config.OpenTravelMenuKey = defaults.OpenTravelMenuKey;
                },
                save: () => helper.WriteConfig(config)
            );

            configMenu.AddParagraph(
                mod: modManifest,
                text: () => helper.Translation.Get("gmcm.paragraph.text")
            );

            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => helper.Translation.Get("gmcm.warp_to_ppf_on_house_exit.name"),
                tooltip: () => helper.Translation.Get("gmcm.warp_to_ppf_on_house_exit.tooltip"),
                getValue: () => config.WarpToPPFOnHouseExit,
                setValue: value => config.WarpToPPFOnHouseExit = value
            );

            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => helper.Translation.Get("gmcm.redirect_farmhands_to_ppf.name"),
                tooltip: () => helper.Translation.Get("gmcm.redirect_farmhands_to_ppf.tooltip"),
                getValue: () => config.RedirectFarmhandsToPPF,
                setValue: value => config.RedirectFarmhandsToPPF = value
            );

            configMenu.AddTextOption(
                mod: modManifest,
                name: () => helper.Translation.Get("gmcm.default_ppf_map_type.name"),
                tooltip: () => helper.Translation.Get("gmcm.default_ppf_map_type.tooltip"),
                getValue: () => config.DefaultPPFMapType ?? "",
                setValue: value =>
                {
                    var v = value?.Trim();
                    config.DefaultPPFMapType = string.IsNullOrWhiteSpace(v) ? null : v;
                }
            );

            configMenu.AddKeybind(
                mod: modManifest,
                name: () => helper.Translation.Get("gmcm.open_travel_menu_key.name"),
                tooltip: () => helper.Translation.Get("gmcm.open_travel_menu_key.tooltip"),
                getValue: () => config.OpenTravelMenuKey,
                setValue: value => config.OpenTravelMenuKey = value
            );
        }
    }
}