#if HARM12
using Harmony;
#else
using HarmonyLib;
#endif
using System;
using System.Reflection;
using Verse;

namespace NightVision
{
    /// <summary>
    /// Container class for settings. Including presentation strings and callback on changed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SettingOption<T>
    {
        public readonly string Label;

        public T TempValue;

        public readonly string Tooltip;

        public SettingOption(string label, T value, Action callBack = null, string tooltip = "")
        {
            Label = label;
            Tooltip = tooltip;
            StoredValue = TempValue = value;
            CallBackOnValueChanged = callBack;
        }

        public T Value
        {
            get { return StoredValue; }
            set
            {
                if (!value.ApproxEq(StoredValue))
                {
                    _valueChanged = true;
                }

                StoredValue = TempValue = value;
            }
        }

        public T StoredValue;

        protected Action CallBackOnValueChanged;

        private bool _valueChanged;


        public void Commit()
        {
            Value = TempValue;

            if (_valueChanged)
            {
                CallBackOnValueChanged?.Invoke();
            }
        }
    }

    /// <summary>
    /// Container class for settings that require patches. Value determines if patching is applied.
    /// </summary>
    public class SettingOption_WithPatch : SettingOption<bool>
    {
        public SettingOption_WithPatch
        (
            string label, bool value, string tooltip, MethodInfo[] targets, HarmonyPatchType[] patchTypes,
            MethodInfo[] patches) : base(label, value, tooltip: tooltip)
        {
            int minCount = Math.Min(targets.Length, patchTypes.Length);
            minCount = Math.Min(minCount, patches.Length);

            CallBackOnValueChanged = () =>
            {
                if (StoredValue)
                {
                    for (int i = 0; i < minCount; i++)
                    {
                        ApplyPatch(targets[i], patchTypes[i], patches[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < minCount; i++)
                    {
                        RemovePatch(targets[i], patches[i]);
                    }
                }

#if DEBUG


                Log.Message(new string('-', 20));
                Log.Message($"SettingOption_WithPatch");
                Log.Message($"Value = {Value}");

                for (int i = 0; i < minCount; i++)
                {
                    Log.Message($"----");

                    Log.Message($"targets = {targets[i]}");
                    Log.Message($"patchs = {patches[i]}");
                    Log.Message($"Result: ");
                    var results = HarmonyLib.Harmony.GetPatchInfo(targets[i]);

                    Log.Message($"Pre: {results.Prefixes.ToStringSafeEnumerable()}");
                    Log.Message($"Post = {results.Postfixes.ToStringSafeEnumerable()}");
                    Log.Message($"Trans = {results.Transpilers.ToStringSafeEnumerable()}");
                    Log.Message($"----");
                }

                Log.Message(new string('-', 20));

#endif
            };
        }

        public SettingOption_WithPatch(
            string label, bool value, string tooltip, MethodInfo target, HarmonyPatchType patchType, MethodInfo patch) :
            base(label, value, tooltip: tooltip)
        {
            CallBackOnValueChanged = () =>
            {
                if (StoredValue)
                {
                    ApplyPatch(target: target, patchType: patchType, patch: patch);
                }
                else
                {
                    RemovePatch(target: target, patch: patch);
                }
#if DEBUG


                Log.Message(new string('-', 20));
                Log.Message($"SettingOption_WithPatch");
                Log.Message($"Value = {Value}");
                Log.Message($"target = {target}");
                Log.Message($"patch = {patch}");
                Log.Message($"Result: ");
                var patches = HarmonyLib.Harmony.GetPatchInfo(target);

                Log.Message($"Pre: {patches.Prefixes.ToStringSafeEnumerable()}");
                Log.Message($"Post = {patches.Postfixes.ToStringSafeEnumerable()}");
                Log.Message($"Trans = {patches.Transpilers.ToStringSafeEnumerable()}");
                Log.Message(new string('-', 20));

#endif
            };
        }

        private static void RemovePatch(MethodInfo target, MethodInfo patch)
        {
            NVHarmonyPatcher.NVHarmony.Unpatch(target, patch);
        }

        private static void ApplyPatch(MethodInfo target, HarmonyPatchType patchType, MethodInfo patch)
        {
            var harmonyMethod = new HarmonyMethod(patch);

            switch (patchType)
            {
                case HarmonyPatchType.Prefix:
                    NVHarmonyPatcher.NVHarmony.Patch(target, harmonyMethod);

                    break;
                case HarmonyPatchType.Postfix:
                    NVHarmonyPatcher.NVHarmony.Patch(target, postfix: harmonyMethod);

                    break;
                case HarmonyPatchType.Transpiler:
                    NVHarmonyPatcher.NVHarmony.Patch(target, transpiler: harmonyMethod);

                    break;
            }
        }
    }
}