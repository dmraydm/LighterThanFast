/*
 * Created by SharpDevelop.
 * User: Etienne
 * Date: 22/11/2017
 * Time: 16:41
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace NewHatcher
{

    [StaticConstructorOnStartup]
    public class LTF_ShieldHelm : Apparel
    {
        private float energy;
        private int layersToSynthetize;
        private float EnergyOnReset = 1f;

        private int ticksToReset = -1;
        private int lastKeepDisplayTick = -9999;

        private int StartingTicksToReset = 3200;
        private int KeepDisplayingTicks = 1000;

        private Vector3 impactAngleVect;

        private int lastAbsorbDamageTick = -9999;

        private const float MinDrawSize = .5f;
        private const float MaxDrawSize = 1.5f;

        float JitterDrawSize = .2f;
        //private const int JitterDurationTicks = 8;
        //private const float MaxDamagedJitterDist = 0.05f;

        private const int layerNumberMax = 7;
        // 1.85 / 7
        //private const float haxLevelPerLayer = .26428f;
        private int layerCounter = 0;

        private const float matrixFullOffset = .05f;

        private static readonly Material BubbleMatrix = MaterialPool.MatFrom("Other/Shield/shieldMatrix", ShaderDatabase.Transparent);
        private static readonly Material BubbleFull = MaterialPool.MatFrom("Other/Shield/shieldFull", ShaderDatabase.Transparent);

        private static readonly Material GreenDot = MaterialPool.MatFrom("Other/Shield/Counter/shieldGreenDot", ShaderDatabase.Transparent);
        private static readonly Material BlueDot = MaterialPool.MatFrom("Other/Shield/Counter/shieldBlueDot", ShaderDatabase.Transparent);
        private static readonly Material GreyDot = MaterialPool.MatFrom("Other/Shield/Counter/layer", ShaderDatabase.Transparent);

        // EnergyMax = number of shield layers
        public float EnergyMax
        {
            get
            {
                return this.GetStatValue(StatDef.Named("LTF_ShieldLayerMax"), true);
            }
        }

        // / 60f
        private float EnergyGainPerTick
        {
            get
            {
                // Slowed speed bc we bench
               return this.GetStatValue(StatDef.Named("LTF_ShieldRechargeRate"), true) / 1000f;
                // fast speed bc we debug
               // return this.GetStatValue(StatDef.Named("LTF_ShieldRechargeRate"), true);
            }
        }

        public float Energy
        {
            get
            {
                return this.energy;
            }
        }

        public bool fillMeSenpai()
        {
            if (LayersToSynthetize > 0)
            {
                energy += 1;
                return true;
            }
            return false;
        }

        public float LayersToSynthetize
        {
            get
            {
                return this.layersToSynthetize;
            }
        }

        public ShieldState ShieldState
        {
            get
            {
                if (this.ticksToReset > 0)
                {
                    return ShieldState.Resetting;
                }
                return ShieldState.Active;
            }
        }

        private bool ShouldDisplay
        {
            get
            {
                Pawn wearer = base.Wearer;
                return  wearer.Spawned && 
                        !wearer.Dead && 
                        !wearer.Downed && 
                        (wearer.InAggroMentalState || wearer.Drafted || (wearer.Faction.HostileTo(Faction.OfPlayer) && !wearer.IsPrisoner) 
                        || Find.TickManager.TicksGame < this.lastKeepDisplayTick + this.KeepDisplayingTicks);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.energy, "energy", 0f, false);
            //Scribe_Values.Look<int>(ref this.layersToSynthetize, "layersToSynthetize", 99, false);
            Scribe_Values.Look<int>(ref this.ticksToReset, "ticksToReset", -1, false);
            Scribe_Values.Look<int>(ref this.lastKeepDisplayTick, "lastKeepDisplayTick", 0, false);
        }
        
        /*
        [DebuggerHidden]
        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            if (Find.Selector.SingleSelectedThing == base.Wearer)
            {
                yield return new Gizmo_EnergyShieldStatus
                {
                    shield = this
                };
            }
        }
        */

        
        public override void Tick()
        {
            base.Tick();
            if (base.Wearer == null)
            {
                this.energy = 0f;
                SetLayerCounter();

                return;
            }

            if (this.ShieldState == ShieldState.Resetting)
            {
                this.ticksToReset--;
                if (this.ticksToReset <= 0)
                {
                    this.Reset();
                }
            }
            else if (this.ShieldState == ShieldState.Active)
            {
                this.energy += this.EnergyGainPerTick;
                if (this.energy > this.EnergyMax)
                {
                    this.energy = this.EnergyMax;
                }
            }
            SetLayerCounter();
        }

        public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
        {
            if (this.ShieldState == ShieldState.Active 
                && ((dinfo.Instigator != null 
                /*&& !dinfo.Instigator.Position.AdjacentTo8WayOrInside(base.Wearer.Position)*/) 
                || dinfo.Def.isExplosive))
            {
                if (dinfo.Instigator != null)
                {
                    AttachableThing attachableThing = dinfo.Instigator as AttachableThing;
                    /// self damage ?
                    if (attachableThing != null && attachableThing.parent == base.Wearer)
                    {
                        return false;
                    }
                }
                // raw : energie descend fonction des degats
                //this.energy -= (float)dinfo.Amount * this.EnergyLossPerDamage;

                // LTF : 1 shieldLayer per hit
                this.energy--;
                if (this.energy < 0) this.energy = 0f;
                //this.energy -= haxLevelPerLayer;
                //this.layerCounter--;

                if (dinfo.Def == DamageDefOf.EMP)
                {
                    this.energy = 0f;
                    //this.layerCounter = 0;
                    SetLayerCounter();
                }
                if (this.layerCounter == 0f)
                {
                    this.Break();
                }
                else
                {
                    this.AbsorbedDamage(dinfo);
                }

                SetLayerCounter();
                return true;
            }
            return false;
        }

        public void KeepDisplaying()
        {
            this.lastKeepDisplayTick = Find.TickManager.TicksGame;
        }

        private void SetLayerCounter()
        {
            // 1.8 * 7 / 1.8 = 7 ; 0.3 * 7 / 1.8 =  ; 
            //float layerCntF = this.energy * this.EnergyMax;
            //this.layerCounter = Mathf.RoundToInt(this.energy) - 1;
            this.layerCounter = Mathf.RoundToInt(this.energy);
            if ( this.layerCounter < 0f) this.layerCounter = 0;
            this.layersToSynthetize = (int)this.EnergyMax - this.layerCounter;
            //Log.Warning("energy:" + energy + "layerNum:" + layerCounter);
        }

        private void AbsorbedDamage(DamageInfo dinfo)
        {
            SoundDefOf.EnergyShieldAbsorbDamage.PlayOneShot(new TargetInfo(base.Wearer.Position, base.Wearer.Map, false));
            this.impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            Vector3 loc = base.Wearer.TrueCenter() + this.impactAngleVect.RotatedBy(180f) * 0.5f;
            float num = Mathf.Min(10f, 2f + (float)dinfo.Amount / 10f);
            MoteMaker.MakeStaticMote(loc, base.Wearer.Map, ThingDefOf.Mote_ExplosionFlash, num);
            int num2 = (int)num;
            for (int i = 0; i < num2; i++)
            {
                MoteMaker.ThrowDustPuff(loc, base.Wearer.Map, Rand.Range(0.8f, 1.2f));
            }
            this.lastAbsorbDamageTick = Find.TickManager.TicksGame;
            this.KeepDisplaying();
        }

        private void Break()
        {
            SoundDefOf.EnergyShieldBroken.PlayOneShot(new TargetInfo(base.Wearer.Position, base.Wearer.Map, false));
            MoteMaker.MakeStaticMote(base.Wearer.TrueCenter(), base.Wearer.Map, ThingDefOf.Mote_ExplosionFlash, 12f);
            for (int i = 0; i < 6; i++)
            {
                Vector3 loc = base.Wearer.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle((float)Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f);
                MoteMaker.ThrowDustPuff(loc, base.Wearer.Map, Rand.Range(0.8f, 1.2f));
            }
            this.energy = 0f;
            SetLayerCounter();
            this.ticksToReset = this.StartingTicksToReset;
        }

        private void Reset()
        {
            if (base.Wearer.Spawned)
            {
                SoundDefOf.EnergyShieldReset.PlayOneShot(new TargetInfo(base.Wearer.Position, base.Wearer.Map, false));
                MoteMaker.ThrowLightningGlow(base.Wearer.TrueCenter(), base.Wearer.Map, 3f);
            }
            this.ticksToReset = -1;
            this.energy = this.EnergyOnReset;

            SetLayerCounter();
        }

        public override void DrawWornExtras()
        {
            if (this.ShieldState == ShieldState.Active && this.ShouldDisplay)
            {
                //Linear interpolation
                //float drawSize = Mathf.Lerp(MinDrawSize, MaxDrawSize, this.energy);
                //float matrixDrawSize = Mathf.Lerp(MinDrawSize, MaxDrawSize, 1);
                float matrixDrawSize = MaxDrawSize;
                float fullDrawSize = Mathf.Lerp(MinDrawSize - matrixFullOffset, MaxDrawSize - matrixFullOffset, this.energy / this.EnergyMax);
                //float fullDrawSize = Mathf.Lerp(MinDrawSize, MaxDrawSize, this.layerCounter / layerNumberMax);
                // - matrixFullOffset

                // x;y wearer
                Vector3 wearerPos = base.Wearer.Drawer.DrawPos;
                wearerPos.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);

                //Vector3 counterPos = wearerPos;
                //counterPos.x -= (float) (0.5);
                //counterPos.x -= .65f;

                Vector3 titlePos = wearerPos;
                //titlePos.x -= .5f;
                titlePos.x -= .65f;
                //titlePos.z += .4f;
                //titlePos.z += .425f;
                titlePos.z += .432f;
                //titlePos.z += .45f;


                // Différence timeElapsedSinceAbsorb

                int timeElapsedSinceAbsorb = Find.TickManager.TicksGame - this.lastAbsorbDamageTick;
                if (timeElapsedSinceAbsorb < 8)
                {
                    float num3 = (float)(8 - timeElapsedSinceAbsorb) / 8f * 0.05f;
                    wearerPos += this.impactAngleVect * num3;
                    matrixDrawSize -= num3;
                    fullDrawSize -= num3;
                }
                

                // shield Matrix
                //float angle = (float)Rand.Range(0, 360);

                Vector3 indicatorS = new Vector3(.18f, 1f, .18f);
                Vector3 counterS = new Vector3(.125f, 1f, 1f);

                //Vector3 blueS = new Vector3(1f, 1f, 1f);
                Vector3 blueS = new Vector3(.1f, 1f, .1f);

                // Matrix
                //float angle = 0;
                Vector3 matrixS = new Vector3(matrixDrawSize, 1f, matrixDrawSize);
                Matrix4x4 matrixMatrix = default(Matrix4x4);
                //Sets this matrix to a translation, rotation and scaling matrix.
                matrixMatrix.SetTRS(wearerPos, Quaternion.AngleAxis(0, Vector3.up), matrixS);

                float jitterV = 0;
                float jitterH = 0;
                if (Rand.Range(0f, 1f) > .995f)
                {
                    jitterV = Rand.Range(-JitterDrawSize, JitterDrawSize);
                    jitterH = Rand.Range(-JitterDrawSize, JitterDrawSize);
                }

                // Full
                Vector3 fullS = new Vector3(fullDrawSize+jitterV, 1f, fullDrawSize+jitterH);
                Matrix4x4 fullMatrix = default(Matrix4x4);
                fullMatrix.SetTRS(wearerPos, Quaternion.AngleAxis(0, Vector3.up), fullS);

                // counter base
                if (this.layerCounter > 0)
                   {

                    //Log.Warning( "Energy : " + this.energy + "/" + this.EnergyMax + " Layers : " + this.layerCounter + "/" + layerNumberMax);
                    
                    for (int i = 0; i < Mathf.FloorToInt(EnergyMax); i++)
                    {

                        Matrix4x4 layerMatrix = default(Matrix4x4);
                        Vector3 dotPos = titlePos;
                        dotPos.z -= (.125f) * (1 + i);

                        //Log.Warning("i : " + i.ToString() + "/" + EnergyMax + "; z : " + dotPos.z);
                        
                        layerMatrix.SetTRS(dotPos, Quaternion.AngleAxis(0, Vector3.up), blueS);
                        Graphics.DrawMesh(MeshPool.plane20, layerMatrix, LTF_ShieldHelm.GreyDot, 0);
                    }

                    for (int i = 0; i < this.layerCounter; i++)
                    {

                        Matrix4x4 blueMatrix = default(Matrix4x4);
                        Vector3 dotPos = titlePos;
                        dotPos.z -= (.125f) * (1 + i);

                        //Log.Warning("i : " + i.ToString() + "/"+ EnergyMax+"; z : " + dotPos.z);
                        float randomAngle = (float)Rand.Range(0, 360);
                        blueMatrix.SetTRS(dotPos, Quaternion.AngleAxis(randomAngle, Vector3.up), blueS);
                        //blueMatrix.SetTRS(titlePos, Quaternion.AngleAxis(0, Vector3.up), blueS);
                        Graphics.DrawMesh(MeshPool.plane14, blueMatrix, LTF_ShieldHelm.BlueDot, 0);

                    }
                    
                    Graphics.DrawMesh(MeshPool.plane10, fullMatrix, LTF_ShieldHelm.BubbleFull, 0);
                    // Graphics.DrawMesh(MeshPool.plane10, indicatorMatrix, LTF_ShieldHelm.Indicator, 0);

                    //Log.Warning("Energy : " + Energy + "/" + EnergyMax + "; ePerTick : " + EnergyGainPerTick + "Layers :" + this.layerCounter+"/"+this.EnergyMax );

                }
                Graphics.DrawMesh(MeshPool.plane10, matrixMatrix, LTF_ShieldHelm.BubbleMatrix, 0);

                Matrix4x4 indicatorMatrix = default(Matrix4x4);
                indicatorMatrix.SetTRS(titlePos, Quaternion.AngleAxis(0, Vector3.up), indicatorS);
                Graphics.DrawMesh(MeshPool.plane10, indicatorMatrix, LTF_ShieldHelm.GreenDot, 0);

            }
        }

        public override bool AllowVerbCast(IntVec3 root, Map map, LocalTargetInfo targ)
        {
            return ReachabilityImmediate.CanReachImmediate(root, targ, map, PathEndMode.Touch, null);
        }
	}
}
