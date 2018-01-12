using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;

namespace NewHatcher

{
    public class WorkGiver_ShieldBench : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForDef(ThingDef.Named("LTF_ShieldBench"));
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
            //return pawn.Map.listerBuildings.AllBuildingsColonistOfDef(ThingDef.Named("LTF_ShieldBench")).Cast<Thing>();
            return pawn.Map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.DeepDrill).Cast<Thing>();
        }

        public override bool ShouldSkip(Pawn pawn)
        {
            List<Building> allBuildingsColonist = pawn.Map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < allBuildingsColonist.Count; i++)
            {
                if (allBuildingsColonist[i].def == ThingDef.Named("LTF_ShieldBench"))
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
                return DefDatabase<JobDef>.GetNamed("LTF_OperateShieldBench");
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t.Faction != pawn.Faction)
            {
                return false;
            }
            Building building = t as Building;
            if (building == null)
            {
                return false;
            }
			// HERE IF IT FAILS
            if (building.IsForbidden(pawn))
            {
                return false;
            }
            LocalTargetInfo target = building;
            if (!pawn.CanReserve(target, 1, -1, null, forced))
            {
                return false;
            }
            Comp_LTF_SynthetizeShieldLayer comp_SynthetizeShieldLayer = null;
            comp_SynthetizeShieldLayer = building.TryGetComp<Comp_LTF_SynthetizeShieldLayer>();
            if (comp_SynthetizeShieldLayer == null)
            { Log.Warning("comp_SynthetizeShieldLayer null"); }

            return comp_SynthetizeShieldLayer.PowerAndWearerInRadius() && !building.IsBurning();
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            //return new Job(JobDefOf.OperateDeepDrill, t, 1500, true);
            //JobDriver_OperateShieldBench whatever = null;
//            ThingDef.Named("LTF_OperateShieldBench").;
            //whatever = JobDef("LTF_OperateShieldBench");
            //whatever.job.def
            //return new Job(JobDef.Named("LTF_OperateShieldBench"), t, 1500, true);
            return new Job(JobDefToUse, t, 1500, true);
        }
    }
}
