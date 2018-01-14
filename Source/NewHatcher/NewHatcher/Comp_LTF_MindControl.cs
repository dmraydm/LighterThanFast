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
            //Log.Warning(">>> DoEffectOn  <<<");

            Pawn mindable = (Pawn)target;
            if (mindable.Dead)
            {
                return;
            }
            
            if (user.CurJob.targetA.Thing != null)
            {
                Thing bench = user.CurJob.targetA.Thing;
                Comp_LTF_MindControl comp_mindControl = bench.TryGetComp<Comp_LTF_MindControl>();

                comp_mindControl.SetTarget(mindable);
                comp_mindControl.ImTheMastermind(user);
                comp_mindControl.InitActorsValues();

                if (mindable.RaceProps.Animal)
                {
                    comp_mindControl.InitAnimalWork();
                }
                else
                {
                    comp_mindControl.InitWork();
                }
                comp_mindControl.ResetProgress();
            }
            else
            {
                Log.Warning("Null worker bench");
            }
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
        string tName = string.Empty;
        string tRace = string.Empty;
        string mName = string.Empty;
        string mRace = string.Empty;

        private int masterMindN = 0;

        private const float NaValue = -9999f;

        private float empathy = NaValue;
        private float manipulation = NaValue;
        private float ascendancy = NaValue;

        private float aleas = NaValue;

        private enum MindVector { Ascen, Manip, Empat, Na };
        string[] vectorName = { "Ascendancy", "Manipulation", "Empathy", "Impossibru" };

        private int bestVector = (int)MindVector.Na;
        private float bestVectorValue = 0f;
        private int worstVector = (int)MindVector.Na;
        private float worstVectorValue = 0f;

        private float animalVector = 0f;

        private const float defaultWorkAmount = 3600f; // 120sec = 60  * 120 = 7200 
        private float workGoal = defaultWorkAmount;
        private float workProgress = 0;

        private bool mindcontrolEnabled = false;

        // progress in work
        public float ProgressToRegister
        {
            get
            {
                return workProgress / workGoal;
            }
        }

        public void SetBestVector()
        {
            int max = (int)MindVector.Na;
            float bestValue = -999f;

            if (empathy > bestValue) {
                max = (int)MindVector.Empat;
                bestValue = empathy;
            }
            if (manipulation > bestValue)
            {
                max = (int)MindVector.Manip;
                bestValue = manipulation;
            }
            if (ascendancy > bestValue) {
                max = (int)MindVector.Ascen;
                bestValue = ascendancy;
            }

            bestVector = max;
            bestVectorValue = bestValue;

            if (bestVectorValue < 0f) {
                Messages.Message(mName + " has no leverage on " + tName + "; reset();", this.parent, MessageTypeDefOf.TaskCompletion);
                TargetReset();
            };

            //Log.Warning(" Best >> emp:" + empathy + "man:" + manipulation + "asc:" + ascendancy + ";max:" + vectorName[bestVector]);
        }

        public void SetWorstVector()
        {
            int min = (int)MindVector.Na;
            float worstValue = 999f;

            if (empathy < worstValue)
            {
                min = (int)MindVector.Empat;
                worstValue = empathy;
            }
            if (manipulation < worstValue)
            {
                min = (int)MindVector.Manip;
                worstValue = manipulation;
            }
            if (ascendancy < worstValue)
            {
                min = (int)MindVector.Ascen;
                worstValue = ascendancy;
            }

            worstVector = min;
            worstVectorValue = worstValue;
            //Log.Warning(" Worst >> emp:"+empathy + "man:"+manipulation + "asc:"+ascendancy+";min:"+ vectorName[worstVector]);
        }
        // get power comp
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            this.powerComp = this.parent.TryGetComp<CompPowerTrader>();

            if (powerComp == null)
            {
                Log.Warning("power comp Null");
            }

            //Log.Warning("PostSpawnSetup end");
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
            //Scribe_Deep.Look<Pawn>(ref this.masterMind, "LTF_masterMind");
            //Scribe_Deep.Look<Pawn>(ref this.mindTarget, "LTF_mindTarget");
            //Scribe_References.Look<Pawn>(ref this.mindTarget, "LTF_mindTarget");
            //Scribe_References.Look<Pawn>(ref this.mindTarget, "LTF_mindTarget");

            Scribe_Values.Look(ref workProgress, "LTF_workProgress");
            Scribe_Values.Look(ref bestVector, "LTF_bestVector");
            Scribe_Values.Look(ref bestVectorValue, "LTF_bestVectorValue");
            Scribe_Values.Look(ref worstVector, "LTF_worstVector");
            Scribe_Values.Look(ref worstVectorValue, "LTF_worstVectorValue");

            Scribe_Values.Look(ref this.mindcontrolEnabled, "LTF_mindEnabled");
        }

        private float SetVector(int vectorId)
        {
            SkillDef firstSkill = null;
            SkillDef secondSkill = null;
            float result = 0f;

            switch (vectorId)
            {
                case (int)MindVector.Empat:
                    firstSkill = SkillDefOf.Social;
                    secondSkill = SkillDefOf.Artistic;
                    break;
                case (int)MindVector.Manip:
                    firstSkill = SkillDefOf.Social;
                    secondSkill = SkillDefOf.Intellectual;
                    break;
                case (int)MindVector.Ascen:
                    firstSkill = SkillDefOf.Intellectual;
                    secondSkill = SkillDefOf.Medicine;
                    break;
                default:
                    Log.Warning("Wtf vector");
                    break;
            }

            // 0-20
            float mS1 = masterMind.skills.GetSkill(firstSkill).Level;
            int mS1P = (int)masterMind.skills.GetSkill(firstSkill).passion;
            float tS1 = mindTarget.skills.GetSkill(firstSkill).Level;
            int tS1P = (int)mindTarget.skills.GetSkill(firstSkill).passion;

            // si n�gatif, mastermind moins skilled que target
            float deltaS1 = mS1 - tS1;
            // si n�gatif, mastermind moins passionn�
            float deltaS1P = mS1P - tS1P;

            float S1Fascination = 1f;

            // masterMind a moins de skill mais plus de passion
            // mindTarget a moins de skill mais plus de passion
            if (((deltaS1 < 0) && (deltaS1P > 0)) || ((deltaS1 > 0) && (deltaS1P < 0)))
            {
                S1Fascination += .125f * deltaS1P;
            }

            // 0-20
            float mS2 = masterMind.skills.GetSkill(secondSkill).Level;
            int mS2P = (int)masterMind.skills.GetSkill(secondSkill).passion;
            float tS2 = mindTarget.skills.GetSkill(secondSkill).Level;
            int tS2P = (int)mindTarget.skills.GetSkill(secondSkill).passion;

            // 0-20
            // si n�gatif, mastermind moins skilled que target
            float deltaS2 = mS2 - tS2;
            // si n�gatif, mastermind moins passionn�
            float deltaS2P = mS2P - tS2P;

            float S2Fascination = 1f;

            // 1 - 1.5
            if (((deltaS2 < 0) && (deltaS2P > 0)) || ((deltaS2 > 0) && (deltaS2P < 0)))
            {
                S2Fascination += .125f * deltaS2P;
            }

            const float scaling = 1 / 40f;
            float s1Ratio = Math.Abs(mS1 / ((tS1 > 0) ? (tS1) : (.1f)) / 10) * ((deltaS1 > 0) ? (1) : (-1));
            float s2Ratio = Math.Abs(mS2 / ((tS2 > 0) ? (tS2) : (.1f)) / 10) * ((deltaS2 > 0) ? (1) : (-1));

            result = ((mS1 - tS1 + s1Ratio) * S1Fascination
                        + (mS2 - tS2 + s2Ratio) * S2Fascination
                     ) * scaling;

            //Log.Warning(vectorName[vectorId] + ": (" + mS1 + "-" + tS1 + "+" + s1Ratio + ") * " + S1Fascination + " + (" + mS2 + " - " + tS2 + "+" + s2Ratio + ") * " + S2Fascination + ") * " + scaling + "=" + result);
            //Log.Warning("result:" + result);

            // output +- 0-1 (1.5)
            return result;
        }

        private void SetAleas()
        {
            aleas = Rand.Range(worstVectorValue * -.5f, worstVectorValue * 1.5f);
        }

        //vector can be empathy/ascendency
        private bool IsVectorSet(float vector)
        {
            return (vector != NaValue);
        }

        // Appel ? debug JobDriver_OperateDeepDrill.cs
        public void MindMineTick(Pawn masterMind)
        {
            if (animalVictim())
            {
                workProgress += animalVector;
            }
            else
            {
                SetAleas();
                //Log.Warning("Tick " + workProgress + "/" + workGoal + ";" + workProgress + " += " + bestVectorValue + " * 3 + " + aleas + "jobDone : " + IsWorkDone());

                workProgress += bestVectorValue * 3 + aleas;
            }

            if (this.workProgress > workGoal)
            {
                mindcontrolEnabled = true;
                //workProgress = 0f;
            }
        }

        public void ImTheMastermind(Pawn newmasterMind)
        {
            if (newmasterMind != null)
            {
                masterMind = newmasterMind;
                mName = masterMind.NameStringShort;
                mRace = masterMind.def.label;

                masterMindN++;
            }
            else
            {
                Log.Warning("cant Mastermind Null");
            }
        }

        public Pawn WhoisMastermind()
        {
            return masterMind;
        }


        public void SetTarget(Pawn newTarget)
        {
            if ((newTarget != null) && (!newTarget.Dead) && (newTarget.Map != null))
            {
                mindTarget = newTarget;
                tName = mindTarget.NameStringShort;
                tRace = mindTarget.def.label;
            }
            else
            {
                Log.Warning("Trying to register a null pawn");
            }

        }
        //TODO
        // psychic delta

        public void InitActorsValues()
        {
            //Log.Warning("InitActorsValues");
            if (animalVictim())
            {
                InitAnimalVector();
            }
            else
            {
                InitLeverageVector();

                SetBestVector();
                SetWorstVector();
            }
        }

        public void InitLeverageVector()
        {
            empathy = SetVector((int)MindVector.Empat);
            manipulation = SetVector((int)MindVector.Manip);
            ascendancy = SetVector((int)MindVector.Ascen);

        }

        public void InitAnimalVector()
        {
            animalVector = 0f;
            float brainGatherFactor = 0f;


            if (mindTarget.RaceProps.TrainableIntelligence == TrainableIntelligenceDefOf.None)
            {
                if (masterMind.skills.GetSkill(SkillDefOf.Intellectual).Level <= 2)
                    brainGatherFactor = 1.5f;
                else if (masterMind.skills.GetSkill(SkillDefOf.Intellectual).Level <= 4)
                    brainGatherFactor = 1.4f;
            }

            if (mindTarget.RaceProps.TrainableIntelligence == TrainableIntelligenceDefOf.Simple)
            {
                if ((masterMind.skills.GetSkill(SkillDefOf.Intellectual).Level > 4) &&
                (masterMind.skills.GetSkill(SkillDefOf.Intellectual).Level <= 8))
                    brainGatherFactor = 1.35f;
            }

            if (mindTarget.RaceProps.TrainableIntelligence == TrainableIntelligenceDefOf.Intermediate)
            {
                if ((masterMind.skills.GetSkill(SkillDefOf.Intellectual).Level > 8) &&
                (masterMind.skills.GetSkill(SkillDefOf.Intellectual).Level <= 11))
                    brainGatherFactor = 1.3f;
            }

            if (mindTarget.RaceProps.TrainableIntelligence == TrainableIntelligenceDefOf.Advanced)
            {
                if ((masterMind.skills.GetSkill(SkillDefOf.Intellectual).Level > 11) &&
                (masterMind.skills.GetSkill(SkillDefOf.Intellectual).Level <= 17))
                    brainGatherFactor = 1.2f;
            }

            const float artificialBoost = 3f; //5 = scale
            float ratio = .5f;
            //if (masterMind.skills.GetSkill(SkillDefOf.Animals).Level * artificialBoost > (mindTarget.RaceProps.wildness / 5))
            if (masterMind.skills.GetSkill(SkillDefOf.Animals).Level * artificialBoost > (mindTarget.RaceProps.wildness / 5))
            {
                ratio = masterMind.skills.GetSkill(SkillDefOf.Animals).Level / (mindTarget.RaceProps.wildness );
            }

            animalVector = brainGatherFactor + masterMind.skills.GetSkill(SkillDefOf.Animals).Level / 3 + ratio;// * artificialBoost;
        }

        public void InitAnimalWork()
        {
            workGoal = defaultWorkAmount * mindTarget.RaceProps.baseBodySize;
        }

        public void InitWork()
        {
            float mBonus = 0;
            float tBonus = 0;

            float mVariousness = (defaultWorkAmount / 3);
            float tVariousness = (defaultWorkAmount / 5);
            float rVariousness = (defaultWorkAmount / 8);


            //if ()
            if (bestVectorValue > 1f) mBonus = 1.2f;
            else if (bestVectorValue > .5f) mBonus = 1;
            else if (bestVectorValue > .25f) mBonus = .7f;
            else mBonus = .3f;

            if (worstVectorValue > 1f) tBonus = 1.2f;
            else if (worstVectorValue > .5f) tBonus = .7f;
            else if (worstVectorValue > -.5f) tBonus = .7f;
            else tBonus = -1.2f;

            float randWorkAmount = 1f;

            if (Rand.Bool)
            {
                randWorkAmount *= -1;
            }
            randWorkAmount *= Rand.Range(worstVectorValue, bestVectorValue);

            //String bla = string.Empty; Log.Warning( bla );
            workGoal = defaultWorkAmount - mBonus * mVariousness + tBonus * tVariousness + rVariousness * randWorkAmount;
            //Log.Warning("Work  : " + workProgress + "/" + workGoal + "(" + mBonus + ";" + tBonus + ";" + randWorkAmount + ")");

        }

        public void ResetProgress()
        {
            workProgress = 0;
        }

        public bool animalVictim(){
            return mindTarget.RaceProps.Animal;
        }

        public bool IsWorkDone()
        {
            return (mindcontrolEnabled);
        }

        public bool IsTargetSet()
        {
            return (mindTarget != null);
        }
        public bool IsMasterMindSet()
        {
            return (masterMind != null);
        }
        public bool AreActorsSet()
        {
            return (IsTargetSet() && IsMasterMindSet());
        }

        public void TargetReset()
        {
            mindcontrolEnabled = false;
            mindTarget = null;
            masterMind = null;
            workProgress = 0;
            //Log.Warning("target reset to null");
            return;
        }

        private void TryMindcontrol()
        {
            if ( (!GotThePower()) )
            {
                Messages.Message(parent.Label + " requires more power.", this.parent, MessageTypeDefOf.TaskCompletion);
            } else if (!ActorInRadius()) {
                    Messages.Message(mName + " did not find " + tName + " in radius.", this.parent, MessageTypeDefOf.TaskCompletion);
            }
             else if ( !IsWorkDone())
            {
                Messages.Message( "Work is not done.", this.parent, MessageTypeDefOf.TaskCompletion);
            }
            else
            {
                if (TryMindReach())
                {
                    MentalStateDef mentalStateEffect = null;
                    //MentalBreakDef mentalBreakEffect = null;
                    /*
                    IEnumerable<MentalBreakDef> theOnlyAllowedMentalBreaks = pawn.story.traits.TheOnlyAllowedMentalBreaks;
                    if (!theOnlyAllowedMentalBreaks.Contains(this.def) && theOnlyAllowedMentalBreaks.Any((MentalBreakDef x) => x.intensity == this.def.intensity && x.Worker.BreakCanOccur(pawn)))
                        mindTarget.MentalState.pawn.MentalState.
                            TryStartMentalState(mentalState, reason2, false, causedByMood, null);

                    TryStart
                    */
                    if (animalVictim())
                    {
                        mentalStateEffect = MentalStateDefOf.Manhunter;
                    }
                    else
                    {
                        switch (bestVector)
                        {
                            case (int)MindVector.Empat:
                                mentalStateEffect = MentalStateDefOf.WanderSad;
                                //...mindTarget.mindState.mentalStateHandler.TryStartMentalState()
                            break;
                            case (int)MindVector.Manip:
                                mentalStateEffect = MentalStateDefOf.PanicFlee;
                                break;
                            case (int)MindVector.Ascen:
                                mentalStateEffect = MentalStateDefOf.Berserk;
                                
                                break;
                            default:
                                Log.Warning("Wtf vector");
                                break;
                        }
                    }

                    mindTarget.mindState.mentalStateHandler.TryStartMentalState(mentalStateEffect, null, true, false, null);
                    TargetReset();
                    return;
                }
                else
                {
                    Log.Error("shield bench tried to enable mindcontrol but couldn't.");
                }
            }
        }

        public bool GotThePower()
        {
                return ((this.powerComp != null) && (this.powerComp.PowerOn));
        }
        public bool ActorInRadius()
        {
            if (!AreActorsSet()) return false;

            return ( TryMindReach() );
        }

        public bool TryMindReach()
        {
            // bench pos & map
            if ((this.benchRadius <= 1) || (this.parent.Position == null) || (this.parent.Map == null))
            {
                Log.Warning("null bench");
                return false;
            }
            if ((mindTarget == null) || (mindTarget.Map == null) || (mindTarget.Position == null))
            {
                Log.Warning("null mindTarget");
                return false;
            }

            if ((masterMind == null) || (masterMind.Map == null) || (masterMind.Position == null))
            {
                Log.Warning("null masterMind");
                return false;
            }

            float benchTargetPawnDistance = 999f;
            benchTargetPawnDistance = mindTarget.Position.DistanceTo(this.parent.Position);

            if (benchTargetPawnDistance > benchRadius)
            {
                Log.Warning("target too far");
                return false;
            }
            return true;

        }

        

        [DebuggerHidden]
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if ((powerComp != null) && (powerComp.PowerOn))
            {
                if (IsTargetSet())
                {
                    yield return new Command_Action
                    {
                        action = new Action(this.ShowReport),
                        defaultLabel = "Mind log",
                        defaultDesc = "Show a detailed report",
                        icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchReport", true)
                    };

                    yield return new Command_Action
                    {
                        action = new Action(this.TargetReset),
                        defaultLabel = "Spare",
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
            String buffer = string.Empty;

            stringBuilder.AppendLine("| Mind log |");
            stringBuilder.AppendLine("+--------------+");


            if ( ! AreActorsSet() )
            {
                stringBuilder.AppendLine("No story to tell.");
                return;
            }

            // Mastermind 

           // stringBuilder.AppendLine("|");
            stringBuilder.AppendLine("|");
            stringBuilder.AppendLine("+---[Actors]");
            stringBuilder.AppendLine("|\t|");
            stringBuilder.AppendLine("|\t+-Tormentor : " + mName + "(" + mRace + ")");
            stringBuilder.AppendLine("|\t|");
            stringBuilder.AppendLine("|\t+-PetaPls\t: " + tName + "(" + tRace + ")");
            stringBuilder.AppendLine("|");
            stringBuilder.AppendLine("+---[Vectors]");
            stringBuilder.AppendLine("|\t|");
            if (animalVictim()){
                stringBuilder.AppendLine("|\t+-Leverage  : " +animalVector );
            }
            else
            {
                stringBuilder.AppendLine("|\t+-Leverage  : " + vectorName[bestVector] + "(" + bestVectorValue + ")");
                stringBuilder.AppendLine("|\t|");
                stringBuilder.AppendLine("|\t+-Weak\t: " + vectorName[worstVector] + "(" + worstVectorValue + ")");
            }

            if (masterMindN > 1)
            {
                stringBuilder.AppendLine("|");
                stringBuilder.AppendLine("+---[Numbers]");
                stringBuilder.AppendLine("|\t|");
                stringBuilder.AppendLine("|\t+-Malus - tormentor number(" + masterMindN+") : " + "TODO");
            }
            //stringBuilder.AppendLine("|");
            stringBuilder.AppendLine("|");
            stringBuilder.AppendLine("+---[Progress]");
            stringBuilder.AppendLine("\t|");
            stringBuilder.AppendLine("\t" + ProgressToRegister.ToStringPercent("F0") );

            Dialog_MessageBox window = new Dialog_MessageBox(stringBuilder.ToString(), null, null, null, null, null, false);
            Find.WindowStack.Add(window);
        }

        public override string CompInspectStringExtra()
        {
            if (this.powerComp == null || !this.powerComp.PowerOn)
            {
                return null;
            }

            if (AreActorsSet())
            {
                float SLP = 0f;
                //  per sec
                if (animalVictim())
                    SLP = animalVector * 60;
                else
                    SLP = bestVectorValue * 60;

                float percWorkPerSec = SLP * 100 / workGoal;

                return string.Concat(new string[]
                {
                    "Tormentor : ",
                    mName+ "(" + mRace + ")",
                    "\n",
                    "Victim : ",
                    tName + "(" + tRace + ")",
                    "\n",
                    "Progress : ",
                    this.ProgressToRegister.ToStringPercent("F0"),
                    " (",
                    percWorkPerSec.ToString("0.00") + " %perSec)"
                });
            }
            return "No target set.";
            
        }
    }
}