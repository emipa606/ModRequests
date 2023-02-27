using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RadWorld
{
	[StaticConstructorOnStartup]
	public class ArchotechEnvironmentShield : Apparel
	{
		private static readonly Material BubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent, new ColorInt(27, 122, 37).ToColor);
		public override void DrawWornExtras()
		{
			float num = 1.2f;
			Vector3 drawPos = base.Wearer.Drawer.DrawPos;
			drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
			float angle = Rand.Range(0, 360);
			Vector3 s = new Vector3(num, 1f, num);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), s);
			Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
		}
	}
}
