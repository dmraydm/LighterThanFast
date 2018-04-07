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
        public Pawn Victim = null;
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
            Tools.Warn("Remote debug: " + LTF_debug + "->" + !LTF_debug, debug);

            LTF_debug =! LTF_debug;
        }

        public override bool CompShouldRemove
        {
            get
            {
                return base.CompShouldRemove || Time2Release || NullInitiator() || NullVictim() || NullBench();
            }
        }

        public Faction SayFaction() => originFaction;
        public String SayKindDefLabel() => originKindDefLabel;
        public PawnKindDef SayKindDef() => originKindDef;


        private bool Time2Release
        {
            get
            {
                return (this.ticksLeft <= 0);
            }
        }
        private bool NullVictim()
        {
            bool test = (Victim == null || Victim.Map == null);
            //Tools.Warn("release" + test);
            return (test);
        }
        private bool NullInitiator()
        {
            bool test = (Initiator == null || Initiator.Dead || Initiator.Faction != Faction.OfPlayer || Initiator.Map == null);
            // Tools.Warn("initiator" + test);
            
            return (test);
        }
        private bool NullBench()
        {
            bool test = (MindControlBench == null || BenchComp == null || !BenchComp.GotThePower);
            //Tools.Warn("bench" + test);
            return (test);
        }


        public override void CompPostMake()
        {
            base.CompPostMake();
            
            if(Victim != parent.pawn)
            {
                Tools.Warn("Translating Init", LTF_debug);
                Victim = parent.pawn;
            }
                

            //victim init
            Tools.Warn("Cant find parent pawn victim", LTF_debug && (Victim == null));

            // bench init
            if (BenchComp == null) {
                if (MindControlBench == null)
                {
                    Tools.Warn("Cant find control bench", LTF_debug);
                } else BenchCompInit();
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
                        //result += Victim.LabelShort + 
                    }
                }

                if (LTF_debug)
                {
                    result += "\n[Debug]: ";

                    result += "; F(hasName"+ ((originFaction == null) ? ("null") : (originFaction.HasName.ToString())) + "):" + ((originFaction == null)?("null"):(originFaction.Name));

                    result += "; KL:" + ((originKindDefLabel == null) ? ("null") : (originKindDefLabel));
                    result += "; K:" + ((originKindDef == null) ? ("null") : (originKindDef.defName));
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
            Scribe_References.Look(ref Victim, "LTF_victim");
            Scribe_References.Look(ref MindControlBench, "LTF_MindControlBench");
            
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (this.parent == null)
            {
                Tools.Warn("Hediff cant find parent",LTF_debug);
                return;
            }
            if (Victim == null)
            {
                Tools.Warn("null victim", LTF_debug);
                return;
            }
            if(Victim.Map == null)
            {
                Tools.Warn(Victim.LabelShort + ":" + parent.Label + " null map", LTF_debug);
                return;
            }
            if (BenchComp == null)
            {
                Tools.Warn("Trying to load comp because it's null", LTF_debug);
                BenchCompInit();
            }

            ticksLeft--;
            Tools.WarnRare(parent.Label + " tick " + ticksLeft, 300, false);

            bool gottaPanic = false;

            if (Time2Release)
            {
                Tools.Warn("Natural end", LTF_debug);
                gottaPanic = true;
            }

            if (NullInitiator()) {
                Tools.Warn("no executioner, "+Victim.LabelShort+" will try to panic", LTF_debug);
                gottaPanic = true;
            }

            if (NullVictim()){
                Tools.Warn("no victim, should never happen", LTF_debug);
                gottaPanic = true;
            }

            if (NullBench())
            {
                Tools.Warn("no bench, did it get destroyed or has no power ?", LTF_debug);
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

            if (Victim.Faction == null)
            {
                Tools.Warn("Wtf null faction", LTF_debug);
            }

            //if (Victim.Faction != BenchComp.GetFactionBackup())
            if (Victim.Faction == Faction.OfPlayer)
            {
                Tools.Warn("Tried to enslave²", LTF_debug);
                return false;
            }
            /*
            else
            {
                Tools.Warn("Enslave " + this.Victim.Label + ":" + Victim.Faction.Name + "=>" + Faction.OfPlayer.Name);
            }
            */
            originFaction = Victim.Faction;
            originKindDefLabel = Victim.KindLabel;
            originKindDef = Victim.kindDef;

            Victim.SetFaction(Faction.OfPlayer, Initiator);

            return true;
            
        }
        private bool Release()
        {
            if (!NullCheck("release"))
                return false;

            if (Victim.Faction == null)
            {
                Tools.Warn("Wtf null faction", LTF_debug);
            }

            if (Victim.Faction != Faction.OfPlayer)
            {
                Tools.Warn("Tried to release²", LTF_debug);
                return false;
            }
            /*
            else
            {
                Tools.Warn(this.parent.Label + " : " + Victim.Faction.Name + " => " + BenchComp.GetFactionBackup());
            }
            */

            //Victim.mindState.mentalStateHandler.Reset();
            Victim.SetFaction(BenchComp.GetFactionBackup(), null);
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
                Tools.Warn("null panic");
                return;
            }

            Victim.mindState.mentalStateHandler.TryStartMentalState(result, null, true, false, null);
        }
        

        private bool NullCheck( string functionCall)
        {
            if (parent == null) {
                Tools.Warn(functionCall + " null comp ", LTF_debug);
                return false;
            }
            
            if (Victim == null)
            {
                Tools.Warn(functionCall + " null pawn ", LTF_debug);
                return false;
            }
            if (Victim.Map == null)
            {
                Tools.Warn(functionCall + " null map ", LTF_debug);
                return false;
            }
            if (Victim.Faction == null)
            {
                Tools.Warn(functionCall + " null faction ", LTF_debug);
                return false;
            }
            if (originFaction == null)
            {
                Tools.Warn(functionCall + " null own ", LTF_debug);
                return false;
            }

            if(Initiator == null)
            {
                Tools.Warn(functionCall + " null initaor ", LTF_debug);
                return false;
            }

            return true;
        }
        private void BenchCompInit()
        {
            BenchComp = MindControlBench.TryGetComp<Comp_LTF_MindControl>();
            if (BenchComp == null)
            {
                Tools.Warn("no bnch comp found", LTF_debug);
            }
        }
        public bool Init(Faction own, Pawn masterMind, Building bench, int duration)
        {
            
            if ((this.parent == null) || (parent.pawn == null))
            {
                Tools.Warn("null comp Init", LTF_debug);
                return false;
            }
            Victim = parent.pawn;

            if (Victim == null)
            {
                Tools.Warn("null pawn Init", LTF_debug);
                return false;
            }

            if (Victim.Map == null)
            {
                Tools.Warn("null map Init", LTF_debug);
                return false;
            }

            //Faction
            Faction almostoriginFaction = null;
            //useless hax ?
            if (Victim.Faction == null)
            {
                Tools.Warn("null faction Init");
                almostoriginFaction = FactionUtility.DefaultFactionFrom(FactionDefOf.Tribe);
            }
            else
            {
                almostoriginFaction = own;
            }

            //Tools.Warn("asking " + parent.Label + own.Name + "->" + forced.Name + "(" + duration + ")" + ("faction found : " + almostoriginFaction.Name);

            MindControlBench = bench;
            BenchCompInit();

            originFaction = own;
            Initiator = masterMind;

            ticksLeft = duration;

            //Tools.Warn("Init done, enslaving");
            return (Enslave());
        }

    }
}