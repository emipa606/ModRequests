using System.Collections.Generic;
using Verse;
using HarmonyLib;
using Settings = Tacticowl.ModSettings_Tacticowl;

namespace Tacticowl
{
    [HarmonyPatch(typeof(Verb), nameof(Verb.TryCastNextBurstShot))]
    static class Verb_TryCastNextBurstShot
    {
        static bool Prepare()
        {
            return Settings.runAndGunEnabled || Settings.dualWieldEnabled;
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.MethodReplacer(AccessTools.Method(typeof(Pawn_StanceTracker), nameof(Pawn_StanceTracker.SetStance)),
				AccessTools.Method(typeof(Verb_TryCastNextBurstShot), nameof(Verb_TryCastNextBurstShot.SetStanceRunAndGun)));
        }
        public static void SetStanceRunAndGun(Pawn_StanceTracker stanceTracker, Stance_Cooldown stance)
        {
            var stanceVerb = stance != null ? stance.verb : null; // lazy should prob put this in a merged conditional but meh it can be null now?
            if (stanceVerb != null && stanceVerb.EquipmentSource != null && stanceVerb.verbProps.violent) // Only do on violent actions rather than including all verbs
		    {
                
                Pawn pawn = stanceTracker.pawn;
                bool success = false;
                if (pawn != null && stance.verb.CurrentTarget.TryGetPawn(out var targetPawn) && targetPawn != null && targetPawn.Faction != null && targetPawn.Faction == pawn.Faction) goto skipAll; // if pawn is null jump to skipAll so we still set stance
                
                    
                
                if (Settings.dualWieldEnabled && stance.verb.EquipmentSource.IsOffHandedWeapon())
                {
                    var offhandStance = pawn.GetOffHandStanceTracker();
                    if (Settings.runAndGunEnabled && (offhandStance.curStance is Stance_RunAndGun || offhandStance.curStance is Stance_RunAndGun_Cooldown) && pawn != null && pawn.pather.Moving)
                    {
                        offhandStance.SetStance(new Stance_RunAndGun_Cooldown(stance.ticksLeft, stance.focusTarg, stanceVerb));
                    }
                    else offhandStance.SetStance(new Stance_Cooldown(stance.ticksLeft, stance.focusTarg, stance.verb));
                    success = true;
                }
                if (Settings.runAndGunEnabled && (stanceTracker.curStance is Stance_RunAndGun || stanceTracker.curStance is Stance_RunAndGun_Cooldown) && pawn != null && pawn.pather.Moving)
                {
                    stanceTracker.SetStance(new Stance_RunAndGun_Cooldown(stance.ticksLeft, stance.focusTarg, stanceVerb));
                    success = true;
                }
                if (success) return;
            }
            
            skipAll:
            stanceTracker.SetStance(stance);
        }
    }
}
