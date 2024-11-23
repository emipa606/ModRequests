using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace HDream
{
    public class BodyPartHediffWishDef : SearchHediffWishDef
    {

        // can only check on pawn
        public bool canReplaceNaturalPart = false;
        public bool shouldBeAnUpgrade = true;
        public bool shouldBeMissingBodyPart = false;

        // cached but check for item to replace on pawn
        public bool replaceToNaturalPart = false;

        // cached
        public bool shouldBeBetterThanNatural = false;
        public float minimalEfficiency = 0;
        public List<ThingDef> excludeIngredient;

        public string bodyPart_Key = "{BodyPart}";

        private List<RecipeDef> cachedRecipeDef = null;
        public List<RecipeDef> CachedRecipeDef
        {
            get
            { 
                if(cachedRecipeDef == null)
                {
                    cachedRecipeDef = new List<RecipeDef>(DefDatabase<RecipeDef>.AllDefsListForReading.Where(recipe => recipe.appliedOnFixedBodyParts != null).ToList());
                    for (int i = cachedRecipeDef.Count - 1; i >= 0; i--)
                    {
                        if (cachedRecipeDef[i].addsHediff != null)
                        {
                            if (replaceToNaturalPart
                                || cachedRecipeDef[i].addsHediff.addedPartProps == null
                                || cachedRecipeDef[i].addsHediff.addedPartProps.partEfficiency < minimalEfficiency
                                || (shouldBeBetterThanNatural && !cachedRecipeDef[i].addsHediff.addedPartProps.betterThanNatural))
                            {
                                cachedRecipeDef.RemoveAt(i);
                                continue;
                            }
                        }
                        else
                        {
                            if (minimalEfficiency > 1 || shouldBeBetterThanNatural)
                            {
                                cachedRecipeDef.RemoveAt(i);
                                continue;
                            }
                        }
                        if (excludeIngredient == null) continue;
                        for (int j = 0; j < excludeIngredient.Count; j++)
                        {
                            if (cachedRecipeDef[i].IsIngredient(excludeIngredient[j]))
                            {
                                cachedRecipeDef.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
                return cachedRecipeDef; 
            }
        }

        protected override List<HediffDef> SearchedDef
        {
            get
            {
                List<HediffDef> hediffDefs = new List<HediffDef>();
                if (!replaceToNaturalPart)
                {
                    List<HediffDef> hediffs = base.SearchedDef;
                    for (int j = 0; j < hediffs.Count; j++)
                    {
                        if (hediffs[j].addedPartProps != null
                            && (!shouldBeBetterThanNatural || hediffs[j].addedPartProps.betterThanNatural)
                            && hediffs[j].addedPartProps.partEfficiency >= minimalEfficiency)
                            hediffDefs.Add(hediffs[j]);
                    }
                }
                return hediffDefs;
            }
        }

        protected override float GetChance(Pawn pawn, float chance)
        {
            float baseChance = base.GetChance(pawn, chance);
            if (baseChance == 0) return 0;
            // not sure if should check for canReplaceNaturalPart in this condition
            if (!shouldBeMissingBodyPart && !replaceToNaturalPart && !shouldBeAnUpgrade && canReplaceNaturalPart) return baseChance;
            int count = 0;
            List<BodyPartRecord> findPart = new List<BodyPartRecord>();
            List<Hediff> miss = pawn.health.hediffSet.hediffs.FindAll(hediff => hediff is Hediff_MissingPart);
            List<Hediff> added = pawn.health.hediffSet.hediffs.FindAll(hediff => hediff is Hediff_AddedPart);
            BodyPartRecord partDef;
            for (int i = 0; i < miss.Count; i++)
            {
                for (int j = 0; j < miss.Count; j++)
                {
                    if (miss[i].Part.parent == miss[j].Part) break;
                    if (j == miss.Count - 1 && (added.Find(hed => hed.Part == miss[i].Part || hed.Part == miss[i].Part.parent) == null))
                    {
                        for (int k = 0; k < CachedRecipeDef.Count; k++)
                        {
                            partDef = HediffWish_Utility.AllParentPartRecordRecursive(miss[i].Part, true).Find(part => CachedRecipeDef[k].appliedOnFixedBodyParts.Contains(part.def));
                            if(partDef != null && !findPart.Contains(partDef))
                            {
                                findPart.Add(partDef);
                                count++;
                                break;
                            }
                        }
                    }
                }
            }
            if (shouldBeMissingBodyPart) { 
                if (count < amountNeeded) return 0;
                return baseChance;
            } 
            added = pawn.health.hediffSet.hediffs.FindAll(hediff => hediff is Hediff_AddedPart);
            for (int i = 0; i < added.Count; i++)
            {
                if (findPart.Contains(added[i].Part)) continue;
                for (int j = 0; j < CachedRecipeDef.Count; j++)
                {
                    if (CachedRecipeDef[j].appliedOnFixedBodyParts.Contains(added[i].Part.def))
                    {
                        if (replaceToNaturalPart)
                        {
                            if (shouldBeAnUpgrade && added[i].def.addedPartProps.betterThanNatural) continue;
                            findPart.Add(added[i].Part);
                            count++;
                            break;
                        }
                        else if (shouldBeAnUpgrade
                            && (CachedRecipeDef[j].addsHediff != null && CachedRecipeDef[j].addsHediff.addedPartProps.partEfficiency > added[i].def.addedPartProps.partEfficiency)
                                || (CachedRecipeDef[j].addsHediff == null && 1 > added[i].def.addedPartProps.partEfficiency)) 
                        {
                            findPart.Add(added[i].Part);
                            count++;
                            break;
                        }
                    }
                }
            }
            if (!canReplaceNaturalPart)
            {
                if (count < amountNeeded) return 0;
                return baseChance;
            }
            List<BodyPartRecord> naturalPart = new List<BodyPartRecord>(pawn.RaceProps.body.AllParts);
            List<BodyPartRecord> parts;
            for (int i = 0; i < miss.Count; i++) {
                parts = HediffWish_Utility.AllParentPartRecordRecursive(miss[i].Part, true);
                for (int j = 0; j < parts.Count; j++)/* if(naturalPart.Contains(parts[j]))*/ naturalPart.Remove(parts[j]); 
            }
            for (int i = 0; i < added.Count; i++) /*if (naturalPart.Contains(added[i].Part))*/ naturalPart.Remove(added[i].Part);
            for (int i = 0; i < naturalPart.Count; i++)
            {
                if (findPart.Contains(naturalPart[i])) continue;
                for (int j = 0; j < CachedRecipeDef.Count; j++)
                {
                    if (CachedRecipeDef[j].addsHediff == null) continue;
                    if (!shouldBeAnUpgrade || CachedRecipeDef[j].addsHediff.addedPartProps.betterThanNatural)
                    {
                        findPart.Add(naturalPart[i]);
                        count++;
                        break;
                    }
                }
            }
            if (count < amountNeeded) return 0;
            return baseChance;
        }
    }
}
