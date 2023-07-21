using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace Renamon
{
    [HarmonyPatch(typeof(GenSpawn), "Spawn", new System.Type[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool) })]
    public static class GenSpawn_Spawn_Patch
    {
        public static void Postfix(Thing newThing, ref WipeMode wipeMode, bool respawningAfterLoad)
        {
            if (!respawningAfterLoad && newThing is Pawn pawn && pawn.Faction != Faction.OfPlayer && pawn.Map.IsPlayerHome)
            {
                var comp = pawn.GetComp<CompPawnMorpher>();
                if (comp != null && !comp.transformedAlready)
                {
                    var hostilePawns = pawn.Map.attackTargetsCache.TargetsHostileToFaction(pawn.Faction).OfType<Pawn>()
                        .Where(x => x.RaceProps.Humanlike && x.def != pawn.def);
                    if (hostilePawns.TryRandomElement(out var hostile))
                    {
                        comp.pawnToTransform = hostile;
                    }
                }
            }
        }
    }
}
