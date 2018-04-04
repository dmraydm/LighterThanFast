/*
 * Created by SharpDevelop.
 * User: Etienne
 * Date: 22/11/2017
 * Time: 16:43
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using Verse;

namespace LighterThanFast
{
	public class CompProperties_LTF_Spawner : CompProperties
	{
		public ThingDef thingToSpawn;

        public bool animalThing = false;
        public string animalName = "Chicken";
        public bool manHunter = false;

        public int spawnCount = 1;
		public IntRange spawnIntervalRange = new IntRange(100, 100);
		public int spawnMaxAdjacent = -1;
		public bool spawnForbidden;
		
		public string spawnVerb = "delivery";
		
		public CompProperties_LTF_Spawner()
		{
			this.compClass = typeof(Comp_LTF_Spawner);
		}
	}
}
