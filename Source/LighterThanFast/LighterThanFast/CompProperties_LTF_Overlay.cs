using System;
using UnityEngine;
using Verse;

namespace LighterThanFast
{
    public class CompProperties_LTF_Overlay : CompProperties
    {
        //public float fireSize = 1f;

        //public Vector3 offset;
        //public string path = "Things/Special/Fire";
        //public color Color.white; ConsoleColor.

        public bool flickable = false;
        public bool refuelable = false;
        

        public CompProperties_LTF_Overlay()
        {
            this.compClass = typeof(Comp_LTF_Overlay);
        }
    }
}