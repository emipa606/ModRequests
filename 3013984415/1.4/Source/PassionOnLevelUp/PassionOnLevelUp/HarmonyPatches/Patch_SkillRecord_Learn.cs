using HarmonyLib;
using RimWorld;
using Verse;

namespace PassionOnLevelUp.HarmonyPatches
{
    [HarmonyPatch(typeof(SkillRecord), "Learn")]
    public class GainPassion
    {
        private static Passion _apathyPassion = (Passion)3;
        private static Passion _naturalPassion = (Passion)4;
        private static Passion _criticalPassion = (Passion)5;

        private static PassionOnLevelUpModSettings settings = LoadedModManager.GetMod<PassionOnLevelUpMod>().GetSettings<PassionOnLevelUpModSettings>();
        
        public static bool Prefix (float xp, bool direct, ref SkillRecord __instance, ref int __state, ref Pawn ___pawn, ref SkillDef ___def)
        {
            __state = -1;
            for (int i = 0 ; i < ___pawn.skills.skills.Count ; i++)
            {
                if (___pawn.skills.skills[i].def == ___def)
                {
                    __state = ___pawn.skills.skills[i].levelInt;
                    return true;
                }
            }
            return true;
        }

        public static void Postfix (float xp, bool direct, ref SkillRecord __instance, ref int __state, ref Pawn ___pawn, ref SkillDef ___def)
        {
            if (___pawn == null || __instance == null || __state == null || ___def == null)
            {
                Log.Error("Something went wrong with Passion On Level Up");
                return;
            }
            
            if (__state == -1)
            {
                return;
            }
            for (int j = 0 ; j < ___pawn?.skills.skills.Count ; j++)
            {
                if (___pawn?.skills.skills[j].def != ___def)
                {
                    continue;
                }

                while (___pawn.skills.skills[j].levelInt > __state)
                {
                    Passion pawnSkillPassion = ___pawn.skills.skills[j].passion;
                    int pawnSkillLevel = ___pawn.skills.skills[j].levelInt;
                    int randomNumber = Verse.Rand.Range(0, 100);

                    if (settings?.chanceToGainCriticalPassion != null && pawnSkillPassion == _naturalPassion && (int)settings?.chanceToGainCriticalPassion > randomNumber && pawnSkillLevel >= (int)settings?.minSkillToGainCriticalPassion)
                    {
                        IncreasePassion(_criticalPassion, __instance, ___pawn, ___def);
                        //Log.Message("With a chance of " + (int)settings?.chanceToGainCriticalPassion);
                    }

                    if (settings?.chanceToGainNaturalPassion != null && pawnSkillPassion == Passion.Major && (int)settings?.chanceToGainNaturalPassion > randomNumber && pawnSkillLevel >= (int)settings?.minSkillToGainNaturalPassion)
                    {
                        IncreasePassion(_naturalPassion, __instance, ___pawn, ___def);
                        //Log.Message("With a chance of " + (int)settings?.chanceToGainNaturalPassion);
                    }

                    if (settings?.chanceToGainMajorPassion != null && pawnSkillPassion == Passion.Minor && (int)settings?.chanceToGainMajorPassion > randomNumber && pawnSkillLevel >= (int)settings?.minSkillToGainMajorPassion)
                    {
                        IncreasePassion(Passion.Major, __instance, ___pawn, ___def);
                        //Log.Message("With a chance of " + (int)settings?.chanceToGainMajorPassion);
                    }

                    if (settings?.chanceToGainMinorPassion != null && pawnSkillPassion == Passion.None && (int)settings?.chanceToGainMinorPassion > randomNumber && pawnSkillLevel >= (int)settings?.minSkillToGainMinorPassion)
                    {
                        IncreasePassion(Passion.Minor, __instance, ___pawn, ___def);
                        //Log.Message("With a chance of " + (int)settings?.chanceToGainMinorPassion);
                    }
                    
                    if (settings?.chanceToRemoveApathy != null && pawnSkillPassion == _apathyPassion && (int)settings?.chanceToRemoveApathy > randomNumber && pawnSkillLevel >= (int)settings?.minSkillToLoseApathy)
                    {
                        IncreasePassion(Passion.None, __instance, ___pawn, ___def);
                        //Log.Message("With a chance of " + (int)settings?.chanceToRemoveApathy);
                    }
                    __state++;
                    
                    //Debug Logs
                    // Log.Warning("xp Logger for Postfix " + xp);
                    // Log.Warning("direct Logger for Postfix " + direct);
                    // Log.Warning("__instance Logger for Postfix " + __instance);
                    // Log.Warning("__state Logger for Postfix " + __state);
                    // Log.Warning("___pawn Logger for Postfix " + ___pawn);
                    // Log.Warning("___def Logger for Postfix " + ___def);
                    // Log.Message("Settings chance to remove apathy" + settings?.chanceToRemoveApathy);
                    // Log.Message("Random generated " + randomNumber);
                    // Log.Message("Minimum skill to loose apathy " + settings.minSkillToLoseApathy);
                }
            }
        }

        private static void IncreasePassion (Passion newPassion, SkillRecord __instance, Pawn ___pawn, SkillDef ___def)
        {
            //Log.Message("Entered Increase Passion Method!");
            VSE.Passions.LearnRateFactorCache.ClearCacheFor(__instance, newPassion);
            __instance.passion = newPassion;
            if (PawnUtility.ShouldSendNotificationAbout(___pawn))
            {
                Messages.Message(string.Concat(___pawn.NameShortColored, " increased their passion for ", ___def.defName, " skill."), ___pawn, MessageTypeDefOf.PositiveEvent);
            }
        }

    }
}