using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class Building_TurretMulti : Building_TurretGunWithGizmo
    {
        ModExtension_Building_TurretMulti Extension => def.GetModExtension<ModExtension_Building_TurretMulti>();

        List<FuelPercentAndGraphicPair> Graphics = new List<FuelPercentAndGraphicPair>();
        public override Material TurretTopMaterial
        {
            get
            {
                if (Extension != null)
                {
                    Material mat = null;
                    if (Graphics.NullOrEmpty())
                    {
                        Graphics = Extension.graphicDatas;
                    }
                    for (int i = 0; i < Graphics.Count; i++)
                    {
                        if (refuelableComp?.FuelPercentOfMax <= Graphics[i].fuelPercent)
                        {
                            mat = MaterialPool.MatFrom(Graphics[i].graphicData.texPath);
                        }
                    }
                    return mat ?? def.building.turretTopMat;
                }
                return this.def.building.turretTopMat;
            }
        }
    }

    public class ModExtension_Building_TurretMulti : DefModExtension
    {
        public List<FuelPercentAndGraphicPair> graphicDatas = new List<FuelPercentAndGraphicPair>();
    }
}
