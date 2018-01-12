using RimWorld;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using UnityEngine;

using Verse;
using Verse.Sound;

namespace NewHatcher
{
    // target effect ; register pawn in benchComp
    public class CompTargetEffect_LTF_MindcontrolRegister : CompTargetEffect
    {
        public override void DoEffectOn(Pawn user, Thing target)
        {
            Log.Warning(">>> DoEffectOn  <<<");

            Pawn mindable = (Pawn)target;
            if (mindable.Dead)
            {
                return;
            }
            
            if (user.CurJob.targetA.Thing != null)
            {
                Thing bench = user.CurJob.targetA.Thing;
                Comp_LTF_MindControl comp_mindControl = bench.TryGetComp<Comp_LTF_MindControl>();

                comp_mindControl.setTarget(mindable);
                comp_mindControl.ImTheMastermind(user);
                comp_mindControl.InitWork();
            }
            else
            {
                Log.Warning("Null worker bench");
            }

            //mindTarget = pawn;
        }
    }
    // masterMind effect
    public class CompUseEffect_LTF_MindcontrolRegister : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
            //Log.Warning(">>> DoEffect <<<");
            //base.DoEffect(usedBy);
            base.DoEffect(usedBy);
            SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera(usedBy.MapHeld);
            //usedBy.records.Increment(RecordDefOf.ArtifactsActivated);
        }
    }

    // Main
    public class Comp_LTF_MindControl : ThingComp
    {
        private CompPowerTrader powerComp;
        private float benchRadius = 35.7f;

        private Pawn masterMind = null;
		private Pawn mindTarget = null;

        private float mindMineProgress = 0;
        private float workerEmpathyForTarget;
        private float mindMineWork = 3600f; // 120sec = 60 tick * 120 = 7200 tick

        private bool mindcontrolEnabled = false;

        // progress in work
        public float ProgressToRegister
        {
            get
            {
                return mindMineProgress / mindMineWork;
            }
        }

        // get power comp
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            this.powerComp = this.parent.TryGetComp<CompPowerTrader>();

            if (powerComp == null) 
            {
                Log.Warning("power comp Null");
            }

            Log.Warning("PostSpawnSetup end");
        }

        /*
        public void DeSpawn()
        {
            base.DeSpawn();
            this.ResetCurrentTarget();
        }
        */

        
        public override void PostExposeData()
        {
            Scribe_Values.Look<float>(ref this.mindMineProgress, "LTF_mindMineProgress", 0f, false);
            Scribe_Values.Look<Pawn>(ref this.masterMind, "LTF_masterMind");
            Scribe_Values.Look<Pawn>(ref this.mindTarget, "LTF_mindTarget");
            Scribe_Values.Look<bool>(ref this.mindcontrolEnabled, "LTF_mindEnabled", false, false);
            
            
        }

        private float CalculateEmpathy ()
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

            Log.Warning("RegisterDone " + mindMineProgress + "/" + mindMineWork);
            workerEmpathyForTarget = CalculateEmpathy();
            

            this.mindMineProgress += workerEmpathyForTarget;

            if (this.mindMineProgress > mindMineWork)
            {
                mindcontrolEnabled = true;

                this.mindMineProgress = 0f;
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


        public void setTarget(Pawn newTarget)
        {
            if((newTarget != null) && (!newTarget.Dead) && (newTarget.Map != null) )
            {
                mindTarget = newTarget;
            }
            else
            {
                Log.Warning("Trying to register a null pawn");
            }
            
        }

        public void InitWork()
        {
            String bla = string.Empty;
            bla += //"p"+masterMind.GetStatValue(StatDefOf.DiplomacyPower) + ' ' +
            " glf: "+masterMind.GetStatValue(StatDefOf.GlobalLearningFactor) + ' ' +
           " ps: "+ masterMind.GetStatValue(StatDefOf.PsychicSensitivity) + ' ' +
            " SI: "+masterMind.GetStatValue(StatDefOf.SocialImpact) + ' ' +
            " RS: "+masterMind.GetStatValue(StatDefOf.ResearchSpeed);

            Log.Warning( bla );
        }

        public bool isTargetSet()
        {
            return (mindTarget != null);
        }
        public bool isMasterMindSet()
        {
            return (masterMind != null);
        }
        public bool areActorsSet()
        {
            return (isTargetSet() && isMasterMindSet());
        }

        public void TargetReset()
        {
            mindcontrolEnabled = false;
            mindTarget = null;
            masterMind = null;
            mindMineProgress = 0;
            Log.Warning("target reset to null");
            return;
        }

        private void TryMindcontrol()
        {
            if (!this.PowerAndTargetPawnInRadius())
            {
                //Messages.Message("DeepDrillExhausted".Translate(), this.parent, MessageTypeDefOf.TaskCompletion);
                Messages.Message(this.parent.Label + " did not find a TargetPawn in radius or no powa", this.parent, MessageTypeDefOf.TaskCompletion);
            }
            else
            {
                if (TryMindReach(out Pawn targetPawn))
                {

                    //mindcontrolEnabled = true;
                    mindTarget = targetPawn;
                    mindTarget.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null, true, false, null);
                    TargetReset();
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
            return ( ((this.powerComp == null) || (this.powerComp.PowerOn)) && areActorsSet() && TryMindReach(out Pawn targetPawn) );
        }

        public bool TryMindReach(out Pawn targetPawn)
        {
            // bench pos & map
            if ((this.benchRadius <= 1) || (this.parent.Position == null) || (this.parent.Map == null))
            {
                Log.Warning("null bench");
                targetPawn = null;
                return false;
            }
            if ((mindTarget == null) || (mindTarget.Map == null) || (mindTarget.Position == null))
            {
                Log.Warning("null mindTarget");
                targetPawn = null;
                return false;
            }

            if ((masterMind == null) || (masterMind.Map == null) || (masterMind.Position == null))
            {
                Log.Warning("null masterMind");
                targetPawn = null;
                return false;
            }

            float benchTargetPawnDistance = 999f;
            benchTargetPawnDistance = mindTarget.Position.DistanceTo(this.parent.Position);

            if (benchTargetPawnDistance > benchRadius)
            {
                Log.Warning("target too far");
                targetPawn = null;
                return false;
            }
            else
            {
                targetPawn = mindTarget;
                return true;
            }

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

            if (isTargetSet())
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
                    action = new Action(this.TryMindcontrol),
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
            if (mindTarget != null)
            {
                stringBuilder.AppendLine(mindTarget.NameStringShort);
                stringBuilder.AppendLine("Progress : " + this.ProgressToRegister.ToStringPercent("F0"));
            }
            else
                stringBuilder.AppendLine("Null");
            stringBuilder.AppendLine();

            // Registered
            stringBuilder.AppendLine("Mindable :");
            stringBuilder.AppendLine("-------------------");
            if (mindTarget != null)
                stringBuilder.AppendLine(mindTarget.NameStringShort);
            else
                stringBuilder.AppendLine("Null");

            Dialog_MessageBox window = new Dialog_MessageBox(stringBuilder.ToString(), null, null, null, null, null, false);
            Find.WindowStack.Add(window);
        }

        public override string CompInspectStringExtra()
        {
            if (this.powerComp == null || !this.powerComp.PowerOn)
            {
                return null;
            }

            float SLP = workerEmpathyForTarget*60; // empathy per sec
            float percRegisterPerSec = SLP * 100 / mindMineWork;

            //string roundedLayers = string.Empty;

            if (TryMindReach(out Pawn targetPawn))
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