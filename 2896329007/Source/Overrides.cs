using System;
using System.Collections.Generic;

namespace Rachek128.RitualAttenuation
{
  /// <summary>
  /// An extension point for overriding the behavior of specific methods. Effectively simulates patching the overrides of methods that aren't overridden from the base class.
  /// </summary>
  public static class Overrides {
    private static Dictionary<Type, Delegate> _qualityDescriptors = new Dictionary<Type, Delegate>();

    private static Dictionary<Type, Delegate> _qualityOffsets = new Dictionary<Type, Delegate>();
    private static Dictionary<Type, Action<Verse.Window>> _windowClose = new Dictionary<Type, Action<Verse.Window>>();

    /// <summary>
    /// Registers a handler function for controlling the behavior of <see name="RimWorld.RitualOutcomeComp_Quality.GetDesc"/>.
    /// </summary>
    public static void RegisterQualityDescriptor<T>(Func<T, RimWorld.LordJob_Ritual, RimWorld.RitualOutcomeComp_Data, string, string> handler)
    where T : RimWorld.RitualOutcomeComp_Quality, new()
    {
      if (handler is null)
        throw new ArgumentNullException(nameof(handler));
      _qualityDescriptors[typeof(T)] = handler;
    }

    /// <summary>
    /// Registers a handler function for controlling the behavior of <see name="RimWorld.RitualOutcomeComp_Quality.QualityOffset"/>.
    /// </summary>
    public static void RegisterQualityOffset<T>(Func<T, RimWorld.LordJob_Ritual, RimWorld.RitualOutcomeComp_Data, float, float> handler)
    where T : RimWorld.RitualOutcomeComp_Quality, new()
    {
      if (handler is null)
        throw new ArgumentNullException(nameof(handler));
      _qualityOffsets[typeof(T)] = handler;
    }

    public static Func<RimWorld.RitualOutcomeComp_Quality, RimWorld.LordJob_Ritual, RimWorld.RitualOutcomeComp_Data, string, string> GetQualityDescriptor(Type t)
    {
      if (!_qualityDescriptors.TryGetValue(t, out var retn))
        return null;
      string Wrapper(RimWorld.RitualOutcomeComp_Quality comp, RimWorld.LordJob_Ritual ritual, RimWorld.RitualOutcomeComp_Data data, string result)
      {
        return (string)retn.DynamicInvoke(new object[]{ comp, ritual, data, result });
      }
      return Wrapper;
    }

    public static Func<RimWorld.RitualOutcomeComp_Quality, RimWorld.LordJob_Ritual, RimWorld.RitualOutcomeComp_Data, float, float> GetQualityOffset(Type t)
    {
      if (!_qualityOffsets.TryGetValue(t, out var retn))
        return null;
      float Wrapper(RimWorld.RitualOutcomeComp_Quality comp, RimWorld.LordJob_Ritual ritual, RimWorld.RitualOutcomeComp_Data data, float result)
      {
        return (float)retn.DynamicInvoke(new object[]{ comp, ritual, data, result });
      }
      return Wrapper;
    }

    public static void RegisterWindowClose<T>(Action<T> handler)
      where T : Verse.Window
    {
      void Wrapper(Verse.Window window)
      {
        if (window is T t)
          handler(t);
      }

      _windowClose[typeof(T)] = Wrapper;
    }

    public static Action<Verse.Window> GetWindowClose(Type t)
    {
      if (!_windowClose.TryGetValue(t, out var retn))
        return null;
      return retn;
    }
  }
}