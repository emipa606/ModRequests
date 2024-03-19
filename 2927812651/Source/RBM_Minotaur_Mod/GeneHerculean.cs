using HarmonyLib;
using RimWorld;
using System;
using Verse;




namespace RBM_Minotaur
{
    [HarmonyPatch]
    public static class Herculean_Patches
    {

        //Postfix to prevent non-Herculean pawns from equiping Herculean Weapons or Equipment

        [HarmonyPatch(typeof(EquipmentUtility), nameof(EquipmentUtility.CanEquip), new Type[] { typeof(Thing), typeof(Pawn), typeof(string), typeof(bool) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal })]
        [HarmonyPostfix]
        public static bool CanEquip_Postfix(bool __result, Thing thing, Pawn pawn, ref string cantReason, bool checkBonded = true)
        {
            if (MinotaurSettings.debugMessages) { Log.Message("RBM Is Running: (Equip Postfix) public static bool CanEquip_Postfix(bool __result, Thing thing, Pawn pawn, ref string cantReason, bool checkBonded = true)"); }

            //If weapon or armour are Herculean, return true only if the parh has the herculean trait 
            if (thing.def.weaponClasses?.Contains(RBM_DefOf.RBM_HerculeanClass) == true || thing.def.apparel?.tags?.Contains("HerculeanApparel") == true)
            {
                if (pawn.genes?.HasGene(RBM_DefOf.RBM_Herculean) == true)
                {
                    return true;
                }
                cantReason = "Pawn is not Herculean";
                return false;
            }

            return __result;
        }


        //Adds the 'crushed' hediff to pawns that have lovin' with Herculean pawns

        [HarmonyPatch(typeof(JobDriver_Lovin), "MakeNewToils")]
        [HarmonyPostfix]
        public static void MakeNewToils_Postfix(ref JobDriver_Lovin __instance, ref Verse.AI.TargetIndex ___PartnerInd)
        {
            if (MinotaurSettings.debugMessages) { Log.Message("RBM Is Running: (Equip Postfix (crushed)) public static void MakeNewToils_Postfix(ref JobDriver_Lovin __instance, ref Verse.AI.TargetIndex ___PartnerInd)"); }
            Pawn Partner = (Pawn)((Thing)__instance.job.GetTarget(___PartnerInd));
            if (Partner.genes.HasGene(RBM_DefOf.RBM_Herculean))
            {
                if (__instance.pawn.story?.traits?.HasTrait(TraitDefOf.Masochist) == true)  //Give a positive version to masochists
                {
                    __instance.pawn.needs.mood.thoughts.memories.TryGainMemory(RBM_DefOf.RBM_CrushedMasochist);
                }
                else
                {
                    __instance.pawn.needs.mood.thoughts.memories.TryGainMemory(RBM_DefOf.RBM_Crushed);
                }

            }
        }
    }

    public class Gene_RBM_Herculean : Gene
    {
        // Adds the 'In Heat' hediff to pawns with the Estrous Cycle gene in ApriMay
        public override void Tick()
        {
            base.Tick();

            //Return if the hash interval is incorrect (not enough time has passed)
            if (!this.pawn.IsHashIntervalTick(1440))
            {
                return;
            }

            if (MinotaurSettings.debugMessages) { Log.Message("RBM Is Running: (Heculean Patch) public override void Tick() (Hash inteval passed) "); }

            //Return if the pawn is not spawned to stop 
            if (!this.pawn.Spawned)
            {
                return;
            }

            if (MinotaurSettings.canEquipHeavy == true)
            {
                if (pawn.story.traits.HasTrait(RBM_DefOf.RBM_Herculean_Trait) == false)
                {
                    pawn.story.traits.GainTrait(new Trait(RBM_DefOf.RBM_Herculean_Trait));
                }
            }
            else
            {
                if (pawn.story.traits.HasTrait(RBM_DefOf.RBM_Herculean_Trait) == true)
                {
                    Trait pawnTrait = pawn.story.traits.GetTrait(RBM_DefOf.RBM_Herculean_Trait);
                    pawn.story.traits.RemoveTrait(pawnTrait);
                }
            }


        }
    }
}

