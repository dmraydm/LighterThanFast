using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using RimWorld;

namespace NewHatcher
{

    public class JobDriver_LTF_PreRegister : JobDriver
    {
        public override bool TryMakePreToilReservations()
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
 
            
            /*
            string tA = string.Empty; string tB = string.Empty; string tC = string.Empty;

            if (job.targetA.Thing != null) tA = job.targetA.Thing.Label;
            Log.Warning("tA:"+tA);
            if (job.targetB.Thing != null) tB = job.targetB.Thing.Label;
            Log.Warning("tB:" + tB);
            if (job.targetC.Thing != null) tC = job.targetC.Thing.Label;
            Log.Warning("tC:" + tC);
            */

            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            this.FailOn(delegate
            {
                Comp_LTF_RegisterPawn comp_registerPawn = null;
                comp_registerPawn = job.targetA.Thing.TryGetComp<Comp_LTF_RegisterPawn>();
                return !comp_registerPawn.PowerAndTargetPawnInRadius();
            });
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            Toil work = new Toil();
            
            work.tickAction = delegate
            {
                Log.Warning("JobDriver_LTF_PreRegister in");
                Pawn actor = work.actor;
                Building building = (Building)actor.CurJob.targetA.Thing;
                Comp_LTF_RegisterPawn registerPawnComp = building.GetComp<Comp_LTF_RegisterPawn>();
                CompTargetable benchTargetableComp = building.GetComp<CompTargetable>();

                benchTargetableComp.DoEffect(actor);
                Log.Warning("Effect Done" );

                registerPawnComp.ImTheMastermind(actor);
                

  //              CompUsable compUsable = building.GetComp<CompUsable>();
//                CompUsable compeffect = building.GetComp<Comp_UseEffect>();

                //registerPawnComp.setToRegisterTarget(thisjob.GetTarget(TargetIndex.A).Thing);

            };
            work.defaultCompleteMode = ToilCompleteMode.Never;
            work.WithEffect(EffecterDefOf.Research, TargetIndex.A);

            Log.Warning("JobDriver_LTF_PreRegister out");

            work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            yield return work;
        }
    }


    public class JobDriver_LTF_ManMindcontrolBench : JobDriver
    {
        public override bool TryMakePreToilReservations()
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            Log.Warning("JobDriver_LTF_ManMindcontrolBench");
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            this.FailOn(delegate
            {
                Comp_LTF_RegisterPawn comp_registerPawn = null;
                comp_registerPawn = job.targetA.Thing.TryGetComp<Comp_LTF_RegisterPawn>();
                return !comp_registerPawn.PowerAndTargetPawnInRadius();
            });
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            Toil work = new Toil();
            work.tickAction = delegate
            {
                Pawn actor = work.actor;

                Building building = (Building)actor.CurJob.targetA.Thing;
                Comp_LTF_RegisterPawn registerPawnComp = building.GetComp<Comp_LTF_RegisterPawn>();

                registerPawnComp.RegisterDone(actor);
                registerPawnComp.ImTheMastermind(actor);

                actor.skills.Learn(SkillDefOf.Mining, 0.00001f, false);
                actor.skills.Learn(SkillDefOf.Shooting, 0.00001f, false);
                actor.skills.Learn(SkillDefOf.Intellectual, 0.00001f, false);
            };
            //ToilCompleteMode.PatherArrival
            work.defaultCompleteMode = ToilCompleteMode.Never;
            //work.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            work.WithEffect(EffecterDefOf.Research, TargetIndex.A);

            work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            yield return work;
        }
    }


    public class JobDriver_LTF_RegisterPawn : JobDriver
    {
        public override bool TryMakePreToilReservations()
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            this.FailOn(delegate
            {
                //Comp_LTF_RegisterPawn comp_registerPawn = this.$this.job.targetA.Thing.TryGetComp<Comp_LTF_RegisterPawn>();
                Comp_LTF_RegisterPawn comp_registerPawn = null;
                comp_registerPawn = job.targetA.Thing.TryGetComp<Comp_LTF_RegisterPawn>();
                return !comp_registerPawn.PowerAndTargetPawnInRadius();
            });
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil work = new Toil();
            work.tickAction = delegate
            {
                Pawn actor = work.actor;

                Building building = (Building)actor.CurJob.targetA.Thing;
                Comp_LTF_RegisterPawn registerPawnComp = building.GetComp<Comp_LTF_RegisterPawn>();

                registerPawnComp.RegisterDone(actor);
                registerPawnComp.ImTheMastermind(actor);

                actor.skills.Learn(SkillDefOf.Mining, 0.00001f, false);
                actor.skills.Learn(SkillDefOf.Shooting, 0.00001f, false);
                actor.skills.Learn(SkillDefOf.Intellectual, 0.00001f, false);
            };
            work.defaultCompleteMode = ToilCompleteMode.Never;
            work.WithEffect(EffecterDefOf.Drill, TargetIndex.A);

            work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            yield return work;
        }
    }
}