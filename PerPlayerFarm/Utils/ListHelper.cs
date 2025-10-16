using PerPlayerFarm.Types;
using StardewModdingAPI;

namespace PerPlayerFarm.Utils
{
    internal static class ListHelper
    {
        public static List<WarpLocations>? ConvertStringForList(string inputString, IMonitor monitor, ITranslationHelper translate, int groupSize = 5)
        {
            try
            {
                var elements = inputString
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

                if (elements.Count % groupSize != 0)
                {
                    monitor.Log($"{translate.Get("derexsv.ppf.log.notice.convert_string_for_list_error_is_not_divisible_by_the_configuration", new { groupSize = groupSize })}", LogLevel.Warn);
                    return null;
                }

                var locationList = elements
                    .Select((element, index) => new { element, index })
                    .GroupBy(x => x.index / groupSize)
                    .Select(group => group.Select(x => x.element).ToArray())
                    .Select(groupOfFive =>
                    {
                        return new WarpLocations
                        {
                            X = int.TryParse(groupOfFive[0], out int x) ? x : 0,
                            Y = int.TryParse(groupOfFive[1], out int y) ? y : 0,
                            TargetName = groupOfFive[2],
                            TargetX = int.TryParse(groupOfFive[3], out int targetX) ? targetX : 0,
                            TargetY = int.TryParse(groupOfFive[4], out int targetY) ? targetY : 0
                        };
                    })
                    .ToList();
                return locationList;
            }
            catch
            {

                monitor.Log($"{translate.Get("derexsv.ppf.log.notice.convert_string_for_list_error_exception")}", LogLevel.Warn);
                return null;
            }
        }


        public static string ConvertListForString(List<WarpLocations> list)
        {
            var stringSegments = list.Select(loc => $"{loc.X} {loc.Y} {loc.TargetName} {loc.TargetX} {loc.TargetY}");
            return string.Join(" ", stringSegments);
        }

    }
}