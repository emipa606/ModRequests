// Decompiled with JetBrains decompiler
// Type: SmallWireGoAway
// Assembly: GoAwaySmallWire, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 7E93E5BD-525E-4143-A965-D90025B5F5E5
// Assembly location: C:\Program Files (x86)\Steam\steamapps\workshop\content\294100\1190364506\Assemblies\GoAwaySmallWire.dll

using Harmony;
using System.Reflection;
using Verse;

[StaticConstructorOnStartup]
internal class SmallWireGoAway
{
  static SmallWireGoAway()
  {
    HarmonyInstance.Create("small.wire.go.away.zzz.mod").PatchAll(Assembly.GetExecutingAssembly());
  }
}
