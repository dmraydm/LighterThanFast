/*
 * Created by SharpDevelop.
 * User: Gouda
 * Date: 22/01/2018
 * Time: 14:22
 * 
 * changefaction
 */

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
    public class HediffCompProperties_LTF_FactionChange : HediffCompProperties
    {
        public Faction ownFaction;
        public Faction forcedFaction;

        public IntRange defaultTicks = default(IntRange);

        public bool manHunter = false;
		
        public HediffCompProperties_LTF_FactionChange()
        {
            this.compClass = typeof(HediffComp_LTF_FactionChange);
        }
    }
}