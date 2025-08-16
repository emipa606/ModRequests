using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace PsychicAnimals
{
    public class AbilitiesWindow : Window, IExposable
    {
        private static readonly List<AbilityDef> abilitieslist = AllAbilitiesList();
        private static readonly List<AbilityDef> abilitylevel1 = AllAbiltiesOfLevelList(1);
        private static readonly List<AbilityDef> abilitylevel2 = AllAbiltiesOfLevelList(2);
        private static readonly List<AbilityDef> abilitylevel3 = AllAbiltiesOfLevelList(3);
        private static readonly List<AbilityDef> abilitylevel4 = AllAbiltiesOfLevelList(4);
        private static readonly List<AbilityDef> abilitylevel5 = AllAbiltiesOfLevelList(5);
        private static readonly List<AbilityDef> abilitylevel6 = AllAbiltiesOfLevelList(6);
        private static readonly List<List<AbilityDef>> ListOfList = new List<List<AbilityDef>>() { abilitylevel1, abilitylevel2, abilitylevel3, abilitylevel4, abilitylevel5, abilitylevel6 };
        private static readonly int listcount = CountOfList();

        private Dictionary<AbilityDef, bool> allowedAbilities;

        readonly float Width = 1280;
        readonly float Height = 720;
        public override Vector2 InitialSize => new Vector2(Width, Height);
        private Vector2 scrollPosition = Vector2.zero;
        public AbilitiesWindow()
        {
            resizeable = false;
            draggable = false;
            doCloseX = true;
            closeOnClickedOutside = true;
            if (allowedAbilities == null)
            {
                allowedAbilities = new Dictionary<AbilityDef, bool>();
                foreach (AbilityDef abi in abilitieslist)
                {
                    if (abi.category.defName == "WordOf")
                    {
                        allowedAbilities.SetOrAdd(abi, false);
                    }
                }
            }
        }
        public Dictionary<AbilityDef, bool> AllowedAbilities()
        {
            if (allowedAbilities == null)
            {
                allowedAbilities = new Dictionary<AbilityDef, bool>();
                foreach (AbilityDef abi in abilitieslist)
                {
                    if (abi.category.defName == "WordOf")
                    {
                        allowedAbilities.SetOrAdd(abi, false);
                    }
                }
                return allowedAbilities;
            }
            return allowedAbilities;
        }
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(new Rect(0, 0, inRect.width, 35), "In order to apply changes for neurotrainers, game restart is required.");


            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(new Rect(0, 0, inRect.width, 35), "At least 1 ability per level is required.");
            if (Widgets.ButtonText(new Rect(Width - 2 * Margin - 200, 0, 150, 40), "Restore Defaults"))
            {
                allowedAbilities = new Dictionary<AbilityDef, bool>();
                foreach (AbilityDef abi in abilitieslist)
                {
                    if (abi.category.defName == "WordOf")
                    {
                        allowedAbilities.SetOrAdd(abi, false);
                    }
                }
                return;
            }
            for (int i = 0; i < 6; i++)
            {
                Widgets.Label(new Rect(i * Width / 6 - i * 5, 45, width: Width / 6, height: 20f), "Level " + (i + 1));
            }
            Rect scrollRect = new Rect(0, 65, Width - 2 * Margin, Height - 6 * Margin);
            Rect scrollVertRect = new Rect(0, 0, scrollRect.x, listcount * 65 + 20);
            int scrollYOffset;
            Widgets.BeginScrollView(scrollRect, ref scrollPosition, scrollVertRect);
            for (int i = 0; i < 6; i++)
            {
                scrollYOffset = 10;
                foreach (var abi in ListOfList[i])
                {
                    Widgets.DrawLineHorizontal(i * Width / 6 - i * 5, scrollYOffset, (Width - 10 * Margin) / 6);
                    Widgets.DrawLineHorizontal(i * Width / 6 - i * 5, scrollYOffset + 60, (Width - 10 * Margin) / 6);
                    Widgets.DrawLineVertical(i * Width / 6 - i * 5, scrollYOffset, 60);
                    Widgets.DrawLineVertical(i * Width / 6 - i * 5 + (Width - 10 * Margin) / 6, scrollYOffset, 60);
                    Rect rect = new Rect(i * Width / 6 + 8 - i * 5, scrollYOffset + 2, 35, 35);
                    TooltipHandler.TipRegion(rect, abi.description);
                    GUI.DrawTexture(rect, abi.uiIcon);
                    Widgets.Label(new Rect(i * Width / 6 + 8 - i * 5, scrollYOffset + 36, Width - 2 * Margin, 25), abi.label);
                    bool cachebool = allowedAbilities.TryGetValue(abi, out cachebool) ? cachebool : true;
                    Widgets.Checkbox(i * Width / 6 + Width / 9 - i * 5, scrollYOffset + 17, ref cachebool);
                    allowedAbilities.SetOrAdd<AbilityDef, bool>(abi, cachebool);
                    scrollYOffset += 65;
                }
            }
            Widgets.EndScrollView();
        }

        private static List<AbilityDef> AllAbilitiesList()
        {
            List<AbilityDef> list = DefDatabase<AbilityDef>.AllDefsListForReading;
            List<AbilityDef> newlist = new List<AbilityDef> { };
            foreach (AbilityDef abi in list)
            {
                if (abi.verbProperties.verbClass == typeof(Verb_CastPsycast))
                {
                    newlist.Add(abi);
                }
            }
            return newlist;
        }

        private static List<AbilityDef> AllAbiltiesOfLevelList(int level)
        {
            List<AbilityDef> list = abilitieslist;
            List<AbilityDef> newlist = new List<AbilityDef> { };
            foreach (AbilityDef abi in list)
            {
                if (abi.level == level)
                {
                    newlist.Add(abi);
                }
            }
            return newlist;
        }

        private static int CountOfList()
        {
            int max = 0;
            foreach (List<AbilityDef> list in ListOfList)
            {
                max = Math.Max(list.Count, max);
            }
            return max;

        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref allowedAbilities, "allowedAbilities", LookMode.Def, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (allowedAbilities == null)
                {
                    allowedAbilities = new Dictionary<AbilityDef, bool>();
                    foreach (AbilityDef abi in abilitieslist)
                    {
                        if (abi.category.defName == "WordOf")
                        {
                            allowedAbilities.SetOrAdd(abi, false);
                        }
                    }
                }
            }
        }
    }
}
