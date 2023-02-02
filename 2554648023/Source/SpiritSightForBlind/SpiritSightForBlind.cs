using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using HarmonyLib;



namespace SpiritSightForBlind
{


    [DefOf]
    public static class BlindHediffDefOf
    {
        public static HediffDef SpiritSight;

        static BlindHediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(BlindHediffDefOf));
        }
    }

    public class SpiritSight : HediffDef
    {
        
    }

    public class Hediff_SpiritSight : Hediff_Level
    {
        public HediffDef SpiritSight;
        public override void PostAdd(DamageInfo? dinfo)
        {
            Log.Message("Spirit Sight was granted successfully");
        }
        
        
        public override void Tick()
        {
            base.Tick();
            SpritSightUpdater.SpiritSightUpdater_Worker(pawn);
            

        }
        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving && combatLogEntry != null)
            {
                LogEntry target = combatLogEntry.Target;
                if (target == null || !Current.Game.battleLog.IsEntryActive(target))
                {
                    combatLogEntry = null;
                }
            }
            Scribe_Values.Look(ref loadID, "loadID", 0);
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref ageTicks, "ageTicks", 0);
            Scribe_Defs.Look(ref source, "source");
            Scribe_Defs.Look(ref sourceBodyPartGroup, "sourceBodyPartGroup");
            Scribe_Defs.Look(ref sourceHediffDef, "sourceHediffDef");
            Scribe_Values.Look(ref severityInt, "severity", 0f);
            Scribe_Values.Look(ref causesNoPain, "causesNoPain", defaultValue: false);
            Scribe_References.Look(ref combatLogEntry, "combatLogEntry");
            Scribe_Values.Look(ref combatLogText, "combatLogText");
            BackCompatibility.PostExposeData(this);
            Scribe_Values.Look(ref level, "level", 0);
        }
    }






    public static class SpritSightGiver
    {
        public static void SpiritSightGiver_Worker(Pawn pawn)
        {
            if (!pawn.health.hediffSet.HasHediff(BlindHediffDefOf.SpiritSight) && !SpritSightRemover.SpritSightRemover_HasSightSource(pawn.health.hediffSet))
            {
                pawn.health.AddHediff(BlindHediffDefOf.SpiritSight);
                
            }
                
                if (ModsConfig.RoyaltyActive == true)
                {

                    int PawnPsylinkLevel = pawn.GetPsylinkLevel();
                    switch (PawnPsylinkLevel)
                    {
                        case 0:
                        case 1:
                        case 2:
                            {
                                
                                Hediff_SpiritSight def = (Hediff_SpiritSight)pawn.health.hediffSet.GetFirstHediffOfDef(BlindHediffDefOf.SpiritSight);
                                if (def.level < 1)
                                    def.SetLevelTo(1);
                                break;
                            }
                        case 3:
                        case 4:
                            {
                                
                                Hediff_SpiritSight def = (Hediff_SpiritSight)pawn.health.hediffSet.GetFirstHediffOfDef(BlindHediffDefOf.SpiritSight);
                                if (def.level < 2)
                                    def.SetLevelTo(2);
                                break;
                            }
                        case 5:
                            {
                                
                                Hediff_SpiritSight def = (Hediff_SpiritSight)pawn.health.hediffSet.GetFirstHediffOfDef(BlindHediffDefOf.SpiritSight);
                                if (def.level < 3)
                                    def.SetLevelTo(3);
                                break;
                            }
                        case 6:
                            {
                                
                                Hediff_SpiritSight def = (Hediff_SpiritSight)pawn.health.hediffSet.GetFirstHediffOfDef(BlindHediffDefOf.SpiritSight);
                                if (def.level < 4)
                                    def.SetLevelTo(4);
                                break;
                            }
                    }

                }
                else
                {
                    
                    Hediff_SpiritSight def = (Hediff_SpiritSight)pawn.health.hediffSet.GetFirstHediffOfDef(BlindHediffDefOf.SpiritSight);
                    def.SetLevelTo(1);

                }
            
                




        }
    }

    public static class SpritSightRemover
    {
        public static void SpiritSightRemover_Worker(Pawn pawn)
        {
            if (pawn.health.hediffSet.HasHediff(BlindHediffDefOf.SpiritSight))
            {
                Hediff_SpiritSight def = (Hediff_SpiritSight)pawn.health.hediffSet.GetFirstHediffOfDef(BlindHediffDefOf.SpiritSight);
                def.ageTicks = 0;
                def.SetLevelTo(0);
                def.Severity = 0f;

            }
        }
        public static bool SpritSightRemover_HasSightSource(HediffSet diffSet)
        {
            List<Hediff> hediffs = diffSet.hediffs;
            int MissingSightSourcesFound = 0;
            for (int i = 0; i < hediffs.Count; i++)
            {
                Hediff_MissingPart hediff_MissingPart;
                if ((hediff_MissingPart = hediffs[i] as Hediff_MissingPart) != null && hediff_MissingPart.Part.def.tags.Contains(BodyPartTagDefOf.SightSource))
                {
                    MissingSightSourcesFound += 1;
                }
            }
            if(MissingSightSourcesFound <= 1)
            {
                return true;
            }
            return false;
        }
    }


    public static class SpritSightUpdater
    {
        public static void SpiritSightUpdater_Worker(Pawn pawn)
        {
            Hediff_SpiritSight def = (Hediff_SpiritSight)pawn.health.hediffSet.GetFirstHediffOfDef(BlindHediffDefOf.SpiritSight);
            if (pawn.health.hediffSet.HasHediff(BlindHediffDefOf.SpiritSight) && ModsConfig.RoyaltyActive && pawn.HasPsylink == true)
            {
                SpritSightGiver.SpiritSightGiver_Worker(pawn);
            }
            
                if(pawn.health.hediffSet.HasHediff(BlindHediffDefOf.SpiritSight) && def.level >= 1 && def.level < 4 && def.ageTicks >= 900000)
                {
                    def.ageTicks = 0;
                    float upgradeChance = 0.5f * (1f / def.level);
                    if (Rand.Chance(upgradeChance))
                    {
                    
                       def.ChangeLevel(1);
                       
                       def.Severity = def.level;
                    }
                }
            if (pawn.health.hediffSet.HasHediff(BlindHediffDefOf.SpiritSight) && SpritSightRemover.SpritSightRemover_HasSightSource(pawn.health.hediffSet))
            {
                SpritSightRemover.SpiritSightRemover_Worker(pawn);
            }
        }
    }









    [StaticConstructorOnStartup]
    public static class SpiritSightForBlindInit
    {
        static SpiritSightForBlindInit()
        {
            var harmony = new Harmony("com.ChadYu.SpiritSightForBlind");
            harmony.PatchAll();
            Log.Message("[Spirit Sight mod]:: Loaded Spirit Sight For Blind mod. Thank you for using my mod");
        }

    }







    [HarmonyPatch(typeof(RitualOutcomeEffectWorker_Blinding), "ApplyExtraOutcome")]
    public static class SpiritSightForBlind_Patch_RitualOutcome
    {
        
        [HarmonyPostfix]
        public static void Postfix(LordJob_Ritual jobRitual, OutcomeChance outcome, string extraOutcomeDesc)
        {
            
            Pawn pawn = ((LordJob_Ritual_Mutilation)jobRitual).mutilatedPawns[0];
            if (ModsConfig.RoyaltyActive && pawn.HasPsylink == true)
            {
                SpiritSightForBlind.SpritSightGiver.SpiritSightGiver_Worker(pawn);
            }
            else
            if (pawn.HasPsylink == false && outcome.Positive && ( (outcome.BestPositiveOutcome(jobRitual) || Rand.Chance(0.5f)) || extraOutcomeDesc != null ))
            {
                SpiritSightForBlind.SpritSightGiver.SpiritSightGiver_Worker(pawn);
            }
        }
    }

    [HarmonyPatch(typeof(PawnCapacityWorker_Sight), "CalculateCapacityLevel")]
    public static class SpiritSightForBlind_Patch_Worker_CalculateCapacityLevel
    {
        public static HediffSet currentPawndiffSet;
        public static PawnCapacityDef capacity;
        [HarmonyPostfix]
        public static void Postfix(ref float __result, HediffSet diffSet)
        {
            currentPawndiffSet = diffSet;
            if (Verse.HealthUtility.IsMissingSightBodyPart(diffSet.pawn) == true && diffSet.HasHediff(BlindHediffDefOf.SpiritSight))
            {
                __result = 0.01f; //must not be 0 otherwise the game will clamp Sight stat at 0, therefore all offsets of mod hediffs are 1 unit below what they should be in XML
            }
            if (SpritSightRemover.SpritSightRemover_HasSightSource(diffSet) == true)
            {

                SpritSightRemover.SpiritSightRemover_Worker(diffSet.pawn);

            }

        }
    }




    [HarmonyPatch(typeof(PawnCapacityWorker_Sight), "CanHaveCapacity")]
    public static class SpiritSightForBlind_Patch_Worker_CanHaveCapacity
    {
        [HarmonyPostfix]
        public static void Postfix(ref bool __result)
        {
            HediffSet diffSet = SpiritSightForBlind_Patch_Worker_CalculateCapacityLevel.currentPawndiffSet; 
            if (diffSet.HasHediff(BlindHediffDefOf.SpiritSight))
            {
                __result = true;
            }
        }
    }


    [HarmonyPatch(typeof(PawnUtility), "IsBiologicallyBlind")]
    public static class IsBiologicallyBlind_Patch_PawnUtility
    {
        [HarmonyPostfix]
        public static void Postfix(ref bool __result, Pawn pawn)
        {
            if (pawn.health.hediffSet.HasHediff(BlindHediffDefOf.SpiritSight))
            {
                __result = false;
            }
        }

    }

    [HarmonyPatch(typeof(PawnUtility), "IsArtificiallyBlind")]
    public static class IsArtificiallyBlind_Patch_PawnUtility
    {
        [HarmonyPostfix]
        public static void Postfix(ref bool __result, Pawn p)
        {
            if (p.health.hediffSet.HasHediff(BlindHediffDefOf.SpiritSight))
            {
                __result = false;
            }
        }

    }

    [HarmonyPatch(typeof(ThoughtWorker_Precept_Blind), "ShouldHaveThought")]
    public static class ShouldHaveThought_Patch_ThoughtWorker_Precept_Blind
    {
        [HarmonyPostfix]
        public static void Postfix(ref ThoughtState __result, Pawn p)
        {
            if (p.health.hediffSet.HasHediff(BlindHediffDefOf.SpiritSight))
            {
                
                __result = ThoughtState.ActiveDefault;
            }
            
        }

    }

    [HarmonyPatch(typeof(ThoughtWorker_Precept_NonBlind), "ShouldHaveThought")]
    public static class ShouldHaveThought_Patch_ThoughtWorker_Precept_NonBlind
    {
        [HarmonyPostfix]
        public static void Postfix(ref ThoughtState __result, Pawn p)
        {
            if (p.health.hediffSet.HasHediff(BlindHediffDefOf.SpiritSight))
            {
                __result = ThoughtState.Inactive; ;
            }
            
        }

    }

    [HarmonyPatch(typeof(ThoughtWorker_Precept_HalfBlind), "IsHalfBlind")]
    public static class IsHalfBlind_Patch_ThoughtWorker_Precept_HalfBlind
    {
        [HarmonyPostfix]
        public static void Postfix(ref bool __result, Pawn p)
        {
            if (p.health.hediffSet.HasHediff(BlindHediffDefOf.SpiritSight))
            {
                __result = false;
            }

        }

    }

    [HarmonyPatch(typeof(ThoughtWorker_Precept_HalfBlind), "ShouldHaveThought")]
    public static class ShouldHaveThought_Patch_ThoughtWorker_Precept_HalfBlind
    {
        [HarmonyPostfix]
        public static void Postfix(ref ThoughtState __result, Pawn p)
        {
            if (p.health.hediffSet.HasHediff(BlindHediffDefOf.SpiritSight))
            {
                __result = ThoughtState.Inactive;
            }
            
        }

    }


    [HarmonyPatch(typeof(ThoughtWorker_Precept_ArtificialBlind), "ShouldHaveThought")]
    public static class ShouldHaveThought_Patch_ThoughtWorker_Precept_ArtificialBlind
    {
        [HarmonyPostfix]
        public static void Postfix(ref ThoughtState __result, Pawn p)
        {
            if (p.health.hediffSet.HasHediff(BlindHediffDefOf.SpiritSight))
            {
                __result = ThoughtState.Inactive;
            }
        }

    }

    [HarmonyPatch(typeof(ThoughtWorker_Precept_NonBlind_Social), "ShouldHaveThought")]
    public static class ShouldHaveThought_Patch_ThoughtWorker_Precept_NonBlind_Social
    {
        [HarmonyPostfix]
        public static void Postfix(ref ThoughtState __result, Pawn p, Pawn otherPawn)
        {
            if (otherPawn.health.hediffSet.HasHediff(BlindHediffDefOf.SpiritSight))
            {
                __result = ThoughtState.Inactive;
            }
            
        }

    }

    [HarmonyPatch(typeof(ThoughtWorker_Precept_Blind_Social), "ShouldHaveThought")]
    public static class ShouldHaveThought_Patch_oughtWorker_Precept_Blind_Social
    {
        [HarmonyPostfix]
        public static void Postfix(ref ThoughtState __result, Pawn p, Pawn otherPawn)
        {
            if (otherPawn.health.hediffSet.HasHediff(BlindHediffDefOf.SpiritSight))
            {
                __result = ThoughtState.ActiveDefault;
            }
            
        }

    }

    [HarmonyPatch(typeof(ThoughtWorker_Precept_HalfBlind_Social), "ShouldHaveThought")]
    public static class ShouldHaveThought_Patch_ThoughtWorker_Precept_HalfBlind_Social
    {
        [HarmonyPostfix]
        public static void Postfix(ref ThoughtState __result, Pawn p, Pawn otherPawn)
        {
            if (otherPawn.health.hediffSet.HasHediff(BlindHediffDefOf.SpiritSight))
            {
                __result = ThoughtState.Inactive;
            }
            
        }

    }

    [HarmonyPatch(typeof(ThoughtWorker_Precept_ArtificialBlind_Social), "ShouldHaveThought")]
    public static class ShouldHaveThought_Patch_ThoughtWorker_Precept_ArtificialBlind_Social
    {
        [HarmonyPostfix]
        public static void Postfix(ref ThoughtState __result, Pawn p, Pawn otherPawn)
        {
            if (otherPawn.health.hediffSet.HasHediff(BlindHediffDefOf.SpiritSight))
            {
                __result = ThoughtState.Inactive;
            }
        }

    }


}


