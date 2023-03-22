using RimWorld;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimTrust.Trade.Ext
{
    [StaticConstructorOnStartup]
    public static class ExtUtil
    {
        private static readonly Texture TradeArrow = (Texture)typeof(TransferableUIUtility).GetField("TradeArrow", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

        public static void PrepareVirtualTrade(Pawn pawn, VirtualTrader trader)
        {
            TradeSession.SetupWith(trader, pawn, false);
            trader.InvokeTradeUI();
        }

        public static float BrokerageFactor(int a)
        {
             /*if (a <= 0)
             {
                 return -0.035f;
             }
             return 0.015f;*/
            return 0.000f; 
        }

        public static void DoCountAdjustInterfaceForSilver(Rect rect, Transferable trad, int _, int min, int max, bool flash)
        {
            rect = rect.Rounded();
            Rect rect2 = new Rect(rect.center.x - 45f, rect.center.y - 12.5f, 90f, 25f).Rounded();
            if (flash)
            {
                GUI.DrawTexture(rect2, TransferableUIUtility.FlashTex);
            }

            bool num = trad is TransferableOneWay transferableOneWay && transferableOneWay.HasAnyThing && transferableOneWay.AnyThing is Pawn && transferableOneWay.MaxCount == 1;
            if (num)
            {
                bool flag = trad.CountToTransfer != 0;
                bool checkOn = flag;
                Widgets.Checkbox(rect2.position, ref checkOn);
                if (checkOn != flag)
                {
                    if (checkOn)
                    {
                        trad.AdjustTo(trad.GetMaximumToTransfer());
                    }
                    else
                    {
                        trad.AdjustTo(trad.GetMinimumToTransfer());
                    }
                }
            }
            else
            {
                Rect rect3 = rect2.ContractedBy(2f);
                rect3.xMax -= 15f;
                rect3.xMin += 16f;
                int val = trad.CountToTransfer;
                string buffer = trad.EditBuffer;
                Widgets.TextFieldNumeric(rect3, ref val, ref buffer, min, max);
                trad.AdjustTo(val);
                trad.EditBuffer = buffer;
            }

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            if (!num)
            {
                int num2 = (trad.PositiveCountDirection == TransferablePositiveCountDirection.Source) ? 1 : (-1);
                int num3 = GenUI.CurrentAdjustmentMultiplier();
                bool flag2 = trad.GetRange() == 1;
                if (trad.CanAdjustBy(num2 * num3).Accepted)
                {
                    Rect rect4 = new Rect(rect2.x - 30f, rect.y, 30f, rect.height);
                    if (flag2)
                    {
                        rect4.x -= rect4.width;
                        rect4.width += rect4.width;
                    }
                    if (Widgets.ButtonText(rect4, "<"))
                    {
                        trad.AdjustBy(num2 * num3);
                        SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    }
                    if (!flag2)
                    {
                        string label = "<<";
                        int? num4 = null;
                        rect4.x -= rect4.width;
                        if (Widgets.ButtonText(rect4, label))
                        {
                            if (num4.HasValue)
                            {
                                trad.AdjustTo(num4.Value);
                            }
                            else if (num2 == 1)
                            {
                                trad.AdjustTo(trad.GetMaximumToTransfer());
                            }
                            else
                            {
                                trad.AdjustTo(trad.GetMinimumToTransfer());
                            }
                            SoundDefOf.Tick_High.PlayOneShotOnCamera();
                        }
                    }
                }
                if (trad.CanAdjustBy(-num2 * num3).Accepted)
                {
                    Rect rect5 = new Rect(rect2.xMax, rect.y, 30f, rect.height);
                    if (flag2)
                    {
                        rect5.width += rect5.width;
                    }
                    if (Widgets.ButtonText(rect5, ">"))
                    {
                        trad.AdjustBy(-num2 * num3);
                        SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                    }
                    if (!flag2)
                    {
                        string label2 = ">>";
                        int? num5 = null;
                        rect5.x += rect5.width;
                        if (Widgets.ButtonText(rect5, label2))
                        {
                            if (num5.HasValue)
                            {
                                trad.AdjustTo(num5.Value);
                            }
                            else if (num2 == 1)
                            {
                                trad.AdjustTo(trad.GetMinimumToTransfer());
                            }
                            else
                            {
                                trad.AdjustTo(trad.GetMaximumToTransfer());
                            }
                            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                        }
                    }
                }
            }

            if (trad.CountToTransfer != 0)
            {
                Rect position = new Rect(rect2.x + rect2.width / 2f - (float)(TradeArrow.width / 2), rect2.y + rect2.height / 2f - (float)(TradeArrow.height / 2), TradeArrow.width, TradeArrow.height);
                TransferablePositiveCountDirection positiveCountDirection = trad.PositiveCountDirection;
                if ((positiveCountDirection == TransferablePositiveCountDirection.Source && trad.CountToTransfer > 0) || (positiveCountDirection == TransferablePositiveCountDirection.Destination && trad.CountToTransfer < 0))
                {
                    position.x += position.width;
                    position.width *= -1f;
                }
                GUI.DrawTexture(position, TradeArrow);
            }
        }
    }
}