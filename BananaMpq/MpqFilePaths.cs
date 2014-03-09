using System;

namespace BananaMpq
{
    public static class MpqFilePaths
    {
        public static string[] GetRelevantAdtFileNames(string continent, int x, int y)
        {
            var prefix = string.Format("{0}_{1}_{2}", GetContinentPrefix(continent), x, y);
            return new[]
            {
                prefix + ".adt",
                prefix + "_obj0.adt",
                prefix + "_obj1.adt",
                //prefix + "_tex0.adt",
                //prefix + "_tex1.adt",
            };
        }

        public static string GetWdtFileName(string continent)
        {
            return GetContinentPrefix(continent) + ".wdt";
        }

        private static string GetContinentPrefix(string continent)
        {
            return string.Format("world\\maps\\{0}\\{0}", continent);
        }

        public static string MapToInternalName(WowContinent continent)
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
                case WowContinent.Pandaria:
                    return "HawaiiMainLand";
                case WowContinent.GoldRushBG:
                    return "GoldRushBG";
                default:
                    throw new ArgumentOutOfRangeException("continent");
            }
        }
    }
}