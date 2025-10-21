using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using UnityEngine;

namespace zed_0xff.CPS;

//public enum BuildingType { Bed, TSS, Cabin, ThePit };

public class CPSSettings : ModSettings
{
    public class TSSSettings : IExposable {
        public bool sounds = true;
        public bool effects = true;
        public bool menus = true;

        public bool unassignColonistBeds = false;
        public bool unassignSlaveBeds = true;
        public bool unassignPrisonerBeds = true;

        public Building_TSS.AI.Config default_ai_config;

        public void ExposeData() {
            Scribe_Values.Look(ref sounds, "sounds", true);
            Scribe_Values.Look(ref effects, "effects", true);
            Scribe_Values.Look(ref menus, "menus", true);

            Scribe_Values.Look(ref unassignColonistBeds, "unassignColonistBeds", false);
            Scribe_Values.Look(ref unassignSlaveBeds,    "unassignSlaveBeds",    true);
            Scribe_Values.Look(ref unassignPrisonerBeds, "unassignPrisonerBeds", true);

            Scribe_Deep.Look(ref default_ai_config, "default_ai_config");
        }
    };

    public TSSSettings tss = new TSSSettings();

//    public List<BuildingType> prisonerBedOrder = new List<BuildingType>{ BuildingType.Bed, BuildingType.TSS, BuildingType.Cabin, BuildingType.ThePit };
//    public bool customPrisonerBedOrder = true;

    public override void ExposeData()
    {
        Scribe_Deep.Look(ref tss, "TSS");
//        Scribe_Collections.Look(ref prisonerBedOrder, "prisonerBedOrder", LookMode.Value);
//        Scribe_Values.Look(ref customPrisonerBedOrder, "customPrisonerBedOrder", true); // TODO: add checkbox
        base.ExposeData();
    }
}

public class CPSMod : Mod
{
    public override string SettingsCategory() => "CPS";

    public static CPSSettings Settings { get; private set; }

    public static List<Assembly> plugins = new List<Assembly>();

    public CPSMod(ModContentPack content) : base(content) {
        Settings = GetSettings<CPSSettings>();

        plugins.Clear();
        if (ModLister.HasActiveModWithName("Vanilla Nutrient Paste Expanded")) {
            LoadPlugin(content, "VNPE");
        }
        if (ModLister.HasActiveModWithName("Vanilla Races Expanded - Sanguophage")) {
            LoadPlugin(content, "VRES");
        }
        if (ModLister.HasActiveModWithName("Dubs Bad Hygiene")) {
            LoadPlugin(content, "DBH");
        }
//        if( plugins.Any() ){
//            GenTypes.ClearCache();
//        }
    }

    private void LoadPlugin(ModContentPack content, string name){
        try {
            string fname = Path.Combine(content.RootDir, "Plugins", "CPS_" + name + ".dll");
            byte[] rawAssembly = File.ReadAllBytes(fname);
            Assembly assembly = AppDomain.CurrentDomain.Load(rawAssembly);
            Log.Message("[d] CPS loaded plugin " + assembly);
            content.assemblies.loadedAssemblies.Add(assembly);
            plugins.Add(assembly);
        } catch(Exception ex) {
            Log.Error("[!] CPS: plugin " + name + " failed to load: " + ex);
        }
    }

    private static Vector2 scrollPosition = Vector2.zero;
    private int PageIndex = 1;

    public override void DoSettingsWindowContents(Rect inRect)
    {
        var tabRect = new Rect(inRect) {
            y = inRect.y + 40f
        };
        var mainRect = new Rect(inRect) {
            height = inRect.height - 40f,
            y = inRect.y + 40f
        };

        Widgets.DrawMenuSection(mainRect);

        var tabs = new List<TabRecord> {
//            new TabRecord("General".Translate(), () => { PageIndex = 0; WriteSettings(); }, PageIndex == 0),
            new TabRecord("TSS".Translate(),     () => { PageIndex = 1; WriteSettings(); }, PageIndex == 1),
        };

        TabDrawer.DrawTabs(tabRect, tabs);

        switch (PageIndex)
        {
            case 0:
                draw_general(mainRect.ContractedBy(15f));
                break;
            case 1:
                draw_tss(mainRect.ContractedBy(15f));
                break;
            default:
                break;
        }
    }

//    void Reorder(BuildingType t, int delta){
//        int index = Settings.prisonerBedOrder.IndexOf(t);
//        if( delta == -1 && index > 0 ){
//            BuildingType tmp = Settings.prisonerBedOrder[index-1];
//            Settings.prisonerBedOrder[index-1] = Settings.prisonerBedOrder[index];
//            Settings.prisonerBedOrder[index] = tmp;
//        }
//        if( delta == 1 && index < Settings.prisonerBedOrder.Count - 1 ){
//            BuildingType tmp = Settings.prisonerBedOrder[index+1];
//            Settings.prisonerBedOrder[index+1] = Settings.prisonerBedOrder[index];
//            Settings.prisonerBedOrder[index] = tmp;
//        }
//    }

//    void DrawThingRow(ref float y, float width, BuildingType t){
//        Rect rect = new Rect(0f, y, width, 28f);
//        string text;
//        switch( t ){
//            case BuildingType.Bed:
//                text = "Regular bed".Translate();
//                break;
//            case BuildingType.ThePit:
//                text = "The Pit".Translate();
//                break;
//            default:
//                text = t.ToString().Translate();
//                break;
//        }
//        Widgets.Label(rect, text);
//
//        Rect buttonRect = new Rect(rect.width - 48f, y, 24f, 24f);
//        if( Widgets.ButtonImage(buttonRect, TexButton.ReorderUp) ){
//            Reorder(t, -1);
//        }
//
//        buttonRect = new Rect(rect.width - 24f, y, 24f, 24f);
//        if( Widgets.ButtonImage(buttonRect, TexButton.ReorderDown) ){
//            Reorder(t, 1);
//        }
//
//        y += 28f;
//    }

    void draw_general(Rect inRect){
        Listing_Standard l = new Listing_Standard();
        l.Begin(inRect);

//        float curY = 0f;
//        var tmpList = new List<BuildingType>();
//        tmpList.AddRange(Settings.prisonerBedOrder); // Collection was modified blah blah
//        Widgets.ListSeparator(ref curY, inRect.width, "Prisoner bed search order");
//        foreach( BuildingType t in tmpList ){
//            DrawThingRow(ref curY, 200f, t);
//        }

        l.End();
    }

    void draw_tss(Rect inRect){
        Listing_Standard l = new Listing_Standard();
        l.Begin(inRect);

        l.CheckboxLabeled("Sounds", ref Settings.tss.sounds);
        l.CheckboxLabeled("Effects", ref Settings.tss.effects);
        l.CheckboxLabeled("Context menus", ref Settings.tss.menus);
        l.Gap();

        l.Label("Unassign beds on entering TSS");
            l.CheckboxLabeled("Prisoners", ref Settings.tss.unassignPrisonerBeds, 20);
            l.CheckboxLabeled("Slaves", ref Settings.tss.unassignSlaveBeds, 20);
            l.CheckboxLabeled("Colonists", ref Settings.tss.unassignColonistBeds, 20);

        l.End();
    }
}
