using Reload.Components;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Reload
{
    public class ReloadUtils
    {
        public static List<Thing> GetReloadableEquipments(Pawn pawn) => pawn?.equipment?.AllEquipmentListForReading?.Where(x => Setting.ReloadableWeapons.Contains(x.def) && x.TryGetComp<CompReload>() != null).Select(x => (Thing)x).ToList() ?? new List<Thing>();
        public static List<Thing> GetReloadNeededEquipments(Pawn pawn) => pawn?.equipment?.AllEquipmentListForReading?.Where(x => Setting.ReloadableWeapons.Contains(x.def) && (x.TryGetComp<CompReload>()?.NeedsReload() ?? false)).Select(x => (Thing)x).ToList() ?? new List<Thing>();
        public static List<Thing> GetReloadableEquipmentsFromInventory(Pawn pawn) => pawn?.inventory?.innerContainer?.Where(x => Setting.ReloadableWeapons.Contains(x.def) && x.TryGetComp<CompReload>() != null).ToList() ?? new List<Thing>();
        public static List<Thing> GetReloadNeededEquipmentsFromInventory(Pawn pawn) => pawn?.inventory?.innerContainer?.Where(x => Setting.ReloadableWeapons.Contains(x.def) && (x.TryGetComp<CompReload>()?.NeedsReload() ?? false)).ToList() ?? new List<Thing>();
        public static List<Thing> GetAllReloadableEquipmentsFromPawn(Pawn pawn)
        {
            List<Thing> weapons = new List<Thing>();
            weapons.AddRange(ReloadUtils.GetReloadableEquipments(pawn) ?? new List<Thing>());
            weapons.AddRange(ReloadUtils.GetReloadableEquipmentsFromInventory(pawn) ?? new List<Thing>());
            return weapons;
        }
        public static List<Thing> GetAllReloadNeededEquipmentsFromPawn(Pawn pawn)
        {
            List<Thing> weapons = new List<Thing>();
            weapons.AddRange(ReloadUtils.GetReloadNeededEquipments(pawn) ?? new List<Thing>());
            weapons.AddRange(ReloadUtils.GetReloadNeededEquipmentsFromInventory(pawn) ?? new List<Thing>());
            return weapons;
        }
    }
}