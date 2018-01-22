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
        private int ticksLeft = 1000;
        //Pawn Slave = null;

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
                return base.CompShouldRemove || this.ticksLeft <= -5;
            }
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            this.ticksLeft = this.Props.defaultTicks.RandomInRange;
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

            if (ticksLeft <= 0)
            {
                //Log.Warning(parent.Label + " releasing ");
                Release();
            }
        }

        //Faction own, Faction forced, int duration
        public bool Enslave()
        {
            if (!NullCheck("enslave"))
                return false;

            //Log.Warning("Trying to enslave " + this.parent.pawn.Label);
            parent.pawn.SetFaction(Props.forcedFaction);
            //Log.Warning("Enslaving Ok");
            return true;
            
        }
        private void Release()
        {
            if (!NullCheck("release"))
                return;

            //  Log.Warning(this.parent.Label + " : " + Props.ownFaction + " " + Props.forcedFaction);
            parent.pawn.SetFaction(Props.ownFaction);

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
            if (Props.ownFaction == null)
            {
                Log.Warning(functionCall + " null own ");
                return false;
            }
            if (Props.forcedFaction == null)
            {
                Log.Warning(functionCall + " null forced ");
                return false;
            }

            return true;
        }



        public bool Init(Faction own, Faction forced, int duration)
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

            //Log.Warning("asking " + parent.Label + own.Name + "->" + forced.Name + "(" + duration + ")");

            Faction almostOwnFaction = null;
            if (parent.pawn.Faction == null)
            {
                Log.Warning("null faction Init");
                almostOwnFaction = FactionUtility.DefaultFactionFrom(FactionDefOf.Tribe);
            }
            else
            {
                almostOwnFaction = own;
            }

            //Log.Warning("faction found : " + almostOwnFaction.Name);

            Props.ownFaction = own;
            Props.forcedFaction = forced;
            ticksLeft = duration;

            //Log.Warning("Init done, enslaving");
            return (Enslave());
        }

    }
}