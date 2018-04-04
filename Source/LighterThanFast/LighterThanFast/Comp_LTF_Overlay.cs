using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace LighterThanFast
{
    [StaticConstructorOnStartup]
    public class Comp_LTF_Overlay : ThingComp
    {
        private static readonly Graphic OverlayGraphic = GraphicDatabase.Get<Graphic_Flicker>("Things/Special/Loop", ShaderDatabase.TransparentPostLight, Vector2.one, Color.white);
                
        public CompProperties_LTF_Overlay Props
        {
            get
            {
                return (CompProperties_LTF_Overlay)this.props;
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();

            bool gottaDraw = true;

            Thing t = null;
            t = this.parent;
            if (t == null) { return; }

            if ( Props.flickable )
            {
                CompFlickable compFlickable = t.TryGetComp<CompFlickable>();
                if (compFlickable == null) { return; }
                if (! compFlickable.SwitchIsOn) gottaDraw = false;
            }

            if ( Props.refuelable )
            {
                CompRefuelable compRefuelable = t.TryGetComp<CompRefuelable>();
                if (compRefuelable == null) { return; }
                if (!compRefuelable.HasFuel) gottaDraw = false;
            }

            if(gottaDraw)
            {
                Vector3 drawPos = this.parent.DrawPos;
                drawPos.y += 0.046875f;
                Comp_LTF_Overlay.OverlayGraphic.Draw(drawPos, Rot4.North, this.parent, 0.5f);
            }
            
        }
    }
}