// Decompiled with JetBrains decompiler
// Type: GoAwaySmallWire.Patch_ThatSmallWire
// Assembly: GoAwaySmallWire, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 7E93E5BD-525E-4143-A965-D90025B5F5E5
// Assembly location: C:\Program Files (x86)\Steam\steamapps\workshop\content\294100\1190364506\Assemblies\GoAwaySmallWire.dll

using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace GoAwaySmallWire
{
  [HarmonyPatch(typeof (PowerNetGraphics))]
  [HarmonyPatch("PrintWirePieceConnecting")]
  internal class Patch_ThatSmallWire
  {
    public static bool Prefix()
    {
      return false;
    }
  }

  [StaticConstructorOnStartup]
  public static class StartUp
  {
    static StartUp()
    {
      var harmony = new Harmony("zzz.invisibleconduit");
      harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
  }
}
