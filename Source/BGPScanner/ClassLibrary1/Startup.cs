using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BGPScanner
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        public static FieldInfo placeWorkersInstantiatedInt;
        public static FieldInfo forbiddable;
        public static FieldInfo powerComp;
        public static Harmony harmony;

        static Startup()
        {
            placeWorkersInstantiatedInt = typeof(BuildableDef).GetField("placeWorkersInstantiatedInt", BindingFlags.NonPublic | BindingFlags.Instance);
            forbiddable = typeof(CompScanner).GetField("forbiddable", BindingFlags.NonPublic | BindingFlags.Instance);
            powerComp = typeof(CompScanner).GetField("powerComp", BindingFlags.NonPublic | BindingFlags.Instance);

            harmony = new Harmony("kikohi.BetterGroundPenetratingScanner");
            harmony.PatchAll();
        }
    }
}
