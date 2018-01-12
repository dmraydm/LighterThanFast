using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using RimWorld;

namespace NewHatcher
{
    public class JobDriver_LTF_MindMine : JobDriver
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
                Comp_LTF_MindControl comp_mindControl = null;
                comp_mindControl = job.targetA.Thing.TryGetComp<Comp_LTF_MindControl>();
                return !comp_mindControl.PowerAndTargetPawnInRadius();
            });
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil work = new Toil();
            work.tickAction = delegate
            {
                Pawn actor = work.actor;

                Building building = (Building)actor.CurJob.targetA.Thing;
                Comp_LTF_MindControl comp_mindControl = building.GetComp<Comp_LTF_MindControl>();

                comp_mindControl.RegisterDone(actor);
                comp_mindControl.ImTheMastermind(actor);

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