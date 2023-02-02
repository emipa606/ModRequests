using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace TraderStockMultiplier
{
    [StaticConstructorOnStartup]
    public static class TraderStockMultiplier_Harmony
    {
        static TraderStockMultiplier_Harmony()
        {
            Harmony harmony = new Harmony("mlph.TraderStockMultiplier");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }


    [HarmonyPatch(typeof(StockGeneratorUtility), nameof(StockGeneratorUtility.TryMakeForStockSingle))]
    public static class TryMakeForStockSingle_Patch
    {
        //トレーダーのシルバー増やす
        //下のパッチでも増えるから控えめ...控えてる?
        //private static readonly SimpleCurve SilverStockFactorFromWealthCurve = new SimpleCurve
        //{
        //    {0f,        1f},
        //    {20000f,    1.1f},
        //    {50000f,    1.3f},
        //    {100000f,   1.5f},
        //    {300000f,   2f},
        //    {3000000f,  3f},
        //    {10000000f, 10f}
        //};

        private static Thing Postfix(Thing __result)
        {
            if (__result == null)
            {
                return __result;
            }
            if (__result.def == ThingDefOf.Silver)
            {
                __result.stackCount = Mathf.RoundToInt(
                    __result.stackCount *
                    CountRangeFactor.SilverStockFactorFromWealthCurve.Evaluate(Find.World.PlayerWealthForStoryteller)
                    );
            }
            return __result;
        }
    }


    public static class CountRangeFactor
    {
        //public static readonly SimpleCurve CountRangeMinFactorFromWealthCurve = new SimpleCurve
        //{
        //    {0f,        1f},
        //    {20000f,    1f},
        //    {50000f,    1.2f},
        //    {100000f,   1.5f},
        //    {300000f,   2f}
        //};

        //public static readonly SimpleCurve CountRangeMaxFactorFromWealthCurve = new SimpleCurve
        //{
        //    {0f,        1f},
        //    {20000f,    1f},
        //    {50000f,    1.2f},
        //    {100000f,   1.5f},
        //    {300000f,   2f},
        //    {3000000f,  5f}
        //};
        private static TraderStockMultiplierSettings settings = LoadedModManager.GetMod(typeof(TraderStockMultiplier)).GetSettings<TraderStockMultiplierSettings>();

        public static SimpleCurve CountRangeMaxFactorFromWealthCurve => settings.CountRangeMaxFactorFromWealthCurve;
        public static SimpleCurve CountRangeMinFactorFromWealthCurve => settings.CountRangeMinFactorFromWealthCurve;
        public static SimpleCurve SilverStockFactorFromWealthCurve => settings.SilverStockFactorFromWealthCurve;



        public static FloatRange AdjustedFloatRange;

        //public static IntRange AdjustedIntRange;

        public static IntRange IntRange_Fix(IntRange intRange)
        {
            return new IntRange(
               Mathf.RoundToInt(intRange.min * CountRangeMinFactorFromWealthCurve.EvaluateWealth()),
               Mathf.RoundToInt(intRange.max * CountRangeMaxFactorFromWealthCurve.EvaluateWealth()));
        }

        public static IntRange IntRange_Fix_Quantity(IntRange intRange)
        {
            if (settings.multiplierOption == MultiplierOption.MultiplyKind)
            {
                return intRange;
            }
            else if (settings.multiplierOption == MultiplierOption.MUltiplyTwiceSquareRoot)
            {
                return new IntRange(
                   Mathf.RoundToInt(intRange.min * Mathf.Sqrt(CountRangeMinFactorFromWealthCurve.EvaluateWealth())),
                   Mathf.RoundToInt(intRange.max * Mathf.Sqrt(CountRangeMaxFactorFromWealthCurve.EvaluateWealth()))
                   );
            }
            else
            {
                return IntRange_Fix(intRange);
            }
        }

        private static FloatRange FloatRange_Fix(FloatRange countRange)
        {
            return new FloatRange(
               countRange.min * CountRangeMinFactorFromWealthCurve.EvaluateWealth(),
               countRange.max * CountRangeMaxFactorFromWealthCurve.EvaluateWealth());
        }

        private static FloatRange FloatRange_Fix_sqrt(FloatRange countRange)
        {
            return new FloatRange(
               countRange.min * Mathf.Sqrt(CountRangeMinFactorFromWealthCurve.EvaluateWealth()),
               countRange.max * Mathf.Sqrt(CountRangeMaxFactorFromWealthCurve.EvaluateWealth())
               );
        }

        public static FloatRange FloatRange_Fix_Quantity(this FloatRange floatRange)
        {
            if (settings.multiplierOption == MultiplierOption.MultiplyKind)
            {
                return floatRange;
            }
            else if (settings.multiplierOption == MultiplierOption.MUltiplyTwiceSquareRoot)
            {
                return FloatRange_Fix_sqrt(floatRange);
            }
            else
            {
                return FloatRange_Fix(floatRange);
            }
        }

        public static int RandomInRange(IntRange intRange)
        {
            return GenMath.RoundRandom(FloatRange_Fix(new FloatRange(intRange.min, intRange.max)).RandomInRange);
        }

        public static int RandomInRange_Kind(IntRange intRange)
        {
            if (settings.multiplierOption == MultiplierOption.MUltiplyTwiceSquareRoot)
            {
                return GenMath.RoundRandom(FloatRange_Fix_sqrt(new FloatRange(intRange.min, intRange.max)).RandomInRange);
            }
            else if (settings.multiplierOption == MultiplierOption.MultiplyQuantity)
            {
                return intRange.RandomInRange;
            }
            else
            {
                return RandomInRange(intRange);
            }
        }

        //public static void IntRange_Adjust(IntRange intRange)
        //{
        //    AdjustedIntRange = new IntRange(
        //       Mathf.RoundToInt(intRange.min * CountRangeMinFactorFromWealthCurve.Evaluate(Find.World.PlayerWealthForStoryteller)),
        //       Mathf.RoundToInt(intRange.max * CountRangeMaxFactorFromWealthCurve.Evaluate(Find.World.PlayerWealthForStoryteller)));
        //}
        public static void FloatRange_Adjust(FloatRange floatRange)
        {
            AdjustedFloatRange = new FloatRange(
               floatRange.min * CountRangeMinFactorFromWealthCurve.EvaluateWealth(),
               floatRange.max * CountRangeMaxFactorFromWealthCurve.EvaluateWealth());
        }

        private static float EvaluateWealth(this SimpleCurve simpleCurve)
        {
            return simpleCurve.Evaluate(Find.World.PlayerWealthForStoryteller);
        }
    }

    public static class CountRangeFactorUtility
    {
        //共通メソッド
        public static MethodBase TargetMethod_GenerateThings_MoveNext(Type targettype)
        {
            //イテレーターの内部クラスのMoveNextメソッド
            //Type innerclass = targettype.GetNestedTypes(AccessTools.all).FirstOrDefault(x => x.GetMethods().Select(y => y.Name).Contains("MoveNext"));
            Type innerclass = targettype.GetNestedTypes(AccessTools.all).FirstOrDefault(x => x.Name.Contains(nameof(StockGenerator.GenerateThings)));
            MethodBase targetmethod = innerclass.GetMethods(AccessTools.all).FirstOrDefault(m => m.Name == "MoveNext");
            //Log.Message(targetmethod.ToString());
            return targetmethod;
        }

        public static List<CodeInstruction> RandomInRange_Patch(IEnumerable<CodeInstruction> instructions, string fieldname, bool refsettings = false)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int index = codes.FirstIndexOf(x => (x.opcode == OpCodes.Ldflda) && (x.operand is FieldInfo field) && field.Name == fieldname);


            if (index == 0)
            {
                Log.Error("Failed to Patch");
                return codes;
            }
            //指定した名前のCountRangeの読み込みをldfldaからldfldに
            codes[index].opcode = OpCodes.Ldfld;

            //RandomInRangeを自作メソッドにすり替え
            if (refsettings)
            {
                codes[index + 1].operand = AccessTools.Method(typeof(CountRangeFactor), nameof(CountRangeFactor.RandomInRange_Kind));
            }
            else
            {
                codes[index + 1].operand = AccessTools.Method(typeof(CountRangeFactor), nameof(CountRangeFactor.RandomInRange));
            }
            //Log.Message("IL code\n\n" + string.Join("\n", codes.Select(x => x.ToString()).ToArray()) + "\n\n");
            return codes;
        }

        public static int RandomCountOf_2(StockGenerator stockGenerator, ThingDef def)
        {
            //コピペしてthisをstockGeneratorにかえた
            IntRange intRange = stockGenerator.countRange;
            if (stockGenerator.customCountRanges != null)
            {
                for (int i = 0; i < stockGenerator.customCountRanges.Count; i++)
                {
                    if (stockGenerator.customCountRanges[i].thingDef == def)
                    {
                        intRange = stockGenerator.customCountRanges[i].countRange;
                        break;
                    }
                }
            }

            //settingを参照して結果をいじるやつ
            intRange = CountRangeFactor.IntRange_Fix_Quantity(intRange);
            FloatRange totalPriceRange = stockGenerator.totalPriceRange.FloatRange_Fix_Quantity();

            if (intRange.max <= 0 && totalPriceRange.max <= 0f)
            {
                return 0;
            }
            if (intRange.max > 0 && totalPriceRange.max <= 0f)
            {
                return intRange.RandomInRange;
            }
            if (intRange.max <= 0 && totalPriceRange.max > 0f)
            {
                return Mathf.RoundToInt(totalPriceRange.RandomInRange / def.BaseMarketValue);
            }
            int num = 0;
            int randomInRange;
            do
            {
                randomInRange = intRange.RandomInRange;
                num++;
                if (num > 100)
                {
                    break;
                }
            }
            while (!totalPriceRange.Includes((float)randomInRange * def.BaseMarketValue));
            return randomInRange;
        }

        public static List<CodeInstruction> RandomCountOf_RefSettings(IEnumerable<CodeInstruction> instructions, Type targettype)
        {
            //settingを参照して結果をいじるやつ
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            //RandomCountOfの直後にDoNothing(int)を呼び出してスタック無駄遣い
            //その後StockGenerator_CategoryとchosenThingDefをスタックに入れて
            //CountRangeFactorUtility.RandomCountOf_2を呼び出し
            FieldInfo[] fields = targettype.GetNestedTypes(AccessTools.all)
                .FirstOrDefault(x => x.Name.Contains(nameof(StockGenerator.GenerateThings))).GetFields(AccessTools.all);

            int index = codes.FirstIndexOf(x => x.opcode == OpCodes.Call && x.operand is MethodInfo method && method.Name == "RandomCountOf");
            if (index == 0)
            {
                Log.Error("Failed to Patch");
                return codes;
            }

            codes.InsertRange(index + 1, new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CountRangeFactorUtility), nameof(CountRangeFactorUtility.DoNothing))),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, fields.FirstOrDefault(x => x.FieldType == targettype)), //親クラスのインスタンスを保存する内部クラスのフィールド
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, fields.FirstOrDefault(x => x.FieldType == typeof(ThingDef))),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CountRangeFactorUtility), nameof(CountRangeFactorUtility.RandomCountOf_2)))
            });

            return codes;
        }


        public static void DoNothing(int _) { }
        public static bool ReturnFalse(bool _) { return false; }
    }

    [HarmonyPatch(typeof(StockGenerator), "RandomCountOf")]
    public static class RandomCountOf_Patch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            //local intRange
            //すべての数に影響
            int index = codes.FirstIndexOf(x => x.opcode == OpCodes.Ldfld && x.operand is FieldInfo field && field.Name == "max");
            // intRange = CountRangeFactor.IntRange_Fix(intRange);
            //を
            // if (intRange.max <= 0 && this.totalPriceRange.max <= 0f)
            // {
            //     return 0;
            // }
            //の直前に挿入
            codes.InsertRange(index, new List<CodeInstruction>{
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CountRangeFactor), nameof(CountRangeFactor.IntRange_Fix))),
                    new CodeInstruction(OpCodes.Stloc_0)
                });

            //totalPriceRange
            //一部の金額指定のやつに影響
            //すべてのtotalPriceRangeを
            //CountRangeFactor.FloatRange_Adjust(totalPriceRange);
            //CountRangeFactor.AdjustedFloatRange
            //に差し替え
            for (int i = 0; i < codes.Count - 1; i++)
            {
                if (codes[i].opcode == OpCodes.Ldflda && codes[i].operand is FieldInfo field && field.Name == "totalPriceRange")
                {
                    codes[i].opcode = OpCodes.Ldfld;
                    codes.Insert(i + 1, new CodeInstruction(
                        OpCodes.Call, AccessTools.Method(typeof(CountRangeFactor), nameof(CountRangeFactor.FloatRange_Adjust))
                        ));
                    codes.Insert(i + 2, new CodeInstruction(
                        OpCodes.Ldsflda, AccessTools.Field(typeof(CountRangeFactor), nameof(CountRangeFactor.AdjustedFloatRange))
                        ));
                    i += 2;
                }
            }

            //Log.Message("IL code\n\n" + string.Join("\n", codes.Select(x => x.ToString()).ToArray()) + "\n\n");
            return codes;
        }
    }


    [HarmonyPatch]
    public static class StockGenerator_MiscItems_Patch
    {
        //art,armor,clothes,weaponrangedの種類数に影響
        public static MethodBase TargetMethod()
        {
            return CountRangeFactorUtility.TargetMethod_GenerateThings_MoveNext(typeof(StockGenerator_MiscItems));
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return CountRangeFactorUtility.RandomInRange_Patch(instructions, "countRange");
        }
    }

    [HarmonyPatch]
    public static class StockGenerator_Animals_Patch
    {
        public static MethodBase TargetMethod()
        {
            return CountRangeFactorUtility.TargetMethod_GenerateThings_MoveNext(typeof(StockGenerator_Animals));
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            //List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            //動物の種類数
            IEnumerable<CodeInstruction> c1 = CountRangeFactorUtility.RandomInRange_Patch(instructions, "kindCountRange");

            //動物の個体数
            //動物全種合わせての数なのでこっちも増やさないといけない
            IEnumerable<CodeInstruction> c2 = CountRangeFactorUtility.RandomInRange_Patch(c1, "countRange");
            return c2;
        }
    }

    [HarmonyPatch]
    public static class StockGenerator_Category_Patch
    {
        //持ってくるものの種類数
        public static MethodBase TargetMethod()
        {
            return CountRangeFactorUtility.TargetMethod_GenerateThings_MoveNext(typeof(StockGenerator_Category));
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            //種類
            List<CodeInstruction> codes = CountRangeFactorUtility.RandomInRange_Patch(instructions, "thingDefCountRange", true);

            //数
            //settingを参照して結果をいじるやつ
            codes = CountRangeFactorUtility.RandomCountOf_RefSettings(codes, typeof(StockGenerator_Category));

            //Log.Message("IL code\n\n" + string.Join("\n", codes.Select(x => x.ToString()).ToArray()) + "\n\n");
            return codes;
        }
    }

    [HarmonyPatch]
    public static class StockGenerator_Tag_Patch
    {
        //持ってくるものの種類数
        public static MethodBase TargetMethod()
        {
            return CountRangeFactorUtility.TargetMethod_GenerateThings_MoveNext(typeof(StockGenerator_Tag));
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            //種類
            List<CodeInstruction> codes = CountRangeFactorUtility.RandomInRange_Patch(instructions, "thingDefCountRange", true);

            //数
            //settingを参照して結果をいじるやつ
            codes = CountRangeFactorUtility.RandomCountOf_RefSettings(codes, typeof(StockGenerator_Tag));

            //Log.Message("IL code\n\n" + string.Join("\n", codes.Select(x => x.ToString()).ToArray()) + "\n\n");
            return codes;
        }
    }

    [HarmonyPatch(typeof(StockGenerator_MultiDef), nameof(StockGenerator_MultiDef.GenerateThings))]
    public static class StockGenerator_MultiDef_Patch
    {
        private static bool Prefix(StockGenerator_MultiDef __instance, ref IEnumerable<Thing> __result)
        {
            List<Thing> result = new List<Thing>();

            int numThingDefsToUse = CountRangeFactor.RandomInRange_Kind(IntRange.one);
            for (int i = 0; i < numThingDefsToUse; i++)
            {
                if (!(from d in DefDatabase<ThingDef>.AllDefs
                      where __instance.HandlesThingDef(d) && d.tradeability.TraderCanSell()
                      select d)
                      .TryRandomElement(out ThingDef chosenThingDef)
                      )
                {
                    break;
                }

                foreach (Thing th in StockGeneratorUtility.TryMakeForStock(chosenThingDef, CountRangeFactorUtility.RandomCountOf_2(__instance, chosenThingDef)))
                {
                    result.Add(th);
                }
            }
            __result = result.AsEnumerable();
            return false;
        }

        //static void Postfix(IEnumerable<Thing> __result)
        //{
        //    //Log.Message("a");
        //    Log.Message(string.Join(",", __result.ToList().Select(x => x.ToString()).ToArray()));
        //}
    }

    [HarmonyPatch]
    public static class StockGenerator_Slaves_Patch
    {
        //奴隷の数に影響
        public static MethodBase TargetMethod()
        {
            return CountRangeFactorUtility.TargetMethod_GenerateThings_MoveNext(typeof(StockGenerator_Slaves));
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return CountRangeFactorUtility.RandomInRange_Patch(instructions, "countRange");
        }
    }

    [HarmonyPatch]
    public static class StockGenerator_Category_Patch2
    {
        private static MethodBase TargetMethod()
        {
            //内部クラスの内部クラスのメソッド
            //メソッド一つしかなかった

            //1.1でなんか変わってた .netの更新による最適化なのかそもそもソースコードが変わったのか知らないけど
            //Type innerclass = typeof(StockGenerator_Category).GetNestedTypes(AccessTools.all).FirstOrDefault(x => x.GetMethods().Select(y => y.Name).Contains("MoveNext"));
            //Type innerclass = typeof(StockGenerator_Category).GetNestedTypes(AccessTools.all).FirstOrDefault(x => x.Name.Contains(nameof(StockGenerator_Category.GenerateThings)));
            //Log.Message(innerclass.ToString());
            //Type innerclass2 = innerclass.GetNestedTypes(AccessTools.all).FirstOrDefault();
            //Log.Message(innerclass2.ToString());
            //MethodBase targetmethod = innerclass2.GetMethods(AccessTools.all).FirstOrDefault();
            //Log.Message(targetmethod.ToString());
            //return targetmethod;

            Type innerclass = typeof(StockGenerator_Category).GetNestedTypes(AccessTools.all).FirstOrDefault(x => x.Name.Contains("<>c__DisplayClass4_0"));
            //Log.Message(innerclass.ToString());
            MethodBase targetmethod = innerclass.GetMethods(AccessTools.all).FirstOrDefault();
            //Log.Message(targetmethod.ToString());
            return targetmethod;
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();

            //同じものを複数回選べるようにする
            //具体的には generatedDefs.Contain(t)の結果をもみ消してfalseを返す
            int index = codes.FirstIndexOf(x => x.opcode == OpCodes.Ldfld && x.operand is FieldInfo field && field.Name == "generatedDefs");
            codes.Insert(index + 4, new CodeInstruction(
                OpCodes.Call,
                AccessTools.Method(typeof(CountRangeFactorUtility), nameof(CountRangeFactorUtility.ReturnFalse))
                ));

            //Log.Message("IL code\n\n" + string.Join("\n", codes.Select(x => x.ToString()).ToArray()) + "\n\n");
            return codes;
        }
    }

    [HarmonyPatch]
    public static class StockGenerator_Tag_Patch2
    {
        private static MethodBase TargetMethod()
        {
            Type innerclass = typeof(StockGenerator_Tag).GetNestedTypes(AccessTools.all).FirstOrDefault(x => x.Name.Contains("<>c__DisplayClass3_0"));
            MethodBase targetmethod = innerclass.GetMethods(AccessTools.all).FirstOrDefault();
            //Log.Message(targetmethod.ToString());
            return targetmethod;
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();

            //同じものを複数回選べるようにする
            //具体的には generatedDefs.Contain(d)の結果をもみ消してfalseを返す
            int index = codes.FirstIndexOf(x => x.opcode == OpCodes.Ldfld && x.operand is FieldInfo field && field.Name == "generatedDefs");
            codes.Insert(index + 3, new CodeInstruction(
                OpCodes.Call,
                AccessTools.Method(typeof(CountRangeFactorUtility), nameof(CountRangeFactorUtility.ReturnFalse))
                ));

            //Log.Message("IL code\n\n" + string.Join("\n", codes.Select(x => x.ToString()).ToArray()) + "\n\n");
            return codes;
        }
    }


    [HarmonyPatch(typeof(ThingSetMaker_TraderStock))]
    [HarmonyPatch(nameof(ThingSetMaker_TraderStock.Generate))]
    [HarmonyPatch(new Type[] { typeof(ThingSetMakerParams), typeof(List<Thing>) })]
    public static class ThingSetMaker_TraderStock_Patch
    {
        //スタック処理してくんないからしょうがないからこっちでやる
        private static void Postfix(ThingSetMakerParams parms, List<Thing> outThings)
        {
            bool flag = parms.traderDef.orbital;
            for (int i = 0; i < outThings.Count - 1; i++)
            {
                Thing t = outThings[i];
                for (int j = i + 1; j < outThings.Count; j++)
                {
                    Thing t2 = outThings[j];
                    if (TransferableUtility.TransferAsOne(t, t2, TransferAsOneMode.Normal))
                    {
                        if (flag || !(t.def.category == ThingCategory.Pawn))
                        {
                            t.TryAbsorbStack(t2, false);
                            if (t2.Destroyed)
                            {
                                outThings.Remove(t2);
                                j--;
                            }
                        }
                    }
                    //ついでにアーティファクトもスタックしてくんないからxmlのPatchでtradeNeverStack消してる
                }
            }
        }
    }


}