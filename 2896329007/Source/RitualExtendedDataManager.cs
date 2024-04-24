using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Rachek128.RitualAttenuation
{
  public class RitualExtendedDataManager : Verse.GameComponent
  {
    private List<RitualExtendedDataRef> ritualData = new List<RitualExtendedDataRef>();
    private List<Tuple<RitualRoleAssignments, RitualOutcomeComp_Quality, RitualExtendedData>> assignmentToDataMap = new List<Tuple<RitualRoleAssignments, RitualOutcomeComp_Quality, RitualExtendedData>>();

    public static RitualExtendedDataManager Instance => Current.Game.GetComponent<RitualExtendedDataManager>();

    public RitualExtendedDataManager(Game game)
    {
      
    }

    public override void GameComponentTick()
    {
      base.GameComponentTick();

      if (Find.TickManager.TicksAbs % 2000 == 0 && Scribe.mode == LoadSaveMode.Inactive)
        Clean();
    }

    public override void ExposeData()
    {
      base.ExposeData();
      if (Scribe.mode == LoadSaveMode.Saving)
      {
        Clean();
      }

      Scribe_Collections.Look(ref ritualData, nameof(ritualData), LookMode.Deep);
    }

    public RitualExtendedDataRef GetOrCreateFor(LordJob_Ritual ritual, RitualOutcomeComp_Quality comp, Action<RitualExtendedDataRef> initializer = null)
    {
      var index = ritualData.FindIndex(x => x.Ritual == ritual && x.Comp == comp);
      if (index == -1)
      {
        var data = new RitualExtendedDataRef(ritual, comp);
        initializer?.Invoke(data);
        ritualData.Add(data);
        return data;
      }
      return ritualData[index];
    }

    public RitualExtendedDataRef GetFor(LordJob_Ritual ritual, RitualOutcomeComp_Quality comp) => ritualData.Find(x => x.Ritual == ritual && x.Comp == comp);

    public RitualExtendedDataRef GetFor(Lord lord, RitualOutcomeComp_Quality comp) => ritualData.Find(x => x.Lord == lord && x.Comp == comp);

    public RitualExtendedData GetOrCreateFor(RitualOutcomeComp_Quality comp, RitualRoleAssignments assignments, Action<RitualExtendedData> initializer = null) {
      var retn = assignmentToDataMap.Find(x => x.Item1 == assignments && x.Item2 == comp)?.Item3;
      if (retn == null)
      {
        retn = new RitualExtendedData();
        initializer?.Invoke(retn);
        assignmentToDataMap.Add(Tuple.Create(assignments, comp, retn));
      }
      return retn;
    }

    public IEnumerable<RitualExtendedData> FindAll(RitualRoleAssignments assignments) => assignmentToDataMap.FindAll(x => x.Item1 == assignments).Select(x => x.Item3);

    public void PromoteToAdHoc(RitualRoleAssignments assignments, Lord lord) {
      var items = assignmentToDataMap.FindAll(x => x.Item1 == assignments);
      assignmentToDataMap.RemoveAll(x => x.Item1 == assignments);
      foreach (var item in items)
      {
        var refdata = new RitualExtendedDataRef(lord, item.Item2);
        refdata.Data = item.Item3;
        this.ritualData.Add(refdata);
      }
    }

    public RitualExtendedData GetFor(RitualOutcomeComp_Quality comp, RitualRoleAssignments assignments) => assignmentToDataMap.Find(x => x.Item1 == assignments && x.Item2 == comp)?.Item3;

    public void Remove(RitualOutcomeComp_Quality comp, RitualRoleAssignments assignments)
    {
      assignmentToDataMap.RemoveAll(x => x.Item1 == assignments && x.Item2 == comp);
    }

    public void Remove(RitualRoleAssignments assignments)
    {
      assignmentToDataMap.RemoveAll(x => x.Item1 == assignments && x.Item2 != null);
    }

    public void RemoveAdHoc(RitualRoleAssignments assignments)
    {
      assignmentToDataMap.RemoveAll(x => x.Item1 == assignments && x.Item2 == null);
    }

    /// <summary>
    /// Remove any stale items prior to saving.
    /// </sumamry>
    private void Clean()
    {
      if (Scribe.mode != LoadSaveMode.Saving && Scribe.mode != LoadSaveMode.Inactive)
      {
        Log.Error("Tried to clean ritual extended data when not safe.");
        return;
      }
      ritualData.RemoveAll(x => x.Ended);
    }
  }
}