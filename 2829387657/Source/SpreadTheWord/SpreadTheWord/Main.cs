using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SpreadTheWord
{

    //Settings
    public class SpreadTheWordSettings : ModSettings
    {
        public bool enableComplexCalculation = false;
        public int numberToRelease = 25;
        public int baseGoodwillNeeded = 100;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableComplexCalculation, "enableComplexCalculation");
            Scribe_Values.Look(ref numberToRelease, "numberToRelease");
            Scribe_Values.Look(ref baseGoodwillNeeded, "baseGoodwillNeeded");
            base.ExposeData();
        }
    }

    //Mod
    [StaticConstructorOnStartup]
    public class SpreadTheWordMod : Mod
    {

        public static SpreadTheWordSettings settings;

        public SpreadTheWordMod(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony("crazedmonkey231.spread.the.word");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            SpreadTheWordMod.settings = this.GetSettings<SpreadTheWordSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Gap();
            listingStandard.Label("-----Base Game Settings-----");
            listingStandard.Gap();
            listingStandard.Label("Base goodwill needed: " + settings.baseGoodwillNeeded);
            settings.baseGoodwillNeeded = (int)listingStandard.Slider(settings.baseGoodwillNeeded, -100f, 100f);
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("Enable complex calculation of pawns needing conversion?", ref settings.enableComplexCalculation, "Complex calculation takes into consideration the number of settlements the faction has. Calculation is the number needed (from slider) multiplied by the amount of colonies of that faction.");
            listingStandard.Gap();
            listingStandard.Label("# of people to release to convert faction: " + settings.numberToRelease);
            settings.numberToRelease = (int)listingStandard.Slider(settings.numberToRelease, 1f, 1000f);
            listingStandard.Gap();
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
            SpreadTheWordMod.settings.Write();
        }

        public override string SettingsCategory()
        {
            return "Spread The Word";
        }

        public override void WriteSettings()
        {

            base.WriteSettings();
        }
    }

    //Faction converstion tracker to check for number of settlements
    public class FactionConversionWorldComponent : WorldComponent
    {
        public FactionConversionWorldComponent(World world) : base(world)
        {
            this.world = world;
        }

        public int getFactionCountOnWorld(Faction faction)
        {
            return this.world.info.factions.Contains(faction.def) ? 1 : 0;
        }
    }

    //Conversion Tracker component storing info
    public class ConversionTrackerComponent : MapComponent
    {

        public Dictionary<string, int> entries = new Dictionary<string, int>();

        public ConversionTrackerComponent(Map map) : base(map)
        {
            this.map = map;
        }

        public void AddEntry(string key, int value)
        {
            if (this.entries.ContainsKey(key))
            {
                this.entries[key] += value;
            }
            else
            {
                this.entries[key] = value;
            }
        }

        public int GetEntry(string key)
        {
            if (this.entries.ContainsKey(key))
            {
                return this.entries[key];
            }

            return 0;

        }

        public void Reset(string key)
        {
            if (this.entries.ContainsKey(key))
            {
                this.entries[key] = 0;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<string, int>(ref this.entries, "entries", LookMode.Value);
        }
    }

    //ConversionTracker Utility class
    public static class ConversionTrackerUtil
    {

        public static void Reset(string key)
        {
            if (isInitialized())
                getComp().Reset(key);
        }

        public static int GetStatAndReset(string key)
        {
            if (isInitialized())
            {
                int valueBeforeReset = GetStat(key);
                Reset(key);
                return valueBeforeReset;
            }
            return 0;
        }

        public static int InsertThenGetStat(string key, int value)
        {
            AddStat(key, value);
            return GetStat(key);
        }

        public static void AddStat(string key, int value)
        {
            if (isInitialized())
                getComp().AddEntry(key, value);
        }

        public static int GetStat(string key)
        {
            if (isInitialized())
                return getComp().GetEntry(key);
            return 0;
        }

        private static bool isInitialized()
        {
            return Find.CurrentMap.GetComponent<ConversionTrackerComponent>() != null;
        }

        private static ConversionTrackerComponent getComp()
        {
            return Find.CurrentMap.GetComponent<ConversionTrackerComponent>();
        }
    }

    //Converted Pawns graph tracker
    public class HistoryAutoRecorderWorker_Proselytizers : HistoryAutoRecorderWorker
    {
        public override float PullRecord()
        {
            return ConversionTrackerUtil.GetStat("crazedmonkey231.spread.the.word");
        }
    }

    public static class StatsFactionUtil
    {
        //Check Ideo goodwill and prisoners released
        public static void CheckCanChangeIdeos(Faction other, Faction player)
        {
            if (other.ideos.PrimaryIdeo == player.ideos.PrimaryIdeo)
                return;
            string key = other.Name;
            int currVal = ConversionTrackerUtil.InsertThenGetStat(key, 1);
            CheckFactionConversion(other, player, key, currVal);
        }

        private static void CheckFactionConversion(Faction other, Faction player, string key, int currVal)
        {
            int numToRelease = SpreadTheWordMod.settings.numberToRelease;

            if (SpreadTheWordMod.settings.enableComplexCalculation)
                numToRelease *= Find.World.GetComponent<FactionConversionWorldComponent>().getFactionCountOnWorld(other);

            if (currVal >= numToRelease && player.RelationWith(other).baseGoodwill >= SpreadTheWordMod.settings.baseGoodwillNeeded)
            {
                ConversionTrackerUtil.Reset(key);
                Find.FactionManager.FirstFactionOfDef(other.def).ideos.SetPrimary(player.ideos.PrimaryIdeo);
                int goodwillChange = 12;
                Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.PeaceTalksSuccess, other.Named(HistoryEventArgsNames.AffectedFaction), goodwillChange.Named(HistoryEventArgsNames.CustomGoodwill)));
                Find.FactionManager.goodwillSituationManager.RecalculateAll(false);
                SendLetter(other);
            }
            else
            {
                Messages.Message(new Message("You converted " + currVal + " out of " + numToRelease + " of " + key + ".", MessageTypeDefOf.NeutralEvent));
            }
        }

        //Send letter the ideo has changed
        private static void SendLetter(Faction other)
        {
            string letterTitle = "Ideoligion Conversion";
            string letterLabel = "Ideoligion Conversion";
            string letterText = other.Name + " has been converted to your ideoligion.";
            ChoiceLetter letter;
            letter = (ChoiceLetter)LetterMaker.MakeLetter((TaggedString)letterLabel, (TaggedString)letterText, LetterDefOf.PositiveEvent);
            letter.title = letterTitle;
            Find.LetterStack.ReceiveLetter((Letter)letter);
        }
    }

    //Def of the interaction mode
    [DefOf]
    public static class STWDefOf
    {

        public static PrisonerInteractionModeDef ConvertThenRelease;

    }

    //Harmony Patch for leaving map
    [HarmonyPatch(typeof(Faction), nameof(Faction.Notify_MemberExitedMap))]
    public static class HarmonyPatch_Tracker_Conversion
    {

        public static void Postfix(Pawn member, bool free, Faction __instance)
        {
            if (member.HostFaction == null)
                return;

            if (__instance.ideos == null || member.HostFaction.ideos == null)
                return;

            if (!member.mindState.AvailableForGoodwillReward)
                return;

            if (!free)
                return;

            if (member.Ideo == Faction.OfPlayer.ideos.PrimaryIdeo)
            {
                ConversionTrackerUtil.InsertThenGetStat("crazedmonkey231.spread.the.word", 1);
                StatsFactionUtil.CheckCanChangeIdeos(__instance, member.HostFaction);
            }
            else
                Messages.Message(new Message("You need to convert people leaving the map in order to spread the word of your ideoligion.", MessageTypeDefOf.NeutralEvent));
        }
    }

    //Harmony Patch for convert then release
    [HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.IdeoConversionAttempt))]
    public static class HarmonyPatch_Tracker_Conversion_Then_Release
    {

        public static void Postfix(bool __result, Pawn ___pawn)
        {
            if (!__result || !___pawn.IsPrisonerOfColony || (___pawn.guest.interactionMode != STWDefOf.ConvertThenRelease || ___pawn.Ideo != Faction.OfPlayer.ideos.PrimaryIdeo))
                return;
            ___pawn.guest.interactionMode = PrisonerInteractionModeDefOf.Release;
        }
    }

    //Work giver warden job patch
    [HarmonyPatch(typeof(WorkGiver_Warden_Convert), nameof(WorkGiver_Warden_Convert.JobOnThing))]
    public static class WorkGiver_Warden_Convert_JobOnThing
    {
        public static void Prefix(Thing t, Pawn pawn, out bool __state)
        {
            Pawn pawn1 = (Pawn)t;
            __state = false;
            if (pawn1 != null && !pawn1.IsPrisonerOfColony)
                return;
            if (pawn1?.guest == null)
                return;
             if (pawn1.guest.interactionMode != STWDefOf.ConvertThenRelease)
                return;
            if (pawn1.Ideo == Faction.OfPlayer.ideos.PrimaryIdeo)
            {
                pawn1.guest.interactionMode = PrisonerInteractionModeDefOf.Release;
            }
            else
            {
                if (pawn1.Ideo != pawn.Ideo && pawn1.guest.ideoForConversion == null)
                    pawn1.guest.ideoForConversion = Faction.OfPlayer.ideos.PrimaryIdeo;
                pawn1.guest.interactionMode = PrisonerInteractionModeDefOf.Convert;
                __state = true;
            }
        }

        public static void Postfix(Thing t, bool __state)
        {
            if (!__state)
                return;
            Pawn pawn = (Pawn)t;
            if (pawn?.guest == null)
                return;
            pawn.guest.interactionMode = STWDefOf.ConvertThenRelease;
        }
    }

    //work giver warden chat patch
    [HarmonyPatch(typeof(WorkGiver_Warden_Chat), nameof(WorkGiver_Warden_Chat.JobOnThing))]
    public static class WorkGiver_Warden_Chat_JobOnThing
    {
        public static void Prefix(Thing t, out bool __state)
        {
            Pawn pawn = (Pawn)t;
            __state = false;
            if (pawn != null && !pawn.IsPrisonerOfColony)
                return;
            if (pawn?.guest == null)
                return;
            if (pawn.guest.interactionMode != STWDefOf.ConvertThenRelease)
                return;
            __state = true;
        }

        public static void Postfix(Thing t, bool __state)
        {
            if (!__state)
                return;
            Pawn pawn = (Pawn)t;
            if (pawn?.guest == null)
                return;
            pawn.guest.interactionMode = STWDefOf.ConvertThenRelease;
        }
    }
}
