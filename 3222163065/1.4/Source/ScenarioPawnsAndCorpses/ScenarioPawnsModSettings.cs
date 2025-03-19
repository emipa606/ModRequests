//using System.Collections.Generic;
//using Verse;
//using UnityEngine;
//using RimWorld;
//using System.Linq;
//using System;

//namespace ScenarioPawnsAndCorpses
//{

//    public class ScenarioPawnsModSettings : ModSettings
//    {
//        public Dictionary<string, bool> childrenBackstoryShuffleable = null;
//        public Dictionary<string, bool> adultBackstoryShuffleable = null;

//        public bool resetValues = false;

//        public override void ExposeData()
//        {
//            base.ExposeData();

//            int childhoodBackstoryCount = -1;
//            int adultBackstoryCount = -1;

//            if (childrenBackstoryShuffleable == null)
//            {
//                Scribe_Values.Look(ref childhoodBackstoryCount, "ScenarioPawnsAndCorpses_ChildhoodCount", 0, true);

//                if (childhoodBackstoryCount > 0)
//                {
//                    Scribe_Collections.Look(ref childrenBackstoryShuffleable, "ScenarioPawnsAndCorpses_ChildrenBackstoryShuffleable", LookMode.Value, LookMode.Value);
//                }
//                else
//                {
//                    childrenBackstoryShuffleable = new Dictionary<string, bool>();
//                }
//            }
//            else
//            {
//                childhoodBackstoryCount = childrenBackstoryShuffleable.Count;

//                Scribe_Values.Look(ref childhoodBackstoryCount, "ScenarioPawnsAndCorpses_ChildhoodCount", 0, true);
//                Scribe_Collections.Look(ref childrenBackstoryShuffleable, "ScenarioPawnsAndCorpses_ChildrenBackstoryShuffleable", LookMode.Value, LookMode.Value);
//            }

//            if (adultBackstoryShuffleable == null)
//            {
//                Scribe_Values.Look(ref adultBackstoryCount, "ScenarioPawnsAndCorpses_AdultCount", 0, true);

//                if (adultBackstoryCount > 0)
//                {
//                    Scribe_Collections.Look(ref adultBackstoryShuffleable, "ScenarioPawnsAndCorpses_AdultBackstoryShuffleable", LookMode.Value, LookMode.Value);
//                }
//                else
//                {
//                    adultBackstoryShuffleable = new Dictionary<string, bool>();
//                }
//            }
//            else
//            {
//                adultBackstoryCount = adultBackstoryShuffleable.Count;

//                Scribe_Values.Look(ref adultBackstoryCount, "ScenarioPawnsAndCorpses_AdultCount", 0, true);
//                Scribe_Collections.Look(ref adultBackstoryShuffleable, "ScenarioPawnsAndCorpses_AdultBackstoryShuffleable", LookMode.Value, LookMode.Value);
//            }
//        }
//    }

//    public class ScenarioPawnsMod : Mod
//    {
//        /// <summary>
//        /// A reference to our settings.
//        /// </summary>
//        ScenarioPawnsModSettings settings;

//        /// <summary>
//        /// A mandatory constructor which resolves the reference to our settings.
//        /// </summary>
//        /// <param name="content"></param>
//        public ScenarioPawnsMod(ModContentPack content) : base(content)
//        {
//            this.settings = this.GetSettings<ScenarioPawnsModSettings>();
//        }

//        /// <summary>
//        /// The (optional) GUI part to set your settings.
//        /// </summary>
//        /// <param name="inRect">A Unity Rect with the size of the settings window.</param>
//        public override void DoSettingsWindowContents(Rect inRect)
//        {
//            //float totalContentHeight = 1000f;
//            //float ScrollBarWidthMargin = 18f;
//            //Vector2 scrollPosition = new Vector2(0f, 0f);

//            //Rect outerRect = inRect.ContractedBy(10f);
//            //Widgets.DrawHighlight(outerRect);
//            //bool scrollBarVisible = totalContentHeight > outerRect.height;
//            //var scrollViewTotal = new Rect(0f, 0f, outerRect.width - (scrollBarVisible ? ScrollBarWidthMargin : 0), totalContentHeight);
//            //Widgets.BeginScrollView(outerRect, ref scrollPosition, scrollViewTotal);

//            Listing_Standard listingStandard = new Listing_Standard();

//            listingStandard.Begin(inRect);
//            listingStandard.CheckboxLabeled("ScenarioPawnsAndCorpses.Mod_ResetMod".Translate(), ref settings.resetValues);

//            listingStandard.Label("ScenarioPawnsAndCorpses.Mod_BackstoryLabel".Translate(BackstorySlot.Childhood.ToString()));

//            foreach (var b in BackstoryDatabase.allBackstories.Select(b => b.Value).Where(b => b.slot == BackstorySlot.Childhood))
//            {
//                if (!this.settings.childrenBackstoryShuffleable.ContainsKey(b.identifier))
//                {
//                    this.settings.childrenBackstoryShuffleable.Add(b.identifier, b.shuffleable);
//                }

//                listingStandard.CheckboxLabeled(b.title, ref this.settings.childrenBackstoryShuffleable[b.identifier]);
//            }

//            listingStandard.Label("ScenarioPawnsAndCorpses.Mod_BackstoryLabel".Translate(BackstorySlot.Adulthood.ToString()));

//            foreach (var b in BackstoryDatabase.allBackstories.Select(b => b.Value).Where(b => b.slot == BackstorySlot.Adulthood))
//            {
//                if (!this.settings.adultBackstoryShuffleable.ContainsKey(b.identifier))
//                {
//                    this.settings.adultBackstoryShuffleable.Add(b.identifier, b.shuffleable);
//                }

//                listingStandard.CheckboxLabeled(b.title, ref this.settings.adultBackstoryShuffleable[b.identifier]);
//            }

//            listingStandard.End();
//            //Widgets.EndScrollView();

//            base.DoSettingsWindowContents(inRect);
//        }

//        public override void WriteSettings()
//        {
//            base.WriteSettings();

//            if (settings.resetValues)
//            {
//                settings.childrenBackstoryShuffleable.Clear();
//                settings.adultBackstoryShuffleable.Clear();
//            }

//            OnDefsLoaded.ApplySettingsToDefs();
//        }

//        /// <summary>
//        /// Override SettingsCategory to show up in the list of settings.
//        /// </summary>
//        /// <returns>The (translated) mod name.</returns>
//        public override string SettingsCategory()
//        {
//            return "ScenarioPawnsAndCorpses.ModName".Translate();
//        }
//    }

//    [StaticConstructorOnStartup]
//    public class OnDefsLoaded
//    {
//        static OnDefsLoaded()
//        {
//            OnDefsLoaded.ApplySettingsToDefs();
//        }

//        public static void ApplySettingsToDefs()
//        {
//            ScenarioPawnsModSettings settings = LoadedModManager.GetMod<ScenarioPawnsMod>().GetSettings<ScenarioPawnsModSettings>();

//            if (settings != null)
//            {
//                if (settings.childrenBackstoryShuffleable != null)
//                {
//                    foreach (var v in LoadedModManager.GetMod<ScenarioPawnsMod>().GetSettings<ScenarioPawnsModSettings>().childrenBackstoryShuffleable)
//                    {
//                        if (BackstoryDatabase.allBackstories.ContainsKey(v.Key))
//                        {
//                            BackstoryDatabase.allBackstories[v.Key].shuffleable = v.Value;
//                        }
//                    }
//                }

//                if (settings.adultBackstoryShuffleable != null)
//                {
//                    foreach (var v in LoadedModManager.GetMod<ScenarioPawnsMod>().GetSettings<ScenarioPawnsModSettings>().adultBackstoryShuffleable)
//                    {
//                        if (BackstoryDatabase.allBackstories.ContainsKey(v.Key))
//                        {
//                            BackstoryDatabase.allBackstories[v.Key].shuffleable = v.Value;
//                        }
//                    }
//                }
//            }
//        }
//    }
//}
