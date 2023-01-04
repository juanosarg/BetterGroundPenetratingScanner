using Verse;

namespace BGPScanner
{
    internal class BGPScannerSettings : ModSettings
    {
        public float timeMultiplier = 1.5f;
        public float autoModeEfficiency = 0.75f;
        public bool autoMode = true;
        public bool allowUnderRoof = false;
        public bool divideByCommonality = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref timeMultiplier, "timeMultiplier");
            Scribe_Values.Look(ref autoModeEfficiency, "autoModeEfficiency");
            Scribe_Values.Look(ref autoMode, "autoMode");
            Scribe_Values.Look(ref allowUnderRoof, "allowUnderRoof");
            Scribe_Values.Look(ref divideByCommonality, "divideByCommonality");
        }
    }
}
