using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace StylishRim
{
    // 最初のころは自分でもプレビュー欲しかったけどウィンドウとかポーンを描画する方法とかよくわからん諦めよう
    // いやあ、できるもんですね
    public class StylishPreviewWindow : Window
    {
        private static readonly string NEXT_ONE = ">";
        private static readonly string NEXT_TEN = ">>";
        private static readonly string PREV_ONE = "<";
        private static readonly string PREV_TEN = "<<";
        private static readonly string PLUS = "+";
        private static readonly string MINUS = "-";
        private static readonly string PORTRAIT = "Portrait?";
        private static Rot4 rot = Rot4.South;
        private static float zoom = 1;
        private static bool isPortrait = true;
        private static bool disableHeadgear = false;
        private static bool disableClothes = false;
        private List<Pawn> pawnList = new List<Pawn>();
        private int pawnIndex = 0;
        public Vector2 size;
        public RenderTexture pawnTexture;
        public bool update = true;
        private Pawn DrawingPawn
        {
            get
            {
                if (pawnList.NullOrEmpty()) return null;
                if (pawnIndex >= pawnList.Count)
                {
                    pawnIndex = 0;
                }
                else if (pawnIndex < 0)
                {
                    pawnIndex = pawnList.Count - 1;
                }
                return pawnList[pawnIndex];
            }
        }
        public void ListClear()
        {
            pawnList.Clear();
        }
        public List<Pawn> PawnList
        {
            get { return pawnList; }
            set { pawnList = value; }
        }
        private string RotString
        {
            get
            {
                switch (rot.AsInt)
                {
                    case 0: return StylishRimSettings.KEY_BACK;
                    case 1: return StylishRimSettings.KEY_RIGHT;
                    case 2: return StylishRimSettings.KEY_FRONT;
                    default: return StylishRimSettings.KEY_LEFT;
                }
            }
        }
        public void Open()
        {
            Find.WindowStack.Add(this);
        }
        public void Draw()
        {
            if (!IsOpen || pawnTexture == null) return;
            DoWindowContents(windowRect);
        }
        public void SetDrawPawn() => SetDrawPawn(DrawingPawn);
        public void SetDrawPawn(Pawn pawn = null)
        {
            if (pawn == null) return;
            Draw();
        }
        public void Update()
        {
            update = true;
        }
        public override void DoWindowContents(Rect inRect)
        {
            Rect pawnRect = new Rect(inRect.center.x - 256, inRect.center.y - 281, 512, 512).ContractedBy(10);
            Rect footerRect = new Rect(inRect.x, inRect.y + 470, inRect.width, 80).ContractedBy(10, 4);
            footerRect.SplitHorizontally(32, out footerRect, out Rect tempRect/*, 4*/);

            bool temp = disableHeadgear;
            Widgets.CheckboxLabeled(footerRect.LeftHalf().ContractedBy(10, 0), StylishRimSettings.LABEL_PORTRAIT_DISABLE_HEADGEAR.Translate(), ref disableHeadgear);
            if (temp != disableHeadgear) Update();

            temp = disableClothes;
            Widgets.CheckboxLabeled(footerRect.RightHalf().ContractedBy(10, 0), StylishRimSettings.LABEL_PORTRAIT_DISABLE_CLOTHES.Translate(), ref disableClothes);
            if (temp != disableClothes) Update();

            tempRect.SplitVertically(47, out Rect target, out tempRect/*, 2*/);

            if (PawnList.Count > 1)
            {
                if (Widgets.ButtonText(target.LeftHalf(), PREV_TEN))
                {
                    pawnIndex -= 10;
                    Update();
                }
                if (Widgets.ButtonText(target.RightHalf(), PREV_ONE))
                {
                    pawnIndex -= 1;
                    Update();
                }
            }

            tempRect.SplitVertically(48, out target, out tempRect/*, 2*/);
            if (PawnList.Count > 1)
            {
                if (Widgets.ButtonText(target.LeftHalf(), NEXT_ONE))
                {
                    pawnIndex += 1;
                    Update();
                }
                if (Widgets.ButtonText(target.RightHalf(), NEXT_TEN))
                {
                    pawnIndex += 10;
                    Update();
                }
            }

            tempRect.SplitVertically(24, out target, out tempRect);
            if (Widgets.ButtonText(target, PLUS))
            {
                zoom = Math.Min(zoom + 0.2f, 3);
                Update();
            }

            tempRect.SplitVertically(48, out target, out tempRect/*, 2*/);
            if (Widgets.ButtonText(target.LeftHalf(), string.Empty))
            {
                zoom = 1;
                Update();
            }
            if (Widgets.ButtonText(target.RightHalf(), MINUS))
            {
                zoom = Math.Max(zoom - 0.2f, 0.2f);
                Update();
            }

            if (Widgets.ButtonText(tempRect, RotString))
            {
                rot.AsInt += 1;
                Update();
            }
            temp = isPortrait;
            Widgets.CheckboxLabeled(new Rect(inRect.x + inRect.width - 106, inRect.y, 96, 32), PORTRAIT, ref isPortrait);
            if (temp != isPortrait) Update();
            if (DrawingPawn != null)
            {
                try
                {
                    if (update)
                    {
                        update = false;
                        PawnGraphicSet graphics = DrawingPawn.Drawer.renderer.graphics;
                        if (graphics != null)
                        {
                            graphics.ResolveAllGraphics();
                        }
                        PawnCacheCameraManager.PawnCacheRenderer.RenderPawn(DrawingPawn, pawnTexture, Vector3.zero, zoom, 0, rot, true, true, !disableHeadgear, !disableClothes, isPortrait, default, null, null, false);
                    }
                    Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 32), (PawnList.Count == 1 ? "" : $"{DrawingPawn.def.defName}[{pawnIndex}]: ") + DrawingPawn.Name.ToStringFull);
                    Widgets.Label(new Rect(inRect.x, inRect.y + 32, inRect.width, 32), $"{DrawingPawn.story.hairDef.modContentPack.Name}: {DrawingPawn.story.hairDef.defName}");
                    GUI.DrawTexture(pawnRect, pawnTexture);
                }
                catch
                {
                    Log.Warning("[Stylish Rim] Failed pawn preview.");
                }
            }
        }

        public override void Close(bool doCloseSound = true)
        {
            base.Close(doCloseSound);
            if (pawnTexture != null)
            {
                pawnTexture.Release();
                pawnTexture = null;
            }
        }
        public override void PreOpen()
        {
            base.PreOpen();
            windowRect.position = new Vector2(windowRect.position.x / 10, windowRect.position.y);
        }
        public StylishPreviewWindow(Vector2 size)
        {
            this.size = size;
            layer = WindowLayer.Super;
            closeOnAccept = false;
            closeOnCancel = false;
            draggable = true;
            closeOnClickedOutside = false;
            pawnTexture = new RenderTexture(512, 512, 32, RenderTextureFormat.ARGB32);
            windowRect.x = 18;
        }
        public override Vector2 InitialSize => size;
    }
}
