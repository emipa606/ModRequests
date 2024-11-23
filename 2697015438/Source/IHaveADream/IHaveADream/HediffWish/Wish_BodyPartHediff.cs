using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace HDream
{
    public class Wish_BodyPartHediff : Wish_Hediff
    {
        public new BodyPartHediffWishDef Def => (BodyPartHediffWishDef)def;

        protected List<HediffDef> hediffToAdd = new List<HediffDef>();
        protected List<BodyPartRecord> affectedPart = new List<BodyPartRecord>();
        protected List<int> ridIndex = new List<int>();
        protected List<int> neededIndex = new List<int>();
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref hediffToAdd, "hediffToAdd");
            Scribe_Collections.Look(ref affectedPart, "affectedPart", LookMode.BodyPart);
            Scribe_Collections.Look(ref ridIndex, "checkerIndex");
            Scribe_Collections.Look(ref neededIndex, "neededIndex");
        }

        //protected override bool MatchingGetRidAtIndex(int index, Hediff hediff)
        //{
        //    return base.MatchingGetRidAtIndex(index, hediff) && hediff.Part != null && hediffPartToGetRid[hediff.Part.def] == index;
        //}
        //protected override void HediffRidNegate()
        //{
        //    base.HediffRidNegate();
        //    hediffPartToGetRid = null;
        //}
        //protected override void HediffRidKeepOnly(int index)
        //{
        //    base.HediffRidKeepOnly(index);
        //    foreach(BodyPartDef part in hediffPartToGetRid.Keys)
        //    {
        //        if(hediffPartToGetRid[part] == index) 
        //        {
        //            hediffPartToGetRid.Remove(part);
        //            break;
        //        }
        //    }
        //}
        //protected override void HediffRidRemoveFromSpecificWish(Wish_Hediff wish)
        //{
        //    base.HediffRidRemoveFromSpecificWish(wish);
        //    hediffPartToGetRid.Remove((wish as Wish_BodyPartHediff).hediffPartToGetRid.First().Key);
        //}

        protected override bool MatchingNeededAtIndex(int index, Hediff hediff)
        {
            if (!base.MatchingNeededAtIndex(index, hediff)) return false;
            int matchAtIndex = neededIndex.IndexOf(index);
            if (matchAtIndex >= 0
                && hediff.def == hediffToAdd[matchAtIndex]
                && hediff.Part == affectedPart[matchAtIndex])
                    return true;

            return false;
        }
        protected override bool DidGetRidAtIndex(int index, List<Hediff> matchingHediffDef)
        {
            if (!base.DidGetRidAtIndex(index, matchingHediffDef)) return false;
            int matchAtIndex = ridIndex.IndexOf(index);

            if (matchAtIndex < 0) return false;
            if (matchingHediffDef.Find(hed => hed.Part == affectedPart[matchAtIndex]) == null
                || (hediffToGetRid[index] == HediffDefOf.MissingBodyPart
                    && pawn.health.hediffSet.hediffs.Find(hed => hed is Hediff_AddedPart && HediffWish_Utility.AllParentPartRecordRecursive(affectedPart[matchAtIndex], true).Contains(hed.Part)) != null))
                        return true;

            return false;
        }
        protected override List<HediffWishInfo> PrimeHediffInfo()
        {
            List<HediffWishInfo> primeWishInfos = base.PrimeHediffInfo();
            List<RecipeDef> recipes = Def.CachedRecipeDef;

            List<HediffWishInfo> sortedWishInfos = new List<HediffWishInfo>();

            List<Hediff> hediffsMissingPart = new List<Hediff>();
            List<Hediff> hediffPartAdded = new List<Hediff>();

            List<BodyPartRecord> allParts = pawn.RaceProps.body.AllParts;
            List<BodyPartRecord> missingPartRecords = new List<BodyPartRecord>();
            List<BodyPartRecord> havingPartRecords = new List<BodyPartRecord>();

            // find hediff for missingPartHediffs & hediffPartAdded
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (hediffs[i] is Hediff_MissingPart) hediffsMissingPart.Add(hediffs[i]);
                else if (hediffs[i] is Hediff_AddedPart) hediffPartAdded.Add(hediffs[i]);
            }
            // missingPartRecords : add the most upward missing part record
            for (int i = 0; i < hediffsMissingPart.Count; i++)
            {
                for (int j = 0; j < hediffsMissingPart.Count; j++)
                {
                    if (hediffsMissingPart[i].Part.parent == hediffsMissingPart[j].Part) break;
                    if(j == hediffsMissingPart.Count - 1)
                    {
                        missingPartRecords.Add(hediffsMissingPart[i].Part);
                    }
                }
            }
            // missingPartRecords : remove missing part record if that part is missing du to an added part
            for (int i = 0; i < hediffPartAdded.Count; i++)
            {
                for (int j = missingPartRecords.Count - 1; j >= 0 ; j--)
                {
                    if (HediffWish_Utility.AllParentPartRecordRecursive(missingPartRecords[j]).Contains(hediffPartAdded[i].Part))
                    {
                        missingPartRecords.RemoveAt(j);
                    }
                }
            }
            // missingPartRecords : add all missing child part
            int count = missingPartRecords.Count;
            for (int i = 0; i < count; i++)
            {
                missingPartRecords.AddRange(HediffWish_Utility.AllChildPartRecordRecursive(missingPartRecords[i]));
            }
            // havingPartRecords : add all part that not in missingPartRecords
            havingPartRecords = allParts.Where(part => !missingPartRecords.Contains(part)).ToList();

            bool shouldContinue;
            for (int h = 0; h < allParts.Count; h++)
            {
                for (int i = 0; i < recipes.Count; i++)
                {
                    if (!recipes[i].appliedOnFixedBodyParts.Contains(allParts[h].def)) continue;
                    shouldContinue = false;
                    // check if recipe is usable (pawn have the parent of recipe body part)
                    if (!havingPartRecords.Contains(allParts[h])
                        && !havingPartRecords.Contains(allParts[h].parent))
                             continue;

                    //Case natural body part recipe
                    if (recipes[i].addsHediff == null)
                    {
                        //NonNaturalBodyPartCase
                        if (!Def.shouldBeMissingBodyPart)
                        {
                            for (int j = 0; j < hediffPartAdded.Count; j++)
                            {
                                if(allParts[h] == hediffPartAdded[j].Part && (!Def.shouldBeAnUpgrade || 1 > hediffPartAdded[j].def.addedPartProps.partEfficiency))
                                {
                                    SetUpData(null, hediffPartAdded[j].def, allParts[h]);
                                    shouldContinue = true;
                                    break;
                                }
                            }
                            if (shouldContinue) continue;
                        }
                        //MissingBodyPartCase;
                        for (int j = 0; j < missingPartRecords.Count; j++)
                        {
                            if (!HediffWish_Utility.AllParentPartRecordRecursive(missingPartRecords[j], true).Contains(allParts[h])) continue;
                            SetUpData(null, hediffsMissingPart.Where(hed => hed.Part == missingPartRecords[j]).First().def, allParts[h]);
                            break;
                        }
                        // NaturalBodyPartCase (no need since it would be the same as recipe)
                    }
                    // Case non natural body part recipe
                    else
                    {
                        for (int j = 0; j < primeWishInfos.Count; j++)
                        {

                            if (recipes[i].addsHediff != primeWishInfos[j].def) continue;

                            // MissingBodyPartCase
                            for (int k = 0; k < missingPartRecords.Count; k++)
                            {
                                if (!HediffWish_Utility.AllParentPartRecordRecursive(missingPartRecords[k], true).Contains(allParts[h])) continue;
                                //hediffsMissingPart.Where(hed => hed.Part == missingPartRecords[k]).First().def
                                SetUpData(recipes[i].addsHediff, null, allParts[h], sortedWishInfos.Count);
                                sortedWishInfos.Add(primeWishInfos[j]);
                                shouldContinue = true;
                                break;
                            }
                            if (shouldContinue) break;

                            if (!Def.shouldBeMissingBodyPart)
                            {
                                // NonNaturalBodyPartCase
                                for (int k = 0; k < hediffPartAdded.Count; k++)
                                {
                                    if (allParts[h] == hediffPartAdded[k].Part && (!Def.shouldBeAnUpgrade || recipes[i].addsHediff.addedPartProps.partEfficiency > hediffPartAdded[k].def.addedPartProps.partEfficiency))
                                    {
                                        // hediffPartAdded[k].def
                                        SetUpData(recipes[i].addsHediff, null, allParts[h], sortedWishInfos.Count);
                                        sortedWishInfos.Add(primeWishInfos[j]);
                                        shouldContinue = true;
                                        break;
                                    }
                                }
                                if (shouldContinue) break;

                                // NaturalBodyPartCase (last possible case so no need to check)
                                if (Def.canReplaceNaturalPart && (!Def.shouldBeAnUpgrade || recipes[i].addsHediff.addedPartProps.betterThanNatural))
                                {
                                    SetUpData(recipes[i].addsHediff, null, allParts[h], sortedWishInfos.Count);
                                    sortedWishInfos.Add(primeWishInfos[j]);
                                }
                            }
                        }
                    }
                }
            }
            if (Def.hediffInfos != null) sortedWishInfos.AddRange(Def.hediffInfos);
            return sortedWishInfos;
        }

        private void SetUpData(HediffDef toAdd, HediffDef toRemove, BodyPartRecord toPart, int indexNeeded = -1)
        {
            if (toAdd == null && toRemove == null) return;
            hediffToAdd.Add(toAdd);
            affectedPart.Add(toPart);
            if(toAdd == null)
            {
                if (hediffToGetRid == null) hediffToGetRid = new List<HediffDef>();
                ridIndex.Add(hediffToGetRid.Count);
                hediffToGetRid.Add(toRemove);
            }
            else ridIndex.Add(-1);
            neededIndex.Add(indexNeeded);
        }

        public override List<Texture2D> DreamIcon()
        {
            if (hediffsNeeded.NullOrEmpty() || Def.CachedRecipeDef.NullOrEmpty()) return null;
            List<Texture2D> icons = new List<Texture2D>();
            List<RecipeDef> recipes = Def.CachedRecipeDef.Where(rec => !rec.ingredients.NullOrEmpty() && hediffsNeeded.Find(hed => hed.def == rec.addsHediff) != null).ToList();
            for (int i = 0; i < recipes.Count; i++)
            {
                if (recipes[i].fixedIngredientFilter?.AnyAllowedDef?.uiIcon != null) icons.Add(recipes[i].fixedIngredientFilter.AnyAllowedDef.uiIcon);
            }
            return icons;
        }

        protected override string FormateList(string text, string separator)
        {
            Dictionary<string, string> partSorted = new Dictionary<string, string>();
            string listing = "";
            if(hediffsNeeded != null)
            {
                for (int i = 0; i < hediffsNeeded.Count; i++)
                {
                    if (neededIndex.Contains(i))
                    {
                        if (!partSorted.ContainsKey(affectedPart[neededIndex.IndexOf(i)].Label)) partSorted.Add(affectedPart[neededIndex.IndexOf(i)].Label, "");
                        partSorted[affectedPart[neededIndex.IndexOf(i)].Label] += hediffsNeeded[i].def.label + separator;
                    }
                    else
                    {
                        listing += hediffsNeeded[i].def.label;
                        if (i != hediffsNeeded.Count - 1) listing += separator;
                    }
                }
            }
            text = text.Replace(Def.covetedObjects_Key, listing);
            listing = "";
            if(HediffToGetRid != null)
            {
                for (int i = 0; i < HediffToGetRid.Count; i++)
                {
                    if (ridIndex.Contains(i))
                    {
                        if (!partSorted.ContainsKey(affectedPart[ridIndex.IndexOf(i)].Label)) partSorted.Add(affectedPart[ridIndex.IndexOf(i)].Label, "");
                        partSorted[affectedPart[ridIndex.IndexOf(i)].Label] += affectedPart[ridIndex.IndexOf(i)].Label + separator;
                    }
                    else
                    {
                        listing += HediffToGetRid[i].label;
                        if (i != HediffToGetRid.Count - 1) listing += separator;
                    }
                }
            }
            text = text.Replace(Def.toRemove_Key, listing);
            listing = "";
            foreach (var desc in partSorted)
            {
                listing += "\n" + desc.Key + " : " + desc.Value.Remove(desc.Value.Length - separator.Length);
            }
            text = text.Replace(Def.bodyPart_Key, listing);
            return text;
        }
    }
}
