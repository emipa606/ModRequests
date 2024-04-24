using HarmonyLib;
using Verse;

namespace Rachek128.RitualAttenuation.Patches
{
  [HarmonyPatch(typeof(Window))]
  [HarmonyPatch(nameof(Window.PostClose))]
  static class Window_Close
  {
    static void Postfix(Window __instance)
    {
      var handler = Overrides.GetWindowClose(__instance.GetType());
      if (handler == null)
        return;

      handler(__instance);
    }
  }
}