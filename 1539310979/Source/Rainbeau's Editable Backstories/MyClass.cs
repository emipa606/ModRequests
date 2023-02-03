using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace REB_Code {
	
	[StaticConstructorOnStartup]
	internal static class REB_Initializer {
		private static Dictionary<PawnNameCategory, REB_NameBank> banks;
		public static List<string> NamesFirstFemale = new List<string>();
		public static List<string> NamesFirstMale = new List<string>();
		public static List<string> NamesLast = new List<string>();
		public static List<string> NamesNicksFemale = new List<string>();
		public static List<string> NamesNicksMale = new List<string>();
		public static List<string> NamesNicksUnisex = new List<string>();
		public static List<string> NamesSetsFemale = new List<string>();
		public static List<string> NamesSetsMale = new List<string>();
		public static List<string> FS1Female = new List<string>();
		public static List<string> FS1Male = new List<string>();
		public static List<string> FS1Last = new List<string>();
		public static List<string> FS2Female = new List<string>();
		public static List<string> FS2Male = new List<string>();
		public static List<string> FS2Last = new List<string>();
		public static List<string> FS3Female = new List<string>();
		public static List<string> FS3Male = new List<string>();
		public static List<string> FS3Last = new List<string>();
		public static List<string> FS4Female = new List<string>();
		public static List<string> FS4Male = new List<string>();
		public static List<string> FS4Last = new List<string>();
		public static List<string> FS5Female = new List<string>();
		public static List<string> FS5Male = new List<string>();
		public static List<string> FS5Last = new List<string>();
		public static List<string> FS6Female = new List<string>();
		public static List<string> FS6Male = new List<string>();
		public static List<string> FS6Last = new List<string>();
		public static List<string> FS7Female = new List<string>();
		public static List<string> FS7Male = new List<string>();
		public static List<string> FS7Last = new List<string>();
		public static List<string> FS8Female = new List<string>();
		public static List<string> FS8Male = new List<string>();
		public static List<string> FS8Last = new List<string>();
		public static List<string> FS9Female = new List<string>();
		public static List<string> FS9Male = new List<string>();
		public static List<string> FS9Last = new List<string>();
		public static int adultCount;
		public static int adultNSCount;
		public static int childCount;
		public static int childNSCount;
		public static int adultCountOther;
		public static int adultNSCountOther;
		public static int childCountOther;
		public static int childNSCountOther;
		public static int fullCount;
		public static int fullBioCount;
		public static int trigger;
		static REB_Initializer() {
			HarmonyInstance harmony = HarmonyInstance.Create("net.rainbeau.rimworld.mod.backstories");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
			LongEventHandler.QueueLongEvent(Setup, "LibraryStartup", false, null);
		}
		public static void Setup() {
			foreach (KeyValuePair<string,Backstory> story in BackstoryDatabase.allBackstories) {
				if (story.Key.Contains("REB_") && Controller.Settings.categorizeSource.Equals(true)) {
					story.Value.title = story.Value.title+" (REB)";
					if (!story.Value.titleFemale.NullOrEmpty()) { story.Value.titleFemale = story.Value.titleFemale+" (REB)"; }
				}
				if (!story.Key.Contains("REB_")) {
					if (story.Key.Equals("ColonySettler53")) {
						trigger=1;
				    }
					if (trigger == 1 && Controller.Settings.categorizeSource.Equals(true)) {
						if (story.Value.shuffleable.Equals(true)) {
							story.Value.title = story.Value.title+" (Vanilla)";
							if (!story.Value.titleFemale.NullOrEmpty()) { story.Value.titleFemale = story.Value.titleFemale+" (Vanilla)"; }
						}
						else {
							story.Value.title = story.Value.title+" (VanillaPK)";
							if (!story.Value.titleFemale.NullOrEmpty()) { story.Value.titleFemale = story.Value.titleFemale+" (VanillaPK)"; }
						}
					}
					if (trigger == 0 && Controller.Settings.categorizeSource.Equals(true)) {
						story.Value.title = story.Value.title+" (Other)";
						if (!story.Value.titleFemale.NullOrEmpty()) { story.Value.titleFemale = story.Value.titleFemale+" (Other)"; }
					}
					if (story.Value.slot == BackstorySlot.Childhood && trigger == 0) {
						if (story.Value.shuffleable.Equals(true)) { childCountOther++; }
						else { childNSCountOther++; }
					}
					else if (trigger == 0) {
						if (story.Value.shuffleable.Equals(true)) { adultCountOther++; }
						else { adultNSCountOther++; }
					}
				}
			}
			string modBasePath = LoadedModManager.RunningMods.First(mcp => mcp.assemblies.loadedAssemblies.Contains(typeof(REB_Initializer).Assembly)).RootDir;
			string namesPath = Path.GetFullPath(Path.Combine(modBasePath, "Name Lists/")).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
			var logFile = File.ReadAllLines(string.Concat(namesPath, "First_Female.txt"));
			foreach (var s in logFile) NamesFirstFemale.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "First_Male.txt"));
			foreach (var s in logFile) NamesFirstMale.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "Last.txt"));
			foreach (var s in logFile) NamesLast.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "Nicks_Female.txt"));
			foreach (var s in logFile) NamesNicksFemale.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "Nicks_Male.txt"));
			foreach (var s in logFile) NamesNicksMale.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "Nicks_Unisex.txt"));
			foreach (var s in logFile) NamesNicksUnisex.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "Sets_Female.txt"));
			foreach (var s in logFile) NamesSetsFemale.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "Sets_Male.txt"));
			foreach (var s in logFile) NamesSetsMale.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "Full_Male.txt"));
			foreach (var s in logFile) {
				NameTriple name = NameTriple.FromString(s);
				if (name.First.NullOrEmpty() || name.Last.NullOrEmpty()) {
					Log.Error("Name Error ("+name+"): This is not a valid full name.");
				}
				else {
					REBPawnNameDatabaseSolid.AddPlayerContentName(name, GenderPossibility.Male);
					fullCount++;
				}
			}
			logFile = File.ReadAllLines(string.Concat(namesPath, "Full_Female.txt"));
			foreach (var s in logFile) {
				NameTriple name = NameTriple.FromString(s);
				if (name.First.NullOrEmpty() || name.Last.NullOrEmpty()) {
					Log.Error("Name Error ("+name+"): This is not a valid full name.");
				}
				else {
					REBPawnNameDatabaseSolid.AddPlayerContentName(name, GenderPossibility.Female);
					fullCount++;
				}
			}
			namesPath = Path.GetFullPath(Path.Combine(namesPath, "FilterSet Name Lists/")).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet1_First_Female.txt"));
			foreach (var s in logFile) FS1Female.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet1_First_Male.txt"));
			foreach (var s in logFile) FS1Male.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet1_Last.txt"));
			foreach (var s in logFile) FS1Last.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet2_First_Female.txt"));
			foreach (var s in logFile) FS2Female.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet2_First_Male.txt"));
			foreach (var s in logFile) FS2Male.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet2_Last.txt"));
			foreach (var s in logFile) FS2Last.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet3_First_Female.txt"));
			foreach (var s in logFile) FS3Female.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet3_First_Male.txt"));
			foreach (var s in logFile) FS3Male.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet3_Last.txt"));
			foreach (var s in logFile) FS3Last.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet4_First_Female.txt"));
			foreach (var s in logFile) FS4Female.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet4_First_Male.txt"));
			foreach (var s in logFile) FS4Male.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet4_Last.txt"));
			foreach (var s in logFile) FS4Last.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet5_First_Female.txt"));
			foreach (var s in logFile) FS5Female.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet5_First_Male.txt"));
			foreach (var s in logFile) FS5Male.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet5_Last.txt"));
			foreach (var s in logFile) FS5Last.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet6_First_Female.txt"));
			foreach (var s in logFile) FS6Female.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet6_First_Male.txt"));
			foreach (var s in logFile) FS6Male.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet6_Last.txt"));
			foreach (var s in logFile) FS6Last.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet7_First_Female.txt"));
			foreach (var s in logFile) FS7Female.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet7_First_Male.txt"));
			foreach (var s in logFile) FS7Male.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet7_Last.txt"));
			foreach (var s in logFile) FS7Last.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet8_First_Female.txt"));
			foreach (var s in logFile) FS8Female.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet8_First_Male.txt"));
			foreach (var s in logFile) FS8Male.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet8_Last.txt"));
			foreach (var s in logFile) FS8Last.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet9_First_Female.txt"));
			foreach (var s in logFile) FS9Female.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet9_First_Male.txt"));
			foreach (var s in logFile) FS9Male.Add(s);
			logFile = File.ReadAllLines(string.Concat(namesPath, "FilterSet9_Last.txt"));
			foreach (var s in logFile) FS9Last.Add(s);
			Log.Message("||========================================");
			Log.Message("|| Setting up Editable Backstories (REB)");
			Log.Message("||========================================");
			Log.Message("|| Backstories Added by REB:");
			Log.Message("||    Childhood Backstories (shuffleable): "+childCount);
			Log.Message("||    Childhood Backstories (non-shuffleable): "+childNSCount);
			Log.Message("||    Adulthood Backstories (shuffleable): "+adultCount);
			Log.Message("||    Adulthood Backstories (non-shuffleable): "+adultNSCount);
			Log.Message("||========================================");
			Log.Message("|| Backstories Added by Other Mods:");
			Log.Message("||    Childhood Backstories (shuffleable): "+childCountOther);
			Log.Message("||    Childhood Backstories (non-shuffleable): "+childNSCountOther);
			Log.Message("||    Adulthood Backstories (shuffleable): "+adultCountOther);
			Log.Message("||    Adulthood Backstories (non-shuffleable): "+adultNSCountOther);
			Log.Message("||========================================");
			Log.Message("|| Names Added by REB:");
			Log.Message("||    First Names (Female): "+NamesFirstFemale.Count);
			Log.Message("||    First Names (Male): "+NamesFirstMale.Count);
			Log.Message("||    Nicknames (Female): "+NamesNicksFemale.Count);
			Log.Message("||    Nicknames (Male): "+NamesNicksMale.Count);
			Log.Message("||    Nicknames (Unisex): "+NamesNicksUnisex.Count);
			Log.Message("||    First/Nick Sets (Female): "+NamesSetsFemale.Count);
			Log.Message("||    First/Nick Sets (Male): "+NamesSetsMale.Count);
			Log.Message("||    Last Names: "+NamesLast.Count);
			Log.Message("||    Full Names (without bios): "+fullCount);
			Log.Message("||    Full Names (with bios): "+fullBioCount);
			Log.Message("||========================================");
			Log.Message("|| FilterSet Name Lists:");
			if (FS1Female.Count == 0 && FS1Male.Count == 0 && FS1Last.Count == 0) {
				Log.Message("||    FS1: No Names Defined");
			}
			else {
				Log.Message("||    FS1 First Names (Female): "+FS1Female.Count);
				Log.Message("||    FS1 First Names (Male): "+FS1Male.Count);
				Log.Message("||    FS1 Last Names: "+FS1Last.Count);
			}
			if (FS2Female.Count == 0 && FS2Male.Count == 0 && FS2Last.Count == 0) {
				Log.Message("||    FS2: No Names Defined");
			}
			else {
				Log.Message("||    FS2 First Names (Female): "+FS2Female.Count);
				Log.Message("||    FS2 First Names (Male): "+FS2Male.Count);
				Log.Message("||    FS2 Last Names: "+FS2Last.Count);
			}
			if (FS3Female.Count == 0 && FS3Male.Count == 0 && FS3Last.Count == 0) {
				Log.Message("||    FS3: No Names Defined");
			}
			else {
				Log.Message("||    FS3 First Names (Female): "+FS3Female.Count);
				Log.Message("||    FS3 First Names (Male): "+FS3Male.Count);
				Log.Message("||    FS3 Last Names: "+FS3Last.Count);
			}
			if (FS4Female.Count == 0 && FS4Male.Count == 0 && FS4Last.Count == 0) {
				Log.Message("||    FS4: No Names Defined");
			}
			else {
				Log.Message("||    FS4 First Names (Female): "+FS4Female.Count);
				Log.Message("||    FS4 First Names (Male): "+FS4Male.Count);
				Log.Message("||    FS4 Last Names: "+FS4Last.Count);
			}
			if (FS5Female.Count == 0 && FS5Male.Count == 0 && FS5Last.Count == 0) {
				Log.Message("||    FS5: No Names Defined");
			}
			else {
				Log.Message("||    FS5 First Names (Female): "+FS5Female.Count);
				Log.Message("||    FS5 First Names (Male): "+FS5Male.Count);
				Log.Message("||    FS5 Last Names: "+FS5Last.Count);
			}
			if (FS6Female.Count == 0 && FS6Male.Count == 0 && FS6Last.Count == 0) {
				Log.Message("||    FS6: No Names Defined");
			}
			else {
				Log.Message("||    FS6 First Names (Female): "+FS6Female.Count);
				Log.Message("||    FS6 First Names (Male): "+FS6Male.Count);
				Log.Message("||    FS6 Last Names: "+FS6Last.Count);
			}
			if (FS7Female.Count == 0 && FS7Male.Count == 0 && FS7Last.Count == 0) {
				Log.Message("||    FS7: No Names Defined");
			}
			else {
				Log.Message("||    FS7 First Names (Female): "+FS7Female.Count);
				Log.Message("||    FS7 First Names (Male): "+FS7Male.Count);
				Log.Message("||    FS7 Last Names: "+FS7Last.Count);
			}
			if (FS8Female.Count == 0 && FS8Male.Count == 0 && FS8Last.Count == 0) {
				Log.Message("||    FS8: No Names Defined");
			}
			else {
				Log.Message("||    FS8 First Names (Female): "+FS8Female.Count);
				Log.Message("||    FS8 First Names (Male): "+FS8Male.Count);
				Log.Message("||    FS8 Last Names: "+FS8Last.Count);
			}
			if (FS9Female.Count == 0 && FS9Male.Count == 0 && FS9Last.Count == 0) {
				Log.Message("||    FS9: No Names Defined");
			}
			else {
				Log.Message("||    FS9 First Names (Female): "+FS9Female.Count);
				Log.Message("||    FS9 First Names (Male): "+FS9Male.Count);
				Log.Message("||    FS9 Last Names: "+FS9Last.Count);
			}
			Log.Message("||========================================");
			REB_Initializer.banks = new Dictionary<PawnNameCategory, REB_NameBank>();
			REB_Initializer.banks.Add(PawnNameCategory.HumanStandard, new REB_NameBank(PawnNameCategory.HumanStandard));
			REB_NameBank nameBank = REB_Initializer.BankOf(PawnNameCategory.HumanStandard);
			if (NamesFirstFemale != null) {
				nameBank.AddNames(REB_NameSlot.First, Gender.Female, NamesFirstFemale);
				NamesFirstFemale.Clear();
			}
			if (NamesFirstMale != null) {
				nameBank.AddNames(REB_NameSlot.First, Gender.Male, NamesFirstMale);
				NamesFirstMale.Clear();
			}
			if (NamesNicksFemale != null) {
				nameBank.AddNames(REB_NameSlot.Nick, Gender.Female, NamesNicksFemale);
				NamesNicksFemale.Clear();
			}
			if (NamesNicksMale != null) {
				nameBank.AddNames(REB_NameSlot.Nick, Gender.Male, NamesNicksMale);
				NamesNicksMale.Clear();
			}
			if (NamesNicksUnisex != null) {
				nameBank.AddNames(REB_NameSlot.Nick, Gender.None, NamesNicksUnisex);
				NamesNicksUnisex.Clear();
			}
			if (NamesSetsFemale != null) {
				nameBank.AddNames(REB_NameSlot.Set, Gender.Female, NamesSetsFemale);
				NamesSetsFemale.Clear();
			}
			if (NamesSetsMale != null) {
				nameBank.AddNames(REB_NameSlot.Set, Gender.Male, NamesSetsMale);
				NamesSetsMale.Clear();
			}
			if (NamesLast != null) {
				nameBank.AddNames(REB_NameSlot.Last, Gender.None, NamesLast);
				NamesLast.Clear();
			}
			if (FS1Female != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet1, Gender.Female, FS1Female);
				FS1Female.Clear();
			}
			if (FS1Male != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet1, Gender.Male, FS1Male);
				FS1Male.Clear();
			}
			if (FS1Last != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet1, Gender.None, FS1Last);
				FS1Last.Clear();
			}
			if (FS2Female != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet2, Gender.Female, FS2Female);
				FS2Female.Clear();
			}
			if (FS2Male != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet2, Gender.Male, FS2Male);
				FS2Male.Clear();
			}
			if (FS2Last != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet2, Gender.None, FS2Last);
				FS2Last.Clear();
			}
			if (FS3Female != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet3, Gender.Female, FS3Female);
				FS3Female.Clear();
			}
			if (FS3Male != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet3, Gender.Male, FS3Male);
				FS3Male.Clear();
			}
			if (FS3Last != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet3, Gender.None, FS3Last);
				FS3Last.Clear();
			}
			if (FS4Female != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet4, Gender.Female, FS4Female);
				FS4Female.Clear();
			}
			if (FS4Male != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet4, Gender.Male, FS4Male);
				FS4Male.Clear();
			}
			if (FS4Last != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet4, Gender.None, FS4Last);
				FS4Last.Clear();
			}
			if (FS5Female != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet5, Gender.Female, FS5Female);
				FS5Female.Clear();
			}
			if (FS5Male != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet5, Gender.Male, FS5Male);
				FS5Male.Clear();
			}
			if (FS5Last != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet5, Gender.None, FS5Last);
				FS5Last.Clear();
			}
			if (FS6Female != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet6, Gender.Female, FS6Female);
				FS6Female.Clear();
			}
			if (FS6Male != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet6, Gender.Male, FS6Male);
				FS6Male.Clear();
			}
			if (FS6Last != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet6, Gender.None, FS6Last);
				FS6Last.Clear();
			}
			if (FS7Female != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet7, Gender.Female, FS7Female);
				FS7Female.Clear();
			}
			if (FS7Male != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet7, Gender.Male, FS7Male);
				FS7Male.Clear();
			}
			if (FS7Last != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet7, Gender.None, FS7Last);
				FS7Last.Clear();
			}
			if (FS8Female != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet8, Gender.Female, FS8Female);
				FS8Female.Clear();
			}
			if (FS8Male != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet8, Gender.Male, FS8Male);
				FS8Male.Clear();
			}
			if (FS8Last != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet8, Gender.None, FS8Last);
				FS8Last.Clear();
			}
			if (FS9Female != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet9, Gender.Female, FS9Female);
				FS9Female.Clear();
			}
			if (FS9Male != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet9, Gender.Male, FS9Male);
				FS9Male.Clear();
			}
			if (FS9Last != null) {
				nameBank.AddNames(REB_NameSlot.FilterSet9, Gender.None, FS9Last);
				FS9Last.Clear();
			}
			foreach (REB_NameBank value in REB_Initializer.banks.Values) {
				value.ErrorCheck();
			}			
		}
		public static REB_NameBank BankOf(PawnNameCategory category) {
			return REB_Initializer.banks[category];
		}
	}

	public class Controller : Mod {
		public static bool settingsChanged = false;
		public static Settings Settings;
		public override string SettingsCategory() { return "REB.EditableBackstories".Translate(); }
		public override void DoSettingsWindowContents(Rect canvas) { Settings.DoWindowContents(canvas); }
		public Controller(ModContentPack content) : base(content) {
			Settings = GetSettings<Settings>();
		}
	}

	public class Settings : ModSettings {
		public bool useLiteMode = false;
		public bool categorizeSource = false;
		public bool allowVanillaTriples = false;
		public void DoWindowContents(Rect canvas) {
			Listing_Standard list = new Listing_Standard();
			list.ColumnWidth = canvas.width;
			list.Begin(canvas);
			list.Gap();
			list.CheckboxLabeled( "REB.UseLiteMode".Translate(), ref useLiteMode, "REB.UseLiteModeTip".Translate() );
			list.Gap();
			list.CheckboxLabeled( "REB.CategorizeSource".Translate(), ref categorizeSource, "REB.CategorizeSourceTip".Translate() );
			list.Gap();
			list.CheckboxLabeled( "REB.AllowVanillaTriples".Translate(), ref allowVanillaTriples, "REB.AllowVanillaTriplesTip".Translate() );
			list.End();
		}
		public override void ExposeData() {
			base.ExposeData();
			Scribe_Values.Look(ref useLiteMode, "useLiteMode", false);
			Scribe_Values.Look(ref categorizeSource, "categorizeSource", false);
			Scribe_Values.Look(ref allowVanillaTriples, "allowVanillaTriples", false);
			Controller.settingsChanged = true;
		}
	}

//
// Name Bank
//

	public static class REBPawnNameDatabaseSolid {
		private static Dictionary<GenderPossibility, List<NameTriple>> solidNames;
		static REBPawnNameDatabaseSolid() {
			REBPawnNameDatabaseSolid.solidNames = new Dictionary<GenderPossibility, List<NameTriple>>();
			foreach (GenderPossibility key in Enum.GetValues(typeof(GenderPossibility))) {
				REBPawnNameDatabaseSolid.solidNames.Add(key, new List<NameTriple>());
			}
		}
		public static void AddPlayerContentName(NameTriple newName, GenderPossibility genderPos) {
			REBPawnNameDatabaseSolid.solidNames[genderPos].Add(newName);
		}
		public static List<NameTriple> GetListForGender(GenderPossibility gp) {
			return REBPawnNameDatabaseSolid.solidNames[gp];
		}
		[DebuggerHidden]
		public static IEnumerable<NameTriple> AllNames() {
			foreach (KeyValuePair<GenderPossibility, List<NameTriple>> kvp in REBPawnNameDatabaseSolid.solidNames) {
				foreach (NameTriple name in kvp.Value) {
					yield return name;
				}
			}
		}
	}

	public static class PKPawnNameDatabaseSolid {
		private static Dictionary<GenderPossibility, List<NameTriple>> solidNames;
		static PKPawnNameDatabaseSolid() {
			PKPawnNameDatabaseSolid.solidNames = new Dictionary<GenderPossibility, List<NameTriple>>();
			foreach (GenderPossibility key in Enum.GetValues(typeof(GenderPossibility))) {
				PKPawnNameDatabaseSolid.solidNames.Add(key, new List<NameTriple>());
			}
		}
		public static void AddPlayerContentName(NameTriple newName, GenderPossibility genderPos) {
			PKPawnNameDatabaseSolid.solidNames[genderPos].Add(newName);
		}
		public static List<NameTriple> GetListForGender(GenderPossibility gp) {
			return PKPawnNameDatabaseSolid.solidNames[gp];
		}
		[DebuggerHidden]
		public static IEnumerable<NameTriple> AllNames() {
			foreach (KeyValuePair<GenderPossibility, List<NameTriple>> kvp in PKPawnNameDatabaseSolid.solidNames) {
				foreach (NameTriple name in kvp.Value) {
					yield return name;
				}
			}
		}
	}
	
	public class REB_NameBank {
		public PawnNameCategory nameType;
		private List<string>[,] names;
		private readonly static int numGenders = Enum.GetValues(typeof(Gender)).Length;
		private readonly static int numSlots = Enum.GetValues(typeof(REB_NameSlot)).Length;
		string modBasePath = LoadedModManager.RunningMods.First(mcp => mcp.assemblies.loadedAssemblies.Contains(typeof(REB_Initializer).Assembly)).RootDir;
		private IEnumerable<List<string>> AllNameLists {
			get {
				for (int i = 0; i < REB_NameBank.numGenders; i++) {
					for (int j = 0; j < REB_NameBank.numSlots; j++) {
						yield return this.names[i, j];
					}
				}
			}
		}
		public REB_NameBank(PawnNameCategory ID) {
			this.nameType = ID;
			this.names = new List<string>[REB_NameBank.numGenders, REB_NameBank.numSlots];
			for (int i = 0; i < REB_NameBank.numGenders; i++) {
				for (int j = 0; j < REB_NameBank.numSlots; j++) {
					this.names[i, j] = new List<string>();
				}
			}
		}
		public void AddNames(REB_NameSlot slot, Gender gender, IEnumerable<string> namesToAdd) {
			IEnumerator<string> enumerator = namesToAdd.GetEnumerator();
			try {
				while (enumerator.MoveNext()) {
					string current = enumerator.Current;
					this.NamesFor(slot, gender).Add(current);
				}
			}
			finally {
				if (enumerator == null) { }
				enumerator.Dispose();
			}
		}
		public void ErrorCheck() {
			IEnumerator<List<string>> enumerator = this.AllNameLists.GetEnumerator();
			try {
				while (enumerator.MoveNext()) {
					List<string> current = enumerator.Current;
					List<string> list = (
					from x in current
					group x by x into g
					where g.Count<string>() > 1
					select g.Key).ToList<string>();
					List<string>.Enumerator enumerator1 = list.GetEnumerator();
					try {
						while (enumerator1.MoveNext()) {
							Log.Error(string.Concat("Duplicated name: ", enumerator1.Current));
						}
					}
					finally {
						((IDisposable)(object)enumerator1).Dispose();
					}
					List<string>.Enumerator enumerator2 = current.GetEnumerator();
					try {
						while (enumerator2.MoveNext()) {
							string str = enumerator2.Current;
							if (str.Trim() == str) {
								continue;
							}
							Log.Error(string.Concat("Trimmable whitespace on name: [", str, "]"));
						}
					}
					finally {
						((IDisposable)(object)enumerator2).Dispose();
					}
				}
			}
			finally {
				if (enumerator == null) { }
				enumerator.Dispose();
			}
		}
		public string GetName(REB_NameSlot slot, Gender gender = 0, bool checkIfAlreadyUsed = true) {
			string str;
			List<string> strs = this.NamesFor(slot, gender);
			int num = 0;
			if (strs.Count == 0) {
				Log.Error(string.Concat(new object[] { "Name list for gender=", gender, " slot=", slot, " is empty." }));
				return "Errorname";
			}
			while (true) {
				str = strs.RandomElement<string>();
				if (checkIfAlreadyUsed && !NameUseChecker.NameWordIsUsed(str)) {
					return str;
				}
				num++;
				if (num > 50) {
					break;
				}
			}
			return str;
		}
		public List<string> NamesFor(REB_NameSlot slot, Gender gender) {
			return this.names[(int)gender, (int)slot];
		}
	}
	
	public static class REB_FileRead {
		public static IEnumerable<string> LinesFromFile(string filePath) {
			string rawText = GenFile.TextFromRawFile(filePath);
			foreach (string line in GenText.LinesFromString(rawText)) {
				yield return line;
			}		
		}
	}

	public enum REB_NameSlot : byte { 
		First,
		Nick,
		Set,
		Last,
		FilterSet1,
		FilterSet2,
		FilterSet3,
		FilterSet4,
		FilterSet5,
		FilterSet6,
		FilterSet7,
		FilterSet8,
		FilterSet9
	}	

//
//	Harmony Patches
//

	[HarmonyPatch(typeof(Backstory), "TitleFor")]
	public static class Patch_Backstory_TitleFor {
		static bool Prefix() {
			if (Controller.settingsChanged.Equals(true)) {
				Controller.settingsChanged = false;
				int trigger = 0;
				if (Controller.Settings.categorizeSource.Equals(true)) {
					foreach (KeyValuePair<string,Backstory> story in BackstoryDatabase.allBackstories) {
						if (story.Key.Contains("REB_")) {
							if (!story.Value.title.Contains("(REB)")) {
								story.Value.title = story.Value.title+" (REB)";
								if (!story.Value.titleFemale.NullOrEmpty()) { story.Value.titleFemale = story.Value.titleFemale+" (REB)"; }
							}
						}
						else {
							if (story.Key.Equals("ColonySettler53")) {
								trigger=1;
						    }
							if (trigger == 1) {
								if (!story.Value.title.Contains("(Vanilla)")) {
									if (story.Value.shuffleable.Equals(true)) {
										story.Value.title = story.Value.title+" (Vanilla)";
										if (!story.Value.titleFemale.NullOrEmpty()) { story.Value.titleFemale = story.Value.titleFemale+" (Vanilla)"; }
									}
									else {
										story.Value.title = story.Value.title+" (VanillaPK)";
										if (!story.Value.titleFemale.NullOrEmpty()) { story.Value.titleFemale = story.Value.titleFemale+" (VanillaPK)"; }
									}
								}
							}
							else {
								if (!story.Value.title.Contains("(Other)")) {
									story.Value.title = story.Value.title+" (Other)";
									if (!story.Value.titleFemale.NullOrEmpty()) { story.Value.titleFemale = story.Value.titleFemale+" (Other)"; }
								}
							}
						}
					}
				}
				else {
					foreach (KeyValuePair<string,Backstory> story in BackstoryDatabase.allBackstories) {
						if (story.Value.title.Contains("(REB)")) {
							story.Value.title = story.Value.title.Substring(0,story.Value.title.Length-6);
							if (!story.Value.titleFemale.NullOrEmpty()) { story.Value.titleFemale = story.Value.titleFemale.Substring(0,story.Value.titleFemale.Length-6); }
						}
						if (story.Value.title.Contains("(Vanilla)")) {
							story.Value.title = story.Value.title.Substring(0,story.Value.title.Length-10);
							if (!story.Value.titleFemale.NullOrEmpty()) { story.Value.titleFemale = story.Value.titleFemale.Substring(0,story.Value.titleFemale.Length-10); }
						}
						if (story.Value.title.Contains("(VanillaPK)")) {
							story.Value.title = story.Value.title.Substring(0,story.Value.title.Length-12);
							if (!story.Value.titleFemale.NullOrEmpty()) { story.Value.titleFemale = story.Value.titleFemale.Substring(0,story.Value.titleFemale.Length-12); }
						}
						if (story.Value.title.Contains("(Other)")) {
							story.Value.title = story.Value.title.Substring(0,story.Value.title.Length-8);
							if (!story.Value.titleFemale.NullOrEmpty()) { story.Value.titleFemale = story.Value.titleFemale.Substring(0,story.Value.titleFemale.Length-8); }
						}
					}
				}
			}
			return true;
		}
	}
	
	[HarmonyPatch(typeof(PawnBioAndNameGenerator), "FillBackstorySlotShuffled")]
	static class Patch_PawnBioAndNameGenerator_FillBackstorySlotShuffled {
		[HarmonyPriority(Priority.Low)]
		static bool Prefix(Pawn pawn, BackstorySlot slot, ref Backstory backstory, List<string> backstoryCategories, FactionDef factionType) {
			if (!(from kvp in BackstoryDatabase.allBackstories
			where kvp.Value.shuffleable
			  && kvp.Value.spawnCategories.Intersect(backstoryCategories).Count() > 0
			  && (((kvp.Value.workDisables & WorkTags.Cleaning) == WorkTags.None)
			      || (((kvp.Value.workDisables & WorkTags.Cleaning) != WorkTags.None) && Rand.Value < 0.8f))
			  && (((kvp.Value.workDisables & WorkTags.Firefighting) == WorkTags.None)
			      || (((kvp.Value.workDisables & WorkTags.Firefighting) != WorkTags.None) && Rand.Value < 0.8f))
			  && (((kvp.Value.workDisables & WorkTags.Hauling) == WorkTags.None)
			      || (((kvp.Value.workDisables & WorkTags.Hauling) != WorkTags.None) && Rand.Value < 0.8f))
			  && (((kvp.Value.workDisables & WorkTags.ManualDumb) == WorkTags.None)
			      || (((kvp.Value.workDisables & WorkTags.ManualDumb) != WorkTags.None) && Rand.Value < 0.8f))
			  && (((kvp.Value.workDisables & WorkTags.Violent) == WorkTags.None)
			      || (((kvp.Value.workDisables & WorkTags.Violent) != WorkTags.None) && Rand.Value < 0.8f))
			  && (!kvp.Value.spawnCategories.Contains("Uncommon")
			      || (kvp.Value.spawnCategories.Contains("Uncommon") && (Rand.Value < 0.333333)))
			  && (!kvp.Value.spawnCategories.Contains("Rare")
			      || (kvp.Value.spawnCategories.Contains("Rare") && (Rand.Value < 0.111111)))
			  && (!kvp.Value.spawnCategories.Contains("Legendary")
			      || (kvp.Value.spawnCategories.Contains("Legendary") && (Rand.Value < 0.037037)))
			  && (!kvp.Value.spawnCategories.Contains("AgeOver40")
			      || (kvp.Value.spawnCategories.Contains("AgeOver40") && pawn.ageTracker.AgeBiologicalYearsFloat > 40f))
			  && (!kvp.Value.spawnCategories.Contains("AgeOver60")
			      || (kvp.Value.spawnCategories.Contains("AgeOver60") && pawn.ageTracker.AgeBiologicalYearsFloat > 60f))
			  && (!kvp.Value.spawnCategories.Contains("AgeOver80")
			      || (kvp.Value.spawnCategories.Contains("AgeOver80") && pawn.ageTracker.AgeBiologicalYearsFloat > 80f))
			  && (!kvp.Value.spawnCategories.Contains("Male")
			      || (kvp.Value.spawnCategories.Contains("Male") && pawn.gender == Gender.Male))
			  && (!kvp.Value.spawnCategories.Contains("Female")
			      || (kvp.Value.spawnCategories.Contains("Female") && pawn.gender == Gender.Female))
			  && kvp.Value.slot == slot 
			  && (slot != BackstorySlot.Adulthood
			    || (!SkillGainsConflict(kvp.Value.skillGainsResolved, pawn.story.childhood.skillGainsResolved, kvp.Value.workDisables, pawn.story.childhood.workDisables, kvp.Value.forcedTraits, pawn.story.childhood.forcedTraits, kvp.Value.requiredWorkTags, pawn.story.childhood.requiredWorkTags)
			    && SameFilterSet(kvp.Value.spawnCategories, pawn.story.childhood.spawnCategories)))
			select kvp.Value).TryRandomElement(out backstory)) {
				Log.Error(string.Concat(new object[] {
					"No shuffled ",slot," found for ",pawn," of ",factionType,". Defaulting."
				}));
				backstory = (from kvp in BackstoryDatabase.allBackstories
				where kvp.Value.slot == slot
				select kvp).RandomElement<KeyValuePair<string, Backstory>>().Value;
			}
			return false;
		}
		private static bool SkillGainsConflict(Dictionary<SkillDef,int> a, Dictionary<SkillDef,int> b, WorkTags aDisabled, WorkTags bDisabled, List<TraitEntry> aTraits, List<TraitEntry> bTraits, WorkTags aRequired, WorkTags bRequired) {
			List<WorkTypeDef> allWorkTypes = DefDatabase<WorkTypeDef>.AllDefsListForReading;
			for (int i = 0; i < allWorkTypes.Count; i++) {
				WorkTypeDef item = allWorkTypes[i];
				if (((item.workTags & aRequired) != WorkTags.None && (item.workTags & bDisabled) != WorkTags.None)
				  || ((item.workTags & bRequired) != WorkTags.None && (item.workTags & aDisabled) != WorkTags.None)) {
					return true;
				}
			}
			if (((WorkTags.ManualDumb & aRequired) != WorkTags.None && ((WorkTags.Cleaning & bDisabled) != WorkTags.None || (WorkTags.Hauling & bDisabled) != WorkTags.None || (WorkTags.PlantWork & bDisabled) != WorkTags.None))
			    || ((WorkTags.ManualDumb & bRequired) != WorkTags.None && ((WorkTags.Cleaning & aDisabled) != WorkTags.None || (WorkTags.Hauling & aDisabled) != WorkTags.None || (WorkTags.PlantWork & aDisabled) != WorkTags.None))) {
				return true;
			}
			if (((WorkTags.ManualDumb & aDisabled) != WorkTags.None && ((WorkTags.Cleaning & bRequired) != WorkTags.None || (WorkTags.Hauling & bRequired) != WorkTags.None || (WorkTags.PlantWork & bRequired) != WorkTags.None))
			    || ((WorkTags.ManualDumb & bDisabled) != WorkTags.None && ((WorkTags.Cleaning & aRequired) != WorkTags.None || (WorkTags.Hauling & aRequired) != WorkTags.None || (WorkTags.PlantWork & aRequired) != WorkTags.None))) {
				return true;
			}
			if (((WorkTags.ManualSkilled & aRequired) != WorkTags.None && ((WorkTags.Cooking & bDisabled) != WorkTags.None || (WorkTags.Crafting & bDisabled) != WorkTags.None || (WorkTags.Mining & bDisabled) != WorkTags.None || (WorkTags.PlantWork & bDisabled) != WorkTags.None))
			    || ((WorkTags.ManualSkilled & bRequired) != WorkTags.None && ((WorkTags.Cooking & aDisabled) != WorkTags.None || (WorkTags.Crafting & aDisabled) != WorkTags.None || (WorkTags.Mining & aDisabled) != WorkTags.None || (WorkTags.PlantWork & aDisabled) != WorkTags.None))) {
				return true;
			}
			if (((WorkTags.ManualSkilled & aDisabled) != WorkTags.None && ((WorkTags.Cooking & bRequired) != WorkTags.None || (WorkTags.Crafting & bRequired) != WorkTags.None || (WorkTags.Mining & bRequired) != WorkTags.None || (WorkTags.PlantWork & bRequired) != WorkTags.None))
			    || ((WorkTags.ManualSkilled & bDisabled) != WorkTags.None && ((WorkTags.Cooking & aRequired) != WorkTags.None || (WorkTags.Crafting & aRequired) != WorkTags.None || (WorkTags.Mining & aRequired) != WorkTags.None || (WorkTags.PlantWork & aRequired) != WorkTags.None))) {
				return true;
			}
			List<SkillDef> allSkills = DefDatabase<SkillDef>.AllDefsListForReading;
			for (int i = 0; i < allSkills.Count; i++) {
				SkillDef skill = allSkills[i];
				if (a.ContainsKey(skill) && b.ContainsKey(skill)) {
					if ((a[skill] > 3 && b[skill] < -3) || (b[skill] > 3 && a[skill] < -3)) {
						return true;
					}
				}
			}
			if (a.ContainsKey(SkillDefOf.Animals)) {
			    if ((bRequired & WorkTags.Animals) != WorkTags.None) {
			    	if (a[SkillDefOf.Animals] < -3) { return true; }
			    }
				if ((bDisabled & WorkTags.Animals) != WorkTags.None) {
					if (a[SkillDefOf.Animals] > 3) { return true; }
			    }
			}
			if (b.ContainsKey(SkillDefOf.Animals)) {
			    if ((aRequired & WorkTags.Animals) != WorkTags.None) {
			    	if (b[SkillDefOf.Animals] < -3) { return true; }
			    }
				if ((aDisabled & WorkTags.Animals) != WorkTags.None) {
					if (b[SkillDefOf.Animals] > 3) { return true; }
			    }
			}
			if (a.ContainsKey(SkillDefOf.Artistic)) {
			    if ((bRequired & WorkTags.Artistic) != WorkTags.None) {
			    	if (a[SkillDefOf.Artistic] < -3) { return true; }
			    }
				if ((bDisabled & WorkTags.Artistic) != WorkTags.None) {
					if (a[SkillDefOf.Artistic] > 3) { return true; }
			    }
			}
			if (b.ContainsKey(SkillDefOf.Artistic)) {
			    if ((aRequired & WorkTags.Artistic) != WorkTags.None) {
			    	if (b[SkillDefOf.Artistic] < -3) { return true; }
			    }
				if ((aDisabled & WorkTags.Artistic) != WorkTags.None) {
					if (b[SkillDefOf.Artistic] > 3) { return true; }
			    }
			}
			if (a.ContainsKey(SkillDefOf.Construction)) {
			    if ((bRequired & WorkTags.ManualSkilled) != WorkTags.None) {
			    	if (a[SkillDefOf.Construction] < -3) { return true; }
			    }
				if ((bDisabled & WorkTags.ManualSkilled) != WorkTags.None) {
					if (a[SkillDefOf.Construction] > 3) { return true; }
			    }
			}
			if (b.ContainsKey(SkillDefOf.Construction)) {
			    if ((aRequired & WorkTags.ManualSkilled) != WorkTags.None) {
			    	if (b[SkillDefOf.Construction] < -3) { return true; }
			    }
				if ((aDisabled & WorkTags.ManualSkilled) != WorkTags.None) {
					if (b[SkillDefOf.Construction] > 3) { return true; }
			    }
			}
			if (a.ContainsKey(SkillDefOf.Cooking)) {
			    if ((bRequired & WorkTags.Cooking) != WorkTags.None || (bRequired & WorkTags.ManualSkilled) != WorkTags.None) {
			    	if (a[SkillDefOf.Cooking] < -3) { return true; }
			    }
				if ((bDisabled & WorkTags.Cooking) != WorkTags.None || (bDisabled & WorkTags.ManualSkilled) != WorkTags.None) {
					if (a[SkillDefOf.Cooking] > 3) { return true; }
			    }
			}
			if (b.ContainsKey(SkillDefOf.Cooking)) {
				if ((aRequired & WorkTags.Cooking) != WorkTags.None || (aRequired & WorkTags.ManualSkilled) != WorkTags.None) {
			    	if (b[SkillDefOf.Cooking] < -3) { return true; }
			    }
				if ((aDisabled & WorkTags.Cooking) != WorkTags.None || (aDisabled & WorkTags.ManualSkilled) != WorkTags.None) {
					if (b[SkillDefOf.Cooking] > 3) { return true; }
			    }
			}
			if (a.ContainsKey(SkillDefOf.Crafting)) {
			    if ((bRequired & WorkTags.Crafting) != WorkTags.None || (bRequired & WorkTags.ManualSkilled) != WorkTags.None) {
			    	if (a[SkillDefOf.Crafting] < -3) { return true; }
			    }
				if ((bDisabled & WorkTags.Crafting) != WorkTags.None || (bDisabled & WorkTags.ManualSkilled) != WorkTags.None) {
					if (a[SkillDefOf.Crafting] > 3) { return true; }
			    }
			}
			if (b.ContainsKey(SkillDefOf.Crafting)) {
				if ((aRequired & WorkTags.Crafting) != WorkTags.None || (aRequired & WorkTags.ManualSkilled) != WorkTags.None) {
			    	if (b[SkillDefOf.Crafting] < -3) { return true; }
			    }
				if ((aDisabled & WorkTags.Crafting) != WorkTags.None || (aDisabled & WorkTags.ManualSkilled) != WorkTags.None) {
					if (b[SkillDefOf.Crafting] > 3) { return true; }
			    }
			}
			if (a.ContainsKey(SkillDefOf.Plants)) {
			    if ((bRequired & WorkTags.PlantWork) != WorkTags.None || (bRequired & WorkTags.ManualSkilled) != WorkTags.None) {
			    	if (a[SkillDefOf.Plants] < -3) { return true; }
			    }
				if ((bDisabled & WorkTags.PlantWork) != WorkTags.None || (bDisabled & WorkTags.ManualSkilled) != WorkTags.None) {
					if (a[SkillDefOf.Plants] > 3) { return true; }
			    }
			}
			if (b.ContainsKey(SkillDefOf.Plants)) {
				if ((aRequired & WorkTags.PlantWork) != WorkTags.None || (aRequired & WorkTags.ManualSkilled) != WorkTags.None) {
			    	if (b[SkillDefOf.Plants] < -3) { return true; }
			    }
				if ((aDisabled & WorkTags.PlantWork) != WorkTags.None || (aDisabled & WorkTags.ManualSkilled) != WorkTags.None) {
					if (b[SkillDefOf.Plants] > 3) { return true; }
			    }
			}
			if (a.ContainsKey(SkillDefOf.Medicine)) {
			    if ((bRequired & WorkTags.Caring) != WorkTags.None) {
			    	if (a[SkillDefOf.Medicine] < -3) { return true; }
			    }
				if ((bDisabled & WorkTags.Caring) != WorkTags.None) {
					if (a[SkillDefOf.Medicine] > 3) { return true; }
			    }
			}
			if (b.ContainsKey(SkillDefOf.Medicine)) {
			    if ((aRequired & WorkTags.Caring) != WorkTags.None) {
			    	if (b[SkillDefOf.Medicine] < -3) { return true; }
			    }
				if ((aDisabled & WorkTags.Caring) != WorkTags.None) {
					if (b[SkillDefOf.Medicine] > 3) { return true; }
			    }
			}
			if (a.ContainsKey(SkillDefOf.Mining)) {
			    if ((bRequired & WorkTags.Mining) != WorkTags.None || (bRequired & WorkTags.ManualSkilled) != WorkTags.None) {
			    	if (a[SkillDefOf.Mining] < -3) { return true; }
			    }
				if ((bDisabled & WorkTags.Mining) != WorkTags.None || (bDisabled & WorkTags.ManualSkilled) != WorkTags.None) {
					if (a[SkillDefOf.Mining] > 3) { return true; }
			    }
			}
			if (b.ContainsKey(SkillDefOf.Mining)) {
				if ((aRequired & WorkTags.Mining) != WorkTags.None || (aRequired & WorkTags.ManualSkilled) != WorkTags.None) {
			    	if (b[SkillDefOf.Mining] < -3) { return true; }
			    }
				if ((aDisabled & WorkTags.Mining) != WorkTags.None || (aDisabled & WorkTags.ManualSkilled) != WorkTags.None) {
					if (b[SkillDefOf.Mining] > 3) { return true; }
			    }
			}
			if (a.ContainsKey(SkillDefOf.Intellectual)) {
			    if ((bRequired & WorkTags.Intellectual) != WorkTags.None) {
			    	if (a[SkillDefOf.Intellectual] < -3) { return true; }
			    }
				if ((bDisabled & WorkTags.Intellectual) != WorkTags.None) {
					if (a[SkillDefOf.Intellectual] > 3) { return true; }
			    }
			}
			if (b.ContainsKey(SkillDefOf.Intellectual)) {
			    if ((aRequired & WorkTags.Intellectual) != WorkTags.None) {
			    	if (b[SkillDefOf.Intellectual] < -3) { return true; }
			    }
				if ((aDisabled & WorkTags.Intellectual) != WorkTags.None) {
					if (b[SkillDefOf.Intellectual] > 3) { return true; }
			    }
			}
			if (a.ContainsKey(SkillDefOf.Social)) {
			    if ((bRequired & WorkTags.Social) != WorkTags.None) {
			    	if (a[SkillDefOf.Social] < -3) { return true; }
			    }
				if ((bDisabled & WorkTags.Social) != WorkTags.None) {
					if (a[SkillDefOf.Social] > 3) { return true; }
			    }
			}
			if (b.ContainsKey(SkillDefOf.Social)) {
			    if ((aRequired & WorkTags.Social) != WorkTags.None) {
			    	if (b[SkillDefOf.Social] < -3) { return true; }
			    }
				if ((aDisabled & WorkTags.Social) != WorkTags.None) {
					if (b[SkillDefOf.Social] > 3) { return true; }
			    }
			}
			if (a.ContainsKey(SkillDefOf.Melee)) {
				if ((bDisabled & WorkTags.Violent) != WorkTags.None) {
					if (a[SkillDefOf.Melee] > 3) { return true; }
			    }
			}
			if (b.ContainsKey(SkillDefOf.Melee)) {
				if ((aDisabled & WorkTags.Violent) != WorkTags.None) {
					if (b[SkillDefOf.Melee] > 3) { return true; }
			    }
			}
			if (a.ContainsKey(SkillDefOf.Shooting)) {
				if ((bDisabled & WorkTags.Violent) != WorkTags.None) {
					if (a[SkillDefOf.Shooting] > 3) { return true; }
			    }
			}
			if (b.ContainsKey(SkillDefOf.Shooting)) {
				if ((aDisabled & WorkTags.Violent) != WorkTags.None) {
					if (b[SkillDefOf.Shooting] > 3) { return true; }
			    }
			}
			if (a.ContainsKey(SkillDefOf.Melee) && a.ContainsKey(SkillDefOf.Shooting)) {
			    if ((bRequired & WorkTags.Violent) != WorkTags.None) {
					if (a[SkillDefOf.Melee] < -3 && a[SkillDefOf.Shooting] < -3) { return true; }
			    }
			}
			if (b.ContainsKey(SkillDefOf.Melee) && b.ContainsKey(SkillDefOf.Shooting)) {
			    if ((aRequired & WorkTags.Violent) != WorkTags.None) {
					if (b[SkillDefOf.Melee] < -3 && b[SkillDefOf.Shooting] < -3) { return true; }
			    }
			}
			return false;
		}
		private static bool SameFilterSet(List<string> a, List<string> b) {
			bool sameSet = true;
			if (a.Contains("FilterSet1")) {
				if (b.Contains("FilterSet1")) { return true; }
				else { sameSet = false; }
			}
			if (a.Contains("FilterSet2")) {
				if (b.Contains("FilterSet2")) { return true; }
				else { sameSet = false; }
			}
			if (a.Contains("FilterSet3")) {
				if (b.Contains("FilterSet3")) { return true; }
				else { sameSet = false; }
			}
			if (a.Contains("FilterSet4")) {
				if (b.Contains("FilterSet4")) { return true; }
				else { sameSet = false; }
			}
			if (a.Contains("FilterSet5")) {
				if (b.Contains("FilterSet5")) { return true; }
				else { sameSet = false; }
			}
			if (a.Contains("FilterSet6")) {
				if (b.Contains("FilterSet6")) { return true; }
				else { sameSet = false; }
			}
			if (a.Contains("FilterSet7")) {
				if (b.Contains("FilterSet7")) { return true; }
				else { sameSet = false; }
			}
			if (a.Contains("FilterSet8")) {
				if (b.Contains("FilterSet8")) { return true; }
				else { sameSet = false; }
			}
			if (a.Contains("FilterSet9")) {
				if (b.Contains("FilterSet9")) { return true; }
				else { sameSet = false; }
			}
			if (b.Contains("FilterSet1") || b.Contains("FilterSet2") || b.Contains("FilterSet3") || b.Contains("FilterSet4") || b.Contains("FilterSet5") || b.Contains("FilterSet6") || b.Contains("FilterSet7") || b.Contains("FilterSet8") || b.Contains("FilterSet9")) {
				sameSet = false;
			}
			if (sameSet == true) { return true; }
			return false;
		}		
	}	

	[HarmonyPatch (typeof (PawnBioAndNameGenerator), "GeneratePawnName")]
	public static class PawnBioAndNameGenerator_GeneratePawnName {
		[HarmonyPriority(Priority.Low)]
		public static bool Prefix(Pawn pawn, ref Name __result, NameStyle style = 0, string forcedLastName = null) {
			if (style != NameStyle.Full) {
				return true;
			}
			RulePackDef nameGenerator = pawn.RaceProps.GetNameGenerator(pawn.gender);
			if (nameGenerator != null) {
				if (nameGenerator.defName.Contains("NamerAnimalGeneric")) {
					if (pawn.Faction != null && !pawn.Faction.def.defName.Contains("Tribe") && pawn.Faction.def.defName != "TribalRaiders") {
						string name;
						REB_NameBank nameBank = REB_Initializer.BankOf(PawnNameCategory.HumanStandard);
						if (Rand.Value > 0.5f) {
							name = nameBank.GetName(REB_NameSlot.First, pawn.gender);
						}
						else {
							if (Rand.Value > 0.67) {
								name = nameBank.GetName(REB_NameSlot.Nick, pawn.gender);
							}
							else {
								name = nameBank.GetName(REB_NameSlot.Nick, Gender.None);
							}
						}
		                __result = new NameSingle(name, false);
						return false;
					}
					else {
						return true;
					}
				}
				return true;
			}
			return true;
		}
	}

	[HarmonyPatch (typeof (PawnBioAndNameGenerator), "GeneratePawnName_Shuffled")]
	public static class PawnBioAndNameGenerator_GeneratePawnName_Shuffled {
		[HarmonyPriority(Priority.Low)]
		public static bool Prefix(Pawn pawn, ref NameTriple __result, string forcedLastName = null) {
			string name_first;
			string name_nick;
			string name_set;
			string name_last;
			PawnNameCategory raceProps = pawn.RaceProps.nameCategory;
			if (raceProps == PawnNameCategory.NoName || raceProps == PawnNameCategory.HumanStandard) {
				REB_NameBank nameBank = REB_Initializer.BankOf(PawnNameCategory.HumanStandard);
				name_last = (forcedLastName == null ? nameBank.GetName(REB_NameSlot.Last, Gender.None, true) : forcedLastName);
				name_first = nameBank.GetName(REB_NameSlot.First, pawn.gender, true);
				name_nick = "";
				name_set = nameBank.GetName(REB_NameSlot.Set, pawn.gender, false);
				if (Rand.Value < 0.33 && name_set.Trim().Length > 0) {
					int break1 = -1;
					int break2 = -1;
					string new_first;
					string new_nick;
					for (int i = 0; i < name_set.Length; i++) {
						if (name_set[i] == ' ' && name_set[i + 1] == '\'' && break1 == -1) {
							break1 = i;
						}
						else if (name_set[i] == '\'') {
							break2 = i;
						}
					}
					if (break1 != -1 && break2 != -1 && (break2 > break1 + 1)) {
						new_first = name_set.Substring(0, break1).Trim();
						new_nick = name_set.Substring(break1 + 2, break2 - break1 - 2).Trim();
						bool nickcheck = (NameUseChecker.AllPawnsNamesEverUsed.Any<Name>((Name x) => {
							NameTriple nameTriple = x as NameTriple;
							return (nameTriple == null ? false : nameTriple.Nick == new_nick);
						}));
						if (nickcheck.Equals(false)) {
							name_first = new_first;
							name_nick = new_nick;
						}
					}
				}
				if (name_nick.Length < 1) {
					int num = 0;
					do {
						num++;
						if (Rand.Value >= 0.33f) {
							name_nick = (Rand.Value >= 0.67f ? name_last : name_first);
						}
						else {
							Gender gender = pawn.gender;
							if (Rand.Value < 0.67f) {
								gender = Gender.None;
							}
							name_nick = nameBank.GetName(REB_NameSlot.Nick, gender, true);
						}
					}
					while (num < 50 && NameUseChecker.AllPawnsNamesEverUsed.Any<Name>((Name x) => {
						NameTriple nameTriple = x as NameTriple;
						return (nameTriple == null ? false : nameTriple.Nick == name_nick);
					}));
				}
				__result = new NameTriple(name_first, name_nick, name_last);
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(PawnBioAndNameGenerator), "GetBackstoryCategoriesFor", null)]
	public static class PawnBioAndNameGenerator_GetBackstoryCategoriesFor {
		public static void Postfix(Pawn pawn, FactionDef faction, ref List<string> __result) {
			if (Controller.Settings.useLiteMode.Equals(false)) {
				for(int i = 0; i < __result.Count; i++) {
					if (__result[i] == "Civil" || __result[i] == "Raider" || __result[i] == "Slave" || __result[i] == "Trader" || __result[i] == "Traveler" || __result[i] == "Tribal") {
						__result[i] = "REB"+__result[i];
					}
				}
			}
			else {
				for(int i = 0; i < __result.Count; i++) {
					if (__result[i] == "Civil") { __result.Add("REBCivil"); }
					if (__result[i] == "Raider") { __result.Add("REBRaider"); }
					if (__result[i] == "Slave") { __result.Add("REBSlave"); }
					if (__result[i] == "Trader") { __result.Add("REBTrader"); }
					if (__result[i] == "Traveler") { __result.Add("REBTraveler"); }
					if (__result[i] == "Tribal") { __result.Add("REBTribal"); }
				}
			}
		}
	}

	[HarmonyPatch(typeof(PawnBioAndNameGenerator), "GiveAppropriateBioAndNameTo")]
	static class Patch_PawnBioAndNameGenerator_GiveAppropriateBioAndNameTo {
		static bool Prefix(Pawn pawn, string requiredLastName, FactionDef factionType) {
			List<string> backstoryCategories = typeof(PawnBioAndNameGenerator).GetMethod("GetBackstoryCategoriesFor", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { pawn, factionType }) as List<string>;
			float baseBioLikelihood = ((float)(SolidBioDatabase.allBios.Count-282)/500);
			if (Controller.Settings.useLiteMode.Equals(true) || baseBioLikelihood > 0.5f) {
				baseBioLikelihood = .5f;
			}
			if (((Rand.Value < (baseBioLikelihood/2)) || ((Rand.Value < (baseBioLikelihood*2)) && pawn.kindDef.factionLeader))) {
				MethodInfo solidBio = typeof(PawnBioAndNameGenerator).GetMethod("TryGiveSolidBioTo", BindingFlags.NonPublic | BindingFlags.Static);
				bool b = (bool) solidBio.Invoke(null, new object[] {pawn, requiredLastName, backstoryCategories});
				if (b.Equals(true)) {
					return false;
				}
			}
			typeof(PawnBioAndNameGenerator).GetMethod("GiveShuffledBioTo", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] {pawn, factionType, requiredLastName, backstoryCategories});
			return false;
		}
	}

	[HarmonyPatch(typeof(PawnBioAndNameGenerator), "GiveShuffledBioTo")]
	static class Patch_PawnBioAndNameGenerator_GiveShuffledBioTo {
		static void Postfix(Pawn pawn, string requiredLastName) {
			REB_NameBank nameBank = REB_Initializer.BankOf(PawnNameCategory.HumanStandard);
			string name_first;
			string name_nick;
			string name_last;
			if (pawn.story.childhood.spawnCategories.Contains("FilterSet1") && nameBank.NamesFor(REB_NameSlot.FilterSet1, Gender.None).Count > 0 && nameBank.NamesFor(REB_NameSlot.FilterSet1, pawn.gender).Count > 0)  {
				name_last = (requiredLastName == null ? nameBank.GetName(REB_NameSlot.FilterSet1, Gender.None, true) : requiredLastName);
				name_first = nameBank.GetName(REB_NameSlot.FilterSet1, pawn.gender, true);
				name_nick = "";
				int num = 0;
				do {
					num++;
					name_nick = (Rand.Value >= 0.67f ? name_last : name_first);
				}
				while (num < 50 && NameUseChecker.AllPawnsNamesEverUsed.Any<Name>((Name x) => {
					NameTriple nameTriple = x as NameTriple;
					return (nameTriple == null ? false : nameTriple.Nick == name_nick);
				}));
				pawn.Name = new NameTriple(name_first, name_nick, name_last);
			}
			if (pawn.story.childhood.spawnCategories.Contains("FilterSet2") && nameBank.NamesFor(REB_NameSlot.FilterSet2, Gender.None).Count > 0 && nameBank.NamesFor(REB_NameSlot.FilterSet2, pawn.gender).Count > 0)  {
				name_last = (requiredLastName == null ? nameBank.GetName(REB_NameSlot.FilterSet2, Gender.None, true) : requiredLastName);
				name_first = nameBank.GetName(REB_NameSlot.FilterSet2, pawn.gender, true);
				name_nick = "";
				int num = 0;
				do {
					num++;
					name_nick = (Rand.Value >= 0.67f ? name_last : name_first);
				}
				while (num < 50 && NameUseChecker.AllPawnsNamesEverUsed.Any<Name>((Name x) => {
					NameTriple nameTriple = x as NameTriple;
					return (nameTriple == null ? false : nameTriple.Nick == name_nick);
				}));
				pawn.Name = new NameTriple(name_first, name_nick, name_last);
			}
			if (pawn.story.childhood.spawnCategories.Contains("FilterSet3") && nameBank.NamesFor(REB_NameSlot.FilterSet3, Gender.None).Count > 0 && nameBank.NamesFor(REB_NameSlot.FilterSet3, pawn.gender).Count > 0)  {
				name_last = (requiredLastName == null ? nameBank.GetName(REB_NameSlot.FilterSet3, Gender.None, true) : requiredLastName);
				name_first = nameBank.GetName(REB_NameSlot.FilterSet3, pawn.gender, true);
				name_nick = "";
				int num = 0;
				do {
					num++;
					name_nick = (Rand.Value >= 0.67f ? name_last : name_first);
				}
				while (num < 50 && NameUseChecker.AllPawnsNamesEverUsed.Any<Name>((Name x) => {
					NameTriple nameTriple = x as NameTriple;
					return (nameTriple == null ? false : nameTriple.Nick == name_nick);
				}));
				pawn.Name = new NameTriple(name_first, name_nick, name_last);
			}
			if (pawn.story.childhood.spawnCategories.Contains("FilterSet4") && nameBank.NamesFor(REB_NameSlot.FilterSet4, Gender.None).Count > 0 && nameBank.NamesFor(REB_NameSlot.FilterSet4, pawn.gender).Count > 0)  {
				name_last = (requiredLastName == null ? nameBank.GetName(REB_NameSlot.FilterSet4, Gender.None, true) : requiredLastName);
				name_first = nameBank.GetName(REB_NameSlot.FilterSet4, pawn.gender, true);
				name_nick = "";
				int num = 0;
				do {
					num++;
					name_nick = (Rand.Value >= 0.67f ? name_last : name_first);
				}
				while (num < 50 && NameUseChecker.AllPawnsNamesEverUsed.Any<Name>((Name x) => {
					NameTriple nameTriple = x as NameTriple;
					return (nameTriple == null ? false : nameTriple.Nick == name_nick);
				}));
				pawn.Name = new NameTriple(name_first, name_nick, name_last);
			}
			if (pawn.story.childhood.spawnCategories.Contains("FilterSet5") && nameBank.NamesFor(REB_NameSlot.FilterSet5, Gender.None).Count > 0 && nameBank.NamesFor(REB_NameSlot.FilterSet5, pawn.gender).Count > 0)  {
				name_last = (requiredLastName == null ? nameBank.GetName(REB_NameSlot.FilterSet5, Gender.None, true) : requiredLastName);
				name_first = nameBank.GetName(REB_NameSlot.FilterSet5, pawn.gender, true);
				name_nick = "";
				int num = 0;
				do {
					num++;
					name_nick = (Rand.Value >= 0.67f ? name_last : name_first);
				}
				while (num < 50 && NameUseChecker.AllPawnsNamesEverUsed.Any<Name>((Name x) => {
					NameTriple nameTriple = x as NameTriple;
					return (nameTriple == null ? false : nameTriple.Nick == name_nick);
				}));
				pawn.Name = new NameTriple(name_first, name_nick, name_last);
			}
			if (pawn.story.childhood.spawnCategories.Contains("FilterSet6") && nameBank.NamesFor(REB_NameSlot.FilterSet6, Gender.None).Count > 0 && nameBank.NamesFor(REB_NameSlot.FilterSet6, pawn.gender).Count > 0)  {
				name_last = (requiredLastName == null ? nameBank.GetName(REB_NameSlot.FilterSet6, Gender.None, true) : requiredLastName);
				name_first = nameBank.GetName(REB_NameSlot.FilterSet6, pawn.gender, true);
				name_nick = "";
				int num = 0;
				do {
					num++;
					name_nick = (Rand.Value >= 0.67f ? name_last : name_first);
				}
				while (num < 50 && NameUseChecker.AllPawnsNamesEverUsed.Any<Name>((Name x) => {
					NameTriple nameTriple = x as NameTriple;
					return (nameTriple == null ? false : nameTriple.Nick == name_nick);
				}));
				pawn.Name = new NameTriple(name_first, name_nick, name_last);
			}
			if (pawn.story.childhood.spawnCategories.Contains("FilterSet7") && nameBank.NamesFor(REB_NameSlot.FilterSet7, Gender.None).Count > 0 && nameBank.NamesFor(REB_NameSlot.FilterSet7, pawn.gender).Count > 0)  {
				name_last = (requiredLastName == null ? nameBank.GetName(REB_NameSlot.FilterSet7, Gender.None, true) : requiredLastName);
				name_first = nameBank.GetName(REB_NameSlot.FilterSet7, pawn.gender, true);
				name_nick = "";
				int num = 0;
				do {
					num++;
					name_nick = (Rand.Value >= 0.67f ? name_last : name_first);
				}
				while (num < 50 && NameUseChecker.AllPawnsNamesEverUsed.Any<Name>((Name x) => {
					NameTriple nameTriple = x as NameTriple;
					return (nameTriple == null ? false : nameTriple.Nick == name_nick);
				}));
				pawn.Name = new NameTriple(name_first, name_nick, name_last);
			}
			if (pawn.story.childhood.spawnCategories.Contains("FilterSet8") && nameBank.NamesFor(REB_NameSlot.FilterSet8, Gender.None).Count > 0 && nameBank.NamesFor(REB_NameSlot.FilterSet8, pawn.gender).Count > 0)  {
				name_last = (requiredLastName == null ? nameBank.GetName(REB_NameSlot.FilterSet8, Gender.None, true) : requiredLastName);
				name_first = nameBank.GetName(REB_NameSlot.FilterSet8, pawn.gender, true);
				name_nick = "";
				int num = 0;
				do {
					num++;
					name_nick = (Rand.Value >= 0.67f ? name_last : name_first);
				}
				while (num < 50 && NameUseChecker.AllPawnsNamesEverUsed.Any<Name>((Name x) => {
					NameTriple nameTriple = x as NameTriple;
					return (nameTriple == null ? false : nameTriple.Nick == name_nick);
				}));
				pawn.Name = new NameTriple(name_first, name_nick, name_last);
			}
			if (pawn.story.childhood.spawnCategories.Contains("FilterSet9") && nameBank.NamesFor(REB_NameSlot.FilterSet9, Gender.None).Count > 0 && nameBank.NamesFor(REB_NameSlot.FilterSet9, pawn.gender).Count > 0)  {
				name_last = (requiredLastName == null ? nameBank.GetName(REB_NameSlot.FilterSet9, Gender.None, true) : requiredLastName);
				name_first = nameBank.GetName(REB_NameSlot.FilterSet9, pawn.gender, true);
				name_nick = "";
				int num = 0;
				do {
					num++;
					name_nick = (Rand.Value >= 0.67f ? name_last : name_first);
				}
				while (num < 50 && NameUseChecker.AllPawnsNamesEverUsed.Any<Name>((Name x) => {
					NameTriple nameTriple = x as NameTriple;
					return (nameTriple == null ? false : nameTriple.Nick == name_nick);
				}));
				pawn.Name = new NameTriple(name_first, name_nick, name_last);
			}
		}
	}

	[HarmonyPatch(typeof(PawnBioAndNameGenerator), "TryGetRandomUnusedSolidName")]
	static class Patch_PawnBioAndNameGenerator_TryGetRandomUnusedSolidName {
		static bool Prefix(Gender gender, ref NameTriple __result, string requiredLastName = null) {
			if ((Controller.Settings.allowVanillaTriples.Equals(true) && Rand.Value < 0.33) || (Controller.Settings.allowVanillaTriples.Equals(false) && Rand.Value < 0.67)) {
				__result = null;
				return false;
			}
			NameTriple nameTriple = null;
			if (Rand.Value < 0.5f) {
				nameTriple = Prefs.RandomPreferredName();
				if (nameTriple != null && (nameTriple.UsedThisGame || (requiredLastName != null && nameTriple.Last != requiredLastName))) {
					nameTriple = null;
				}
			}
			List<NameTriple> listForGender;
			List<NameTriple> list;
			if (Controller.Settings.allowVanillaTriples.Equals(true) && Rand.Value > 0.25f) {
				if (Controller.Settings.useLiteMode.Equals(false) && Rand.Value > 0.95f) {
					listForGender = PKPawnNameDatabaseSolid.GetListForGender(GenderPossibility.Either);
					list = (gender != Gender.Male) ? PKPawnNameDatabaseSolid.GetListForGender(GenderPossibility.Female) : PawnNameDatabaseSolid.GetListForGender(GenderPossibility.Male);
				}
				else {
					listForGender = PawnNameDatabaseSolid.GetListForGender(GenderPossibility.Either);
					list = (gender != Gender.Male) ? PawnNameDatabaseSolid.GetListForGender(GenderPossibility.Female) : PawnNameDatabaseSolid.GetListForGender(GenderPossibility.Male);
				}
			}
			else {
				listForGender = REBPawnNameDatabaseSolid.GetListForGender(GenderPossibility.Either);
				list = (gender != Gender.Male) ? REBPawnNameDatabaseSolid.GetListForGender(GenderPossibility.Female) : PawnNameDatabaseSolid.GetListForGender(GenderPossibility.Male);
			}
			List<NameTriple> list2;
			if (listForGender.Count == 0 && list.Count == 0) {
				Log.Error("No solid pawn name list for gender: " + gender + ".", false);
				__result = null;
				return false;
			}
			if (listForGender.Count == 0) { list2 = list; }
			else if (list.Count == 0) { list2 = listForGender; }
			else {
				float num = ((float)listForGender.Count + 0.1f) / ((float)(listForGender.Count + list.Count) + 0.1f);
				if (Rand.Value < num) { list2 = listForGender; }
				else { list2 = list; }
			}
			if (nameTriple != null && list2.Contains(nameTriple)) {
				__result = nameTriple;
				return false;
			}
			list2.Shuffle<NameTriple>();
			__result = (from name in list2
			where (requiredLastName == null || !(name.Last != requiredLastName)) && !name.UsedThisGame
			select name).FirstOrDefault<NameTriple>();
			return false;
		}
	}

	[HarmonyPatch(typeof(PawnGenerator), "GenerateBodyType")]
	public static class Patch_PawnGenerator_GenerateBodyType {
		static bool Prefix(Pawn pawn) {
			BodyTypeDef bodyGlobal;
			BodyTypeDef bodyMale;
			BodyTypeDef bodyFemale;
			float randBodyType;
			if (pawn.story.adulthood != null) {
				bodyGlobal = pawn.story.adulthood.BodyTypeFor(Gender.None);
				bodyMale = pawn.story.adulthood.BodyTypeFor(Gender.Male);
				bodyFemale = pawn.story.adulthood.BodyTypeFor(Gender.Female);
			}
			else {
				bodyGlobal = pawn.story.childhood.BodyTypeFor(Gender.None);
				bodyMale = pawn.story.childhood.BodyTypeFor(Gender.Male);
				bodyFemale = pawn.story.childhood.BodyTypeFor(Gender.Female);
			}
			if (bodyGlobal != null) {
				pawn.story.bodyType = bodyGlobal;
			}
			else if ((bodyMale == BodyTypeDefOf.Male && pawn.gender == Gender.Male)
			  || (bodyFemale == BodyTypeDefOf.Female && pawn.gender == Gender.Female)) {
				randBodyType = Rand.Value;
				if (randBodyType < 0.05) {
					pawn.story.bodyType = BodyTypeDefOf.Hulk;
				}
				else if (randBodyType < 0.1) {
					pawn.story.bodyType = BodyTypeDefOf.Fat;
				}
				else if (randBodyType < 0.5) {
					pawn.story.bodyType = BodyTypeDefOf.Thin;
				}
				else if (pawn.gender == Gender.Female) {
					pawn.story.bodyType = BodyTypeDefOf.Female;
				}
				else {
					pawn.story.bodyType = BodyTypeDefOf.Male;
				}		
			}
			else if (pawn.gender == Gender.Female) {
				pawn.story.bodyType = bodyFemale;
			}
			else {
				pawn.story.bodyType = bodyMale;
			}
			return false;
		}
	}

	[HarmonyPatch(typeof(SolidBioDatabase), "LoadAllBios")]
	public static class Patch_SolidBioDatabase_LoadAllBios {
		static bool Prefix() {
			foreach (PawnBio pawnBio in DirectXmlLoader.LoadXmlDataInResourcesFolder<PawnBio>("Backstories/Solid")) {
				pawnBio.name.ResolveMissingPieces(null);
				if (pawnBio.childhood == null || pawnBio.adulthood == null) {
					PawnNameDatabaseSolid.AddPlayerContentName(pawnBio.name, pawnBio.gender);
				}
				else {
					PKPawnNameDatabaseSolid.AddPlayerContentName(pawnBio.name, pawnBio.gender);
					pawnBio.PostLoad();
					pawnBio.ResolveReferences();
					foreach (string str in pawnBio.ConfigErrors()) {
						Log.Error(str, false);
					}
					SolidBioDatabase.allBios.Add(pawnBio);
					pawnBio.childhood.shuffleable = false;
					pawnBio.childhood.slot = BackstorySlot.Childhood;
					pawnBio.adulthood.shuffleable = false;
					pawnBio.adulthood.slot = BackstorySlot.Adulthood;
					BackstoryHardcodedData.InjectHardcodedData(pawnBio);
					BackstoryDatabase.AddBackstory(pawnBio.childhood);
					BackstoryDatabase.AddBackstory(pawnBio.adulthood);					
				}
			}
			return false;
		}
	}

//
//	Read Backstories From XML Files
//
	
	public class BackstoryDef : Def {
		#region XML Data
		public string baseDesc;
        public string bodyTypeGlobal;
        public string bodyTypeMale;
        public string bodyTypeFemale;
		public string title;
		public string titleFemale;
		public string titleShort;
		public string titleShortFemale;
		public BackstorySlot slot = BackstorySlot.Adulthood;
		public bool shuffleable = true;
		public bool addToDatabase = true;
		public List<WorkTags> workAllows = new List<WorkTags>();
		public List<WorkTags> workDisables = new List<WorkTags>();
		public List<WorkTags> requiredWorkTags = new List<WorkTags>();
		public List<BackstoryDefSkillListItem> skillGains = new List<BackstoryDefSkillListItem>();
		public List<string> spawnCategories = new List<string>();
		public List<BackstoryDefTraitListItem> forcedTraits = new List<BackstoryDefTraitListItem>();
		public List<BackstoryDefTraitListItem> disallowedTraits = new List<BackstoryDefTraitListItem>();
		#endregion
		public static BackstoryDef Named(string defName) {
			return DefDatabase<BackstoryDef>.GetNamed(defName);
		}
		public override void ResolveReferences() {
			base.ResolveReferences();
			if (!this.addToDatabase) return;
			if (BackstoryDatabase.allBackstories.ContainsKey(this.UniqueSaveKey())) {
				Log.Error("Backstory Error ("+this.defName+"): Duplicate defName.");
				return;
			}
			Backstory b = new Backstory();
			if (!string.IsNullOrEmpty(this.title) || !string.IsNullOrEmpty(this.titleFemale))
				b.SetTitle((string.IsNullOrEmpty(this.title) ? this.titleFemale : this.title),(string.IsNullOrEmpty(this.titleFemale) ? this.title : this.titleFemale));
			else {
				return;
			}
			if (!string.IsNullOrEmpty(titleShort) || !string.IsNullOrEmpty(titleShortFemale))
				b.SetTitleShort((string.IsNullOrEmpty(this.titleShort) ? this.titleShortFemale : this.titleShort),(string.IsNullOrEmpty(this.titleShortFemale) ? this.titleShort : this.titleShortFemale));
			else
				b.SetTitleShort((string.IsNullOrEmpty(this.title) ? this.titleFemale : this.title),(string.IsNullOrEmpty(this.titleFemale) ? this.title : this.titleFemale));
			if (!baseDesc.NullOrEmpty())
				b.baseDesc = baseDesc;
			else {
				b.baseDesc = "Empty.";
			}
			bool bodyTypeSet = false;
			if (!string.IsNullOrEmpty(bodyTypeGlobal)) {
				bodyTypeSet = SetGlobalBodyType(b, bodyTypeGlobal);
			}
			if (!bodyTypeSet) {
				if (!SetMaleBodyType(b, bodyTypeMale)) {
					SetMaleBodyType(b, "Male");
				}
				if (!SetFemaleBodyType(b, bodyTypeFemale)) {
					SetFemaleBodyType(b, "Female");
				}
			}
			b.slot = slot;
			b.shuffleable = shuffleable;
			if (spawnCategories.NullOrEmpty()) {
				return;
			}
			else
				for(int i = 0; i < spawnCategories.Count; i++) {
					if (spawnCategories[i] == "Civil" || spawnCategories[i] == "Raider" || spawnCategories[i] == "Slave" || spawnCategories[i] == "Trader" || spawnCategories[i] == "Traveler" || spawnCategories[i] == "Tribal") {
						spawnCategories[i] = "REB"+spawnCategories[i];
					}
				}
				b.spawnCategories = spawnCategories;
			if (workAllows.Count > 0) {
				foreach (WorkTags current in Enum.GetValues(typeof(WorkTags))) {
					if (!workAllows.Contains(current)) {
						b.workDisables |= current;
					}
				}
			}
			else if (workDisables.Count > 0) {
				foreach (var tag in workDisables) {
					b.workDisables |= tag;
				}
			}
			else {
				b.workDisables = WorkTags.None;
			}
			if (requiredWorkTags.Count > 0) {
				foreach (var tag in requiredWorkTags) {
					b.requiredWorkTags |= tag;
				}
			}
			else {
				b.requiredWorkTags = WorkTags.None;
			}
			Dictionary<SkillDef, int> d = new Dictionary<SkillDef, int>();
			foreach (BackstoryDefSkillListItem i in skillGains) {
				SkillDef def = DefDatabase<SkillDef>.GetNamed(i.key, false);
				if (def == null) {
					if (i.key.Equals("Growing")) {
						Log.Message("Editable Backstories: Converting SkillDef of [" + i.key + "] to [" + SkillDefOf.Plants.ToString() + "] for Backstory.Title [" + b.title + "]");
						def = SkillDefOf.Plants;
					}
					else {
						Log.Error("Editable Backstories: Unable to find SkillDef of [" + i.key + "] for Backstory.Title [" + b.title + "]");
					}
				}
				if (def != null) {
					d.Add(def, i.value);
				}
			}
			b.skillGainsResolved = d;
			Dictionary<string, int> fTraitList = forcedTraits.ToDictionary(i => i.key, i=> i.value);
			if (fTraitList.Count > 0) {
				b.forcedTraits = new List<TraitEntry>();
				foreach (KeyValuePair<string,int> trait in fTraitList) {
					b.forcedTraits.Add(new TraitEntry(TraitDef.Named(trait.Key), trait.Value));
				}
			}
			Dictionary<string, int> dTraitList = disallowedTraits.ToDictionary(i => i.key, i=> i.value);
			if (dTraitList.Count > 0) {
				b.disallowedTraits = new List<TraitEntry>();
				foreach (KeyValuePair<string,int> trait in dTraitList) {
					b.disallowedTraits.Add(new TraitEntry(TraitDef.Named(trait.Key), trait.Value));
				}
			}
			b.ResolveReferences();
			b.PostLoad();
			b.identifier = this.UniqueSaveKey();
			bool flag = false;
			foreach (var s in b.ConfigErrors(false)) {
				Log.Error("Backstory Error ("+b.identifier+"): "+s);
				if (!flag) {
					flag = true;
				}
			}
			if (!flag)	{
				if (b.slot.Equals(BackstorySlot.Adulthood)) {
					if (b.shuffleable.Equals(true)) {
						REB_Initializer.adultCount++;
					}
					else {
						REB_Initializer.adultNSCount++;
					}
				}
				if (b.slot.Equals(BackstorySlot.Childhood)) {
					if (b.shuffleable.Equals(true)) {
						REB_Initializer.childCount++;
					}
					else {
						REB_Initializer.childNSCount++;
					}
				}
				BackstoryDatabase.allBackstories.Add(b.identifier, b);
			}
		}
		private static bool SetGlobalBodyType(Backstory b, string s) {
			BodyTypeDef def;
			if (TryGetBodyTypeDef(s, out def)) {
				typeof(Backstory).GetField("bodyTypeGlobal", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(b, def.ToString());
				typeof(Backstory).GetField("bodyTypeGlobalResolved", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(b, def);
				return true;
			}
			return false;
		}
		private static bool SetMaleBodyType(Backstory b, string s) {
			BodyTypeDef def;
			if (TryGetBodyTypeDef(s, out def)) {
				typeof(Backstory).GetField("bodyTypeMale", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(b, def.ToString());
				typeof(Backstory).GetField("bodyTypeMaleResolved", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(b, def);
				return true;
			}
			return false;
		}
		private static bool SetFemaleBodyType(Backstory b, string s) {
			BodyTypeDef def;
			if (TryGetBodyTypeDef(s, out def)) {
				typeof(Backstory).GetField("bodyTypeFemale", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(b, def.ToString());
				typeof(Backstory).GetField("bodyTypeFemaleResolved", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(b, def);
				return true;
			}
			return false;
		}
		private static bool TryGetBodyTypeDef(string s, out BodyTypeDef def) {
			if (string.IsNullOrEmpty(s)) {
				def = null;
				return false;
			}
			def = DefDatabase<BodyTypeDef>.GetNamed(s, false);
			if (def == null)
				return false;
			return true;
		}
	}

	public static class BackstoryDefExt {
		public static string UniqueSaveKey(this BackstoryDef def) {
			return "REB_" + def.defName;
		}
	}

	public struct BackstoryDefSkillListItem {
		public string key;
		public int value;
	}
	
	public struct BackstoryDefTraitListItem {
		public string key;
		public int value;
	}

//
//	Read Names From XML Files
//

	public class NameDef : Def {
		#region XML Data
		public GenderPossibility gender;
		public NameTriple name;
		public string childhoodStory;
		public string adulthoodStory;
		#endregion
		public static NameDef Named(string defName) {
			return DefDatabase<NameDef>.GetNamed(defName);
		}
		public PawnBioType BioType {
			get { return PawnBioType.Undefined; }
		}
		public override void ResolveReferences() {
			base.ResolveReferences();
			PawnBio bio = new PawnBio();
			bio.name = name;
			bio.gender = gender;
			if (bio.gender != GenderPossibility.Male && bio.gender != GenderPossibility.Female) {
				bio.gender = GenderPossibility.Either;
			}
			bio.PostLoad();
			if (bio.name.First.NullOrEmpty() || bio.name.Last.NullOrEmpty()) {
				if (!childhoodStory.NullOrEmpty() || !adulthoodStory.NullOrEmpty()) {
					Log.Error("Backstory Error ("+bio.name+"): A locked backstory can only be attached to a full name.");
				}
				if (bio.name.First.NullOrEmpty() && bio.name.Last.NullOrEmpty()) {
					if (!bio.name.Nick.NullOrEmpty()) {
						if (bio.gender == GenderPossibility.Male) {
							REB_Initializer.NamesNicksMale.Add(bio.name.Nick);
						}
						else if (bio.gender == GenderPossibility.Female) {
							REB_Initializer.NamesNicksFemale.Add(bio.name.Nick);
						}
						else {
							REB_Initializer.NamesNicksUnisex.Add(bio.name.Nick);
						}
					}
				}
				else if (bio.name.First.NullOrEmpty()) {
					if (!bio.name.Last.NullOrEmpty()) {
						REB_Initializer.NamesLast.Add(bio.name.Last);
					}
				}
				else if (bio.name.Last.NullOrEmpty()) {
					if (!bio.name.First.NullOrEmpty()) {
						if (bio.gender == GenderPossibility.Male) {
							REB_Initializer.NamesFirstMale.Add(bio.name.First);
						}
						else if (bio.gender == GenderPossibility.Female) {
							REB_Initializer.NamesFirstFemale.Add(bio.name.First);
						}
						else {
							REB_Initializer.NamesFirstMale.Add(bio.name.First);
							REB_Initializer.NamesFirstFemale.Add(bio.name.First);
						}
					}
				}
			}
			else {
				bio.name.ResolveMissingPieces(null);
				if ((!childhoodStory.NullOrEmpty() && adulthoodStory.NullOrEmpty())
				  || (childhoodStory.NullOrEmpty() && !adulthoodStory.NullOrEmpty())) {
					Log.Error("Backstory Error ("+bio.name+"): A locked backstory must include both a childhood story and an adulthood story.");
				}
				if (!childhoodStory.NullOrEmpty() && !BackstoryDatabase.allBackstories.ContainsKey("REB_"+childhoodStory)) {
					Log.Error("Backstory Error ("+bio.name+"): Childhood backstory '"+childhoodStory+"' does not exist.");
					childhoodStory = "";
				}
				if (!adulthoodStory.NullOrEmpty() && !BackstoryDatabase.allBackstories.ContainsKey("REB_"+adulthoodStory)) {
					Log.Error("Backstory Error ("+bio.name+"): Adulthood backstory '"+adulthoodStory+"' does not exist.");
					adulthoodStory = "";
				}
				if (!childhoodStory.NullOrEmpty() && !adulthoodStory.NullOrEmpty()) {
					bio.childhood = BackstoryDatabase.allBackstories["REB_"+childhoodStory];
					bio.adulthood = BackstoryDatabase.allBackstories["REB_"+adulthoodStory];
					bio.pirateKing = true;
					SolidBioDatabase.allBios.Add(bio);
					REB_Initializer.fullBioCount++;
				}
				else {
					REBPawnNameDatabaseSolid.AddPlayerContentName(bio.name, bio.gender);
					REB_Initializer.fullCount++;
				}
			}
		}
	}
    
}
