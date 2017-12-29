using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using RimWorld;

namespace NewHatcher
{
    public class JobDriver_OperateShieldBench : JobDriver
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
                //Comp_LTF_SynthetizeShieldLayer comp_FillShieldLayer = this.$this.job.targetA.Thing.TryGetComp<Comp_LTF_SynthetizeShieldLayer>();
                Comp_LTF_SynthetizeShieldLayer comp_FillShieldLayer = null;
                comp_FillShieldLayer = job.targetA.Thing.TryGetComp<Comp_LTF_SynthetizeShieldLayer>();
                return !comp_FillShieldLayer.PowerAndWearerInRadius();
            });
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil work = new Toil();
            work.tickAction = delegate
            {
                Pawn actor = work.actor;
                Building building = (Building)actor.CurJob.targetA.Thing;
                Comp_LTF_SynthetizeShieldLayer comp = building.GetComp<Comp_LTF_SynthetizeShieldLayer>();
                comp.SynthetizeLayerDone(actor);

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