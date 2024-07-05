using System;
using RimWorld;
using UnityEngine;
using Verse;
namespace RimWorldRealFoW
{
	public class Graphic_MoteSoundWave : Graphic_Mote
	{

		protected override bool ForcePropertyBlock
		{
			get
			{
				return true;
			}
		}

		public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
		{
			MoteSoundWave moteSoundWave = (MoteSoundWave)thing;
			float alpha = moteSoundWave.Alpha;
			if (alpha <= 0f)
			{
				return;
			}
			Graphic_Mote.propertyBlock.SetColor(ShaderPropertyIDs.ShockwaveColor, new Color(1f, 0.5f, 1f, alpha));
			Graphic_Mote.propertyBlock.SetFloat(ShaderPropertyIDs.ShockwaveSpan, moteSoundWave.CalculatedShockwaveSpan());
			base.DrawMoteInternal(loc, rot, thingDef, thing, 0);
		}

		// Token: 0x06001591 RID: 5521 RVA: 0x0007ED34 File Offset: 0x0007CF34
		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"MoteSplash(path=",
				this.path,
				", shader=",
				base.Shader,
				", color=",
				this.color,
				", colorTwo=unsupported)"
			});
		}
	}
}
