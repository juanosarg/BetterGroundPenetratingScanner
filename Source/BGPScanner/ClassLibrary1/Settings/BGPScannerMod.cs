using RimWorld;
using UnityEngine;
using Verse;

namespace BGPScanner
{
    internal class BGPScannerMod : Mod
    {
        public static BGPScannerSettings settings;

        public BGPScannerMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<BGPScannerSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard lst = new Listing_Standard();
            lst.Begin(inRect);
            lst.CheckboxLabeled("BGPScanner_DivideByCommonality".Translate(), ref settings.divideByCommonality, "BGPScanner_DivideByCommonalityDesc".Translate());
            lst.Label($"{"BGPScanner_Multiplier".Translate()}: {settings.timeMultiplier:F2}");
            settings.timeMultiplier = lst.Slider(settings.timeMultiplier, 1f, 5f);
            lst.CheckboxLabeled("BGPScanner_Auto".Translate(), ref settings.autoMode, "BGPScanner_AutoDsc".Translate());
            if (settings.autoMode)
            {
                lst.Label($"{"BGPScanner_AutoEff".Translate()}: {settings.autoModeEfficiency:F2}");
                settings.autoModeEfficiency = lst.Slider(settings.autoModeEfficiency, 0.1f, 5f);
            }
            lst.CheckboxLabeled("BGPScanner_AllowUnderRoof".Translate(), ref settings.allowUnderRoof);
            lst.End();
            base.WriteSettings();
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            var scanner = DefDatabase<ThingDef>.GetNamed("GroundPenetratingScanner");

            if (settings.allowUnderRoof)
            {
                scanner.placeWorkers.RemoveAll(p => p == typeof(PlaceWorker_NotUnderRoof));
            }
            else if (scanner.placeWorkers.Find(p => p == typeof(PlaceWorker_NotUnderRoof)) == null)
            {
                scanner.placeWorkers.Add(typeof(PlaceWorker_NotUnderRoof));
            }

            Startup.placeWorkersInstantiatedInt.SetValue(scanner, null);
        }

        public override string SettingsCategory() => "BGPScanner_Name".Translate();
    }
}
