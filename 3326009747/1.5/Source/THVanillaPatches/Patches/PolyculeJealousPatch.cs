using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace THVanillaPatches.Patches
{
    public class PolyculeJealousPatch(THPatchDef def) : ToggleablePatchParent(def)
    {
        protected override List<PatchInfo> Patches => new List<PatchInfo>
        {
            new PatchInfo(AccessTools.Method(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.RemoveDirectRelation), new[] {typeof(PawnRelationDef), typeof(Pawn)}), Postfix: new HarmonyMethod(GetType(), nameof(RemoveDirectRelationPostfix))),
            new PatchInfo(AccessTools.Method(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.RemoveDirectRelation), new[] {typeof(DirectPawnRelation)}), Postfix: new HarmonyMethod(GetType(), nameof(RemoveDirectRelationPostfixAlt))),
            new PatchInfo(AccessTools.Method(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.AddDirectRelation)), Postfix: new HarmonyMethod(GetType(), nameof(AddDirectRelationPostfix)))
        };

        protected override void DisablePatches()
        {
            base.DisablePatches();
            if (Current.Game != null)
            {
                PawnsFinder.All_AliveOrDead?.Where(pawn => !pawn.Dead).Do(pawn => pawn?.needs?.mood?.thoughts?.situational?.Notify_SituationalThoughtsDirty());
            }
        }

        protected override void EnablePatches()
        {
            base.EnablePatches();
            if (Current.Game != null)
            {
                PawnsFinder.All_AliveOrDead?.Where(pawn => !pawn.Dead).Do(pawn => pawn?.needs?.mood?.thoughts?.situational?.Notify_SituationalThoughtsDirty());
            }
        }

        private static void RemoveDirectRelationPostfix(ref Pawn_RelationsTracker __instance, Pawn ___pawn, PawnRelationDef def, Pawn otherPawn)
        {
            UpdateIfNeeded(___pawn, def);
        }
        
        private static void RemoveDirectRelationPostfixAlt(ref Pawn_RelationsTracker __instance, Pawn ___pawn, DirectPawnRelation relation)
        {
            UpdateIfNeeded(___pawn, relation.def);
        }

        private static void AddDirectRelationPostfix(ref Pawn_RelationsTracker __instance, Pawn ___pawn, PawnRelationDef def, Pawn otherPawn)
        {
            UpdateIfNeeded(___pawn, def);
        }
        
        private static void UpdateIfNeeded(Pawn pawn, PawnRelationDef def)
        {
            if (pawn == null) return;
            if (!LovePartnerRelationUtility.IsLovePartnerRelation(def)) return;
            pawn.needs?.mood?.thoughts?.situational?.Notify_SituationalThoughtsDirty();
            foreach (Pawn relatedPawn in pawn.relations.RelatedPawns)
            {
                relatedPawn?.needs?.mood?.thoughts?.situational?.Notify_SituationalThoughtsDirty();
            }
        }
    }
}