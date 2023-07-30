using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace CM_Meeseeks_Box
{
    public static class MeeseeksUtility
    {
        private static Pawn designatingMeeseeks = null;
        public static Pawn DesignatingMeeseeks => designatingMeeseeks;

        private static SoundDef[] PoofInSounds = { MeeseeksDefOf.CM_Meeseeks_Box_Poof_In };
        private static SoundDef[] PoofOutSounds = { MeeseeksDefOf.CM_Meeseeks_Box_Poof_Out };

        private static SoundDef[] AcceptTaskSounds = { MeeseeksDefOf.CM_Meeseeks_Box_Sound_OK, MeeseeksDefOf.CM_Meeseeks_Box_Sound_OK_2, MeeseeksDefOf.CM_Meeseeks_Box_Sound_Can_Do, MeeseeksDefOf.CM_Meeseeks_Box_Sound_Can_Do_2 };
        private static SoundDef[] FinishTaskSounds = { MeeseeksDefOf.CM_Meeseeks_Box_Sound_All_Done };
        private static SoundDef[] GreetingSounds = { MeeseeksDefOf.CM_Meeseeks_Box_Sound_Im_Mr_Meeseeks_Look_At_Me, MeeseeksDefOf.CM_Meeseeks_Box_Sound_Look_At_Me, MeeseeksDefOf.CM_Meeseeks_Box_Sound_Im_Mr_Meeseeks, MeeseeksDefOf.CM_Meeseeks_Box_Sound_Im_Mr_Meeseeks_2, MeeseeksDefOf.CM_Meeseeks_Box_Sound_Im_Mr_Meeseeks_3 };

        private static SoundDef[] CriticalBreakSounds = { MeeseeksDefOf.CM_Meeseeks_Box_Sound_I_Cant_Take_It_Anymore };

        public static Pawn lastCreatedMeeseeks = null;

        public static TargetInfo GetTargetInfo(Thing target)
        {
            TargetInfo targetInfo = null;
            if (target != null && target.SpawnedOrAnyParentSpawned)
                targetInfo = new TargetInfo(target.PositionHeld, target.MapHeld);
            return targetInfo;
        }

        public static void PlayPoofInSound(Thing target)
        {
            PoofInSounds.RandomElement().PlayOneShot(GetTargetInfo(target));
        }

        public static void PlayPoofOutSound(Thing target)
        {
            PoofOutSounds.RandomElement().PlayOneShot(GetTargetInfo(target));
        }

        public static void PlayAcceptTaskSound(Thing target, Voice voice)
        {
            PlayVoicedSound(target, voice, AcceptTaskSounds.RandomElement());
        }

        public static void PlayFinishTaskSound(Thing target, Voice voice)
        {
            PlayVoicedSound(target, voice, FinishTaskSounds.RandomElement());
        }

        public static void PlayGreetingSound(Thing target, Voice voice)
        {
            PlayVoicedSound(target, voice, GreetingSounds.RandomElement());
        }

        public static void PlayCriticalBreakSound(Thing target, Voice voice)
        {
            PlayVoicedSound(target, voice, CriticalBreakSounds.RandomElement());
        }

        public static void PlayVoicedSound(Thing target, Voice voice, SoundDef soundDef)
        {
            if (!MeeseeksMod.settings.meeseeksSpeaks)
                return;
            //Logger.MessageFormat(target, "{0} playing sound {2} with voice: {1}", target, soundDef.defName, voice);

            SoundInfo soundInfo = GetTargetInfo(target);
            soundInfo.pitchFactor = voice.pitch;
            soundInfo.volumeFactor = ((float)voice.volume / 100.0f) * MeeseeksMod.settings.meeseeksVolume;
            soundDef.PlayOneShot(soundInfo);
        }

        public static void DesignateMeeseeksWorkArea(Pawn mrMeeseeksLookAtMe)
        {

        }

        public static void SpawnMeeseeks(Pawn creator, ThingWithComps creatingThing, Map map)
        {
            PawnKindDef pawnKindDef = MeeseeksDefOf.MeeseeksKind;

            Pawn mrMeeseeksLookAtMe = PawnGenerator.GeneratePawn(pawnKindDef, Faction.OfPlayer);
            lastCreatedMeeseeks = mrMeeseeksLookAtMe;

            QualityCategory boxQuality = QualityCategory.Normal;
            creatingThing.TryGetQuality(out boxQuality);

            int qualityInt = (int)boxQuality;

            float multiplier = ((float)qualityInt + 1.0f) / ((int)QualityCategory.Legendary + 1);
            int skillLevel = Mathf.RoundToInt(multiplier * multiplier * 20.0f);

            // Enable all work types
            foreach (WorkTypeDef item in from w in DefDatabase<WorkTypeDef>.AllDefs
                                         where !w.alwaysStartActive && !mrMeeseeksLookAtMe.WorkTypeIsDisabled(w)
                                         select w)
            {
                // Our patch overrides the priority, but we need to make sure they are all on
                mrMeeseeksLookAtMe.workSettings.SetPriority(item, 3);
            }

            foreach (SkillRecord skill in mrMeeseeksLookAtMe.skills.skills)
            {
                skill.Level = skillLevel;
                skill.passion = Passion.None;
            }

            // Max out needs
            foreach (Need need in mrMeeseeksLookAtMe.needs.AllNeeds)
            {
                if (need.def.defName != "Mood")
                    need.CurLevelPercentage = 1.0f;
            }

            IntVec3 summonPosition = MeeseeksUtility.FindSpawnPosition(creatingThing);
            GenSpawn.Spawn(mrMeeseeksLookAtMe, summonPosition, map);
            mrMeeseeksLookAtMe.Rotation = Rot4.South;

            CompMeeseeksMemory compMeeseeksMemory = mrMeeseeksLookAtMe.GetComp<CompMeeseeksMemory>();
            compMeeseeksMemory.SetQuality(boxQuality);
            compMeeseeksMemory.SetCreator(creator);

            // The voices for greeting and task acceptance can overlap depending on the speed the game is running,
            //   especially if another Meeseeks created us, so just don't do it in that case
            CompMeeseeksMemory creatorMemory = creator.GetComp<CompMeeseeksMemory>();
            bool creatorIsMeeseeks = creatorMemory != null;
            if (!creatorIsMeeseeks)
                MeeseeksUtility.PlayGreetingSound(mrMeeseeksLookAtMe, compMeeseeksMemory.Voice);
            else
                compMeeseeksMemory.temporarilyBlockTask = true;

            Thing smoke = ThingMaker.MakeThing(ThingDefOf.Gas_Smoke);
            GenSpawn.Spawn(smoke, summonPosition, map);
            MeeseeksUtility.PlayPoofInSound(smoke);

            //ThingDef moteDef = DefDatabase<ThingDef>.GetNamedSilentFail("Mote_PsycastSkipOuterRingExit");
            //if (moteDef != null)
            //    MoteMaker.MakeAttachedOverlay(mrMeeseeksLookAtMe, moteDef, Vector3.zero, 1.0f);

            //GenExplosion.DoExplosion(mrMeeseeksLookAtMe.PositionHeld, mrMeeseeksLookAtMe.MapHeld, 1.0f, DamageDefOf.Smoke, null, -1, -1f, MeeseeksDefOf.CM_Meeseeks_Box_Poof_In, null, null, null, ThingDefOf.Gas_Smoke, 1f);

            GlobalTargetInfo meeseeksGlobalTarget = new GlobalTargetInfo(mrMeeseeksLookAtMe);

            if (!creatorIsMeeseeks && creator.IsColonistPlayerControlled && MeeseeksMod.settings.cameraJumpOnCreation)
                CameraJumper.TryJump(meeseeksGlobalTarget);

            if (!creatorIsMeeseeks && MeeseeksMod.settings.autoSelectOnCreation)
                CameraJumper.TrySelect(meeseeksGlobalTarget);

            if (!creatorIsMeeseeks && MeeseeksMod.settings.autoPauseOnCreation)
                Find.TickManager.Pause();
        }

        static private IntVec3 FindSpawnPosition(Thing spawningThing)
        {
            if (Prefs.DevMode && MeeseeksMod.settings.screenShotDebug)
                return spawningThing.Position + new IntVec3(-1, 0, 0);

            IntVec3 spawnPosition = spawningThing.Position;
            List<IntVec3> randomOffsets = GenAdj.AdjacentCells8WayRandomized();
            foreach (IntVec3 randomOffset in randomOffsets)
            {
                spawnPosition = spawningThing.Position + randomOffset;
                if (spawnPosition.InBounds(spawningThing.Map) && spawnPosition.Walkable(spawningThing.Map))
                {
                    Building_Door building_Door = spawnPosition.GetEdifice(spawningThing.Map) as Building_Door;
                    // TODO: Could anything other than a pawn summon a meeseeks this way? If so, change this function to use Pawn
                    if (building_Door == null)// || building_Door.CanPhysicallyPass(caster))
                    {
                        return spawnPosition;
                    }
                }
            }

            return spawningThing.Position;
        }

        public static void DespawnMeeseeks(Pawn mrMeeseeksLookAtMe)
        {
            CompMeeseeksMemory compMeeseeksMemory = mrMeeseeksLookAtMe.GetComp<CompMeeseeksMemory>();
            compMeeseeksMemory.CleanupMemory();

            Thing smoke = ThingMaker.MakeThing(ThingDefOf.Gas_Smoke);
            GenSpawn.Spawn(smoke, mrMeeseeksLookAtMe.PositionHeld, mrMeeseeksLookAtMe.MapHeld);
            MeeseeksUtility.PlayPoofOutSound(mrMeeseeksLookAtMe);

            if (mrMeeseeksLookAtMe.carryTracker.CarriedThing != null)
                mrMeeseeksLookAtMe.carryTracker.TryDropCarriedThing(mrMeeseeksLookAtMe.PositionHeld, ThingPlaceMode.Near, out Thing thing);
            mrMeeseeksLookAtMe.Strip();

            //ThingDef moteDef = DefDatabase<ThingDef>.GetNamedSilentFail("Mote_PsycastSkipOuterRingExit");
            //if (moteDef != null)
            //    MoteMaker.MakeAttachedOverlay(pawn, moteDef, Vector3.zero, 1.0f);

            //GenExplosion.DoExplosion(pawn.PositionHeld, pawn.MapHeld, 1.0f, DamageDefOf.Smoke, null, -1, -1f, MeeseeksDefOf.CM_Meeseeks_Box_Poof_Out, null, null, null, ThingDefOf.Gas_Smoke, 1f);
            if (mrMeeseeksLookAtMe.Corpse != null)
                mrMeeseeksLookAtMe.Corpse.Destroy();
            else
                mrMeeseeksLookAtMe.Destroy();
        }
    }
}
