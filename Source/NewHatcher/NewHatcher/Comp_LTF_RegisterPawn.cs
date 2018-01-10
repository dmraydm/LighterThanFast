using RimWorld;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using UnityEngine;

using Verse;
using Verse.Sound;


using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace NewHatcher
{
    public class CompTargetEffect_PreRegisterPawn : CompTargetEffect
    {
        public override void DoEffectOn(Pawn user, Thing target)
        {
            Log.Warning(">>> DoEffectOn  <<<");
            //user though
            Pawn pawn = (Pawn)target;
            if (pawn.Dead)
            {
                return;
            }
            
            if (user.CurJob.targetA.Thing != null)
            {
                Thing bench = user.CurJob.targetA.Thing;
                Comp_LTF_RegisterPawn comp_RegisterPawn = bench.TryGetComp<Comp_LTF_RegisterPawn>();

                comp_RegisterPawn.setToRegisterTarget(pawn);
            }
            else
            {
                Log.Warning("Null worker bench");
            }

            //toRegistertarget = pawn;
        }
    }

    public class CompUseEffect_LTF_MindcontrolRegister : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
            Log.Warning(">>> DoEffect <<<");
            //base.DoEffect(usedBy);
            base.DoEffect(usedBy);
            SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera(usedBy.MapHeld);
            //usedBy.records.Increment(RecordDefOf.ArtifactsActivated);
        }
    }

    public class Comp_LTF_RegisterPawn : ThingComp
    {
        private CompPowerTrader powerComp;
        private CompMannable mannableComp;

        public Thing mindcontrolGun;
        private CompEquippable MindcontrolGunCompEq;
                 
        private bool mindcontrolEnabled = false;
		
        private float registerPawnProgress	= 0;
        private float workerEmpathyForTarget;
		
		private Pawn masterMind = null;
		private Pawn toRegisterTarget = null;
		private Pawn registeredTarget = null;

        private const float registerPawnWork = 99999f; // 120sec = 60 tick * 120 = 7200 tick

        private float benchRadius = 35.7f;

        

        protected int burstCooldownTicksLeft;
        protected int burstWarmupTicksLeft;

        protected StunHandler stunner;
        protected LocalTargetInfo currentTargetInt = LocalTargetInfo.Invalid;
        protected LocalTargetInfo forcedTarget = LocalTargetInfo.Invalid;
        private LocalTargetInfo lastAttackedTarget;
        private int lastAttackTargetTick;
        private const float SightRadiusTurret = 13.4f;



        public float ProgressToRegister
        {
            get
            {
                return registerPawnProgress / registerPawnWork;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            this.powerComp = this.parent.TryGetComp<CompPowerTrader>();
            this.mannableComp = this.parent.TryGetComp<CompMannable>();
            //this.equippableComp = this.parent.TryGetComp<CompEquippable>();

            //if ( (powerComp == null) || (mannableComp == null) || (equippableComp == null))
            if (powerComp == null) 
            {
                Log.Warning("power comp Null");
            }
            if (mannableComp == null)
            {
                Log.Warning("manable comp null");
            }
            //this.MakeGun();
            Log.Warning("PostSpawnSetup end");
        }

        public CompEquippable MindBenchCompEq
        {
            get
            {
                return this.mindcontrolGun.TryGetComp<CompEquippable>();
            }
        }
        /*
        public void DeSpawn()
        {
            base.DeSpawn();
            this.ResetCurrentTarget();
        }
        */

        public Verb AttackVerb
        {
            get
            {
                return this.MindBenchCompEq.verbTracker.PrimaryVerb;
            }
        }

        public void MakeGun()
        {
            this.mindcontrolGun = ThingMaker.MakeThing(this.parent.def.building.turretGunDef, null);
            this.UpdateGunVerbs();
            Log.Warning("MakeGun end");
        }

        private void UpdateGunVerbs()
        {
            List<Verb> allVerbs = this.mindcontrolGun.TryGetComp<CompEquippable>().AllVerbs;
            for (int i = 0; i < allVerbs.Count; i++)
            {
                Verb verb = allVerbs[i];
                verb.caster = this.parent;
                verb.castCompleteCallback = new Action(this.BurstComplete);
               Log.Warning(this.parent.Label + i + ":" + verb.ToString());
            }
            //Log.Warning("updateverbs end");
        }

        // public override LocalTargetInfo CurrentTarget
        public LocalTargetInfo CurrentTarget
        {
            get
            {
                return this.currentTargetInt;
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look<float>(ref this.registerPawnProgress, "LTF_registerPawnProgress", 0f, false);
        }

        private float CalculateEmpathy (Pawn masterMind)
        {
            float intelligenceValue = masterMind.GetStatValue(StatDefOf.ResearchSpeed, true);
            float rangedValue = masterMind.GetStatValue(StatDefOf.ShootingAccuracy, true);
            float craftValue = masterMind.GetStatValue(StatDef.Named("SmithingSpeed"), true);
//            float meleeValue = masterMind.GetStatValue(StatDefOf.MeleeDodgeChance, true);
            //float yeldValue = masterMind.GetStatValue(StatDefOf., true);

            float factor = (intelligenceValue + Rand.Range(rangedValue * .75f, rangedValue * 1.25f)) * craftValue;

            Log.Warning("Empathy:" +factor);

            return factor;
        }

        // Appel ? debug JobDriver_OperateDeepDrill.cs
        public void RegisterDone(Pawn masterMind)
        {
            //float statValue = layerSynthetizeer.GetStatValue(StatDefOf.MiningSpeed, true);

            workerEmpathyForTarget = CalculateEmpathy(masterMind);
            // registerPawnWork;

            this.registerPawnProgress += workerEmpathyForTarget;

            if (this.registerPawnProgress > registerPawnWork)
            {
                this.TryEnableMindcontrol();

                this.registerPawnProgress = 0f;
                this.workerEmpathyForTarget = 0f;
            }
        }

        public void ImTheMastermind(Pawn newmasterMind)
        {
            if (newmasterMind != null)
            {
                masterMind = newmasterMind;
            }
            else
            {
                Log.Warning("Mastermind : can register Null");
            }
            
        }
        /*
        Turret?
        public override void OrderAttack(LocalTargetInfo targ)
        */
        public void OrderAttack(LocalTargetInfo targ)
        {
            if (!targ.IsValid)
            {
                if (this.forcedTarget.IsValid)
                {
                    this.TargetReset();
                }
                return;
            }
        }

        public void setToRegisterTarget(Pawn newTarget)
        {
            if(newTarget != null)
            {
                toRegisterTarget = newTarget;
            }
            else
            {
                Log.Warning("Trying to register a null pawn");
            }
            
        }

        public bool isTargetSet()
        {
            return (toRegisterTarget != null);
        }

        public void TargetReset()
        {
            toRegisterTarget = null;
            Log.Warning("target reset to null");
            return;
        }

        private bool CanSetForcedTarget
        {
            get
            {
                return this.mannableComp != null && (parent.Faction == Faction.OfPlayer || this.MannedByColonist) && !this.MannedByNonColonist;
            }
        }

        private bool CanToggleHoldFire
        {
            get
            {
                return (parent.Faction == Faction.OfPlayer || this.MannedByColonist) && !this.MannedByNonColonist;
            }
        }

        private bool MannedByColonist
        {
            get
            {
                return this.mannableComp != null && this.mannableComp.ManningPawn != null && this.mannableComp.ManningPawn.Faction == Faction.OfPlayer;
            }
        }

        private bool MannedByNonColonist
        {
            get
            {
                return this.mannableComp != null && this.mannableComp.ManningPawn != null && this.mannableComp.ManningPawn.Faction != Faction.OfPlayer;
            }
        }

        private void TryEnableMindcontrol()
        {
            if (!this.PowerAndTargetPawnInRadius())
            {
                //Messages.Message("DeepDrillExhausted".Translate(), this.parent, MessageTypeDefOf.TaskCompletion);
                Messages.Message(this.parent.Label + " did not find a TargetPawn in radius or no powa", this.parent, MessageTypeDefOf.TaskCompletion);
            }
            else
            {
                if (TryFindNextTargetPawn(out Pawn targetPawn))
                {
					registeredTarget = targetPawn;
					toRegisterTarget = null;
                    mindcontrolEnabled = true;
                    return;
                }
                else
                {
                    Log.Error("shield bench tried to enable mindcontrol but couldn't.");
                }
            }
        }

        public bool PowerAndTargetPawnInRadius()
        {
            return ((this.powerComp == null) || (this.powerComp.PowerOn) || (TryFindNextTargetPawn(out Pawn targetPawn)));
            /*
            if !((this.powerComp == null) || (this.powerComp.PowerOn))
            {
                return false;
            }

            if (TryFindNextTargetPawn(out Pawn targetPawn))
            {
                target = targetPawn;
                return true;
            }
            return false;
            */
        }

        public bool TryFindNextTargetPawn(out Pawn targetPawn)
        {
            // bench pos & map
            if ((this.benchRadius <= 1) || (this.parent.Position == null) || (this.parent.Map == null))
            {
                Log.Warning("null if you ask me");
                targetPawn = null;
                return false;
            }

               List < Pawn > allPawnsSpawned = parent.Map.mapPawns.SpawnedPawnsInFaction( Faction.OfPlayer );
                //List < Pawn > allPawnsSpawned = this.parent.Map.mapPawns.
                int pawnNum = allPawnsSpawned.Count;

                if (pawnNum < 1)
                {
                    Log.Warning("no colonist lol");
                    targetPawn = null;
                    return false;
                }

                for (int pawnI = 0; pawnI < pawnNum; pawnI++)
                {
                    Pawn maybeTargetPawn = null;
                    maybeTargetPawn = allPawnsSpawned[pawnI];

                    float benchTargetPawnDistance = 999f;
                    if ( (maybeTargetPawn == null) || (maybeTargetPawn.Map == null) || (maybeTargetPawn.Position == null) )
                    {
                        continue;
                    }
                    else
                    {
                        benchTargetPawnDistance = maybeTargetPawn.Position.DistanceTo(this.parent.Position);
                        if (benchTargetPawnDistance > benchRadius)
                        {
                            continue;
                        }
                    }
                    targetPawn = maybeTargetPawn;
                    return true;
                }
        
            targetPawn = null;
            return false;
        }

        

        [DebuggerHidden]
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action
            {
                action = new Action(this.ShowReport),
                defaultLabel = "Mind logs",
                defaultDesc = "Show a report of actors",
                icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchReport", true)
            };

            /*
            if (this.CanSetForcedTarget)
            {
                yield return new Command_VerbTarget
                {
                    defaultLabel = "Pre register",
                    defaultDesc = "Record someone to mind map",
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack", true),
                    verb = this.MindcontrolGunCompEq.PrimaryVerb,
                    hotKey = KeyBindingDefOf.Misc4
                };
            }
            */

            if (toRegisterTarget != null)
            {
                yield return new Command_Action
                {
                    action = new Action(this.TargetReset),
                    defaultLabel = "Reset",
                    defaultDesc = "Reset the target",
                    icon = ContentFinder<Texture2D>.Get("UI/Buttons/Delete", true)
                };
            }
            

            if (mindcontrolEnabled)
            {
                yield return new Command_Action
                {
                    action = new Action(this.TargetReset),
                    defaultLabel = "Mind control",
                    defaultDesc = "Take control maybe",
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/MindControl", true)
                };
            }
        }
        /*
         public override GizmoResult GizmoOnGUI(Vector2 topLeft)
		{
			Rect overRect = new Rect(topLeft.x, topLeft.y, this.Width, 75f);
			Find.WindowStack.ImmediateWindow(1523289473, overRect, WindowLayer.GameUI, delegate
			{
				Rect rect = overRect.AtZero().ContractedBy(6f);
				Rect rect2 = rect;
				rect2.height = overRect.height / 2f;
				Text.Font = GameFont.Tiny;
				Widgets.Label(rect2, "FuelLevelGizmoLabel".Translate());
				Rect rect3 = rect;
				rect3.yMin = overRect.height / 2f;
				float fillPercent = this.refuelable.Fuel / this.refuelable.Props.fuelCapacity;
				Widgets.FillableBar(rect3, fillPercent, Gizmo_RefuelableFuelStatus.FullBarTex, Gizmo_RefuelableFuelStatus.EmptyBarTex, false);
				if (this.refuelable.Props.targetFuelLevelConfigurable)
				{
					float num = this.refuelable.TargetFuelLevel / this.refuelable.Props.fuelCapacity;
					float x = rect3.x + num * rect3.width - (float)Gizmo_RefuelableFuelStatus.TargetLevelArrow.width * 0.5f / 2f;
					float y = rect3.y - (float)Gizmo_RefuelableFuelStatus.TargetLevelArrow.height * 0.5f;
					GUI.DrawTexture(new Rect(x, y, (float)Gizmo_RefuelableFuelStatus.TargetLevelArrow.width * 0.5f, (float)Gizmo_RefuelableFuelStatus.TargetLevelArrow.height * 0.5f), Gizmo_RefuelableFuelStatus.TargetLevelArrow);
				}
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(rect3, this.refuelable.Fuel.ToString("F0") + " / " + this.refuelable.Props.fuelCapacity.ToString("F0"));
				Text.Anchor = TextAnchor.UpperLeft;
			}, true, false, 1f);
			return new GizmoResult(GizmoState.Clear);
            }
         */
        public void ShowReport()
        {
            StringBuilder stringBuilder = new StringBuilder();

            // Mastermind + empathy
            stringBuilder.AppendLine("Master mind :");
            stringBuilder.AppendLine("-------------");
            if (masterMind != null)
            {
                stringBuilder.AppendLine(masterMind.NameStringShort);
                stringBuilder.AppendLine("Empathy for target : " + workerEmpathyForTarget);
            }
            else
                stringBuilder.AppendLine("Null");
            stringBuilder.AppendLine();

            // Target + work left
            stringBuilder.AppendLine("Target :\t");
            stringBuilder.AppendLine("--------");
            if (toRegisterTarget != null)
            {
                stringBuilder.AppendLine(toRegisterTarget.NameStringShort);
                stringBuilder.AppendLine("Progress : " + this.ProgressToRegister.ToStringPercent("F0"));
            }
            else
                stringBuilder.AppendLine("Null");
            stringBuilder.AppendLine();

            // Registered
            stringBuilder.AppendLine("Mindable :");
            stringBuilder.AppendLine("-------------------");
            if (registeredTarget != null)
                stringBuilder.AppendLine(registeredTarget.NameStringShort);
            else
                stringBuilder.AppendLine("Null");

            Dialog_MessageBox window = new Dialog_MessageBox(stringBuilder.ToString(), null, null, null, null, null, false);
            Find.WindowStack.Add(window);
        }

        protected void BurstComplete()
        {
            if (this.parent.def.building.turretBurstCooldownTime >= 0f)
            {
                this.burstCooldownTicksLeft = this.parent.def.building.turretBurstCooldownTime.SecondsToTicks();
            }
            else
            {
                this.burstCooldownTicksLeft = this.MindcontrolGunCompEq.PrimaryVerb.verbProps.defaultCooldownTime.SecondsToTicks();
            }
        }

        public override string CompInspectStringExtra()
        {
            if (this.powerComp == null || !this.powerComp.PowerOn)
            {
                return null;
            }

            float SLP = workerEmpathyForTarget*60; // empathy per sec
            float percRegisterPerSec = SLP * 100 / registerPawnWork;

            //string roundedLayers = string.Empty;

            if (TryFindNextTargetPawn(out Pawn targetPawn))
            {
                return string.Concat(new string[]
                {
                    "Target : ",
                    targetPawn.LabelShort,
                    ".",
                    targetPawn.def.label,
                    "\n",
                    "Progress : ",
                    this.ProgressToRegister.ToStringPercent("F0"),
                    " (",
                    percRegisterPerSec.ToString("0.00") + " %perSec)"
                });
            }
            return "No targetPawn within " + benchRadius + " tiles";
            
        }
    }
}