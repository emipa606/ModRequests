/*
# Random Factions Rimworld Mod
Author: Dr. Plantabyte (aka Christopher C. Hall)
## CC BY 4.0

This work is licensed on the [Attribution 4.0 International (CC BY 4.0)](https://creativecommons.org/licenses/by/4.0/) Creative Commons License.


### You are free to:

* **Share** — copy and redistribute the material in any medium or format
* **Adapt** — remix, transform, and build upon the material
    for any purpose, even commercially.


### Under the following terms:

* **Attribution** — You must give appropriate credit, provide a link to the license, and indicate if changes were made. You may do so in any reasonable manner, but not in any way that suggests the licensor endorses you or your use.

* **No additional restrictions** — You may not apply legal terms or technological measures that legally restrict others from doing anything the license permits.

### Guarentees:

The licensor cannot revoke these freedoms as long as you follow the license terms.
 */


using HarmonyLib;
using HugsLib.Logs;
using HugsLib.Settings;
using HugsLib.Utils;
using RandomFactions.filters;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine.SceneManagement;
using Verse;

namespace RandomFactions
{

    public class RandomFactionsMod : HugsLib.ModBase
    {
        private SettingHandle<bool> removeOtherFactions;
        private SettingHandle<int> xenoPercentHandle;
        private SettingHandle<bool> allowDuplicates;
        private Dictionary<FactionDef, int> zeroCountRecord = new Dictionary<FactionDef, int>();
        private Dictionary<FactionDef, int> randCountRecord = new Dictionary<FactionDef, int>();
        private Dictionary<string, FactionDef> patchedXenotypeFactions = new Dictionary<string, FactionDef>();
        public static string RANDOM_CATEGORY_NAME = "Random";
        public static string XENOPATCH_CATEGORY_NAME = "Xenopatch";

        public static bool isXenotypePatchable(FactionDef def)
        {
            return !(def.isPlayer || def.hidden || def.maxConfigurableAtWorldCreation <= 1 
                || RANDOM_CATEGORY_NAME.EqualsIgnoreCase(def.categoryTag));
        }
        public static bool isXenotypeReplaceable(FactionDef def)
        {
            return isXenotypePatchable(def) && (def.xenotypeSet == null || def.xenotypeSet.BaselinerChance >= 0.65);
        }

        public static string defListToString(IEnumerable<Def> allDefs)
        {
            string s = "";
            foreach (var def in allDefs)
            {
                if (s.Length > 0) { s += ", "; }
                s += def.defName;
            }
            return s;
        }

        public static string xenoFactionDefName(XenotypeDef xdef, FactionDef fdef)
        {
            return xdef.defName + fdef.defName;
        }

        public RandomFactionsMod() {
            // constructor (invoked by reflection, do not add parameters)
            Logger.Trace("RandomFactions constructed");
        }
        public override string ModIdentifier
        {
            /*
Each ModBase class needs to have a unique identifier. Provide yours by overriding the ModIdentifier property. The identifier will be used in the settings XML to store your settings, so avoid spaces and special characters. You will get an exception if you provide an improper identifier.
             */
            get
            {
                return "RandFactions";
            }
        }
        /*
Property Notes: HugsLib.ModBase.*

.Logger

The Logger property allows a mod to write identifiable messages to the console. Error and Warning methods are also available. Calling:
Logger.Message("test");
will result in the following console output:
[ModIdentifier] test
Additionally, the Trace method of the logger will write a console message only if Rimworld is in Dev mode.

.ModIsActive

Returns true if the mod is enabled in the Mods dialog. Disabled mods would not be loaded or instantiated, but if a mod was enabled, and then disabled in the Mods dialog this property will return false.
This property is no longer useful as of A17, since the game restarts when the mod configuration changes.

.Settings

Returns the ModSettingsPack for your mod, from where you can get your SettingsHandles. See the wiki page of creating configurable settings for more information.

.ModContentPack

Returns the ModContentPack for your mod. This can be used to access the name and PackageId, as well as loading custom files from your mod's directory.

.HarmonyInstance

All assemblies that declare a class that extends ModBase are automatically assigned a HarmonyInstance and their Harmony patches are applied. This is where the Harmony instance for each ModBase instance is stored.

.HarmonyAutoPatch

Override this and return false if you don't want a HarmonyInstance to be automatically created and the patches in your assembly applied. Having multiple ModBase classes in your assembly will produce warnings if their HarmonyAutoPatch is not disabled, but your assembly will only be patched once.
*/

        //        public override void EarlyInitialize()
        //        {
        //            /*
        //Called during Verse.Mod instantiation, and only if your class has the [EarlyInit] attribute.
        //Nothing is yet loaded at this point, so you might want to place your initialization code in Initialize, instead this method is mostly used for custom patching.
        //You will not receive any callbacks on Update, FixedUpdate, OnGUI and SettingsChanged until after the Initialize callback comes through.
        //Initialize will still be called at the normal time, regardless of the [EarlyInit] attribute.*/
        //            base.EarlyInitialize();
        //        }

        public override void Initialize()
        {
            /*
Called once when the mod is first initialized, closely after it is instantiated.
If the mods configuration changes, or Defs are reloaded, this method is not called again.*/
            base.Initialize();
        }

        public override void DefsLoaded()
        {
            /*
Called after all Defs are loaded.
This happens when game loading has completed, after Initialize is called. This is a good time to inject any Random defs. Make sure you call HugsLib.InjectedDefHasher.GiveShortHasToDef on any defs you manually instantiate to avoid def collisions (it's a vanilla thing).
Since A17 it no longer matters where you initialize your settings handles, since the game automatically restarts both when the mod configuration or the language changes. This means that both Initialize and DefsLoaded are only ever called once per ModBase instance.*/
            base.DefsLoaded();

            // add mod options
            removeOtherFactions = Settings.GetHandle<bool>(
            "removeOtherFactions",
            "Re-organise Factions",
            "Removes all non-random factions from the starting faction list in the New World screen during New Colony creation.",
            true);

            if (removeOtherFactions.Value == true) {
                zeroCountFactionDefs();
            }

            this.xenoPercentHandle = Settings.GetHandle<int>("PercentXenotype", "% Xenotype Fequency", "If Biotech DLC is detected, then random factions will substitute baseliners for xenotypes this percent of the time (default 15%)", 15, Validators.IntRangeValidator(0, 100));
            //xenoPercentHandle.ValueChanged += handle => {
            //    Logger.Message("Xenotype changed to " + xenoPercentHandle.Value);
            //};

            // add procedural defs
            // if Biotech DLC is installed, patch-in xeno versions of human factions
            if (Verse.ModsConfig.BiotechActive)
            {
                createXenoFactions();
                Logger.Message("Created Xenotype versions of Baseliner factions.");
                // if VFE mods are installed, need to tell VFE Core to update the cache or there will be a Dictionary key error
                // call VFECore.ScenPartUtility.SetCache() by reflection (which is tricky because it is in a different assdembly)
                bool done = false;
                foreach (Assembly asmb in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (done) break;
                    foreach (Type classType in asmb.GetTypes())
                    {
                        if (done) break;
                        if ("ScenPartUtility".Equals(classType.Name))
                        {
                            if (classType != null)
                            {
                                //Logger.Message(string.Format("Found {0}.{1}", asmb.FullName, classType.Name));
                                var methodHandle = classType.GetMethod("SetCache");
                                if (methodHandle != null)
                                {
                                    methodHandle.Invoke(null, null);
                                    Logger.Message("Invoked VFECore.ScenPartUtility.SetCache() to refresh VFE internal cache");
                                    done = true;
                                }
                            }
                        }
                    }
                }
                // if VFE is installed, then we also need to invoke Find.World.GetComponent<NewFactionSpawningState>().Ignore(factionDef); 
                // to keep it from pestering the player about 100+ new factions being added to the game

            }

            // allow or disallow picking factions already added:
            allowDuplicates = Settings.GetHandle<bool>(
            "allowDuplicates",
            "Allow Duplicate Factions",
            "Allows Random Factions to randomly add a faction that already exists in the world.",
            false);
            
        }

        private void createXenoFactions()
        {
            var newDefs = new List<FactionDef>();
            foreach(var def in DefDatabase<FactionDef>.AllDefs)
            {
                var oldDefName = def.defName;
                if (isXenotypePatchable(def))
                {
                    foreach (var xenodef in DefDatabase<XenotypeDef>.AllDefs) {
                        var defCopy = cloneDef(def);
                        defCopy.defName = xenoFactionDefName(xenodef, defCopy);
                        defCopy.categoryTag = XENOPATCH_CATEGORY_NAME;
                        defCopy.label = xenodef.label + " " + defCopy.label;
                        var xenoChance = new XenotypeChance(xenodef, 1.0f);
                        List<XenotypeChance> xenotypeChances = new List<XenotypeChance>
                        {
                            xenoChance
                        };
                        var newXenoSet = new XenotypeSet();
                        // I think Ludeon Studios hates procedural generation. Why make XenotypeSet read-only with no constructor?!
                        // Need to use reflection voodoo to modify private variable (whose name might change in a future version)
                        FieldInfo[] fields = typeof(XenotypeSet).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                        foreach (FieldInfo field in fields)
                        {
                            if (field.FieldType.IsAssignableFrom(xenotypeChances.GetType()))
                            {
                                field.SetValue(newXenoSet, xenotypeChances);
                            }
                        }
                        defCopy.xenotypeSet = newXenoSet;
                        defCopy.maxConfigurableAtWorldCreation = 0; // NOTE:Faction Control messes with this number
                        defCopy.hidden = true; // will unhide at game load time
                        newDefs.Add(defCopy);
                        //Logger.Trace(string.Format("Generated procedural faction def {0} has xenotype set: {1}", defCopy.defName, XenotypeSetToString(defCopy.xenotypeSet)));
                    }
                }
            }
            foreach (var def in newDefs)
            {

                patchedXenotypeFactions.Add(def.defName, def);
                DefDatabase<FactionDef>.Add(def);
            }
        }

        private FactionDef cloneDef(FactionDef def)
        {
            // use reflection magic to do a 1-deep clone of the def
            FactionDef cpy = new FactionDef();
            reflectionCopy(def, cpy);
            cpy.debugRandomId = (ushort)(def.debugRandomId + 1);
            return cpy;
        }

        private void reflectionCopy(object A, object B)
        {
            FieldInfo[] fields = A.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                var value = field.GetValue(A);
                field.SetValue(B, value);
            }
            /*
            PropertyInfo[] props = A.GetType().GetProperties();
            foreach(PropertyInfo prop in props)
            {
                var value = prop.GetValue(A, null);
                prop.SetValue(B, value, null);
            }*/
        }

        public override void SettingsChanged()
        {
            /*
Called after the player closes the Mod Settings dialog after changing any setting.
Note, that the setting changed may belong to another mod.*/
            base.SettingsChanged();
            if (removeOtherFactions.Value == true)
            {
                zeroCountFactionDefs();
            } else
            {
                undoZeroCountFactionDefs();
            }
        }

        private void zeroCountFactionDefs()
        {
            /*
            var hasVFEMechanoids = false;
            var hasVFEInsects = false;
            bool hasVFEMechanoids = ModLister.GetActiveModWithIdentifier("OskarPotocki.VFE.Mechanoid") != null;
            foreach (var m in Verse.ModLister.AllInstalledMods)
            {
                if (m.PackageId.EqualsIgnoreCase("OskarPotocki.VFE.Mechanoid")) { hasVFEMechanoids = true; }
                if (m.PackageId.EqualsIgnoreCase("OskarPotocki.VFE.Insectoid")) { hasVFEInsects = true; }
            }*/
            foreach (var def in DefDatabase<FactionDef>.AllDefs)
            {
                if (!def.hidden && !def.isPlayer && !RANDOM_CATEGORY_NAME.EqualsIgnoreCase(def.categoryTag)
                    && !"Empire".EqualsIgnoreCase(def.defName))
                {
                    zeroCountRecord[def] = def.startingCountAtWorldCreation; // save for later undo operation
                    def.startingCountAtWorldCreation = 0;
                }/*
                else if ("Mechanoid".EqualsIgnoreCase(def.defName) && hasVFEMechanoids)
                {
                    def.startingCountAtWorldCreation = 0;
                }
                else if ("Insect".EqualsIgnoreCase(def.defName) && hasVFEInsects)
                {
                    def.startingCountAtWorldCreation = 0;
                }*/
            }

            foreach (var def in randCountRecord.Keys)
            {
                var val = randCountRecord[def];
                def.startingCountAtWorldCreation = val;
            }
        }

        private void undoZeroCountFactionDefs()
        {
            foreach (var def in zeroCountRecord.Keys)
            {
                var val = zeroCountRecord[def];
                def.startingCountAtWorldCreation = val;
            }
            foreach (var def in DefDatabase<FactionDef>.AllDefs)
            {
                if (RANDOM_CATEGORY_NAME.EqualsIgnoreCase(def.categoryTag))
                {
                    randCountRecord[def] = def.startingCountAtWorldCreation;
                    def.startingCountAtWorldCreation = 0;
                }
            }
        }

        public override void Update()
        {
            /*
Called on each frame.
Keep in mind that frame rate varies significantly, so this callback is recommended only to do any custom drawing.*/
        }

        public override void FixedUpdate()
        {
            /*
Called on each physics update by Unity.
This is like Update, but independent of frame rate.*/
            base.FixedUpdate();
        }

        public override void OnGUI()
        {
            /*
Called when the Unity GUI system is redrawn or receives an input event.
This is a good time to draw custom GUI overlays and controls.
OnGUI will no longer be called during loading screens and will have UI scaling automatically applied.
Also useful for listening for input events, such as key strokes. Here's an example of a key binding listener:

if (Event.current.type == EventType.KeyDown) {
	if (KeyBindingDefOf.Misc1.JustPressed) {
		// do things
	}
}
             */
            base.OnGUI();
        }

        public override void SceneLoaded(Scene scene)
        {
            /*
Called after a Unity scene change. Receives a UnityEngine.SceneManagement.Scene type argument.
There are two scenes in Rimworld- Entry and Play, which stand for the menu, and the game itself. Use Verse.GenScene to check which scene has been loaded.
Note, that not everything may be initialized after the scene change, and the game may be in the middle of map loading or generation.*/
            base.SceneLoaded(scene);
            if (Verse.GenScene.InEntryScene)
            {
                hideXenoPatches(false);
            } else
            {
                hideXenoPatches(true);
            }
        }

        private void hideXenoPatches(bool hide)
        {
            foreach(var def in DefDatabase<FactionDef>.AllDefs)
            {
                if(XENOPATCH_CATEGORY_NAME.EqualsIgnoreCase(def.categoryTag))
                {
                    def.hidden = hide;
                }
            }
        }

        public override void WorldLoaded()
        {
            /*
Called after the game has started and the world has been initialized.
Any maps may not have been initialized at this point.
This is a good place to get your UtilityWorldObjects with the data you store in the save file. See the appropriate wiki page on how to use those.
This is only called after the game has started, not on the "select landing spot" world map.
*/
            base.WorldLoaded();
            Logger.Message("World loaded! Applying Random generation rules to factions...");
            var world = Find.World;
            fixVFENewFactionPopups(world);
            Logger.Trace(string.Format("Found {0} faction definitions: {1}", DefDatabase<FactionDef>.DefCount,
                defListToString(DefDatabase<FactionDef>.AllDefs)));
            var hasRoyalty = Verse.ModsConfig.RoyaltyActive;
            var hasIdeology = Verse.ModsConfig.IdeologyActive;
            var hasBiotech = Verse.ModsConfig.BiotechActive;
            if (hasBiotech)
            {
                Logger.Trace(string.Format("Found {0} xenotype definitions: {1}", DefDatabase<XenotypeDef>.DefCount,
                defListToString(DefDatabase<XenotypeDef>.AllDefs)));
            }

            // load save data store (if it exists)
            //var dataSore = Find.World.GetComponent<RandFacDataStore>();
            // ignore generated factions when choosing random factions
            var ignoreList = new List<string>();
            foreach(var defName in patchedXenotypeFactions.Keys)
            {
                ignoreList.Add(defName);
            }
            int xenoPercent = xenoPercentHandle.Value;
            if (!hasBiotech) xenoPercent = 0;
            RandomFactionGenerator fgen = new RandomFactionGenerator(world, xenoPercent, DefDatabase<FactionDef>.AllDefs, ignoreList.ToArray(), hasRoyalty, hasIdeology, hasBiotech, Logger);
            var allFactionList = new List<Faction>();
            var replaceList = new List<Faction>();
            foreach (var fac in Find.FactionManager.AllFactions) { allFactionList.Add(fac); }
            foreach(var fac in allFactionList)
            {
                Logger.Trace(string.Format("Found faction: {0} ({1})\tisPlayer == {2}\tisRandom == {3}\tisDefeated == {4}", fac.Name, fac.def.defName, fac.IsPlayer, fac.def.categoryTag.EqualsIgnoreCase(RANDOM_CATEGORY_NAME), fac.defeated)); // TODO: remove
                if (fac.def.categoryTag.EqualsIgnoreCase(RANDOM_CATEGORY_NAME) && fac.defeated == false)
                {
                    Logger.Trace(string.Format(">>> Detected random faction {0} ({1})", fac.Name, fac.def.defName)); // TODO: remove
                    replaceList.Add(fac);
                }
            }
            foreach(var pfFac in replaceList)
            {
                if (pfFac.def.defName.EqualsIgnoreCase("RF_RandomFaction"))
                {
                    fgen.replaceWithRandomNonHiddenFaction(pfFac, allowDuplicates.Value);
                }
                else if (pfFac.def.defName.EqualsIgnoreCase("RF_RandomPirateFaction"))
                {
                    fgen.replaceWithRandomNonHiddenEnemyFaction(pfFac, allowDuplicates.Value);
                }
                else if (pfFac.def.defName.EqualsIgnoreCase("RF_RandomRoughFaction"))
                {
                    fgen.replaceWithRandomNonHiddenWarlordFaction(pfFac, allowDuplicates.Value);
                }
                else if (pfFac.def.defName.EqualsIgnoreCase("RF_RandomTradeFaction"))
                {
                    fgen.replaceWithRandomNonHiddenTraderFaction(pfFac, allowDuplicates.Value);
                }
                else if (pfFac.def.defName.EqualsIgnoreCase("RF_RandomMechanoid"))
                {
                    fgen.replaceWithRandomNamedFaction(pfFac, allowDuplicates.Value, "Mechanoid", "VFE_Mechanoid");
                }
                else if (pfFac.def.defName.EqualsIgnoreCase("RF_RandomInsectoid"))
                {
                    fgen.replaceWithRandomNamedFaction(pfFac, allowDuplicates.Value, "Insect", "VFEI_Insect");
                }
                else
                {
                    Logger.Warning("Faction defName {0} not recognized! Cannot replace faction {1} ({2})", pfFac.def.defName, pfFac.Name, pfFac.def.defName);
                }
            }
            Logger.Message(string.Format("...Random faction generation complete! Replaced {0} factions.", replaceList.Count));

        }
        public override void MapComponentsInitializing(Map map)
        {
            /*
Called during the initialization of a map, more exactly right after Verse.Map.ConstructComponents(). Receives a Verse.Map type argument.
This is a good place for sneaky business and getting access to data that is unavailable after map loading has completed.
This is right before the map is populated with data from a save file.*/
            base.MapComponentsInitializing(map);
        }

        public override void MapGenerated(Map map)
        {
            /*
Called right after a new map has finished generating. This is the equivalent of creating a MapComponent and overriding its MapGenerated method, but without the need to pollute save files with unnecessary map components.*/
            base.MapGenerated(map);
        }

        public override void MapLoaded(Map map)
        {
            /*
Called after map loading and generation is complete and after Verse.MapDrawer.RegenerateEverythingNow was executed. Receives a Verse.Map type argument.
This is a good place to run initialization code specific to a game map.
Note, that this method may be called zero or multiple times after loading a save, depending on how many maps the player has active at the moment.*/
            base.MapLoaded(map);
        }

        public override void MapDiscarded(Map map)
        {
            /*
Called after a map has been abandoned or otherwise made inaccessible. Works on player bases, encounter maps, destroyed faction bases, etc. This is a good place to clean up any map-related data in your World and UtilityWorldObjects to avoid bloating the save file.*/
            base.MapDiscarded(map);
        }

        public override void Tick(int currentTick)
        {
            /*
Called during each tick, when a game is loaded. Receives an int argument, which is the number of the current tick.
Will be called even if the player is on the world map and no map is currently loaded.
Will not be called on the "select landing spot" world map.
*/
            base.Tick(currentTick);
        }

        public override void ApplicationQuit()
        {
            /*
Called before the game process shuts down. This is a good place to update any non-critical mod setting values or write any custom data files.
"Quit to OS", clicking the "X" button on the window, and pressing Alt+F4 all execute this event. There are still ways to forcibly terminate the game process, as well as the possibility of a crash, so this callback is not 100% reliable.
Modified mod settings are automatically saved after this call.
*/
            base.ApplicationQuit();
        }
        private string XenotypeSetToString(XenotypeSet xs)
        {
            if (xs == null) return "N/A";
            string s = "";
            for (int n = 0; n < xs.Count; n++)
            {
                var x = xs[n];
                if (s.Length > 0) { s += ", "; }
                s += ((int)(x.chance * 100)) + "% " + x.xenotype.defName;
            }
            return s;
        }



        private void fixVFENewFactionPopups(World world)
        {
            IEnumerable<FactionDef> patchFacs = this.patchedXenotypeFactions.Values;
            IEnumerable<FactionDef> randFacs = FactionDefFilter.filterFactionDefs(
                DefDatabase<FactionDef>.AllDefs, new CategoryTagFactionDefFilter(RANDOM_CATEGORY_NAME));
            // invoke Find.World.GetComponent<VFECore.NewFactionSpawningState>().Ignore(factionDef) 
            // for all off-books factions or the player will be buried in pop-up spam
            Type NewFactionSpawningStateClassType = null;
            MethodInfo IgnoreMethodHandle = null;
            bool done = false;
            foreach (Assembly asmb in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (done) break;
                foreach (Type classType in asmb.GetTypes())
                {
                    if (done) break;
                    if ("NewFactionSpawningState".Equals(classType.Name))
                    {
                        //Logger.Message(string.Format("Found {0}.{1}", asmb.FullName, classType.Name));
                        var methodHandles = classType.GetMethods(); // calling classType.GetMethod("Ignore") throws ambiguous match exception
                        // looking for VFECore.NewFactionSpawningState.Ignore(IEnumerable<FactionDef> factions)
                        foreach (var methodHandle in methodHandles) { 
                            ParameterInfo[] methParams = methodHandle.GetParameters();
                            //string t = "";
                            //foreach (var param in methParams) { t += " " + param.ParameterType.Name; }
                            //Logger.Message(string.Format("Found {0}.{1}.{2}({3})", asmb.FullName, classType.Name, methodHandle.Name, t));
                            if ("Ignore".Equals(methodHandle.Name)
                                && methParams.Length == 1
                                && methParams[0].ParameterType.IsAssignableFrom(patchFacs.GetType()))
                            {
                                NewFactionSpawningStateClassType = classType;
                                IgnoreMethodHandle = methodHandle;
                                done = true;
                                break;
                            }
                        }
                    }
                }
            }
            if(NewFactionSpawningStateClassType != null && IgnoreMethodHandle != null)
            {
                // VFE installed and WorldComponent VFECore.NewFactionSpawningState found
                // tell VFE to ignore patch factions
                object vfecomp = world.GetComponent(NewFactionSpawningStateClassType);
                if (vfecomp != null)
                {
                    IgnoreMethodHandle.Invoke(vfecomp, new object[] { randFacs });
                    IgnoreMethodHandle.Invoke(vfecomp, new object[] { patchFacs });
                    Logger.Message("Invoked World.GetComponent<VFECore.NewFactionSpawningState>().Ignore(...) to tell VFE to ignore random and xenotype-patched faction defs");
                }
            }
        }

    }

}