using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;

namespace LighterThanFast
{
    public class WorkGiver_LTF_MindMine : WorkGiver_Scanner
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
            //return pawn.Map.listerBuildings.AllBuildingsColonistOfDef(ThingDef.Named("LTF_MindcontrolBench")).Cast<Thing>();
            return pawn.Map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.DeepDrill).Cast<Thing>();
        }
        
        public override bool ShouldSkip(Pawn pawn, bool forced=true)
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
                return DefDatabase<JobDef>.GetNamed("JobDriver_LTF_MindMine");
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t.Faction != pawn.Faction)
            {
                //Log.Warning("strike! bench not mine");
                return false;
            }
            Building building = t as Building;
            if (building == null)
            {
                //Log.Warning("strike! building null");
                return false;
            }
            // HERE IF IT FAILS
            if (building.IsForbidden(pawn))
            {
               // Log.Warning("strike! forbiden");
                return false;
            }
            LocalTargetInfo target = building;
            if (!pawn.CanReserve(target, 1, -1, null, forced))
            {
                //Log.Warning("strike! cant reserver");
                return false;
            }
            Comp_LTF_MindControl comp_mindControl = null;
            comp_mindControl = building.TryGetComp<Comp_LTF_MindControl>();
            if (comp_mindControl == null )
                { Log.Warning("comp_mindControl null"); }

            if (!comp_mindControl.GotThePower)
            {
                //Log.Warning("strike! no powwer");
                return false;
            }

            if (!comp_mindControl.ActorInRadius()){
                //Log.Warning("strike! no one");
                return false;
            }
            if (building.IsBurning())
            {
                //Log.Warning("strike! on fire");
                return false;
            }

            if (comp_mindControl.IsWorkDone)
            {
               //Log.Warning("strike! job done");
                return false;
            }

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            //return new Job(JobDefOf.OperateDeepDrill, t, 1500, true);
            //JobDriver_OperateShieldBench whatever = null;
            //            ThingDef.Named("JobDriver_LTF_MindMine").;
            //whatever = JobDef("JobDriver_LTF_MindMine");
            //whatever.job.def
            //return new Job(JobDef.Named("JobDriver_LTF_MindMine"), t, 1500, true);
            return new Job(JobDefToUse, t, 1500, true);
        }
    }
}
