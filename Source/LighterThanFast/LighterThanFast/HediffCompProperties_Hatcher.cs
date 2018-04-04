/*
 * Created by SharpDevelop.
 * User: Etienne
 * Date: 19/11/2017
 * Time: 14:22
 * 
 * Zoltan Eggs
 */
 
using Verse;

namespace LighterThanFast
{
    public class HediffCompProperties_Hatcher : HediffCompProperties
    {

        public float hatcherDaystoHatch = 0.3f;
		public string thingToHatch = "LTF_SlugDew";
		
        public HediffCompProperties_Hatcher()
        {
            this.compClass = typeof(HediffComp_Hatcher);
        }
    }
}
