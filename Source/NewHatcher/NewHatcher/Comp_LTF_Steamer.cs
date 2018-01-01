using Verse;
using System;
using RimWorld;

namespace NewHatcher
{
    public class Comp_LTF_Steamer : ThingComp
    {
        private Thing steamEmitter;

        private int ticksUntilSpray = 500;

        private int sprayTicksLeft;

        public Action startSprayCallback;

        public Action endSprayCallback;

        public CompProperties_LTF_Steamer Props
        {
            get
            {
                return (CompProperties_LTF_Steamer)this.props;
            }
        }

        public override void CompTickRare()
        {
            steamEmitter = this.parent;
            //Log.Warning(steamEmitter.Label + " tick = " + this.sprayTicksLeft);

            if (steamEmitter == null)
            {
                return;
            }

            if (this.sprayTicksLeft > 0)
            {
                //this.sprayTicksLeft--;
                this.sprayTicksLeft -= 250;
                if (Rand.Value < 1f)
                {
                    //Log.Warning("Puffing");
                    MoteMaker.ThrowAirPuffUp(steamEmitter.TrueCenter(), steamEmitter.Map);

                }
                if (Find.TickManager.TicksGame % 20 == 0)
                {
                    GenTemperature.PushHeat(steamEmitter, 40f);
                }
                if (this.sprayTicksLeft <= 0)
                {
                    if (this.endSprayCallback != null)
                    {
                        this.endSprayCallback();
                    }
                    this.ticksUntilSpray = Rand.RangeInclusive(500, 2000);
                }
            }
            else
            {
                this.ticksUntilSpray-=250;
                if (this.ticksUntilSpray <= 0)
                {
                    if (this.startSprayCallback != null)
                    {
                        this.startSprayCallback();
                    }
                    this.sprayTicksLeft = Rand.RangeInclusive(200, 500);
                }
            }
        }
    }
}
