using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimTwins
{
    [HarmonyPatch(typeof(Hediff_Pregnant), "DoBirthSpawn")]
    public static class Hediff_Pregnant_DoBirthSpawn_Patch
    {

        // This method will be called instead of the original GiveBirth method
        public static bool Prefix(Pawn mother, Pawn father)
        {
            bool isQuadruplet = Rand.Chance(0.005f);
            bool isTriplet = Rand.Chance(0.01f);
            bool isTwin = Rand.Chance(0.05f);

            int numBabies = 1;

            if (isQuadruplet)
            {
                numBabies = 4;
            }
            else if (isTriplet)
            {
                numBabies = 3;
            }
            else if (isTwin)
            {
                numBabies = 2;
            }

            if (mother.RaceProps.Humanlike && !ModsConfig.BiotechActive)
            {
                return false;
            }
            int num = numBabies;

            PawnGenerationRequest request = new PawnGenerationRequest(mother.kindDef, mother.Faction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, allowDead: false, allowDowned: true, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Newborn);
            Pawn pawn = null;
            for (int i = 0; i < num; i++)
            {
                pawn = PawnGenerator.GeneratePawn(request);

                if (PawnUtility.TrySpawnHatchedOrBornPawn(pawn, mother))
                {

                    if (pawn.playerSettings != null && mother.playerSettings != null)
                    {
                        pawn.playerSettings.AreaRestriction = mother.playerSettings.AreaRestriction;
                    }
                    if (pawn.RaceProps.IsFlesh)
                    {
                        pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, mother);
                        if (father != null)
                        {
                            pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, father);
                        }
                    }
                    if (mother.Spawned)
                    {
                        mother.GetLord()?.AddPawn(pawn);
                        pawn.Position = mother.Position.RandomAdjacentCell8Way();
                    }
                }
                else
                {
                    Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                }
                TaleRecorder.RecordTale(TaleDefOf.GaveBirth, mother, pawn);
            }
            if (mother.Spawned)
            {
                for (int i = 0; i < num; i++)
                {
                    FilthMaker.TryMakeFilth(mother.Position.RandomAdjacentCell8Way(), mother.Map, ThingDefOf.Filth_AmnioticFluid, mother.LabelIndefinite(), 5);
                }

                mother.caller?.DoCall();
                pawn.caller?.DoCall();
            }

            if (numBabies == 1)
            {
                Messages.Message(mother.Name.ToStringShort + " gave birth to " + numBabies + " baby!", MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                Messages.Message(mother.Name.ToStringShort + " gave birth to " + numBabies + " babies!", MessageTypeDefOf.PositiveEvent);
            }
            return false;
        }
    }
}
