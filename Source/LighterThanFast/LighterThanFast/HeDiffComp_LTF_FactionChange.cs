using RimWorld;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using UnityEngine;

using Verse;

using Verse.Sound;

namespace LighterThanFast
{
    // Main
    public class HediffComp_LTF_FactionChange : HediffComp
    {
        private int ticksLeft = 0;

        public Faction originFaction = null;
        public string originKindDefLabel = null;
        public PawnKindDef originKindDef = null;

        public Pawn Initiator = null;
        public Building MindControlBench = null;
        public Comp_LTF_MindControl BenchComp = null;

        public bool LTF_debug = false;

        public HediffCompProperties_LTF_FactionChange Props
        {
            get
            {
                return (HediffCompProperties_LTF_FactionChange)this.props;
            }
        }

        public void ToggleDebug(bool debug=false)
        {
            //Caller debug, not hediff one
            if (debug)
                Log.Warning("gotta debug: " + LTF_debug + "->" + !LTF_debug);

            LTF_debug =! LTF_debug;
        }

        public override bool CompShouldRemove
        {
            get
            {
                return base.CompShouldRemove || Time2Release() || NullInitiator() || NullVictim() || NullBench();
            }
        }

        public Faction SayFaction() => originFaction;
        public String SayKindDefLabel() => originKindDefLabel;
        public PawnKindDef SayKindDef() => originKindDef;


        private bool Time2Release()
        {
            bool test = (this.ticksLeft <= 0);
            //Log.Warning("time 2 release" + test);
            return (test);
        }

        private bool NullVictim()
        {
            bool test = (parent.pawn == null || parent.pawn.Map == null);
            //Log.Warning("release" + test);
            return (test);
        }
        private bool NullInitiator()
        {
            bool test = (Initiator == null || Initiator.Dead || Initiator.Faction != Faction.OfPlayer || Initiator.Map == null);
           // Log.Warning("initiator" + test);
            return (test);
        }

        private bool NullBench()
        {
            bool test = (MindControlBench == null || BenchComp == null || !BenchComp.GotThePower);
            //Log.Warning("bench" + test);
            return (test);
        }
        

        public override void CompPostMake()
        {
            base.CompPostMake();
            if (MindControlBench != null)
            {
                CompInit();
            }
        }

        public override string CompTipStringExtra
        {
            get
            {
                string result = string.Empty;
                //int ticksLeft = (int)(ticksLeft * 60000);
                if (originFaction != null)
                {
                    if (this.ticksLeft > 0)
                    {
                        result = ticksLeft.ToStringTicksToPeriod(true, false, true) + " before "+ Initiator.LabelShort +" releases to " + originFaction.Name + "."; ;
                        //result += parent.pawn.LabelShort + 
                    }
                }

                if (LTF_debug)
                {
                    result += "\n[debugOn]";

                    result += ";F(hasName"+ ((originFaction == null) ? ("null") : (originFaction.HasName.ToString())) + "):" + ((originFaction == null)?("null"):(originFaction.Name));

                    result += ";KL:" + ((originKindDefLabel == null) ? ("null") : (originKindDefLabel));
                    result += ";K:" + ((originKindDef == null) ? ("null") : (originKindDef.defName));
                }
                return result;
            }
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref ticksLeft, "LTF_SlaveTicks");

            Scribe_References.Look(ref originFaction, "LTF_originFaction");
            Scribe_Defs.Look(ref originKindDef, "LTF_originKind");
            Scribe_Values.Look(ref originKindDefLabel, "LTF_originKindLabel");
            
            Scribe_References.Look(ref Initiator, "LTF_mindIniator");
            Scribe_References.Look(ref MindControlBench, "LTF_MindControlBench");
            
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (this.parent == null)
            {
                Log.Warning("null comp");
                return;
            }

            if (this.parent.pawn == null)
            {
                Log.Warning("null pawn");
                return;
            }

            if(this.parent.pawn.Map == null)
            {
                Log.Warning(parent.Label + " null map");
                return;
            }

            if (BenchComp == null)
            {
                //Log.Warning("Trying to load comp");
                CompInit();
            }

            ticksLeft--;
            //Log.Warning(parent.Label + " tick " + ticksLeft);

            bool gottaPanic = false;

            if (Time2Release())
            {
                //Log.Warning("Natural end");
                gottaPanic = true;
            }

            if (NullInitiator()) {
                //Log.Warning("no master");
                gottaPanic = true;
            }
            if (NullVictim()){
                Log.Warning("no victim how the f*");
                gottaPanic = true;
            }

            if (NullBench())
            {
                Log.Warning("no bench");
                gottaPanic = true;
            }

            if (gottaPanic)
            {
                Release();
                Panic();
            }
        }

        //Faction own, Faction forced, int duration
        public bool Enslave()
        {
            if (!NullCheck("enslave"))
                return false;

            if (parent.pawn.Faction == null)
            {
                Log.Warning("Wtf null faction");
            }

            //if (parent.pawn.Faction != BenchComp.GetFactionBackup())
            if (parent.pawn.Faction == Faction.OfPlayer)
            {
                Log.Warning("Tried to enslave²");
                return false;
            }
            /*
            else
            {
                Log.Warning("Enslave " + this.parent.pawn.Label + ":" + parent.pawn.Faction.Name + "=>" + Faction.OfPlayer.Name);
            }
            */
            originFaction = parent.pawn.Faction;
            originKindDefLabel = parent.pawn.KindLabel;
            originKindDef = parent.pawn.kindDef;

            parent.pawn.SetFaction(Faction.OfPlayer, Initiator);

            return true;
            
        }
        private bool Release()
        {
            if (!NullCheck("release"))
                return false;

            if (parent.pawn.Faction == null)
            {
                Log.Warning("Wtf null faction");
            }


            if (parent.pawn.Faction != Faction.OfPlayer)
            {
                Log.Warning("Tried to release²");
                return false;
            }
            /*
            else
            {
                Log.Warning(this.parent.Label + " : " + parent.pawn.Faction.Name + " => " + BenchComp.GetFactionBackup());
            }
            */

            //parent.pawn.mindState.mentalStateHandler.Reset();
            parent.pawn.SetFaction(BenchComp.GetFactionBackup(), null);
            return true;
        }
        private void Panic()
        {
            MentalStateDef result = null;

            if(Rand.Range(0,1) > .5f)
            {
                result = MentalStateDefOf.PanicFlee;
            }
            else
            {
                result = MentalStateDefOf.Berserk;
            }

            if (result == null)
            {
                Log.Warning("null panic");
                return;
            }

            parent.pawn.mindState.mentalStateHandler.TryStartMentalState(result, null, true, false, null);
        }
        

        private bool NullCheck( string functionCall)
        {
            if (parent == null) {
                Log.Warning(functionCall + " null comp ");
                return false;
            }
            
            if (parent.pawn == null)
            {
                Log.Warning(functionCall + " null pawn ");
                return false;
            }
            if (parent.pawn.Map == null)
            {
                Log.Warning(functionCall + " null map ");
                return false;
            }
            if (parent.pawn.Faction == null)
            {
                Log.Warning(functionCall + " null faction ");
                return false;
            }
            if (originFaction == null)
            {
                Log.Warning(functionCall + " null own ");
                return false;
            }

            if(Initiator == null)
            {
                Log.Warning(functionCall + " null initaor ");
                return false;
            }

            return true;
        }
        private void CompInit()
        {
            BenchComp = MindControlBench.TryGetComp<Comp_LTF_MindControl>();
            if (BenchComp == null)
            {
                Log.Warning("no bnch comp found");
            }
        }
        public bool Init(Faction own, Pawn masterMind, Building bench, int duration)
        {
            
            if (this.parent == null)
            {
                Log.Warning("null comp Init");
                return false;
            }
            if (parent.pawn == null)
            {
                Log.Warning("null pawn Init");
                return false;
            }

            if (parent.pawn.Map == null)
            {
                Log.Warning("null map Init");
                return false;
            }

            //Faction
            Faction almostoriginFaction = null;
            //useless hax ?
            if (parent.pawn.Faction == null)
            {
                Log.Warning("null faction Init");
                almostoriginFaction = FactionUtility.DefaultFactionFrom(FactionDefOf.Tribe);
            }
            else
            {
                almostoriginFaction = own;
            }

            //Log.Warning("asking " + parent.Label + own.Name + "->" + forced.Name + "(" + duration + ")" + ("faction found : " + almostoriginFaction.Name);

            MindControlBench = bench;
            CompInit();

            originFaction = own;
            Initiator = masterMind;

            ticksLeft = duration;

            //Log.Warning("Init done, enslaving");
            return (Enslave());
        }

    }
}