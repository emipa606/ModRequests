using System;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Rachek128.RitualAttenuation.Patches
{
  [Verse.StaticConstructorOnStartup]
  static class Window_Overrides
  {
    private static readonly FieldInfo _assignments;
    private static readonly FieldInfo _ritual;

    private static readonly FieldInfo _organizer;
    static Window_Overrides()
    {
      Overrides.RegisterWindowClose<Dialog_BeginRitual>(Close);
      _assignments = HarmonyLib.AccessTools.DeclaredField(typeof(Dialog_BeginRitual), "assignments");
      _ritual = HarmonyLib.AccessTools.DeclaredField(typeof(Dialog_BeginRitual), "ritual");
      _organizer = HarmonyLib.AccessTools.DeclaredField(typeof(Dialog_BeginRitual), "organizer");
    }

    private static void Close(Dialog_BeginRitual obj)
    {
      var assignments = (RitualRoleAssignments)_assignments.GetValue(obj);
      var entries = RitualExtendedDataManager.Instance.FindAll(assignments).ToList();
      if (entries.Count == 0)
        return;

      if (_ritual.GetValue(obj) == null)
      {
        Pawn organizer = (Pawn)_organizer.GetValue(obj);

        RitualExtendedDataManager.Instance.PromoteToAdHoc(assignments, organizer.GetLord());
      }
    }
  }
}