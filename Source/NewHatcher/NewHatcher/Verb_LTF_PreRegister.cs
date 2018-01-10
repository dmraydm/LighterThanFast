using Verse;
using RimWorld;

namespace NewHatcher
{
	public class Verb_LTF_PreRegister : Verb_LaunchProjectile
    {
		protected override int ShotsPerBurst
		{
			get
			{
				return this.verbProps.burstShotCount;
			}
		}

		public override void WarmupComplete()
		{
            Log.Warning("Verb WarmupComplete");
            base.WarmupComplete();
			if (base.CasterIsPawn && base.CasterPawn.skills != null)
			{
                Log.Warning("WarmupComplete");
                /*
				float xp = 6f;
				Pawn pawn = this.currentTarget.Thing as Pawn;
				if (pawn != null && pawn.HostileTo(this.caster) && !pawn.Downed)
				{
					xp = 240f;
				}
				base.CasterPawn.skills.Learn(SkillDefOf.Shooting, xp, false);
                */
			}
		}

		protected override bool TryCastShot()
		{
            Log.Warning("Verb TryCastShot");
            bool flag = true;
            /*
             * base.TryCastShot();
			if (flag && base.CasterIsPawn)
			{
				base.CasterPawn.records.Increment(RecordDefOf.ShotsFired);
			}
            */
			return flag;
		}
	}
}