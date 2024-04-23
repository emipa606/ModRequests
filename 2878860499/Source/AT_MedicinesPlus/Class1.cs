using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AT_MedicinesPlus
{
    public class AT_MedicinesPlus_class : Mod
    {
        public static Harmony harmonyInst;

        public AT_MedicinesPlus_class(ModContentPack content)
            : base(content)
        {
            harmonyInst = new Harmony("AT_MedicinesPlus.Atlas");
        }
    }
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            global::AT_MedicinesPlus.AT_MedicinesPlus_class.harmonyInst.PatchAll();
        }
    }
    public class Hediff_Addictol : HediffWithComps
    {
        public override void Tick()
        {
            base.Tick();
            if (!pawn.IsHashIntervalTick(2500))
            {
                return;
            }
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (hediff.def.IsAddiction && hediff.def.defName != "LuciferiumAddiction" && hediff.def.defName != "AddictolAddiction")
                {
                    hediff.Severity -= 0.005f;
                    break;
                }
            }
        }
    }
    public static class HediffDefOf
    {
        public static HediffDef AniracetamHigh;

        public static HediffDef AniracetamBrainFog;

        public static HediffDef HypnotolHigh;

        static HediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(AT_MedicinesPlus.HediffDefOf));
        }
    }
    public class Hediff_AniracetamHigh : Hediff_High
    {
        public override void PostRemoved()
        {
            if (!pawn.health.hediffSet.HasHediff(AT_MedicinesPlus.HediffDefOf.AniracetamBrainFog))
            {
                Hediff hediff = HediffMaker.MakeHediff(AT_MedicinesPlus.HediffDefOf.AniracetamBrainFog, pawn);
                hediff.Severity = 0.2f;
                pawn.health.AddHediff(hediff);
            }
            else
            {
                pawn.health.hediffSet.GetFirstHediffOfDef(AT_MedicinesPlus.HediffDefOf.AniracetamBrainFog).Severity += 0.2f;
            }
        }
    }
    internal class Hediff_AniracetamFog : HediffWithComps
    {
        public override bool Visible => !pawn.health.hediffSet.HasHediff(HediffDefOf.AniracetamHigh) && base.Visible;
    }
    public class Hediff_Clozapine : HediffWithComps
    {
        public override void Tick()
        {
            base.Tick();
            if (pawn.InMentalState && pawn.MentalState.def.IsExtreme && pawn.IsHashIntervalTick(150) && pawn.MentalState.Age >= pawn.MentalStateDef.minTicksBeforeRecovery / 10)
            {
                pawn.MentalState.RecoverFromState();
            }
        }
    }
    public class Hediff_HypnotolAddiction : Hediff_Addiction
    {
        private Random rnd = new Random();

        private Faction oldFaction;

        private PawnKindDef oldPawnKind;

        public override void PostMake()
        {
            if (pawn.IsPrisonerOfColony)
            {
                base.PostMake();
                oldFaction = pawn.Faction;
                oldPawnKind = pawn.kindDef;
                Faction.OfPlayer.TryAffectGoodwillWith(oldFaction, -15, true, true, (HistoryEventDef)null, (GlobalTargetInfo?)null);
                pawn.SetFaction(Faction.OfPlayer);
            }
        }

        public override void PostRemoved()
        {
            if (oldFaction != null)
            {
                base.PostRemoved();
                pawn.SetFaction(oldFaction);
                pawn.kindDef = oldPawnKind;
                pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(AT_MedicinesPlus.HediffDefOf.HypnotolHigh));
                if (rnd.Next(2) == 0)
                {
                    Find.LetterStack.ReceiveLetter("LetterLabel_HypnotolWornOff".Translate(), "LetterText_HypnotolWornOff".Translate(pawn.LabelShort), LetterDefOf.NeutralEvent, (LookTargets)pawn, (Faction)null, (Quest)null, (List<ThingDef>)null, (string)null);
                    return;
                }
                Find.LetterStack.ReceiveLetter("LetterLabel_HypnotolBerserk".Translate(), "LetterText_HypnotolBerserk".Translate(pawn.LabelShort), LetterDefOf.NegativeEvent, (LookTargets)pawn, (Faction)null, (Quest)null, (List<ThingDef>)null, (string)null);
                pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, (string)null, true, false, false,(Pawn)null,false,false,false);
            }
        }
    }
    internal static class Patch_DrugPolicy
    {
        [HarmonyPatch(typeof(DrugPolicy), "ExposeData")]
        public class ExposeData
        {
            [HarmonyPostfix]
            internal static void Prefix(DrugPolicy __instance, List<DrugPolicyEntry> ___entriesInt)
            {
                bool flag = Scribe.mode != LoadSaveMode.PostLoadInit;
                if (Scribe.mode != LoadSaveMode.PostLoadInit)
                {
                    return;
                }
                List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
                foreach (ThingDef itemSel in allDefsListForReading)
                {
                    if (itemSel.category == ThingCategory.Item && itemSel.IsDrug && !___entriesInt.Exists((DrugPolicyEntry drugPol) => drugPol.drug == itemSel))
                    {
                        DrugPolicyEntry item = new DrugPolicyEntry
                        {
                            drug = itemSel,
                            allowedForAddiction = true
                        };
                        ___entriesInt.Add(item);
                    }
                }
                ___entriesInt.RemoveAll(delegate (DrugPolicyEntry e)
                {
                    object obj = e?.drug?.GetCompProperties<CompProperties_Drug>();
                    return obj == null;
                });
                ___entriesInt.SortBy((DrugPolicyEntry e) => e.drug.GetCompProperties<CompProperties_Drug>().listOrder);
            }
        }
    }
}
