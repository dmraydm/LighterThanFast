using RimWorld;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;


namespace LighterThanFast
{
    public class Tools
    {

        public static string OkStr(bool boolean=false)
        {
            return "[" + ((boolean) ? ("OK") : ("KO")) + "]";
        }

        public static void Warn(string warning, bool debug = false)
        {
            if(debug)
                Log.Warning(warning);
        }
        public static void WarnRare(string warning, int period=300, bool debug = false)
        {
            if (debug)
            {
                bool display = ((Find.TickManager.TicksGame % period)==0);
                if (display)
                    Log.Warning(warning);
            }
        }

        //Quality
        public static bool BestQuality(CompQuality compQuality)
        {
            if (compQuality == null)
                return false;
            return (compQuality.Quality == QualityCategory.Legendary);
        }
        public static bool WorstQuality(CompQuality compQuality)
        {
            if (compQuality == null)
                return false;
            return (compQuality.Quality == QualityCategory.Awful);
        }
        public static string BetterQuality(CompQuality comp)
        {
            return (VirtualQuality(comp, 1));
        }
        public static string WorseQuality(CompQuality comp)
        {
            return (VirtualQuality(comp, -1));
        }
        public static string VirtualQuality(CompQuality comp, int relativeChange = 0)
        {
            string answer = "no quality comp";
            if (comp != null)
                answer = comp.Quality.AddLevels(relativeChange).GetLabelShort();

            return (answer);
        }

        public static string PawnResumeString(Pawn pawn)
        {
            return (pawn?.LabelShort.CapitalizeFirst() +
                    ", " +
                    (int)pawn?.ageTracker?.AgeBiologicalYears + " y/o" +
                    " " + pawn?.gender.ToString()?.Translate()?.ToLower() +
                    ", " + pawn?.def?.label + "("+pawn.kindDef+")"
                    );
        }

        //debug Toggle kinda pointless
        public static string DebugStatus(bool debug)
        {
            return (debug+ "->" + !debug);
        }

        //PauseOnError for debug purpose
        public static void PauseOnErrorToggle()
        {
            Prefs.PauseOnError = !Prefs.PauseOnError;
        }

    }
}
