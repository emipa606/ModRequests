﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vehicles;
using Verse;

namespace JAHV
{
    public class Avenger : VehiclePawn
    {
        private static JAHVSettings modSettings = null;

        public static JAHVSettings ModSettings
        {
            get
            {
                if (modSettings == null)
                {
                    modSettings = LoadedModManager.GetMod<JAHVMod>().GetSettings<JAHVSettings>();
                }
                return modSettings;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                if (!(gizmo is Command_Action) || !(((Command_Action)gizmo).defaultLabel == "VF_LoadCargo".Translate()))
                {
                    yield return gizmo;
                }
            }
            if (cargoToLoad.NullOrEmpty())
            {
                yield return new Command_Action
                {
                    defaultLabel = "Load Vehicle",
                    icon = base.VehicleDef.LoadCargoIcon,
                    action = delegate
                    {
                        Find.WindowStack.Add(new Dialog_LoadVehicle(this));
                    }
                };
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            inventory.DropAllNearPawn(base.Position);
            base.Destroy(mode);
        }

        public override string GetInspectString()
        {
            string inspect = base.GetInspectString();
            int totalTiles = 0;
            foreach (Thing thing in inventory.innerContainer)
            {
                if (thing is VehiclePawn)
                {
                    totalTiles += thing.def.size.Area;
                }
            }
            int vehicleMaxSize = ModSettings.maxShipSizeX * ModSettings.maxShipSizeY;
            return inspect + "\nVehicle Cargo tiles available: {0}/{1}".Formatted(vehicleMaxSize - totalTiles, vehicleMaxSize);
        }
    }
}
