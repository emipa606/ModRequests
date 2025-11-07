using RimWorld;
using RimWorld.Planet;
using System;
using Verse;

namespace BillDoorsPredefinedCharacter
{
    public static class PDCharacterUtil
    {
        public static bool InPeril(this PredefinedCharacterParmDef def)
        {
            if (!BDPDC_Mod.Tracker.ContainsKey(def)) return false;
            var pawn = BDPDC_Mod.Tracker[def];
            if (pawn == null) return false;
            return !pawn.Faction.IsPlayer && (pawn.IsSlave || pawn.IsPrisoner);
        }

        public static bool AtHome(this PredefinedCharacterParmDef def)
        {
            if (!BDPDC_Mod.Tracker.ContainsKey(def)) return true;
            var pawn = BDPDC_Mod.Tracker[def];
            if (pawn == null) return true;
            if (pawn.SpawnedOrAnyParentSpawned) return false;
            return Find.WorldPawns.Contains(pawn) && !(pawn.IsColonist || pawn.IsSlave || pawn.IsPrisoner || pawn.Dead || pawn.IsCaravanMember());
        }

        public static Pawn MakePawn(this PredefinedCharacterParmDef def, Faction faction = null)
        {
            return PredefinedCharacterMaker.MakePawn(def, faction);
        }

        public static Pawn GetPawn(this PredefinedCharacterParmDef def, Predicate<PredefinedCharacterParmDef, Pawn> existingPawnPredicate = null, Action<Pawn> postAction = null, Faction faction = null, bool rePostProcessExisting = true)
        {
            return PredefinedCharacterMaker.GetPawn(def, existingPawnPredicate, postAction, faction, rePostProcessExisting);
        }
    }
}
