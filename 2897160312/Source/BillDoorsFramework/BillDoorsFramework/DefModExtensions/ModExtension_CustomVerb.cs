using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class ModExtension_LGISdualModeNPCpatch : DefModExtension
    {
        public float chance = 1f;
    }

    public class ModExtension_FireAllAtOnceCE : DefModExtension
    {
        public int barrelCount = 1;
    }

    public class ModExtension_RandomBurstBreak : DefModExtension
    {
        public float chance = 0.08f;
        public IntRange randomBurst = new IntRange(0, 0);
    }

    public class ModExtension_Verb_Shotgun : DefModExtension
    {
        public int ShotgunPellets = 1;
        public ThingDef extraProjectile;
        public int extraProjectileCount = 1;
    }
    public class ModExtension_DropItemWhenFire : DefModExtension
    {
        public ThingDef Thingdef;
        public bool alwaysOnGround;
    }

    public class ModExtension_MultiShot : DefModExtension
    {
        public int shotCount;
    }

    public class ModExtension_OneUse : DefModExtension
    {
        public bool tryFindNewWeapon;
        public bool generateSidearm;
    }

    public class PawnKindModExtension_Sidearm : DefModExtension
    {
        public ThingDef weaponDef;
    }

    public class ModExtension_ProjOriginOffset : DefModExtension
    {
        public List<Vector2> offsets = new List<Vector2>();

        public Vector2 GetOffsetFor(int index)
        {
            if (offsets.NullOrEmpty()) return Vector2.zero;
            int i = index % offsets.Count;
            return offsets[i];
        }
    }

    [StaticConstructorOnStartup]
    public static class LGISdualModeNPCpatch
    {
        [HarmonyPatch(typeof(PawnGenerator), "PostProcessGeneratedGear", new Type[] { typeof(Thing), typeof(Pawn) })]
        static class ProjectilesSpawn_PostFix
        {
            [HarmonyPostfix]
            static void Postfix(Thing gear)
            {
                CompSecondaryVerb comp = gear.TryGetComp<CompSecondaryVerb>();
                ModExtension_LGISdualModeNPCpatch extension = gear.def.GetModExtension<ModExtension_LGISdualModeNPCpatch>();
                if (extension != null && comp != null && Rand.Chance(extension.chance))
                {
                    comp.SwitchToSecondary();
                }
            }
        }
    }
}
