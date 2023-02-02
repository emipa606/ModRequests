using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace Clockwork
{

	//[StaticConstructorOnStartup]
	//public class ClockworkOverlayDrawer
	//{
	//	private static readonly Material requiresSteamMat;

	//	private static readonly float BaseAlt;

	//	private Vector3 curOffset;

	//	private DrawBatch drawBatch = new DrawBatch();

	//	static ClockworkOverlayDrawer()
	//	{
	//		requiresSteamMat = MaterialPool.MatFrom("UI/RequiresSteam", ShaderDatabase.MetaOverlay);
	//		BaseAlt = AltitudeLayer.MetaOverlays.AltitudeFor();
	//	}

	//	public void RenderRequiresSteamOverlay(Thing t)
	//	{
	//		RenderPulsingOverlay(t, requiresSteamMat, 2, MeshPool.plane08);
	//	}

	//	private void RenderPulsingOverlay(Thing thing, Material mat, int altInd, Mesh mesh)
	//	{
	//		Vector3 drawPos = thing.TrueCenter();
	//		drawPos.y = BaseAlt + 3f / 74f * (float)altInd;
	//		drawPos += curOffset;
	//		RenderPulsingOverlayInternal(thing, mat, drawPos, mesh);
	//	}

	//	private void RenderPulsingOverlayInternal(Thing thing, Material mat, Vector3 drawPos, Mesh mesh)
	//	{
	//		float num = ((float)Math.Sin((Time.realtimeSinceStartup + 397f * (float)(thing.thingIDNumber % 571)) * 4f) + 1f) * 0.5f;
	//		num = 0.3f + num * 0.7f;
	//		Material material = FadedMaterialPool.FadedVersionOf(mat, num);
	//		Graphics.DrawMesh(mesh, drawPos, Quaternion.identity, material, 0);
	//	}

	// }
}
