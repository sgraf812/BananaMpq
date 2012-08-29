using System;

namespace BananaMpq
{
    public static class MpqFilePaths
    {
        public static string[] GetRelevantAdtFileNames(WowContinent continent, int x, int y)
        {
            var prefix = string.Format("{0}_{1}_{2}", GetContinentPrefix(continent), x, y);
            return new[]
            {
                prefix + ".adt",
                prefix + "_obj0.adt"
            };
        }

        public static string GetWdtFileName(WowContinent continent)
        {
            return GetContinentPrefix(continent) + ".wdt";
        }

        private static string GetContinentPrefix(WowContinent continent)
        {
            return string.Format("world\\maps\\{0}\\{0}", MapToFolder(continent));
        }

        private static string MapToFolder(WowContinent continent)
        {
            switch (continent)
            {
                case WowContinent.Kalimdor:
                    return "Kalimdor";
                case WowContinent.EasternKingdoms:
                    return "Azeroth";
                case WowContinent.Outlands:
                    return "Expansion01";
                case WowContinent.Northrend:
                    return "Northrend";
                default:
                    throw new ArgumentOutOfRangeException("continent");
            }
        }
    }
}