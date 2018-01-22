/*
 * Created by SharpDevelop.
 * User: Etienne
 * Date: 22/11/2017
 * Time: 16:41
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using Verse;
using RimWorld;


namespace NewHatcher
{
public class Comp_LTF_Spawner : ThingComp
	{
		private int ticksUntilSpawn;

		public CompProperties_LTF_Spawner Props
		{
			get
			{
				return (CompProperties_LTF_Spawner)this.props;
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!respawningAfterLoad)
			{
				this.ReleaseHoldingOn();
			}
		}

		public override void CompTick()
		{
			if (this.parent.Map == null )
			{
				return;
			}
			this.ticksUntilSpawn--;
			
			this.CheckShouldSpawn();
		}

		public override void CompTickRare()
		{
			if (this.parent.Map == null)
			{
				return;
			}
			this.ticksUntilSpawn -= 250;
			this.CheckShouldSpawn();
		}

		private void CheckShouldSpawn()
		{
			int ticksIn = this.ticksUntilSpawn;
			int ticksOut = 0;
			//Log.Warning( "Checking");
			if (this.ticksUntilSpawn <= 0)
			{
				//Log.Warning( pawn.Label + " tries to spawn");
				this.TryDoSpawn();
				this.ReleaseHoldingOn();
				ticksOut = this.ticksUntilSpawn;
				//Log.Warning( pawn.Label + " : " + ticksIn + " -> " + ticksOut);
			}
		}

		public bool TryDoSpawn()
		{
			if (this.Props.spawnMaxAdjacent >= 0)
			{
				int num = 0;
				for (int i = 0; i < 9; i++)
				{
					List<Thing> thingList = (this.parent.Position + GenAdj.AdjacentCellsAndInside[i]).GetThingList(this.parent.Map);
					for (int j = 0; j < thingList.Count; j++)
					{
						if (thingList[j].def == this.Props.thingToSpawn)
						{
							num += thingList[j].stackCount;
							if (num >= this.Props.spawnMaxAdjacent)
							{
								return false;
							}
						}
					}
				}
			}
			IntVec3 center;
			if (this.TryFindSpawnCell(out center))
			{
                if (this.Props.animalThing)
                {
                    if (this.Props.animalName.Equals("Megascarab"))
                    {
                        //PawnKindDef.
                        Pawn insect = PawnGenerator.GeneratePawn(PawnKindDefOf.Megascarab, null);
                        GenSpawn.Spawn(insect, CellFinder.RandomClosewalkCellNear(center, this.parent.Map, 4, null), this.parent.Map);
                        return true;
                    }
                    else
                    {
                        // exception si pas de pawnKindDef
                        //Pawn animal = PawnGenerator.GeneratePawn(PawnKindDef.Named("Chicken"), this.parent.Faction);
                        Pawn animal = PawnGenerator.GeneratePawn(PawnKindDef.Named( this.Props.animalName ), this.parent.Faction);
                        GenSpawn.Spawn(animal, CellFinder.RandomClosewalkCellNear(center, this.parent.Map, 4, null), this.parent.Map);
                        if (Props.manHunter)
                        {
                            animal.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, null, false, false, null);
                        }

                        /*
                        Pawn animal = PawnGenerator.GeneratePawn(PawnKindDefOf.Thrumbo, null);
                        GenSpawn.Spawn(animal, CellFinder.RandomClosewalkCellNear(center, this.parent.Map, 4, null), this.parent.Map);
                        */
                        return true;
                    }

                }
                else
                {
                    Thing thing = ThingMaker.MakeThing(this.Props.thingToSpawn, null);
                    thing.stackCount = this.Props.spawnCount;
                    Thing t;
                    GenPlace.TryPlaceThing(thing, center, this.parent.Map, ThingPlaceMode.Direct, out t, null);
                    if (this.Props.spawnForbidden)
                    {
                        t.SetForbidden(true, true);
                    }
                }
				return true;
			}
			return false;
		}

		private bool TryFindSpawnCell(out IntVec3 result)
		{
			foreach (IntVec3 current in GenAdj.CellsAdjacent8Way(this.parent).InRandomOrder(null))
			{
				if (current.Walkable(this.parent.Map))
				{
					Building edifice = current.GetEdifice(this.parent.Map);
					if (edifice == null || !this.Props.thingToSpawn.IsEdifice())
					{
						Building_Door building_Door = edifice as Building_Door;
						if (building_Door == null || building_Door.FreePassage)
						{
							if (GenSight.LineOfSight(this.parent.Position, current, this.parent.Map, false, null, 0, 0))
							{
								bool flag = false;
								List<Thing> thingList = current.GetThingList(this.parent.Map);
								for (int i = 0; i < thingList.Count; i++)
								{
									Thing thing = thingList[i];
									if (thing.def.category == ThingCategory.Item && (thing.def != this.Props.thingToSpawn || thing.stackCount > this.Props.thingToSpawn.stackLimit - this.Props.spawnCount))
									{
										flag = true;
										break;
									}
								}
								if (!flag)
								{
									result = current;
									return true;
								}
							}
						}
					}
				}
			}
			result = IntVec3.Invalid;
			return false;
		}

		private void ReleaseHoldingOn()
		{
			this.ticksUntilSpawn += this.Props.spawnIntervalRange.RandomInRange;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			//Scribe_References.Look<Pawn>(ref this.ticksUntilSpawn, "father", false);
			Scribe_Values.Look<int>(ref this.ticksUntilSpawn, "ticksUntilSpawn " , 0, false);
		}
		
		
		public override string CompInspectStringExtra()
		{
			string text = base.CompInspectStringExtra();
			string result = string.Empty;
			int num = this.ticksUntilSpawn % 60000;
						
			if (num > 0)
			{
                result = num.ToStringTicksToPeriod(true, false, true) + " before ";
                if (this.Props.animalThing)
                {
                    result += this.Props.animalName;
                }
                else
                {
                    result += this.Props.thingToSpawn.label;
                }
                result += " " + this.Props.spawnVerb;
            }

			if (!text.NullOrEmpty())
			{
			result = "\n" + text;
			}

			return result;
		}
	}
}
