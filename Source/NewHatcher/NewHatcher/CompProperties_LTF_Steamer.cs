/*
 * Created by SharpDevelop.
 * User: Etienne
 * Date: 22/11/2017
 * Time: 16:43
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using Verse;

namespace NewHatcher
{
    public class CompProperties_LTF_Steamer : CompProperties
    {

        public int MinTicksBetweenSprays = 200;
        public int MaxTicksBetweenSprays = 400;

        public int MinSprayDuration = 60;
        public int MaxSprayDuration = 120;

        public float SprayThickness = 0.6f;

        public CompProperties_LTF_Steamer()
        {
            this.compClass = typeof(Comp_LTF_Steamer);
        }
    }
}