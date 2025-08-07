using System.Collections.Generic;
using RimWorld;
using Verse;
using HarmonyLib;

namespace Stranger_Danger
{
    static class CustomMethods
    {
        public enum PawnType { Unknown, Colonist, Friendly, Slave, Innocent, Guilty, Active };
        public enum HideType { Story, Traits, Passion, Skills };

        public static bool GetSetting(PawnType pawnType, HideType hideType)
        {
            if (pawnType == PawnType.Unknown)
                return false;

            switch (10 * (int)pawnType + (int)hideType)
            {
                //Colonist
                case 10: return Settings.Get().colonistStory;
                case 11: return Settings.Get().colonistTraits;
                case 12: return Settings.Get().colonistPassion;
                case 13: return Settings.Get().colonistSkills;
                //Friendly
                case 20: return Settings.Get().friendStory;
                case 21: return Settings.Get().friendTraits;
                case 22: return Settings.Get().friendPassion;
                case 23: return Settings.Get().friendSkills;
                //Slave
                case 30: return Settings.Get().slaveStory;
                case 31: return Settings.Get().slaveTraits;
                case 32: return Settings.Get().slavePassion;
                case 33: return Settings.Get().slaveSkills;
                //Innocent
                case 40: return Settings.Get().innocentStory;
                case 41: return Settings.Get().innocentTraits;
                case 42: return Settings.Get().innocentPassion;
                case 43: return Settings.Get().innocentSkills;
                //Guilty
                case 50: return Settings.Get().guiltyStory;
                case 51: return Settings.Get().guiltyTraits;
                case 52: return Settings.Get().guiltyPassion;
                case 53: return Settings.Get().guiltySkills;
                //Active
                case 60: return Settings.Get().activeStory;
                case 61: return Settings.Get().activeTraits;
                case 62: return Settings.Get().activePassion;
                case 63: return Settings.Get().activeSkills;
            }

            return false;
        }

        public static bool ShouldHide(Pawn pawn, HideType hideType)
        {
            if (pawn == null)
                return false;

            if (pawn.Dead && !Settings.Get().hideDead)
                return false;

            PawnType pawnType = PawnCheck(pawn);

            if (ShownInPast(pawn, pawnType, hideType))
                return false;

            return GetSetting(pawnType, hideType);
        }

        public static bool ShownInPast(Pawn pawn, PawnType pawnType, HideType hideType)
        {
            CompStrangerDanger comp = pawn.TryGetComp<CompStrangerDanger>();
            if (comp == null)
                return false;

            if (comp.FormerColonist && !GetSetting(PawnType.Colonist, hideType))
                return true;

            if (comp.FormerSlave && !GetSetting(PawnType.Slave, hideType))
                return true;

            if (pawnType == PawnType.Guilty && comp.FormerInnocent && !GetSetting(PawnType.Innocent, hideType))
                return true;

            return false;
        }

        public static PawnType PawnCheck(Pawn pawn)
        {
            CompStrangerDanger comp = pawn.TryGetComp<CompStrangerDanger>();
            if (comp == null)
                return PawnType.Unknown;

            //Colonist
            if (pawn.IsFreeNonSlaveColonist)
            {
                comp.FormerColonist = true;
                return PawnType.Colonist;
            }

            if (pawn.IsSlaveOfColony)
            {
                comp.FormerSlave = true;
                return PawnType.Slave;
            }

            //Slave
            if (pawn.IsSlave || pawn.kindDef == PawnKindDefOf.Slave)
                return PawnType.Slave;

            /*if (IDontKnowWhatImDoing.VFEMedievalFound && pawn.kindDef == PawnKindDef.Named("VFEM_SellSword"))
            {
                return PawnType.Slave;
            }//*/

            //Raider & Prisoner        
            if (Faction.OfPlayerSilentFail == null)
                return PawnType.Unknown;

            if (!pawn.Faction.HostileTo(Faction.OfPlayer))
                return PawnType.Friendly;

            if (!pawn.IsPrisonerOfColony)
                return PawnType.Active;

            if (pawn.guilt.IsGuilty)
                return PawnType.Guilty;

            comp.FormerInnocent = true;
            return PawnType.Innocent;
        }

    }
}
