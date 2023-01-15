using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BGPScanner
{
    [HarmonyPatch(typeof(Alert_CannotBeUsedRoofed))]
    [HarmonyPatch("UnusableBuildings", MethodType.Getter)]
    public static class Alert_CannotBeUsedRoofed_UnusableBuildings
    {
        public static void Postfix(ref List<Thing> __result)
        {
            List<Thing> result = new List<Thing>();
            for (int i = 0; i < __result.Count; i++)
            {
                var r = __result[i];
                if (r.def.defName == "GroundPenetratingScanner" && BGPScannerMod.settings.allowUnderRoof)
                    continue;

                result.Add(r);
            }
            __result = result;
        }
    }
}
