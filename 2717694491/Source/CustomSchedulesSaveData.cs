using System.Collections.Generic;
using Verse;

namespace CustomSchedules
{
    public class CustomSchedulesSaveData : MapComponent
    {
        public List<int> cuShPawnIds = new List<int>();
        public List<string> cuShLastManualAreas = new List<string>();

        public CustomSchedulesSaveData(Map map)
          : base(map)
        {
        }

        /// <summary>
        /// The part that writes our settings to file. Note that saving is by ref.
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Collections.Look(ref cuShPawnIds, "cuShPawnIds", LookMode.Value);
            Scribe_Collections.Look(ref cuShLastManualAreas, "cuShLastManualAreas", LookMode.Value);
            base.ExposeData();
        }

        public bool PawnScheduleEnabled(Pawn _pawn)
        {
            return cuShPawnIds.Contains(_pawn.thingIDNumber);
        }

        public void AddPawnToList(Pawn _pawn)
        {
            cuShPawnIds.Add(_pawn.thingIDNumber);
            Area lastArea = _pawn.playerSettings.EffectiveAreaRestriction;
            cuShLastManualAreas.Add(lastArea == null ? "unrestricted" : lastArea.Label);
        }

        public void RemovePawnFromList(Pawn _pawn)
        {
            int index = cuShPawnIds.IndexOf(_pawn.thingIDNumber);

            SetAssignment(_pawn, _pawn.Map.areaManager.AllAreas.FirstOrDefault(
                area => area.Label == cuShLastManualAreas[index]));

            cuShPawnIds.RemoveAt(index);
            cuShLastManualAreas.RemoveAt(index);
        }

        public void AssignLastManualAreaForPawn(Pawn _pawn)
        {
            if (cuShPawnIds.Contains(_pawn.thingIDNumber))
            {
                int index = cuShPawnIds.IndexOf(_pawn.thingIDNumber);

                SetAssignment(_pawn, _pawn.Map.areaManager.AllAreas.FirstOrDefault(
                    area => area.Label == cuShLastManualAreas[index]));
            }
        }

        public void SetAssignment(Pawn pawn, Area newArea)
        {
            pawn.playerSettings.AreaRestriction = newArea;
        }
    }
}