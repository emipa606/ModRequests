using RimWorld;
using System.Collections.Generic;
using System;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;

namespace VPEArchocaster
{
    public static class VPEACUtils
    {

        

        public static float getTotalXP(this Pawn pawn)
        {
            float xp = 0f;
            for(int i = pawn.Psycasts().level-pawn.Psycasts().points+1; i <= pawn.Psycasts().level; i++)
            {
                xp += Hediff_PsycastAbilities.ExperienceRequiredForLevel(i);
            }
            xp += pawn.Psycasts().experience;
            return xp;
        }
       


        public static void applySettings()
        {
            VPEArchocasterMod_Settings.maxThrallLearningMultiplier = Math.Max(VPEArchocasterMod_Settings.allowThrallsLearning * 2, 0.2f);
            if ((VPEArchocasterMod_Settings.allowThrallsLearning > float.Epsilon) && !
                (VPEArchocasterMod_Settings.allowThrallsLearning + 0.001 > 1 &&
                VPEArchocasterMod_Settings.allowThrallsLearning - 0.001 < 1))
            {
                applyThrallPatch();
            } else
            {
                removeThrallPatch();
            }
        }



        public static void removeThrallPatch()
        {
            TraitDef thrall = DefDatabase<TraitDef>.GetNamed("VPE_Thrall");
            foreach (TraitDegreeData i in thrall.degreeDatas)
            {
                if (i.label == null || i.label != "thrall")
                {
                    continue;
                }
                if (i.statFactors == null)
                {
                    i.statFactors = new List<StatModifier>();
                }


                foreach (StatModifier j in i.statFactors)
                {
                    if (j.stat == StatDefOf.GlobalLearningFactor)
                    {
                        i.statFactors.Remove(j);
                    }
                }
            }
        }
        public static void applyThrallPatch()
        {
            TraitDef thrall = DefDatabase<TraitDef>.GetNamed("VPE_Thrall");
            foreach (TraitDegreeData i in thrall.degreeDatas)
            {
                if (i.label == null || i.label != "thrall")
                {
                    continue;
                }
                if (i.statFactors == null)
                {
                    i.statFactors = new List<StatModifier>();
                }

                get_thrallLearning(i.statFactors).value = VPEArchocasterMod_Settings.allowThrallsLearning;

            }
        }


        public static StatModifier get_thrallLearning(List<StatModifier> data)
        {
            StatModifier res = null;
            foreach (StatModifier i in data)
            {
                if (i.stat == StatDefOf.GlobalLearningFactor)
                {
                    res = i;
                    break;
                }
            }
            if (res == null)
            {
                res = new StatModifier();
                res.stat = StatDefOf.GlobalLearningFactor;
                data.Add(res);
            }

            return res;
        }



















    }
}
