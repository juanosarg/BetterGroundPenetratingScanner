using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace BGPScanner
{
    [StaticConstructorOnStartup]
    public class CompBetterDeepScanner : CompDeepScanner
    {
        private static readonly Texture2D questionMark = ContentFinder<Texture2D>.Get("ui/icons/questionmark");

        public List<ThingDef> things = null;
        public string choosedOutcome = null;
        public Command_Action command = null;

        public new CompProperties_BetterScannerMineralsDeep Props => props as CompProperties_BetterScannerMineralsDeep;

        private ThingDef ChoosedOutcome => DefDatabase<ThingDef>.GetNamed(choosedOutcome);

        private float ScanFindGuaranteedDays
        {
            get
            {
                if (choosedOutcome != null)
                {
                    var time = Props.scanFindGuaranteedDays * BGPScannerMod.settings.timeMultiplier;
                    return BGPScannerMod.settings.divideByCommonality ? time / ChoosedOutcome.deepCommonality : time;
                }
                else
                {
                    return Props.scanFindGuaranteedDays;
                }
            }
        }

        private float ScanFindMtbDays
        {
            get
            {
                if (choosedOutcome != null)
                {
                    var time = Props.scanFindMtbDays * BGPScannerMod.settings.timeMultiplier;
                    return BGPScannerMod.settings.divideByCommonality ? time / ChoosedOutcome.deepCommonality : time;
                }
                else
                {
                    return Props.scanFindMtbDays;
                }
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            things = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(t => t.deepCommonality != 0);

            command = new Command_Action
            {
                action = () =>
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    for (int i = 0; i < things.Count; i++)
                    {
                        ThingDef thingDef = things[i];
                        FloatMenuOption floatMenuOption = new FloatMenuOption(thingDef.LabelCap, () =>
                        {
                            foreach (object selectedObject in Find.Selector.SelectedObjects)
                            {
                                if (selectedObject is Thing thing)
                                {
                                    CompBetterDeepScanner comp = thing.TryGetComp<CompBetterDeepScanner>();
                                    if (comp != null)
                                        comp.choosedOutcome = thingDef.defName;
                                }
                            }
                        }, extraPartWidth: 29f, extraPartOnGUI: rect => Widgets.InfoCardButton(rect.x + 5f, rect.y + ((rect.height - 24f) / 2f), thingDef));
                        options.Add(floatMenuOption);
                    }

                    FloatMenuOption f = new FloatMenuOption("Random outcome", () =>
                    {
                        foreach (object selectedObject in Find.Selector.SelectedObjects)
                        {
                            if (selectedObject is Thing thing)
                            {
                                CompBetterDeepScanner comp = thing.TryGetComp<CompBetterDeepScanner>();
                                if (comp != null)
                                    comp.choosedOutcome = null;
                            }
                        }
                    }, extraPartWidth: 29f);
                    options.Add(f);

                    Find.WindowStack.Add(new FloatMenu(options));
                }
            };
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            if (choosedOutcome == null)
            {
                command.defaultLabel = $"{"CommandSelectMineralToScanFor".Translate()}: random";
                command.icon = questionMark;
            }
            else
            {
                var def = ChoosedOutcome;
                command.defaultLabel = $"{"CommandSelectMineralToScanFor".Translate()}: {def.LabelCap}";
                command.icon = def.uiIcon;
                command.iconAngle = def.uiIconAngle;
                command.iconOffset = def.uiIconOffset;
            }
            yield return command;
        }

        public override string CompInspectStringExtra()
        {
            string str = "";
            if (lastScanTick > (Find.TickManager.TicksGame - 30))
                str = (string)(str + ("UserScanAbility".Translate() + ": " + lastUserSpeed.ToStringPercent() + "\n" + "ScanAverageInterval".Translate() + ": " + "PeriodDays".Translate((ScanFindMtbDays / lastUserSpeed).ToString("F1")) + "\n"));
            return (string)(str + "ScanningProgressToGuaranteedFind".Translate() + ": " + (daysWorkingSinceLastFinding / ScanFindGuaranteedDays).ToStringPercent());
        }

        public override void CompTick()
        {
            if (BGPScannerMod.settings.autoMode && CanUseNow && !parent.Map.reservationManager.IsReservedByAnyoneOf(parent, Faction.OfPlayer))
            {
                lastScanTick = Find.TickManager.TicksGame;
                lastUserSpeed = BGPScannerMod.settings.autoModeEfficiency;
                daysWorkingSinceLastFinding += lastUserSpeed / 60000f;
                if (!TickDoesFind(lastUserSpeed))
                    return;

                DoFindNoPawn();
                daysWorkingSinceLastFinding = 0f;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref choosedOutcome, "choosedOutcome", null);
        }

        protected override void DoFind(Pawn worker)
        {
            Map map = parent.Map;
            if (!CellFinderLoose.TryFindRandomNotEdgeCellWith(10, x => CanScatterAt(x, map), map, out IntVec3 result))
                Log.Error("Could not find a center cell for deep scanning lump generation!");
            ThingDef def = ChooseLumpDef();
            int numCells = Mathf.CeilToInt(def.deepLumpSizeRange.RandomInRange);
            foreach (IntVec3 intVec3 in GridShapeMaker.IrregularLump(result, map, numCells))
            {
                if (CanScatterAt(intVec3, map) && !intVec3.InNoBuildEdgeArea(map))
                    map.deepResourceGrid.SetAt(intVec3, def, def.deepCountPerCell);
            }
            string key = !"LetterDeepScannerFoundLump".CanTranslate() ? (!"DeepScannerFoundLump".CanTranslate() ? "LetterDeepScannerFoundLump" : "DeepScannerFoundLump") : "LetterDeepScannerFoundLump";
            Find.LetterStack.ReceiveLetter("LetterLabelDeepScannerFoundLump".Translate() + ": " + def.LabelCap, key.Translate((NamedArgument)def.label, worker.Named("FINDER")), LetterDefOf.PositiveEvent, new LookTargets(result, map));
        }

        protected override bool TickDoesFind(float scanSpeed)
        {
            return Find.TickManager.TicksGame % 59 == 0
                   && (Rand.MTBEventOccurs(ScanFindMtbDays / scanSpeed, 60000f, 59f)
                       || (ScanFindGuaranteedDays > 0f && daysWorkingSinceLastFinding >= ScanFindGuaranteedDays));
        }

        private bool CanScatterAt(IntVec3 pos, Map map)
        {
            int index = CellIndicesUtility.CellToIndex(pos, map.Size.x);
            TerrainDef terrainDef = map.terrainGrid.TerrainAt(index);
            return (terrainDef == null || !terrainDef.IsWater || terrainDef.passability != Traversability.Impassable) && terrainDef.affordances.Contains(ThingDefOf.DeepDrill.terrainAffordanceNeeded) && !map.deepResourceGrid.GetCellBool(index);
        }

        private ThingDef ChooseLumpDef()
        {
            if (choosedOutcome != null)
                return ChoosedOutcome;

            return DefDatabase<ThingDef>.AllDefs.RandomElementByWeight(def => def.deepCommonality);
        }

        private void DoFindNoPawn()
        {
            Map map = parent.Map;
            if (!CellFinderLoose.TryFindRandomNotEdgeCellWith(10, x => CanScatterAt(x, map), map, out IntVec3 result))
                Log.Error("Could not find a center cell for deep scanning lump generation!");

            ThingDef def = ChooseLumpDef();
            int numCells = Mathf.CeilToInt(def.deepLumpSizeRange.RandomInRange);
            foreach (IntVec3 intVec3 in GridShapeMaker.IrregularLump(result, map, numCells))
            {
                if (CanScatterAt(intVec3, map) && !intVec3.InNoBuildEdgeArea(map))
                    map.deepResourceGrid.SetAt(intVec3, def, def.deepCountPerCell);
            }

            Find.LetterStack.ReceiveLetter("LetterLabelDeepScannerFoundLump".Translate() + ": " + def.LabelCap, "BGPScanner_AutoFind".Translate(def.label), LetterDefOf.PositiveEvent, new LookTargets(result, map));
        }
    }
}