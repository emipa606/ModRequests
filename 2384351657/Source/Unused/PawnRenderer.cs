using System;
using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;

namespace SSTHDweapons
{
    [HarmonyPatch]
    public static void DrawSheath(
      Pawn pawn,
      Thing eq,
      Vector3 drawLoc,
      float aimAngle,
      Graphic graphic)
    {
      float num = aimAngle % 360f;
      if ((CompSheath) ThingCompUtility.TryGetComp<CompSheath>(eq) == null)
        return;
      Graphics.DrawMesh(graphic.MeshAt(((Thing) pawn).get_Rotation()), drawLoc, Quaternion.AngleAxis(num, Vector3.get_up()), graphic.MatAt(((Thing) pawn).get_Rotation(), (Thing) null), 0);
    }
            }
        }
    }
}