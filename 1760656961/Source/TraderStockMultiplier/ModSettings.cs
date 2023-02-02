using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TraderStockMultiplier
{
    public class TraderStockMultiplier : Mod
    {
        private Vector2 scrollPosition = new Vector2(0f, 0f);
        private static float ViewRectHeight { get; set; }

        private bool showgraph1 = false;
        private bool showgraph2 = false;
        private bool showgraph3 = false;
        private static readonly SimpleCurveDrawerStyle style = new SimpleCurveDrawerStyle
        {
            DrawCurveMousePoint = true,
            DrawLegend = true,
            LabelX = "Wealth",
            DrawPoints = false
        };
        private bool showmark = false;


        public TraderStockMultiplierSettings settings;

        public TraderStockMultiplier(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<TraderStockMultiplierSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect viewRect = new Rect(0, 0, inRect.width - 30, ViewRectHeight);
            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect, true);

            Rect viewRect2 = new Rect(0, 0, viewRect.width, 99999);

            Listing_Standard listing_Standard = new Listing_Standard
            {
                ColumnWidth = (float)(viewRect.width - 10.0),
            };
            listing_Standard.Begin(viewRect2);


            listing_Standard.Label("TraderStockMultiplierDescription".Translate());
            listing_Standard.GapLine(24);

            if (listing_Standard.ButtonText("reset"))
            {
                settings.CountRangeMinFactorFromWealthCurve = new SimpleCurve
                {
                    {0f,        1f},
                    {20000f,    1f},
                    {50000f,    1.2f},
                    {100000f,   1.5f},
                    {300000f,   2f}
                };

                settings.CountRangeMaxFactorFromWealthCurve = new SimpleCurve
                {
                    {0f,        1f},
                    {20000f,    1f},
                    {50000f,    1.2f},
                    {100000f,   1.5f},
                    {300000f,   2f},
                    {3000000f,  5f}
                };

                settings.SilverStockFactorFromWealthCurve = new SimpleCurve
                {
                    {0f,        1f},
                    {20000f,    1.1f},
                    {50000f,    1.3f},
                    {100000f,   1.5f},
                    {300000f,   2f},
                    {3000000f,  3f},
                    {9999999f, 10f}
                };
            }
            listing_Standard.GapLine();


            listing_Standard.Label("CountRangeMinFactorFromWealthCurve".Translate());
            SimpleCurveEditor(listing_Standard, ref settings.CountRangeMinFactorFromWealthCurve);
            listing_Standard.GapLine();

            listing_Standard.Label("CountRangeMaxFactorFromWealthCurve".Translate());
            SimpleCurveEditor(listing_Standard, ref settings.CountRangeMaxFactorFromWealthCurve);
            listing_Standard.GapLine();

            listing_Standard.Label("SilverStockFactorFromWealthCurve".Translate());
            SimpleCurveEditor(listing_Standard, ref settings.SilverStockFactorFromWealthCurve);
            listing_Standard.GapLine(24);

            Rect buttonrect = listing_Standard.GetRect(Text.LineHeight);
            WidgetRow widgetRow = new WidgetRow(buttonrect.x, buttonrect.y);

            if (widgetRow.ButtonText("CountRangeMinFactorFromWealthCurve".Translate()))
            {
                showgraph1 = !showgraph1;
            }
            if (widgetRow.ButtonText("CountRangeMaxFactorFromWealthCurve".Translate()))
            {
                showgraph2 = !showgraph2;
            }
            if (widgetRow.ButtonText("SilverStockFactorFromWealthCurve".Translate()))
            {
                showgraph3 = !showgraph3;
            }
            if (Current.ProgramState == ProgramState.Playing)
            {
                if (widgetRow.ButtonText("CurrentWealth".Translate()))
                {
                    showmark = !showmark;
                }
            }
            else
            {
                showmark = false;
            }


            List<SimpleCurveDrawInfo> curves = new List<SimpleCurveDrawInfo>();
            if (showgraph1)
            {
                curves.Add(new SimpleCurveDrawInfo
                {
                    curve = settings.CountRangeMinFactorFromWealthCurve,
                    labelY = "Multiplier",
                    label = "CountRangeMinFactorFromWealthCurve".Translate(),
                    color = Color.white

                });
            }
            if (showgraph2)
            {
                curves.Add(new SimpleCurveDrawInfo
                {
                    curve = settings.CountRangeMaxFactorFromWealthCurve,
                    labelY = "Multiplier",
                    label = "CountRangeMaxFactorFromWealthCurve".Translate(),
                    color = Color.green
                });
            }
            if (showgraph3)
            {
                curves.Add(new SimpleCurveDrawInfo
                {
                    curve = settings.SilverStockFactorFromWealthCurve,
                    labelY = "Multiplier",
                    label = "SilverStockFactorFromWealthCurve".Translate(),
                    color = Color.yellow
                });
            }

            if (showgraph1 || showgraph2 || showgraph3)
            {
                Rect rect = listing_Standard.GetRect(200);
                Rect legend = listing_Standard.GetRect(30);
                List<CurveMark> marks = null;
                if (showmark)
                {
                    marks = new List<CurveMark>
                    {
                        new CurveMark(Find.World.PlayerWealthForStoryteller, "CurrentWealth".Translate(), Color.cyan)
                    };
                }
                SimpleCurveDrawer.DrawCurves(rect, curves, style, marks, legend);
            }

            listing_Standard.GapLine();

            listing_Standard.Label("MultiplierOption".Translate());
            if (listing_Standard.RadioButton("MultiplyKind".Translate(), settings.multiplierOption == MultiplierOption.MultiplyKind))
            {
                settings.multiplierOption = MultiplierOption.MultiplyKind;
            }
            if (listing_Standard.RadioButton("MultiplyQuantity".Translate(), settings.multiplierOption == MultiplierOption.MultiplyQuantity))
            {
                settings.multiplierOption = MultiplierOption.MultiplyQuantity;
            }
            if (listing_Standard.RadioButton("MultiplyTwice".Translate(), settings.multiplierOption == MultiplierOption.MultiplyTwice))
            {
                settings.multiplierOption = MultiplierOption.MultiplyTwice;
            }
            if (listing_Standard.RadioButton("MUltiplyTwiceSquareRoot".Translate(), settings.multiplierOption == MultiplierOption.MUltiplyTwiceSquareRoot))
            {
                settings.multiplierOption = MultiplierOption.MUltiplyTwiceSquareRoot;
            }






            listing_Standard.End();
            ViewRectHeight = listing_Standard.CurHeight;
            Widgets.EndScrollView();
        }

        public override string SettingsCategory()
        {
            return "Trader Stock Multiplier";
        }

        public override void WriteSettings()
        {
            settings.CountRangeMinFactorFromWealthCurve.SortPoints();
            settings.CountRangeMaxFactorFromWealthCurve.SortPoints();
            settings.SilverStockFactorFromWealthCurve.SortPoints();
            base.WriteSettings();
        }

        private void SimpleCurveEditor(Listing_Standard listing_Standard, ref SimpleCurve curve)
        {
            for (int num = 0; num < curve.PointsCount; num++)
            {
                CurvePoint point = curve.Points[num];

                Rect rect1 = listing_Standard.GetRect(Text.LineHeight);
                rect1.width = (rect1.width - 55) / 2;
                Rect rect2 = new Rect(rect1)
                {
                    x = rect1.xMax + 5
                };
                Rect rect3 = new Rect(rect1)
                {
                    width = 20,
                    x = rect2.xMax + 5
                };
                Rect rect4 = new Rect(rect3)
                {
                    x = rect3.xMax + 5
                };

                float x = point.x;
                string buffer1 = x.ToString();
                Widgets.TextFieldNumeric<float>(rect1, ref x, ref buffer1, 0, 1E+09f);

                float y = point.y;
                string buffer2 = y.ToString();
                Widgets.TextFieldNumeric<float>(rect2, ref y, ref buffer2, 0, 1E+09f);

                if (x != point.x || y != point.y)
                {
                    curve.Points[num] = new CurvePoint(x, y);
                    curve.View.SetViewRectAround(curve);
                }

                if (Widgets.ButtonText(rect3, "+"))
                {
                    curve.Add(new CurvePoint(point), true);
                    curve.View.SetViewRectAround(curve);
                }
                if (Widgets.ButtonText(rect4, "-"))
                {
                    curve.Points.RemoveAt(num);
                    if (curve.PointsCount == 0)
                    {
                        curve.Add(0, 1);
                    }
                    curve.View.SetViewRectAround(curve);
                }

                listing_Standard.Gap(listing_Standard.verticalSpacing);
            }
        }

    }

    public class TraderStockMultiplierSettings : ModSettings
    {
        public SimpleCurve CountRangeMinFactorFromWealthCurve = new SimpleCurve
        {
            {0f,        1f},
            {20000f,    1f},
            {50000f,    1.2f},
            {100000f,   1.5f},
            {300000f,   2f}
        };

        public SimpleCurve CountRangeMaxFactorFromWealthCurve = new SimpleCurve
        {
            {0f,        1f},
            {20000f,    1f},
            {50000f,    1.2f},
            {100000f,   1.5f},
            {300000f,   2f},
            {3000000f,  5f}
        };

        public SimpleCurve SilverStockFactorFromWealthCurve = new SimpleCurve
        {
            {0f,        1f},
            {20000f,    1.1f},
            {50000f,    1.3f},
            {100000f,   1.5f},
            {300000f,   2f},
            {3000000f,  3f},
            {9999999f, 10f}
        };

        public MultiplierOption multiplierOption = MultiplierOption.MUltiplyTwiceSquareRoot;


        public override void ExposeData()
        {
            base.ExposeData();

            List<CurvePoint> points1 = SilverStockFactorFromWealthCurve.ToList();
            List<CurvePoint> points2 = CountRangeMinFactorFromWealthCurve.ToList();
            List<CurvePoint> points3 = CountRangeMaxFactorFromWealthCurve.ToList();

            Scribe_Collections.Look<CurvePoint>(ref points2, "CountRangeMinFactorFromWealthCurve");
            Scribe_Collections.Look<CurvePoint>(ref points3, "CountRangeMaxFactorFromWealthCurve");
            Scribe_Collections.Look<CurvePoint>(ref points1, "SilverStockFactorFromWealthCurve");
            //Scribe_Values.Look(ref SilverStockFactorFromWealthCurve, "SilverStockFactorFromWealthCurve");
            //Scribe_Values.Look(ref CountRangeMinFactorFromWealthCurve, "CountRangeMinFactorFromWealthCurve");
            //Scribe_Values.Look(ref CountRangeMaxFactorFromWealthCurve, "CountRangeMaxFactorFromWealthCurve");

            if (points2 != null)
            {
                CountRangeMinFactorFromWealthCurve = ListToSimpleCurve(points2);
            }
            if (points3 != null)
            {
                CountRangeMaxFactorFromWealthCurve = ListToSimpleCurve(points3);
            }
            if (points1 != null)
            {
                SilverStockFactorFromWealthCurve = ListToSimpleCurve(points1);
            }


            Scribe_Values.Look(ref multiplierOption, "MultiplierOption");
        }

        private SimpleCurve ListToSimpleCurve(List<CurvePoint> list)
        {
            SimpleCurve curves = new SimpleCurve();
            foreach (CurvePoint curvePoint in list)
            {
                curves.Add(curvePoint);
            }
            return curves;
        }
    }


    public enum MultiplierOption
    {
        MultiplyKind,
        MultiplyQuantity,
        MultiplyTwice,
        MUltiplyTwiceSquareRoot
    }


}
