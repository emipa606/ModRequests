using HarmonyLib;
using RimWorld;
using RT_SolarFlareShield;
using System.Linq;
using UnityEngine;
using Verse;
using Resources = RT_SolarFlareShield.Resources;

namespace VSSR
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            new Harmony("VSSRMod").PatchAll();
        }
    }

    [HarmonyPatch(typeof(CompRTSolarFlareShield), "PostDraw")]
    public static class CompRTSolarFlareShield_PostDraw_Patch
    {
        public static bool Prefix(CompRTSolarFlareShield __instance)
        {
            Vector3 val = new(2f, 2f, 2f);
            val.y = AltitudeLayer.VisEffects.AltitudeFor();
            Matrix4x4 val2 = default(Matrix4x4);
            val2.SetTRS(__instance.parent.DrawPos + Altitudes.AltIncVect + new Vector3(0, 0, 0.3f), Quaternion.AngleAxis(__instance.rotatorAngle, Vector3.up), val);
            Graphics.DrawMesh(MeshPool.plane10, val2, Resources.rotatorTexture, 0);
            return false;
        }
    }
}
