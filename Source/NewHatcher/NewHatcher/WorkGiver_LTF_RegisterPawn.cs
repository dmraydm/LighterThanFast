using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;

namespace NewHatcher

{
    public class WorkGiver_MindcontrolBench : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForDef(ThingDef.Named("LTF_MindcontrolBench"));
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.InteractionCell;
            }
        }

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
			// ??????????
            return pawn.Map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.DeepDrill).Cast<Thing>();
        }

        public override bool ShouldSkip(Pawn pawn)
        {
            List<Building> allBuildingsColonist = pawn.Map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < allBuildingsColonist.Count; i++)
            {
                if (allBuildingsColonist[i].def == ThingDef.Named("LTF_MindcontrolBench"))
                {
                    CompPowerTrader comp = allBuildingsColonist[i].GetComp<CompPowerTrader>();
                    if (comp == null || comp.PowerOn)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public JobDef JobDefToUse
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("LTF_RegisterPawn");
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            // non faction bench
            if (t.Faction != pawn.Faction)
            {
                return false;
            }
            Building building = t as Building;

            // Null bench
            if (building == null)
            {
                return false;
            }
            // Forbidden
            if (building.IsForbidden(pawn))
            {
                return false;
            }
            LocalTargetInfo target = building;
            // Pawn can reserve
            if (!pawn.CanReserve(target, 1, -1, null, forced))
            {
                return false;
            }

            Comp_LTF_RegisterPawn comp_RegisterPawn = building.TryGetComp<Comp_LTF_RegisterPawn>();
            // No target set
            if (!comp_RegisterPawn.isTargetSet())
            {
                return false;
            }
            return comp_RegisterPawn.PowerAndTargetPawnInRadius() && !building.IsBurning();
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return new Job(JobDefToUse, t, 1500, true);
        }
    }
}
