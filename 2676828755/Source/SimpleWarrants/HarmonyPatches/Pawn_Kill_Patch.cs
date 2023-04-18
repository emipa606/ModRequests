using HarmonyLib;
using RimWorld;
using Verse;

namespace SimpleWarrants
{
    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Pawn_Kill_Patch
    {
        public static void Postfix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            if (dinfo.HasValue && dinfo.Value.Instigator is Pawn instigator && instigator.RaceProps.Humanlike && instigator.Faction == Faction.OfPlayer 
                && __instance.RaceProps.Animal && __instance.Faction is null && __instance.def.BaseMarketValue >= ThingDefOf.Thrumbo.BaseMarketValue)
            {
                if (WarrantsManager.Instance.CanPutWarrantOn(instigator) && Rand.Chance(0.25f))
                {
                    WarrantsManager.Instance.PutWarrantOn(instigator, "SW.Poaching".Translate(), Utils.AnyHostileToPlayerFaction());
                }
            }
        }
    }
}
