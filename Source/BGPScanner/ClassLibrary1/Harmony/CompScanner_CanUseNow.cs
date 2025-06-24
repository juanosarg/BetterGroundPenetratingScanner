using HarmonyLib;
using RimWorld;
using Verse;

namespace BGPScanner
{
    [HarmonyPatch(typeof(CompScanner))]
    [HarmonyPatch("CanUseNow", MethodType.Getter)]
    public static class CompScanner_CanUseNow
    {
        [HarmonyPrefix]
        private static bool Prefix(CompScanner __instance, ref AcceptanceReport __result)
        {
            if (BGPScannerMod.settings.allowUnderRoof)
            {
                var powerComp = (CompPowerTrader)Startup.powerComp.GetValue(__instance);
                var forbiddable = (CompForbiddable)Startup.forbiddable.GetValue(__instance);
                __result = __instance.parent.Spawned && (powerComp == null || powerComp.PowerOn) && (forbiddable == null || !forbiddable.Forbidden) && __instance.parent.Faction == Faction.OfPlayer;
                return false;
            }

            return true;
        }
    }
}
