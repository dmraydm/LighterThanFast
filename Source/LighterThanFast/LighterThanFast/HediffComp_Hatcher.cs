/*
 * Created by SharpDevelop.
 * User: Etienne
 * Date: 19/11/2017
 * Time: 14:22
 * 
 * Zoltan Eggs
 */
using System;
using Verse;
using RimWorld;

namespace LighterThanFast
{
    public class HediffComp_Hatcher : HediffComp
    {
        private int HatchingTicker = 0;
        public HediffCompProperties_Hatcher Props
        {
            get
            {
                return (HediffCompProperties_Hatcher)this.props;
            }
        }
        public override void CompPostTick(ref float severityAdjustment)
        {
            Hatch();
        }
        public void Hatch()
        {
            if (HatchingTicker < (this.Props.hatcherDaystoHatch*60000)) {
                HatchingTicker += 1;
            } else
            {
                if (this.parent.pawn.Map != null) {

                    //GenSpawn.Spawn(ThingDef.Named("EggChickenUnfertilized"), this.parent.pawn.Position, this.parent.pawn.Map);
                    //GenPlace.TryPlaceThing(ThingDef.Named(this.Props.thingToHatch), this.parent.pawn.Position, this.parent.pawn.Map, ThingPlaceMode.Near, null);
                    //Messages.Message(this.Def.label, MessageSound.Standard);
                    GenSpawn.Spawn(ThingDef.Named(this.Props.thingToHatch), this.parent.pawn.Position, this.parent.pawn.Map);
                    Log.Warning( this.parent.pawn.Label + " poping " + this.Props.thingToHatch );
                }
                HatchingTicker = 0;
                //this.parent.pawn.Position.
            }
            //this.parent.Destroy(DestroyMode.Vanish);
        }
    }
    
   
}
