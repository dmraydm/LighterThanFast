using RimWorld;
using System;
using UnityEngine;
using Verse;

using System.Collections.Generic;

//using NewHatcher.LTF_ShieldHelm;

namespace LighterThanFast
{
    public class Comp_LTF_SynthetizeShieldLayer : ThingComp
    {
        private CompPowerTrader powerComp;

        private float shieldLayerProgress=0;
        //private int storedLayer=0;
        //private int storedLayerMax = 6;

        //private float energyYieldPct;
        private float energySynthesisCapacity;

        float layerMulti = 3.5f;

        private const float shieldLayerWork = 7200f; // 120sec = 60 tick * 120 = 7200 tick
        private float benchRadius = 13;

        public float ProgressToNextLayerPercent
        {
            get
            {
                return shieldLayerProgress / shieldLayerWork;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            this.powerComp = this.parent.TryGetComp<CompPowerTrader>();
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look<float>(ref this.shieldLayerProgress, "LTF_shieldLayerProgress", 0f, false);
            //Scribe_Values.Look<int>(ref this.storedLayer, "LTF_storedLayer", 0, false);
            //Scribe_Values.Look<float>(ref this.energySynthesisCapacity, "LTF_energySynthesisCapacity", 0f, false);
        }

        private float CalculateEnergySynthesisCapacity (Pawn layerSynthetizer)
        {
            float intelligenceValue = layerSynthetizer.GetStatValue(StatDefOf.ResearchSpeed, true);
            float rangedValue = layerSynthetizer.GetStatValue(StatDefOf.ShootingAccuracy, true);
            float craftValue = layerSynthetizer.GetStatValue(StatDef.Named("SmithingSpeed"), true);
//            float meleeValue = layerSynthetizer.GetStatValue(StatDefOf.MeleeDodgeChance, true);
            //float yeldValue = layerSynthetizer.GetStatValue(StatDefOf., true);

            return (intelligenceValue + Rand.Range(rangedValue*.75f , rangedValue*1.25f)) * craftValue * layerMulti;
        }

        // Appel ? debug JobDriver_OperateDeepDrill.cs
        public void SynthetizeLayerDone(Pawn layerSynthetizer)
        {
            //float statValue = layerSynthetizeer.GetStatValue(StatDefOf.MiningSpeed, true);

            energySynthesisCapacity = CalculateEnergySynthesisCapacity(layerSynthetizer);
            // shieldLayerWork;

            this.shieldLayerProgress += energySynthesisCapacity;

            if (this.shieldLayerProgress > shieldLayerWork)
            {
                this.TryDispenseLayer();

                /*
                storedLayer += 1;
                if (storedLayer > storedLayerMax)
                {
                    storedLayer = storedLayerMax;
                }
                */

                this.shieldLayerProgress = 0f;
                this.energySynthesisCapacity = 0f;
            }
        }

        private void TryDispenseLayer()
        {
            if (!this.PowerAndWearerInRadius())
            {
                //Messages.Message("DeepDrillExhausted".Translate(), this.parent, MessageTypeDefOf.TaskCompletion);
                Messages.Message(this.parent.Label + " did not find a wearer in radius or no powa", this.parent, MessageTypeDefOf.TaskCompletion);
            }
            else
            {
                if (TryFindNextWearer(out int countLayersToSynthetize, out IntVec3 cell, out LTF_ShieldHelm LTF_apparel))
                {
                    LTF_ShieldHelm fillMeSenpai = null;
                    fillMeSenpai = LTF_apparel;
                    fillMeSenpai.fillMeSenpai();
                    return;
                }
                else
                {
                    Log.Error("shield bench tried to dispense a layer but couldn't.");
                }
            }
        }

        public bool TryFindNextWearer( out int countLayersToSynthetize, out IntVec3 cell, out LTF_ShieldHelm LTF_apparel)
        {

            // bench pos & map
            if ((benchRadius <= 1) || (this.parent.Position == null) || (this.parent.Map == null))
            {
                Log.Warning("null if you ask me");
                LTF_apparel = null;
                countLayersToSynthetize = 0;
                cell = IntVec3.Invalid;
                return false;
            }

            List < Pawn > allPawnsSpawned = this.parent.Map.mapPawns.SpawnedPawnsInFaction( Faction.OfPlayer );
            int pawnNum = allPawnsSpawned.Count;
            
            if ( pawnNum < 1  )
            {
                Log.Warning("no colonist lol");
                LTF_apparel = null;
                countLayersToSynthetize = 0;
                cell = IntVec3.Invalid;
                return false;
            }

            //Log.Warning("found " + pawnNum + " pawns");
            for (int pawnI = 0; pawnI < pawnNum; pawnI++)
            {
                Pawn maybeWearer = null;
                maybeWearer = allPawnsSpawned[pawnI];

                float benchWearerDistance = 999f;

                // Non null
                if ((maybeWearer == null) || (maybeWearer.Map == null) || (maybeWearer.Position == null) )
                {
                    //Log.Warning("Discarding Null pawn");
                    continue;
                }
                else
                {
                    benchWearerDistance = maybeWearer.Position.DistanceTo(this.parent.Position);
                    if (benchWearerDistance > benchRadius)
                    {
                        //Log.Warning("Discarding too far : " + maybeWearer.LabelShort + " / " + benchWearerDistance.ToString("0.0")+">"+ benchRadius);
                        continue;
                    }
                    
                }

                // free colonist
                if (maybeWearer.IsFreeColonist)
                {
                    // head covered
                    if ((maybeWearer.apparel.BodyPartGroupIsCovered(BodyPartGroupDefOf.FullHead) == true))
                    {
                        List<Apparel> wornApparel = maybeWearer.apparel.WornApparel;
                       // Log.Warning(wornApparel.Count + " cloths found");
                        for (int j = 0; j < wornApparel.Count; j++)
                        {
                            //wearing shield helm
                            if (wornApparel[j].def.defName == "LTF_shieldHelm")
                            {
                                LTF_ShieldHelm shieldHelm = null;
                                shieldHelm = (LTF_ShieldHelm)wornApparel[j];
                                if (shieldHelm != null)
                                {
                                    // besoin de layers
                                    if (shieldHelm.LayersToSynthetize > 0)
                                    {
                                        //Log.Warning("** A winner is " + maybeWearer.LabelShort + "/" + shieldHelm.def.label + "/" + shieldHelm.LayersToSynthetize + "**");
                                        countLayersToSynthetize = (int)shieldHelm.LayersToSynthetize;
                                        LTF_apparel = shieldHelm;
                                        cell = maybeWearer.Position;
                                        return true;
                                    }
                                    //else Log.Warning("Discarding helm : full\t" + wornApparel[j].def.label);
                                }
                                //else Log.Warning("Null helm : full\t" + wornApparel[j].def.label);
                            }
                           // else Log.Warning("Discarding cloth "+j+"/"+ wornApparel.Count + " : not helm\t" + wornApparel[j].def.defName);
                        }
                    }
                    //else Log.Warning("Discarding pawn : not covered\t" + maybeWearer.LabelShort);
                }
               // else Log.Warning("Discarding pawn : not a free colonist\t" + maybeWearer.LabelShort);
            }

            LTF_apparel = null;
            countLayersToSynthetize = 0;
            cell = IntVec3.Invalid;
            return false;
        }

        public bool PowerAndWearerInRadius()
        {
            return (this.powerComp == null || this.powerComp.PowerOn) && TryFindNextWearer( out int num, out IntVec3 intVec, out LTF_ShieldHelm LTF_apparel);
        }
        

        public override string CompInspectStringExtra()
        {
            if (this.powerComp == null || !this.powerComp.PowerOn)
            {
                return null;
            }

            float SLP = energySynthesisCapacity*60; // energy per sec
            float percSynthPerSec = SLP * 100 / shieldLayerWork;

            //string roundedLayers = string.Empty;

            if (TryFindNextWearer(out int countLayersToSynthetize, out IntVec3 c, out LTF_ShieldHelm LTF_apparel))
            {
               // roundedLayers = shieldLayerProgress.ToString("0");
                return string.Concat(new string[]
                {
                    //"ResourceBelow".Translate(),
                    /*
                    "stored ",
                    storedLayer.ToString(),
                    "/",
                    storedLayerMax.ToString(),
                    " ; ",
                    */
                    "Target : ",
                    LTF_apparel.Wearer.LabelShort,
                    ".",
                    LTF_apparel.def.label,
                    //p.LabelShort,
                    " [ " + LTF_apparel.Energy.ToString("0.0") + " / ",
                    LTF_apparel.EnergyMax.ToString("0.0") + " ]",
                    "\n",
                    "Progress : ",
                    this.ProgressToNextLayerPercent.ToStringPercent("F0"),
                    /*[",
                    roundedLayers + "/",
                    shieldLayerWork + "]",
                    " ",
                    */
                    " (",
                    percSynthPerSec.ToString("0.00") + " %perSec)"
                });
            }
            //return "ResourceBelow".Translate() + ": " + "NothingLower".Translate();
            return "No shield wearer within " + benchRadius + " tiles";
            
        }
    }
}