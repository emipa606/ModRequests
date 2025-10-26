using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.AI;
using RimWorld;
using RimWorld.Planet;


namespace Clutter_StructureWall
{

    public class C_StructureWallModUI : ModSettings
    {
        public static bool ModdedWallsActive = true;
        public static bool WallTextureActive = true;
        //public static bool RebootSettingChange = false;
        private const float LineHeight = 28f;
        private static readonly Color ListDarkColor = new Color(0f, 0f, 0f, 0.3f);
        private static readonly Color lightColor = new Color(0.4f, 0.4f, 0.4f, 0.15f);
        
        public void DoWindowContents(Rect canvas)
        {
            Rect MainRect = new Rect(canvas.x + 10f, canvas.y + 10f, canvas.width - 10f, 240f);
            Rect MainRect2 = new Rect(MainRect.x,MainRect.height+MainRect.y+10f, canvas.width - 10f, 240f);
            ValWallMenu(MainRect);
            ModdedWallMenu(MainRect2);
        }

        public void ModdedWallMenu(Rect MainRect)
        {
            Widgets.DrawBox(MainRect);
            Rect viewRect = MainRect.ContractedBy(8f);
            Rect PrevMain = new Rect(viewRect.width - 190f, viewRect.y + 4f, 204f, 204f);
            float posY = viewRect.y;
            DrawArea(PrevMain, ListDarkColor);
            Rect PrevTex = PrevMain.ContractedBy(4f);
            //  Text.Font = GameFont.Medium;
            Rect LabelRect = new Rect(viewRect.x + 4f, viewRect.y - 6f, viewRect.width, viewRect.height);
            Widgets.Label(LabelRect, "--------------------------------------------------\nModded Wall`s\n--------------------------------------------------");
            // Text.Font = GameFont.Small;
            posY += LineHeight;
            SelectableBox2(new Rect(viewRect.width / 2 + 70f, posY - 20f, 150f, 38f));
            posY += LineHeight;
            Rect TextRect = new Rect(viewRect.x + 4f, posY, viewRect.width - 220f, viewRect.height - 72f);
            DrawArea(TextRect, ListDarkColor);
            Rect TextBox = TextRect.ContractedBy(4f);
            Widgets.Label(TextBox, "[Description]\n\nAdds two additional walls, mostly for looks.\n\n[Require Game Client Restart]");
            if (ModdedWallsActive)
            {
                Widgets.DrawTextureFitted(PrevTex, UITex.ModWallPrev, 1);
            }
            else
            {
                Widgets.DrawTextureFitted(PrevTex, UITex.ModWallPrev2, 1);
            }
        }

        public void SelectableBox2(Rect areaRect)
        {
            DrawArea(areaRect, ListDarkColor);
            Rect areaCon = areaRect.ContractedBy(6f);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(areaCon, ModWallString());
            Text.Anchor = TextAnchor.UpperLeft;
            DrawArea(new Rect((areaRect.x - 30f) + areaRect.width, areaRect.y + 6f, 26f, 26f), ListDarkColor);
            Widgets.Checkbox(new Vector2((areaRect.x - 29f) + areaRect.width, areaRect.y + 6f), ref ModdedWallsActive);
            if (Widgets.ButtonInvisible(areaRect))
            {
                ModdedWallsActive = !ModdedWallsActive;

            }
            if (Mouse.IsOver(areaRect))
            {
                Widgets.DrawHighlight(areaRect);
            }

        }

        public void ValWallMenu(Rect MainRect)
        {
           

            Widgets.DrawBox(MainRect);
            Rect viewRect = MainRect.ContractedBy(8f);
            Rect PrevMain = new Rect(viewRect.width - 190f, viewRect.y + 4f, 204f, 204f);
            float posY = viewRect.y;
            DrawArea(PrevMain, ListDarkColor);
            Rect PrevTex = PrevMain.ContractedBy(4f);
            //  Text.Font = GameFont.Medium;
            Rect LabelRect = new Rect(viewRect.x + 4f, viewRect.y - 6f, viewRect.width, viewRect.height);
            Widgets.Label(LabelRect, "--------------------------------------------------\nVanilia Wall`s Textures Remake\n--------------------------------------------------");
            // Text.Font = GameFont.Small;
            posY += LineHeight;
            // Widgets.DrawLineHorizontal(LabelRect.x-8f, posY, viewRect.width - 210f);
            SelectableBox(new Rect(viewRect.width / 2 + 70f, posY - 20f, 150f, 38f));
            posY += LineHeight;
            Rect TextRect = new Rect(viewRect.x + 4f, posY, viewRect.width - 220f, viewRect.height - 72f);
            DrawArea(TextRect, ListDarkColor);
            Rect TextBox = TextRect.ContractedBy(4f);
            Widgets.Label(TextBox, "[Description]\n\nReplaces vanilia wall textures with slightly redone ones.\n\n[Save compatibile]\n\n[Require Game Client Restart]");
            if (WallTextureActive)
            {
                Widgets.DrawTextureFitted(PrevTex, UITex.WallPrev, 1);
            }
            else
            {
                Widgets.DrawTextureFitted(PrevTex, UITex.WallPrev2, 1);
            }
        }

        public void SelectableBox(Rect areaRect)
        {
            DrawArea(areaRect, ListDarkColor);
            Rect areaCon = areaRect.ContractedBy(6f);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(areaCon, ValWallString());
            Text.Anchor = TextAnchor.UpperLeft;
            DrawArea(new Rect((areaRect.x - 30f)+areaRect.width, areaRect.y + 6f, 26f, 26f), ListDarkColor);
            Widgets.Checkbox(new Vector2((areaRect.x-29f)+areaRect.width, areaRect.y+6f), ref WallTextureActive);
            if(Widgets.ButtonInvisible(areaRect))
            {
                WallTextureActive = !WallTextureActive;

            }
            if (Mouse.IsOver(areaRect))
            {
                Widgets.DrawHighlight(areaRect);
            }
       
        }

        

        

       
        public string ValWallString()
        {
            if(WallTextureActive)
            {
            return "Enabled";
            }
            return "Disabled";
        }

        public string ModWallString()
        {
            if (ModdedWallsActive)
            {
                return "Enabled";
            }
            return "Disabled";
        }

        public void DrawArea(Rect areaRect, Color colorArea)
        {
            
            Widgets.DrawBoxSolid(areaRect, colorArea);
            Widgets.DrawBox(areaRect);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref WallTextureActive, "WallTextureActive");
            Scribe_Values.Look(ref ModdedWallsActive, "ModdedWallsActive");
        }


        public bool ConbatExtendedPresent()
        {
            return ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name == "Combat Extended");
        }

       



    }
}

