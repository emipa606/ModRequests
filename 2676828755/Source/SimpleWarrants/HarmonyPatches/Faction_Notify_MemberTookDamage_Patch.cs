using HarmonyLib;
using RimWorld;
using Verse;

namespace SimpleWarrants
{
    [HarmonyPatch(typeof(Faction), "Notify_MemberTookDamage")]
    public static class Faction_Notify_MemberTookDamage_Patch
    {
        public static void Postfix(Faction __instance, Pawn member, DamageInfo dinfo)
        {
            if (member.IsPrisoner && member.Faction != Faction.OfPlayer && dinfo.Instigator is Pawn instigator && instigator.Faction == Faction.OfPlayer
                && !member.InMentalState && !PrisonBreakUtility.IsPrisonBreaking(member) && !SlaveRebellionUtility.IsRebelling(member))
            {
                if (WarrantsManager.Instance.CanPutWarrantOn(instigator) && Rand.Chance(0.05f))
                {
                    WarrantsManager.Instance.PutWarrantOn(instigator, "SW.Torture".Translate(), __instance);
                }
            }
        }
    }
}
