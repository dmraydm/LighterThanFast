using System;
using UnityEngine;
using Verse;


namespace LighterThanFast
{
    [StaticConstructorOnStartup]
    public class Gfx
    {
        // >>>>>>>>>>> **Mind control bench**
        //  **** Materials path ****
        private static string BuildingPath = "Things/Building/";
        private static string MindPath = BuildingPath + "MindcontrolBench/";
        private static string OverlayDir = "Overlay/";
        private static string MindOverlay = MindPath + OverlayDir;

        // GFX
        public static readonly Material femaleMat = MaterialPool.MatFrom(MindOverlay+"gender/female", ShaderDatabase.Transparent);
        public static readonly Material maleMat = MaterialPool.MatFrom(MindOverlay+"gender/male", ShaderDatabase.Transparent);

        public static readonly Material animalMat = MaterialPool.MatFrom(MindOverlay+"nature/animal", ShaderDatabase.Transparent);
        public static readonly Material humanMat = MaterialPool.MatFrom(MindOverlay+"nature/human", ShaderDatabase.Transparent);
        public static readonly Material alienMat = MaterialPool.MatFrom(MindOverlay+"nature/alien", ShaderDatabase.Transparent);

        public static readonly Material empathyMat = MaterialPool.MatFrom(MindOverlay+"leverage/empathy", ShaderDatabase.Transparent);
        public static readonly Material manipulationMat = MaterialPool.MatFrom(MindOverlay+"leverage/manipulation", ShaderDatabase.Transparent);
        public static readonly Material ascensionMat = MaterialPool.MatFrom(MindOverlay+"leverage/ascension", ShaderDatabase.Transparent);

        public static readonly Material controlMat = MaterialPool.MatFrom(MindOverlay+"effect/control", ShaderDatabase.Transparent);
        public static readonly Material foolMat = MaterialPool.MatFrom(MindOverlay+"effect/fool", ShaderDatabase.Transparent);
        public static readonly Material selfharmMat = MaterialPool.MatFrom(MindOverlay+"effect/selfharm", ShaderDatabase.Transparent);

        public static readonly Material lockMat = MaterialPool.MatFrom(MindOverlay+"target/lock", ShaderDatabase.Transparent);
        public static readonly Material notInRangeMat = MaterialPool.MatFrom(MindOverlay+"target/notInRange", ShaderDatabase.Transparent);

        public static readonly Material relationMat = MaterialPool.MatFrom(MindOverlay+"relation", ShaderDatabase.Transparent);

        public static readonly Material powerMat = MaterialPool.MatFrom(MindOverlay+"powerOn", ShaderDatabase.Transparent);
        public static readonly Material clockworkMat = MaterialPool.MatFrom(MindOverlay+"work", ShaderDatabase.Transparent);
        public static readonly Material readyMat = MaterialPool.MatFrom(MindOverlay+"ready", ShaderDatabase.Transparent);

        //Bars
        public static readonly Material workBarSMat = MaterialPool.MatFrom(MindOverlay+"bar/workBarS", ShaderDatabase.Transparent);
        public static readonly Material workBarMMat = MaterialPool.MatFrom(MindOverlay+"bar/workBarM", ShaderDatabase.Transparent);
        public static readonly Material workBarLMat = MaterialPool.MatFrom(MindOverlay+"bar/workBarL", ShaderDatabase.Transparent);

        public static readonly Material powerBarMat = MaterialPool.MatFrom(MindOverlay+"bar/powerBar", ShaderDatabase.Transparent);

        public const int workBarNum = 27;
        public const int powerBarNum = 10;


        //Gizmo
        private static string GizmoPath = "UI/Commands/";
        private static string DebugPath = GizmoPath + "Debug/";
        private static string HaxPath = GizmoPath + "Hax/";
        private static string BenchPath = GizmoPath + "Mind/";

        public static Texture2D DebugMindGz = ContentFinder<Texture2D>.Get(DebugPath+"DebugMind", true);
        public static Texture2D DebugOnGz = ContentFinder<Texture2D>.Get(DebugPath + "DebugOn", true);
        public static Texture2D DebugOffGz = ContentFinder<Texture2D>.Get(DebugPath + "DebugOff", true);

        public static Texture2D HaxAddGz = ContentFinder<Texture2D>.Get(HaxPath + "HaxAdd", true);
        public static Texture2D HaxSubGz = ContentFinder<Texture2D>.Get(HaxPath + "HaxSub", true);
        public static Texture2D HaxFullGz = ContentFinder<Texture2D>.Get(HaxPath + "HaxFull", true);
        public static Texture2D HaxEmptyGz = ContentFinder<Texture2D>.Get(HaxPath + "HaxEmpty", true);
        public static Texture2D HaxWorseGz = ContentFinder<Texture2D>.Get(HaxPath + "HaxWorse", true);
        public static Texture2D HaxBetterGz = ContentFinder<Texture2D>.Get(HaxPath + "HaxBetter", true);

        public static Texture2D MindFactionChangeGz = ContentFinder<Texture2D>.Get(BenchPath + "FactionChange", true);
        public static Texture2D MindManhunterGz = ContentFinder<Texture2D>.Get(BenchPath + "Manhunter", true);
        public static Texture2D MindCancelGz = ContentFinder<Texture2D>.Get(BenchPath + "MindCancel", true);
        public static Texture2D MindLogGz = ContentFinder<Texture2D>.Get(BenchPath + "MindLog", true);

        //
        public static void PulseWarning     (Thing thing, Material mat, Vector3 drawPos, Mesh mesh)
        {
            float num = (Time.realtimeSinceStartup + 397f * (float)(thing.thingIDNumber % 571)) * 4f;
            float num2 = ((float)Math.Sin((double)num) + 1f) * 0.5f;
            num2 = 0.3f + num2 * 0.7f;
            Material material = FadedMaterialPool.FadedVersionOf(mat, num2);

            Vector3 dotS = new Vector3(.6f, 1f, .6f);
            Matrix4x4 matrix = default(Matrix4x4);

            matrix.SetTRS(drawPos, Quaternion.AngleAxis(0f, Vector3.up), dotS);

            Graphics.DrawMesh(mesh, matrix, material, 0);
        }
        public static void DrawDot(Vector3 benchPos, Vector3 dotSize, Material dotGfx, float x, float z)
        {
            Vector3 dotPos = benchPos;
            dotPos.x += x;
            dotPos.z += z;

            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(dotPos, Quaternion.AngleAxis(0f, Vector3.up), dotSize);

            Graphics.DrawMesh(MeshPool.plane14, matrix, dotGfx, 0);
        }
        public static void Draw1x1(Vector3 benchPos, Vector3 dotSize, Material dotGfx, float x, float z)
        {
            Vector3 dotPos = benchPos;
            dotPos.x += x;
            dotPos.z += z;

            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(dotPos, Quaternion.AngleAxis(0f, Vector3.up), dotSize);

            Graphics.DrawMesh(MeshPool.plane10, matrix, dotGfx, 0);
        }

        /*
        public static void DrawDot          (Vector3 benchPos, Material dotM, Mesh mesh, float x, float z, Thing bench, bool pulse)
        {
            Vector3 dotPos = benchPos;
            dotPos.x += x;
            dotPos.z += z;

            Material material = dotM;

            if (pulse)
            {
                float num = (Time.realtimeSinceStartup + 397f * (float)(bench.thingIDNumber % 571)) * 4f;
                float num2 = ((float)Math.Sin((double)num) + 1f) * 0.5f;
                num2 = 0.3f + num2 * 0.7f;
                material = FadedMaterialPool.FadedVersionOf(dotM, num2);
            }
            Vector3 dotS = new Vector3(.32f, 1f, .29f);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(dotPos, Quaternion.AngleAxis(0f, Vector3.up), dotS);
            //            Graphics.DrawMesh(mesh, benchPos, matrix, dotM, 0);
            Graphics.DrawMesh(mesh, matrix, material, 0);
        }
        public static void DrawDot          (Vector3 benchPos, Vector3 dotS, Matrix4x4 matrix, Material dotM, float x, float z)
        {
            Vector3 dotPos = benchPos;
            dotPos.x += x;
            dotPos.z += z;
            matrix.SetTRS(dotPos, Quaternion.AngleAxis(0f, Vector3.up), dotS);
            if (MeshPool.plane14 == null)
            {
                Log.Warning("14 null");
                return;
            }

            Graphics.DrawMesh(MeshPool.plane14, matrix, dotM, 0);
        }
        public static void DrawDot(Vector3 benchPos, Material dotGfx, Mesh mesh, Thing bench, float x, float z, bool pulse=false, float width = 1, float height = 1)
        {
            Vector3 dotPos = benchPos;
            dotPos.x += x;
            dotPos.z += z;

            Material material = dotGfx;
            
            if (pulse)
            {
                float opacity = PulseOpacity(bench);
                material = FadedMaterialPool.FadedVersionOf(dotGfx, opacity);
            }
            Vector3 dotS = new Vector3(width, 1f, height);
            Matrix4x4 matrix = default(Matrix4x4);

            matrix.SetTRS(dotPos, Quaternion.AngleAxis(0f, Vector3.up), dotS);
            Graphics.DrawMesh(mesh, matrix, material, 0);
        }
        */

        public static void DrawBenchSize(Vector3 benchPos, Material dotGfx, Thing bench, float x=0, float z=0)
        {
            Vector3 benchSize = new Vector3(bench.def.size.x, 1f, bench.def.size.z);
            Draw1x1(benchPos, benchSize, dotGfx, x, z);
        }

        public static void DrawBar(Vector3 benchPos, float x, float z, int i)
        {
            float zOffset = 0f;

            Material wBarMat = null;
            Mesh mesh = MeshPool.plane10;
            if (i < 8)
            {
                wBarMat = workBarSMat;
            }
            else if (i < 21)
            {
                wBarMat = workBarMMat;
                zOffset += .013f;
            }
            else
            {
                wBarMat = workBarLMat;
                zOffset += .02f;
            }

            float myX = -1.145f + (i * .0825f);
            float myY = .562f + zOffset;

            Tools.Warn("wBar x;y: "+ myX+ ";"+ myY , false);
            FlickerBar(benchPos, wBarMat, mesh, myX, myY);
        }
        public static void FlickerBar(Vector3 benchPos, Material dotM, Mesh mesh, float x, float z)
        {
            Vector3 dotPos = benchPos;
            dotPos.x += x;
            dotPos.z += z;

            Material fMat = dotM;
            if (Rand.Chance(0.85f))
                fMat = FadedMaterialPool.FadedVersionOf(dotM, .65f);

            Vector3 barS = new Vector3(.35f, 1f, .275f);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(dotPos, Quaternion.AngleAxis(0f, Vector3.up), barS);

            Graphics.DrawMesh(mesh, matrix, fMat, 0);
        }

        public static float PulseOpacity(Thing thing)
        {
            float num = (Time.realtimeSinceStartup + 397f * (float)(thing.thingIDNumber % 571)) * 4f;
            float num2 = ((float)Math.Sin((double)num) + 1f) * 0.5f;
            num2 = 0.3f + num2 * 0.7f;

            return num2;
        }
    }
}
