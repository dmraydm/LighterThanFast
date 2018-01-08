using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using RimWorld;

namespace NewHatcher
{
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
            work.defaultCompleteMode = ToilCompleteMode.Never;
            work.WithEffect(EffecterDefOf.Drill, TargetIndex.A);

            work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            yield return work;
        }
    }

    public class JobDriver_LTF_PreRegister : JobDriver
    {
        public override bool TryMakePreToilReservations()
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            Log.Warning("JobDriver_LTF_PreRegister");
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
                registerPawnComp.ImTheMastermind(actor);

  //              CompUsable compUsable = building.GetComp<CompUsable>();
//                CompUsable compeffect = building.GetComp<Comp_UseEffect>();

                //registerPawnComp.setToRegisterTarget(thisjob.GetTarget(TargetIndex.A).Thing);

            };
            work.defaultCompleteMode = ToilCompleteMode.Instant;
            work.WithEffect(EffecterDefOf.Drill, TargetIndex.A);

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