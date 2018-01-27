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
    //TODO 
    // problem if 2 pawns minded : backup faciton relations will be decreased even if both pawns are not from the same faciton

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

                comp_mindControl.ResetProgress();
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
            SoundDefOf.LessonActivated.PlayOneShotOnCamera(usedBy.MapHeld);
            //usedBy.records.Increment(RecordDefOf.ArtifactsActivated);
        }
    }

    // Main
    [StaticConstructorOnStartup]
    public class Comp_LTF_MindControl : ThingComp
    {
        private CompPowerTrader powerComp;
        private float benchRadius = 35.7f;
        private float goodwillImpact = -30f;

        private Pawn masterMind = null;
        private Pawn mindTarget = null;
        private Pawn lastTarget = null;
        private Faction backupFaction = null;

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
        bool mindReachable = false;
        // GFX
        //private static readonly Graphic OverlayGraphic = GraphicDatabase.Get<Graphic_
            //Graphic_Flicker>("Things/Special/Loop", ShaderDatabase.TransparentPostLight, Vector2.one, Color.white);

        private static readonly Material femaleGfx = MaterialPool.MatFrom("Things/Building/MindcontrolBench/icon/gender/female", ShaderDatabase.Transparent);
        private static readonly Material maleGfx = MaterialPool.MatFrom("Things/Building/MindcontrolBench/icon/gender/male", ShaderDatabase.Transparent);

        private static readonly Material animalGfx = MaterialPool.MatFrom("Things/Building/MindcontrolBench/icon/nature/animal", ShaderDatabase.Transparent);
        private static readonly Material humanGfx = MaterialPool.MatFrom("Things/Building/MindcontrolBench/icon/nature/human", ShaderDatabase.Transparent);
        private static readonly Material alienGfx = MaterialPool.MatFrom("Things/Building/MindcontrolBench/icon/nature/alien", ShaderDatabase.Transparent);

        private static readonly Material empathyGfx = MaterialPool.MatFrom("Things/Building/MindcontrolBench/icon/leverage/empathy", ShaderDatabase.Transparent);
        private static readonly Material manipulationGfx = MaterialPool.MatFrom("Things/Building/MindcontrolBench/icon/leverage/manipulation", ShaderDatabase.Transparent);
        private static readonly Material ascensionGfx = MaterialPool.MatFrom("Things/Building/MindcontrolBench/icon/leverage/ascension", ShaderDatabase.Transparent);

        private static readonly Material controlGfx = MaterialPool.MatFrom("Things/Building/MindcontrolBench/icon/effect/control", ShaderDatabase.Transparent);
        private static readonly Material foolGfx = MaterialPool.MatFrom("Things/Building/MindcontrolBench/icon/effect/fool", ShaderDatabase.Transparent);
        private static readonly Material selfharmGfx = MaterialPool.MatFrom("Things/Building/MindcontrolBench/icon/effect/selfharm", ShaderDatabase.Transparent);

        private static readonly Material lockGfx = MaterialPool.MatFrom("Things/Building/MindcontrolBench/icon/target/lock", ShaderDatabase.Transparent);
        private static readonly Material notInRangeGfx = MaterialPool.MatFrom("Things/Building/MindcontrolBench/icon/target/notInRange", ShaderDatabase.Transparent);

        private static readonly Material relationGfx = MaterialPool.MatFrom("Things/Building/MindcontrolBench/icon/relation", ShaderDatabase.Transparent);

        private static readonly Material readyGfx = MaterialPool.MatFrom("Things/Building/MindcontrolBench/icon/ready", ShaderDatabase.Transparent);

        public override void PostDraw()
        {
            base.PostDraw();
 
            if( (!powerComp.PowerOn) || (!AreActorsSet()) )
            {
                //Log.Warning("nopeverlay");
                return;
            }

            
            Vector3 dotS = new Vector3(.11f, 1f, .11f);
            Matrix4x4 matrix = default(Matrix4x4);

            Vector3 benchPos = this.parent.DrawPos;
            benchPos.y += 4;

            if (mindTarget.gender == Gender.Female)            {
                DrawDot(benchPos, dotS, matrix, femaleGfx, -1.03f, .22f);
            }else{
                DrawDot(benchPos, dotS, matrix, maleGfx, -.83f, .135f);
            }

            
            if (animalVictim())            {
                DrawDot(benchPos, dotS, matrix, animalGfx, -0.63f, .22f);
            }else if (humanVictim())            {
                DrawDot(benchPos, dotS, matrix, humanGfx, -0.63f, .04f);
            }else{
                DrawDot(benchPos, dotS, matrix, alienGfx, -.43f, .135f);
            }

            switch (bestVector)
            {
                case (int)MindVector.Empat:
                    DrawDot(benchPos, dotS, matrix, empathyGfx, -1.03f, .03f);
                    break;
                case (int)MindVector.Manip:
                    DrawDot(benchPos, dotS, matrix, manipulationGfx, -1.03f, -.14f);
                    break;
                case (int)MindVector.Ascen:
                    DrawDot(benchPos, dotS, matrix, ascensionGfx, -.83f, -.055f);
                    break;
                default:
                    Log.Warning("Wtf pstdraw vector");
                    break;
            }

            if ( ( animalVictim() && !(backupFaction == null) ) ||
                ( !animalVictim() && backupFaction != Faction.OfPlayer))
                DrawDot(benchPos, dotS, matrix, relationGfx, -.43f, -.05f);

            //DrawDot(benchPos, dotS, matrix, foolGfx, -.83f, -.235f);
            //DrawDot(benchPos, dotS, matrix, controlGfx, -.63f, -.15f);

            if (!mindReachable)
                DrawDot(benchPos, dotS, matrix, notInRangeGfx, -.43f, -.235f);

            //DrawDot(benchPos, dotS, matrix, selfharmGfx, -1.03f, -.32f);

            //DrawDot(benchPos, dotS, matrix, lockGfx, -.63f, -.32f);

            if (mindcontrolEnabled)
                DrawPulse((Thing)parent, readyGfx, benchPos);

        }

        private void DrawDot(Vector3 benchPos, Vector3 dotS, Matrix4x4 matrix, Material dotGfx, float x, float z)
        {
            Vector3 dotPos = benchPos;
            dotPos.x += x;
            dotPos.z += z;
            matrix.SetTRS(dotPos, Quaternion.AngleAxis(0f, Vector3.up), dotS);
            Graphics.DrawMesh(MeshPool.plane14, matrix, dotGfx, 0);
        }


        private void DrawPulse(Thing thing, Material mat, Vector3 drawPos)
        {
            float num = (Time.realtimeSinceStartup + 397f * (float)(thing.thingIDNumber % 571)) * 4f;
            float num2 = ((float)Math.Sin((double)num) + 1f) * 0.5f;
            num2 = 0.3f + num2 * 0.7f;
            Material material = FadedMaterialPool.FadedVersionOf(mat, num2);

            drawPos.x += .9f;
            drawPos.z += -.32f;

            Vector3 readyS = new Vector3(.25f, 1f, .25f);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(drawPos, Quaternion.AngleAxis(0f, Vector3.up), readyS);

            Graphics.DrawMesh(MeshPool.plane14, matrix, material, 0);
        }

        // progress in work
        public float ProgressToRegister
        {
            get
            {
                return workProgress / workGoal;
            }
        }

        // calculate the best way for iniator to mine target
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

        // calculate the worst way for iniator to mine target.
        // target fight back ?
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
            if (masterMind != null)
            {
                mName = masterMind.NameStringShort;
                mRace = masterMind.def.label;
            }

            if (mindTarget != null)
            {
                tName = mindTarget.NameStringShort;
                tRace = mindTarget.def.label;
            }
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
            base.PostExposeData();
            Scribe_References.Look(ref masterMind, "LTF_masterMind");
            Scribe_References.Look(ref mindTarget, "LTF_mindTarget");

            Scribe_References.Look(ref backupFaction, "LTF_backupFaction");

            Scribe_Values.Look(ref tName, "LTF_tName");
            Scribe_Values.Look(ref tRace, "LTF_tRace");
            Scribe_Values.Look(ref mName, "LTF_mName");
            Scribe_Values.Look(ref mRace, "LTF_mRace");

            Scribe_Values.Look(ref workProgress, "LTF_workProgress");

            Scribe_Values.Look(ref bestVector, "LTF_bestVector");
            Scribe_Values.Look(ref bestVectorValue, "LTF_bestVectorValue");
            Scribe_Values.Look(ref worstVector, "LTF_worstVector");
            Scribe_Values.Look(ref worstVectorValue, "LTF_worstVectorValue");

            Scribe_Values.Look(ref this.mindcontrolEnabled, "LTF_mindEnabled");
            Scribe_Values.Look(ref this.mindReachable, "LTF_mindReachable");
        }
        public Faction GetFactionBackup()
        {
            return (backupFaction);
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
                    Log.Warning("Wtf set vector");
                    break;
            }

            // 0-20
            float mS1 = masterMind.skills.GetSkill(firstSkill).Level;
            int mS1P = (int)masterMind.skills.GetSkill(firstSkill).passion;
            float tS1 = mindTarget.skills.GetSkill(firstSkill).Level;
            int tS1P = (int)mindTarget.skills.GetSkill(firstSkill).passion;

            // si négatif, mastermind moins skilled que target
            float deltaS1 = mS1 - tS1;
            // si négatif, mastermind moins passionné
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
            // si négatif, mastermind moins skilled que target
            float deltaS2 = mS2 - tS2;
            // si négatif, mastermind moins passionné
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
                //ThingUtility.
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
                ResetProgress();
                mindTarget = newTarget;
                tName = mindTarget.NameStringShort;
                tRace = mindTarget.def.label;
                if (!animalVictim())
                    backupFaction = mindTarget.Faction;
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
                ratio = masterMind.skills.GetSkill(SkillDefOf.Animals).Level / (mindTarget.RaceProps.wildness);
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

            //this.CompatibilityFactorCurve.Evaluate(initiator.relations.CompatibilityWith(recipient));

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
            mindcontrolEnabled = false;
        }

        public bool animalVictim() {
            return mindTarget.RaceProps.Animal;
        }

        public bool humanVictim()
        {
            return (mindTarget.kindDef.race.defName == "Human");
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
            backupFaction = null;
            mindTarget = null;
            masterMind = null;
            ResetProgress();
            VectorReset();
            //Log.Warning("target reset to null");
            return;
        }

        private void VectorReset()
        {
            bestVector = (int)MindVector.Na;
            bestVectorValue = 0f;
            worstVector = (int)MindVector.Na;
            worstVectorValue = 0f;
        }

        private bool TryFactionChange()
        {
            if (animalVictim())
            {
                Log.Warning("not allowed for animals");
                return false;
            }

            if ((!GotThePower()))
            {
                Messages.Message(parent.Label + " requires more power.", this.parent, MessageTypeDefOf.TaskCompletion);
                return false;
            }
            if (!IsTargetSet()) {
                Messages.Message(tName + " /target is Missing", this.parent, MessageTypeDefOf.TaskCompletion);
                return false;
            }
            if (!IsMasterMindSet())
            {
                Messages.Message(mName + " /mastermind is Missing", this.parent, MessageTypeDefOf.TaskCompletion);
                return false;
            }

            if (!ActorInRadius())
            {
                Messages.Message(mName + " did not find " + tName + " in radius.", this.parent, MessageTypeDefOf.TaskCompletion);
                return false;
            }
            if (!IsWorkDone())
            {
                Messages.Message("Work is not done.", this.parent, MessageTypeDefOf.TaskCompletion);
                return false;
            }

            if (mindTarget.Faction == masterMind.Faction)
            {
                Messages.Message(mName + " and " + tName + " are from the same faction. How wrong for so long ?", this.parent, MessageTypeDefOf.TaskCompletion);
                return false;
            }

            if (TryMindReach())
            {
                if ((mindTarget == null) || (masterMind == null))
                {
                    Log.Warning("how so far ?");
                    return false;
                }
                Hediff changeFactionHediff = null;

                BodyPartRecord bodyPart = null;
                HediffDef hediff2use = null;

                //Log.Warning("retrieving hediif def");
                hediff2use = HediffDef.Named("Hediff_LTF_FactionChange");

                mindTarget.RaceProps.body.GetPartsWithTag("ConsciousnessSource").TryRandomElement(out bodyPart);
                if (bodyPart == null)
                {
                    Log.Warning("null body part");
                    return false;
                }

                //Log.Warning("Found bodypart " + bodyPart.def);
                changeFactionHediff = HediffMaker.MakeHediff(hediff2use, mindTarget, bodyPart);
                if (changeFactionHediff == null)
                {
                    Log.Warning("hediff maker null");
                    return false;
                }

                List<Hediff> allHediffs = mindTarget.health.hediffSet.hediffs;
                for (int i = 0; i < allHediffs.Count; i++)
                {
                    if (allHediffs[i].def == hediff2use)
                    {
                        Log.Warning("Already enslaved cant do it twice");
                        return false;
                    }
                }

                HediffComp_LTF_FactionChange newComp = changeFactionHediff.TryGetComp<HediffComp_LTF_FactionChange>();
                if (newComp == null)
                {
                    Log.ErrorOnce("newComp not found",1);
                    return false;
                }

                if (!newComp.Init(mindTarget.Faction, masterMind, (Building)parent, 10000))
                {
                    Log.Warning("failed to Init");
                    return false;
                }

                mindTarget.health.AddHediff(changeFactionHediff, bodyPart, null);
                return true;
            }
            return false;
        }

        // Gizmo Commands

        public void TargetResetCmd()
        {
            TargetReset();
            SoundDefOf.LessonDeactivated.PlayOneShotOnCamera(null);
        }

        private void TryDisorientAndReset()
        {
            if (mindTarget.Faction != masterMind.Faction)
            {
                if(!TryFactionChange()) return;
            }
                

            if( TryMindControl() )
            {
                BadWillHumanlike();
                SoundDef.Named("LetterArriveBadUrgentSmall").PlayOneShotOnCamera(null);
                //lastTarget = mindTarget;
                TargetReset();
            }
            
        }

        private void TryEnslaveAndReset()
        {
            if( TryFactionChange())
            {
                BadWillHumanlike();
                SoundDef.Named("LetterArriveGood").PlayOneShotOnCamera(null);
                //lastTarget = mindTarget;
                TargetReset();
            }
        }

        private void TryManhunterAndReset()
        {
            if (TryManhunter())
            {
                BadWillAnimal();
                SoundDef.Named("LetterArriveBadUrgentBig").PlayOneShotOnCamera(null);
                lastTarget = mindTarget;
                TargetReset();
                
            }
        }

        // For animals
        private bool TryManhunter()
        {

            if (!animalVictim())
            {
                Log.Warning("not allowed for humans");
                return false;
            }

            if ((!GotThePower()))
            {
                Messages.Message(parent.Label + " requires more power.", this.parent, MessageTypeDefOf.TaskCompletion);
                return false;
            }

            if (!ActorInRadius())
            {
                Messages.Message(mName + " did not find " + tName + " in radius.", this.parent, MessageTypeDefOf.TaskCompletion);
                return false;
            }

            if (!IsWorkDone())
            {
                Messages.Message("Work is not done.", this.parent, MessageTypeDefOf.TaskCompletion);
                return false;
            }

            if (TryMindReach())
            {
                MentalStateDef chosenState = null;
                chosenState = MentalStateDefOf.Manhunter;
                Log.Warning(tName + "state : " + chosenState.defName);
                mindTarget.mindState.mentalStateHandler.TryStartMentalState(chosenState, null, true, false, null);

                return true;
            }
            else
            {
                Log.Error("mind control bench tried to enable mindcontrol but couldn't.");
                return false;
            }

        }
    
        // For humanlike
        // Applies mentalstate
        private bool TryMindControl()
        {
            if (animalVictim())
            {
                Log.Warning("not allowed for animals");
                return false;
            }

            if ( (!GotThePower()) )
            {
                Messages.Message(parent.Label + " requires more power.", this.parent, MessageTypeDefOf.TaskCompletion);
                return false;
            }

            if (!ActorInRadius()) {
                Messages.Message(mName + " did not find " + tName + " in radius.", this.parent, MessageTypeDefOf.TaskCompletion);
                return false;
            }

            if ( !IsWorkDone())
            {
                Messages.Message( "Work is not done.", this.parent, MessageTypeDefOf.TaskCompletion);
                return false;
            }

            if (TryMindReach())
            {
                MentalStateDef chosenState = null;
                if ((mindTarget == null) || (masterMind == null))
                {
                    Log.Warning("how so far ?");
                    return false;
                }

                    switch (bestVector)
                    {
                        case (int)MindVector.Empat:
                            // will wake up if sleeping
                            //mentalStateEffect = MentalStateDefOf.WanderSad;
                        break;
                        case (int)MindVector.Manip:
                            //mentalStateEffect = MentalStateDefOf.PanicFlee;
                            //mentalStateEffect = MentalStateDefOf.WanderSad;
                            break;
                        case (int)MindVector.Ascen:
                            /*
                            mentalStateEffect = MentalStateDefOf.Berserk;
                            mindTarget.mindState.mentalStateHandler.TryStartMentalState(mentalStateEffect, null, true, false, null);
                            */
                            break;
                        default:
                            Log.Warning("Wtf mind vector");
                            break;
                    }

                    Thought_Memory victimBreak = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("LTF_MindBreak"));
                    Thought_Memory victimShame = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("LTF_MinedShame"));
                    // Initiator
                    Thought_Memory InitiatorPride = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("LTF_MinedSomeonePride"));
                    Thought_Memory InitiatorShame = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("LTF_MinedSomeoneShame"));

                    if ( (victimBreak == null) || (victimShame == null) || (victimBreak == null) || (victimBreak == null))
                    {
                        Log.Warning("Thought null");
                    }
                    else
                    {
                        mindTarget.needs.mood.thoughts.memories.TryGainMemory(victimBreak, masterMind);
                        mindTarget.needs.mood.thoughts.memories.TryGainMemory(victimShame, masterMind);

                        masterMind.needs.mood.thoughts.memories.TryGainMemory(InitiatorPride, mindTarget);
                        masterMind.needs.mood.thoughts.memories.TryGainMemory(InitiatorShame, mindTarget);

                        //wakes up target if laying
                        mindTarget.ClearMind();
                        if (mindTarget.story != null)
                        {
                            //Log.Warning(tName + " ok Story ");
                            List<Trait> targetTraits = new List<Trait>();
                            targetTraits = mindTarget.story.traits.allTraits;
                            //Log.Warning(tName + " traits found: " + targetTraits.Count );

                            IEnumerable<MentalBreakDef> breaks = DefDatabase<MentalBreakDef>.AllDefsListForReading;
                            MentalBreakDef chosenBreak = breaks.RandomElement<MentalBreakDef>();
                            if (chosenBreak == null) {
                                Log.Warning("null break");
                            }
                            //Log.Warning(tName + "break : " + chosenBreak.defName);

                            chosenState = chosenBreak.mentalState;
                            if (chosenState == null)
                            {
                                Log.Warning("null state");
                            }

                            /*for (int i = 0; i < targetTraits.Count; i++){
                                Log.Warning(tName + " : " + targetTraits[i].CurrentData.label);
                                TraitDegreeData currentData = targetTraits[i].CurrentData;
                                if (currentData.randomMentalState != null){
                                    if (trait.CurrentData.theOnlyAllowedMentalBreaks != null){
                                        Log.Warning(tName + " : " + trait.CurrentData.label);
                                        for (int j = 0; j < trait.CurrentData.theOnlyAllowedMentalBreaks.Count; j++){
                                            Log.Warning (tName + " : " + trait.CurrentData.theOnlyAllowedMentalBreaks[j].defName );
                                        }
                                    }
                                }
                                }
                            */

                        }
                        else
                        {
                            Log.Warning(tName + " : null story");
                            return false;
                        }
                    }

                Log.Warning(tName + "state : " + chosenState.defName);
                mindTarget.mindState.mentalStateHandler.TryStartMentalState(chosenState, null, true, false, null);

                return true;
            }
            else
            {
                Log.Error("mind control bench tried to enable mindcontrol but couldn't.");
                return false;
            }
            
        }

        public bool GotThePower()
        {
                return ((this.powerComp != null) && (this.powerComp.PowerOn));
        }
        public bool ActorInRadius()
        {
            return ( TryMindReach() );
        }

        public bool TryMindReach()
        {
            mindReachable = false;
            // bench pos & map
            if ((this.benchRadius <= 1) || (this.parent.Position == null) || (this.parent.Map == null))
            {
                Log.Warning("null bench");
                return false;
            }
            if ((mindTarget == null) || (mindTarget.Map == null) || (mindTarget.Position == null))
            {
                //Log.Warning("null mindTarget");
                ResetProgress();
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
                Messages.Message("Target is too far." , this.parent, MessageTypeDefOf.TaskCompletion);
                return false;
            }

            mindReachable = true;
            return true;

        }

        

        [DebuggerHidden]
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if ((powerComp != null) && (powerComp.PowerOn))
            {
                if (AreActorsSet())
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
                        action = new Action(this.TargetResetCmd),
                        defaultLabel = "Spare",
                        defaultDesc = "Reset the target",
                        icon = ContentFinder<Texture2D>.Get("UI/Buttons/Delete", true)
                    };

                    if (mindcontrolEnabled)
                    {
                        // Humanlike
                        if (!animalVictim())
                        {
                            yield return new Command_Action
                            {
                                action = new Action(this.TryDisorientAndReset),
                                defaultLabel = "Fool me once",
                                defaultDesc = "This is madness",
                                icon = ContentFinder<Texture2D>.Get("UI/Commands/MindControl", true)
                            };


                            yield return new Command_Action
                            {
                                action = new Action(this.TryEnslaveAndReset),
                                defaultLabel = "Mind control",
                                defaultDesc = "Take control for a moment",
                                icon = ContentFinder<Texture2D>.Get("UI/Commands/FactionChange", true)
                            };
                        }
                        //Animal
                        else
                        {
                            yield return new Command_Action
                            {
                                action = new Action(this.TryManhunterAndReset),
                                defaultLabel = "Go my minions",
                                defaultDesc = "Make an animal angry",
                                icon = ContentFinder<Texture2D>.Get("UI/Commands/Manhunter", true)
                            };
                        }
                            
                    }

                    if ((Prefs.DevMode) && (!IsWorkDone()))
                    {
                        yield return new Command_Action
                        {
                            defaultLabel = "Haaax: insta ready",
                            action = delegate
                            {
                                workProgress = workGoal;
                                mindcontrolEnabled = true;
                            }
                        };
                    }
                }
            }
        }

        void BadWillAnimal()
        {
            if(mindTarget.Faction != null)
            {
                mindTarget.Faction.AffectGoodwillWith(masterMind.Faction, goodwillImpact);
                mindTarget.Faction.SetHostileTo(masterMind.Faction, true);
                Messages.Message(mindTarget.Faction.Name + " </3 " + Faction.OfPlayer.Name + "(" + goodwillImpact + ")", MessageTypeDefOf.NegativeEvent);
            }
        }

        void BadWillHumanlike()
        {
            if (backupFaction == null)
            {
                Log.Warning("back faction null");
                return;
            }
            if(masterMind.Faction != backupFaction)
            {
                backupFaction.AffectGoodwillWith(masterMind.Faction, goodwillImpact);
                backupFaction.SetHostileTo(masterMind.Faction, true);
                Messages.Message(backupFaction.Name + " </3 " + Faction.OfPlayer.Name + "(" + goodwillImpact + ")", MessageTypeDefOf.NegativeEvent);

            }  
        }
        

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
            stringBuilder.AppendLine("|\t+-Tormentor : " + mName + "(" + mRace + ")" + " from " + masterMind.Faction.Name + ".");
            stringBuilder.AppendLine("|\t|");

            if (animalVictim())
            {
                if(mindTarget.Faction == null)
                {
                    stringBuilder.AppendLine("|\t+-PetaPls\t: " + tName + "(" + tRace + ").");
                }
                else
                {
                    stringBuilder.AppendLine("|\t+-FactionAnimal\t: " + tName + "(" + tRace + ")" + " from " + mindTarget.Faction.Name + ".");
                }
                    
            }
            else
            {
                stringBuilder.AppendLine("|\t+-Victim\t: " + tName + "(" + tRace + ")" + " from " + mindTarget.Faction.Name + ".");
            }

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
            return "No victim registered.";
            
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            if ( mindTarget != null && mindTarget.Map != null)
            {
                GenDraw.DrawLineBetween(this.parent.TrueCenter(), mindTarget.TrueCenter());
            }
                
        }

       
    }
}