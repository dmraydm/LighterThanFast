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
    // Main
    public class HediffComp_LTF_FactionChange : HediffComp
    {
        private int ticksLeft = 0;

        public Faction ownFaction = null;
        public Faction forcedFaction = null;
        public Pawn Initiator = null;
        public Building MindControlBench = null;
        public Comp_LTF_MindControl BenchComp = null;

        public HediffCompProperties_LTF_FactionChange Props
        {
            get
            {
                return (HediffCompProperties_LTF_FactionChange)this.props;
            }
        }

        public override bool CompShouldRemove
        {
            get
            {
                return base.CompShouldRemove || Time2Release() || NullInitiator() || NullVictim() || NullBench();
            }
        }

        private bool Time2Release()
        {
            bool test = (this.ticksLeft <= -5);
            //Log.Warning("release" + test);
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
            bool test = (MindControlBench == null || BenchComp == null || !BenchComp.GotThePower());
            //Log.Warning("bench" + test);
            return (test);
        }
        

        public override void CompPostMake()
        {
            base.CompPostMake();
            // no need maybe
            //this.ticksLeft = this.Props.defaultTicks.RandomInRange;
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
                if (ownFaction != null)
                {
                    if (this.ticksLeft > 0)
                    {
                        result = ticksLeft.ToStringTicksToPeriod(true, false, true) + " before "+ Initiator.Name +" release.\n";
                        result += parent.pawn.Name + " belongs to " + ownFaction.Name +".";
                    }
                }
                return result;
            }
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref ticksLeft, "LTF_SlaveTicks");

            Scribe_References.Look(ref ownFaction, "LTF_ownFaction");
            Scribe_References.Look(ref forcedFaction, "LTF_forcedFaction");

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

            ticksLeft--;
            //Log.Warning(parent.Label + " tick " + ticksLeft);

            if ((ticksLeft <= 0) || NullInitiator() || NullVictim() || NullBench())
            {
                //Log.Warning(parent.Label + " releasing ");
                Release();
                Panic();
            }
        }

        //Faction own, Faction forced, int duration
        public bool Enslave()
        {
            if (!NullCheck("enslave"))
                return false;

            //Log.Warning("Trying to enslave " + this.parent.pawn.Label);
            if (parent.pawn.Faction == forcedFaction)
            {
                Log.Warning("Tried to enslave²");
                return false;
            }

            parent.pawn.SetFaction(forcedFaction, Initiator);

            return true;
            
        }
        private void Release()
        {
            if (!NullCheck("release"))
                return;

            //  Log.Warning(this.parent.Label + " : " + ownFaction + " " + forcedFaction);
            if (parent.pawn.Faction != ownFaction)
            {
                Log.Warning("Tried to release²");
                return;
            }

            //parent.pawn.mindState.mentalStateHandler.Reset();
            

            parent.pawn.SetFaction(ownFaction, null);
            
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
            if (ownFaction == null)
            {
                Log.Warning(functionCall + " null own ");
                return false;
            }
            if (forcedFaction == null)
            {
                Log.Warning(functionCall + " null forced ");
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
            Faction almostOwnFaction = null;
            //useless hax ?
            if (parent.pawn.Faction == null)
            {
                Log.Warning("null faction Init");
                almostOwnFaction = FactionUtility.DefaultFactionFrom(FactionDefOf.Tribe);
            }
            else
            {
                almostOwnFaction = own;
            }

            //Log.Warning("asking " + parent.Label + own.Name + "->" + forced.Name + "(" + duration + ")" + ("faction found : " + almostOwnFaction.Name);

            MindControlBench = bench;
            CompInit();

            ownFaction = own;
            Initiator = masterMind;

            forcedFaction = Initiator.Faction;

            ticksLeft = duration;

            //Log.Warning("Init done, enslaving");
            return (Enslave());
        }

    }
}