using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimTrust.Trade
{
    [DefOf]
    public class HediffDef_Neural : HediffDef
    {
        public static HediffDef NeuralOverdose;
        public static HediffDef NeuralFatigue;
        public static HediffDef NeuralImplant;
        public static ThoughtDef Ascension;

        static void HeDiffDefOf_NeuralOverdose()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDef_Neural));
        }
        static void HeDiffDefOf_NeuralFatigue()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDef_Neural));
        }
        static void HeDiffDefOf_NeuralImplant()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDef_Neural));
        }
      /*  static void HeDiffDefOf_Ascension()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDef_Neural));
        }*/
    }
    /*public class ThoughtDef_Ascension : ThoughtDef
    { 
        public static ThoughtDef Ascension;

        static void ThoughtDefOf_Ascension()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThoughtDef_Ascension));
        }
    }*/

    public static class Methods
    {

        internal static bool debug = false;
        internal static List<Tradeable> cacheNotes = new List<Tradeable>();
        private static readonly FieldInfo tradeables = AccessTools.Field(typeof(TradeDeal), "tradeables");

        public static int TrustFunds;
        public static List<int> LegacySkills = new List<int>();
        public static int PawnXpTotal;
        public static Pawn LegacyPawn;
        public static List<Thing> TrusteeCollectorThings = new List<Thing>();

        public static bool DoExecute(this TradeDeal This)
        {

            Pair<int, int> currencyFmt = Utility.GetCurrencyFmt();
            if (debug)
            {
                Log.Message(currencyFmt.First + "," + currencyFmt.Second);
            }
            UpdateCurrencyCount(This, currencyFmt);
            bool flag = false;
            foreach (Tradeable item in (List<Tradeable>)tradeables.GetValue(This))
            {
                if (item.ActionToDo != 0)
                {
                    flag = true;
                }
                item.ResolveTrade();
            }
            This.Reset();
            if (flag)
            {
                Utility.ResetCacheNotes();
            }
            return flag;
        }

        public static bool CanColonyAffordTrade(TradeDeal This)
        {
            int num = This.CurrencyTradeable.CountPostDealFor(Transactor.Colony);
            int notesBalanceAvaliable = Utility.GetNotesBalanceAvaliable(Transactor.Colony);
            if (num + notesBalanceAvaliable > 0)
            {
                return true;
            }
            return false;
        }

        public static void UpdateCurrencyCount(TradeDeal This, Pair<int, int> currencyfmt)
        {
            This.CurrencyTradeable.ForceTo(currencyfmt.Second);
            int num = Math.Abs(currencyfmt.First);
            Transactor transactor = (currencyfmt.First >= 0) ? Transactor.Trader : Transactor.Colony;
            for (int num2 = cacheNotes.Count - 1; num2 > -1; num2--)
            {
                int num3 = cacheNotes[num2].CountHeldBy(transactor);
                if (debug)
                {
                    Utility.DebugprintfUpdateCurrencyCount(num, num3, transactor);
                }
                if (num3 != 0)
                {
                    if (num < num3)
                    {
                        num3 = num;
                        num = 0;
                    }
                    else
                    {
                        num -= num3;
                    }
                    if (transactor == Transactor.Colony)
                    {
                        num3 = -num3;
                    }
                    if (debug)
                    {
                        Utility.DebugprintfUpdateCurrencyCount(num, num3, transactor);
                    }
                    cacheNotes[num2].ForceTo(num3);
                    if (num == 0)
                    {
                        break;
                    }
                }
            }
            if (debug)
            {
                Utility.DebugOutputTradeables((List<Tradeable>)tradeables.GetValue(This));
            }
        }

        public static void UpdateTrustFunds(int increase)
        {
            Methods.TrustFunds += increase;
        }
        public static void UpdatePawnSkillFromLegacy(Pawn pawn)
        {
            int index = 0;
            int index2 = 0;
            List<string> initSkillRecord = new List<string>() { "Shooting", "Melee", "Construction", "Mining", "Cooking", "Plants", "Animals", "Crafting", "Artistic", "Medical", "Social", "Intellectual" };
            List<SkillRecord> skills = pawn.skills.skills;
            //Log.Message("Pawn selected: " + pawn.Name.ToString());
            int SkillXPBeforeLearn = 0;

            foreach (string item in initSkillRecord)
            {
                //Log.Message("foreach loop item = " + item);
                if (item != "Medical")
                {
                    index2 = skills.FindIndex(x => x.def.defName.ToString() == item);
                }
                else
                {
                    index2 = skills.FindIndex(x => x.def.defName.ToString() == "Medicine");
                }

                var skilltemp = skills[index];
                skills[index] = skills[index2];
                skills[index2] = skilltemp;

                //Log.Message("before if (LegacySkills[index] != 0 " + item);
                if (LegacySkills[index] != 0)
                {
                    //Log.Message("Legacy Skill " + skills[index].def.defName.ToString() + " selected at index ( " + index + ") with xp: " + LegacySkills[index]);
                    SkillXPBeforeLearn =  Methods.SkillXPTotal(skills[index]);
                    
                    skills[index].Learn(Methods.LegacySkills[index] * 0.01f);
                    //Log.Message("pawn skill selected: " + skills[index].def.defName.ToString() + " after learn with xp: " + Methods.SkillXPTotal(skills[index]) + " (+" + ((double)Methods.SkillXPTotal(skills[index])- (double)SkillXPBeforeLearn) + ")");
                }
                index++;
            }
        }
        public static void SaveTrustFunds()
        {
            string TrustName = "";
            if (TrustName == "")
            {
                TrustName = "default";
            }

            string str1 = System.IO.Path.Combine(GenFilePaths.SaveDataFolderPath, "TrustFunds");
            str1.Replace('/', '\\');
            if (!System.IO.Directory.Exists(str1))
            {
                Log.Message("creating folder : " + str1);
                System.IO.Directory.CreateDirectory(str1);
                Log.Message("folder created successfully");
            }

            string orstr2 = System.IO.Path.Combine(str1, TrustName);
            string str2 = orstr2 + ".rwtf";

            SafeSaver.Save(str2, "RWTF", (Action)(() =>
            {
                ScribeMetaHeaderUtility.WriteMetaHeader();
                Log.Message("SaveTrustFunds");
                Scribe_Values.Look<int>(ref TrustFunds, "TrustFunds", 0);

            }));
        }

        public static void SaveLegacySkills()
        {
            string TrustName = "";
            if (TrustName == "")
            {
                TrustName = "default";
            }

            string str1 = System.IO.Path.Combine(GenFilePaths.SaveDataFolderPath, "TrustFunds");
            str1.Replace('/', '\\');
            if (!System.IO.Directory.Exists(str1))
            {
                Log.Message("creating folder : " + str1);
                System.IO.Directory.CreateDirectory(str1);
                Log.Message("folder created successfully");
            }

            string orstr2 = System.IO.Path.Combine(str1, TrustName);
            string str2 = orstr2 + ".rwls";

            SafeSaver.Save(str2, "RWLS", (Action)(() =>
            {
                ScribeMetaHeaderUtility.WriteMetaHeader();
                int xp = 0;
                int index = 0;

                List<string> initSkillRecord = new List<string>() { "Shooting", "Melee", "Construction", "Mining", "Cooking", "Plants", "Animals", "Crafting", "Artistic", "Medical", "Social", "Intellectual" };
                foreach (string item in initSkillRecord)
                {

                    //xp = item.xpSinceLastLevel + item.XpTotalEarned;
                    xp = Methods.LegacySkills[index];
                    if (xp != 0)
                    { Scribe_Values.Look<int>(ref xp, item, 0); }
                    else
                    { Scribe_Values.Look<int>(ref xp, item, 1); }
                    index++;
                }
            }));
        }

        public static int SkillXPTotal(SkillRecord skillRecord)
        {
            int SkillXpTotal = 0;
            SkillXpTotal = (int)(skillRecord.xpSinceLastLevel + skillRecord.XpTotalEarned);
            return SkillXpTotal;
        }
        public static int PawnXPTotal(Pawn pawn)
        {
            PawnXpTotal = 0;
            foreach (SkillRecord item in pawn.skills.skills)
            {
                if (item.TotallyDisabled)
                {
                    PawnXpTotal = +0;
                }
                else
                {
                    PawnXpTotal += SkillXPTotal(item);
                }
            }
            return PawnXpTotal;
        }
        public static float SkillXPTotalPercent(SkillRecord skillRecord, Pawn pawn)
        {
            float SkillXpTotalPercent = 0F;

            if (!skillRecord.TotallyDisabled)
            {
                if (!(PawnXPTotal(pawn) == 0))
                {
                    SkillXpTotalPercent = (float)SkillXPTotal(skillRecord) / PawnXPTotal(pawn);
                }
                else
                {
                    SkillXpTotalPercent = 0F;
                }
            }
            return SkillXpTotalPercent;
        }

        public static string SelectSkillToSafeNIC(Pawn pawn)
        {
            string SelectedSkillString = "";
            SkillRecord SelectedSkill;
            List<SkillRecord> skills = pawn.skills.skills;
            skills.SortByDescending(x => x.XpTotalEarned + x.xpSinceLastLevel);
            foreach (SkillRecord skill in skills)
            {
                if (!skill.TotallyDisabled)
                {
                    SelectedSkillString = skill.def.defName.ToString();
                    SelectedSkill = skill;
                    break;
                }
                else
                {
                }

            }
            return SelectedSkillString;
        }

        public static SkillRecord SelectSkillRecordToSafeNIC(Pawn pawn)
        {
            SkillRecord SelectedSkill;

            List<SkillRecord> skills = pawn.skills.skills;
            skills.SortByDescending(x => x.XpTotalEarned + x.xpSinceLastLevel);
            SelectedSkill = skills[0];

            return SelectedSkill;
        }

        public static void UpdateLegacySkills(string skill, Pawn p, int tier)
        {
            List<string> initSkillRecord = new List<string>() { "Shooting", "Melee", "Construction", "Mining", "Cooking", "Plants", "Animals", "Crafting", "Artistic", "Medical", "Social", "Intellectual" };
            if (skill == "Medicine") { skill = "Medical"; }
            int index = initSkillRecord.FindIndex(x => x == skill);
            const double maxSkillXP = 297000d;
            SkillRecord SkillRecord = p.skills.skills[0];
            double skillXPtotallearned = Methods.SkillXPTotal(SkillRecord);
            double xpmult = 0.075;
            if (tier != 1 && tier != 2)
                { 
                    tier = 1; 
                }
            if (!Methods.LegacySkills.Count.Equals(0))
            {
                double skillXPtolearn = skillXPtotallearned - LegacySkills[index];

                if (skillXPtolearn > 0)
                {
                    double skillSafePercentage = (Math.Pow(((double)(maxSkillXP - LegacySkills[index]) / maxSkillXP), 2));
                    double skillXPtosafe = (Math.Pow(((double)(maxSkillXP - LegacySkills[index]) / maxSkillXP), 2) * skillXPtolearn * (xpmult * tier));
                    Log.Message("LegacySkill " + skill + " before save: " + LegacySkills[index]);
                    Log.Message("skillXPtosafe: " + (int)skillXPtosafe + " with Percentage " + skillSafePercentage * 100 + "%");
                    LegacySkills[index] += (int)skillXPtosafe;
                    Log.Message("LegacySkill after save: " + LegacySkills[index]);
                    MoteMaker.ThrowText(p.DrawPos, p.Map, skill + " +" + (int)skillXPtosafe + " XP", Color.green, 5f);
                }
            }
        }

        public static void InitiateLegacySkillSave()
        {
            string TrustName = "";
            if (TrustName == "")
            {
                TrustName = "default";
            }

            string str1 = System.IO.Path.Combine(GenFilePaths.SaveDataFolderPath, "TrustFunds");
            str1.Replace('/', '\\');
            if (!System.IO.Directory.Exists(str1))
            {
                Log.Message("creating folder : " + str1);
                System.IO.Directory.CreateDirectory(str1);
                Log.Message("folder created successfully");
            }

            string orstr2 = System.IO.Path.Combine(str1, TrustName);
            string str2 = orstr2 + ".rwls";

            SafeSaver.Save(str2, "RWLS", (Action)(() =>
            {
                Log.Message("InitiateLegacySkillSave");
                ScribeMetaHeaderUtility.WriteMetaHeader();
                int xp = 0;
                List<string> initSkillRecord = new List<string>() { "Shooting", "Melee", "Construction", "Mining", "Cooking", "Plants", "Animals", "Crafting", "Artistic", "Medical", "Social", "Intellectual" };

                foreach (string item in initSkillRecord)
                {
                    Scribe_Values.Look<int>(ref xp, item, 1);
                }
            }));
        }
        public static void NicOverdoseToPawn(Pawn p)
        {
            BodyPartRecord part = p.RaceProps.body.corePart;
            Hediff hediff = HediffMaker.MakeHediff(HediffDef_Neural.NeuralOverdose, p);
            hediff.Severity = 1;
            hediff.Part = part.parts[11].parts[0].parts[0].parts[0];
            p.health.AddHediff(hediff);

            Hediff_Injury hediff_injury = (Hediff_Injury)HediffMaker.MakeHediff(HediffDefOf.Stab, p, null);
            hediff_injury.Part = part.parts[11].parts[0];
            hediff_injury.sourceBodyPartGroup = BodyPartGroupDefOf.FullHead;
            hediff_injury.sourceHediffDef = HediffDefOf.Stab;
            hediff_injury.Severity = 15;
            hediff_injury.Part.def.bleedRate = 5;
            p.health.hediffSet.hediffs.Add(hediff_injury);
        }

        public static void NicFatigueToPawn(Pawn p)
        {
            BodyPartRecord part = p.RaceProps.body.corePart;
            Hediff hediff = HediffMaker.MakeHediff(HediffDef_Neural.NeuralFatigue, p);
            hediff.Severity = 1;
            hediff.Part = part.parts[11].parts[0].parts[0].parts[0];
            p.health.AddHediff(hediff);
        }

        public static int CalculateColonyValuables()
        {
            string test = "";
            int totalValue = 0;
            string end = "";
            if (Find.CurrentMap.IsPlayerHome)
            {
                foreach (Thing placedArt in Find.CurrentMap.listerThings.AllThings)
                {
                    if (placedArt.def.defName.StartsWith("Sculpture"))
                    {
                        CompQuality compQuality = placedArt.GetInnerIfMinified().TryGetComp<CompQuality>();
                        if (compQuality.Quality != QualityCategory.Awful && compQuality.Quality != QualityCategory.Poor && compQuality.Quality != QualityCategory.Normal)
                        {
                            test += placedArt.def.defName;
                            test += " (placed): ";
                            test += placedArt.MarketValue * placedArt.stackCount;
                            totalValue += (int)(placedArt.stackCount * placedArt.MarketValue);
                            test += "\n";
                            TrusteeCollectorThings.Add(placedArt);
                        }
                    }
                }

                foreach (Thing item in TradeUtility.AllLaunchableThingsForTrade(Find.AnyPlayerHomeMap))
                {
                    if (item.def.defName == "Silver" || item.def.defName == "Gold" || item.def.defName == "BankNote")
                    {
                        test += item.def.defName;
                        test += ": ";
                        test += item.MarketValue * item.stackCount;
                        totalValue += (int)(item.stackCount * item.MarketValue);
                        test += "\n";
                        TrusteeCollectorThings.Add(item);
                    }
                    if (item.GetInnerIfMinified().def.defName.StartsWith("Sculpture"))
                    {
                        CompQuality compQuality = item.GetInnerIfMinified().TryGetComp<CompQuality>();
                        if (compQuality.Quality != QualityCategory.Awful && compQuality.Quality != QualityCategory.Poor && compQuality.Quality != QualityCategory.Normal)
                        {
                            test += item.GetInnerIfMinified().def.defName;
                            test += ": ";
                            test += compQuality.Quality.GetLabelShort();
                            test += ": ";
                            test += item.GetInnerIfMinified().MarketValue;
                            totalValue += (int)(item.GetInnerIfMinified().stackCount * item.GetInnerIfMinified().MarketValue);
                            test += "\n";
                            TrusteeCollectorThings.Add(item);
                        }
                    }
                }
            }
            end = test + totalValue;
            //Log.Message(end);
            int netGain = (int)(totalValue * 0.6);
            //Log.Message("Net gain on Trustee collection (60%): " + netGain.ToString());
            return (netGain / 1000);
        }

        public static void TrusteeTakeThings(List<Thing> ThingsToDestroy)
        {
            foreach (Thing thing in ThingsToDestroy)
            {
                if (!thing.Destroyed)
                {
                    thing.Destroy();
                }
                if (!thing.Discarded)
                {
                    thing.Discard();
                }
            }
        }
        public static Boolean ColonyHasNeuralImplant(Map map)
        {
            bool ColonyHasNeuralImplant = false;
            if (map.IsPlayerHome)
            {
                foreach (Pawn pawn in map.mapPawns.FreeColonists)
                {
                    var NeuralImplantOnPawn = pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDef_Neural.NeuralImplant);
                    if (NeuralImplantOnPawn != null)
                    {
                        ColonyHasNeuralImplant = true;
                        break;
                    }
                }
                return ColonyHasNeuralImplant;
            }
            return false;
        }
    }
     
    public class LoadTrustFunds
    {

        static string TrustName;
        static string LegacySkillsName;

     
        public static void LoadTrust()
        {
            Log.Message("Loading trust");
            TrustName = "default";
            string file = System.IO.Path.Combine(System.IO.Path.Combine(GenFilePaths.SaveDataFolderPath, "TrustFunds"), TrustName + ".rwtf");
            if (!System.IO.File.Exists(file))
            {
                Log.Error("File Doesnt exist");
                return;
            }

            Scribe.loader.InitLoading(file);
            Scribe_Values.Look<int>(ref Methods.TrustFunds, "TrustFunds", 0);
            Scribe.loader.FinalizeLoading();
        }
        public static void LoadLegacySkills()
        {
            Log.Message("Loading Legacy Skills");
            LegacySkillsName = "default";
            string file = System.IO.Path.Combine(System.IO.Path.Combine(GenFilePaths.SaveDataFolderPath, "TrustFunds"), LegacySkillsName + ".rwls");
            if (!System.IO.File.Exists(file))
            {
                Log.Message("File Doesnt exist.. creating..");
                Methods.InitiateLegacySkillSave();
            }
            
            List<string> initSkillRecord = new List<string>() { "Shooting", "Melee", "Construction", "Mining", "Cooking", "Plants", "Animals", "Crafting", "Artistic", "Medical", "Social", "Intellectual" };
            //Log.Message("initiate loading LegacySkills");
            int index = 0;
            Methods.LegacySkills = new List<int> { };

            foreach (string item in initSkillRecord)
            {
                int temploadedxp = 0;

                Scribe.loader.InitLoading(file);
                Scribe_Values.Look<int>(ref temploadedxp, item, 0);
                Scribe.loader.FinalizeLoading();
                
                Methods.LegacySkills.Add(temploadedxp);
                index++;
            }
        }
    }
}