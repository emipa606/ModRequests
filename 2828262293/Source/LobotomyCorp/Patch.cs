using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using System.Reflection;

namespace LobotomyCorp
{
    /*
    [StaticConstructorOnStartup]
    static class Patch
    {
        [HarmonyPatch(typeof(Pawn))]
        [HarmonyPatch("Kill")]
        internal class Patch_Pawn_Kill_Post
        {
            static void Postfix(Pawn __instance)
            {
                Log.Message(__instance.Label);
                Log.Message(__instance.MapHeld?.ToString() ?? "no map");

                // 人間限定
                if (__instance.RaceProps.Humanlike == true)
                {
                    // 勢力なし、または、敵対していない勢力の場合
                    if ((__instance.Faction == null) || (__instance.Faction.HostileTo(Faction.OfPlayer) == false))
                    {
                        // 大鳥がいれば発狂
                        IEnumerable<Pawn> bigBirds = __instance.MapHeld.mapPawns.AllPawnsSpawned.Where<Pawn>(p => p.def.defName == "AIR_BigBird");
                        if (bigBirds.EnumerableNullOrEmpty() == false)
                        {
                            foreach (Pawn bird in bigBirds)
                            {
                                Messages.Message("大鳥が脱走しました。", new LookTargets(__instance), MessageTypeDefOf.ThreatBig, false);
                                bird.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, null, true, false, null, false);
                            }
                        }
                    }
                }
            }
        }

        static Patch()
        {
            Harmony harmony = new Harmony("LobotomyCorp");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message("patched");
        }
    }
    */
}
