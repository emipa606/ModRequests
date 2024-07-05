using System;
using UnityEngine;
using Verse;
namespace RimWorldRealFoW
{
	// Token: 0x0200030F RID: 783
	public class MoteSoundWave : Mote
	{

		protected override bool EndOfLife
		{
			get
			{
				return base.AgeSecs >= this.targetSize / this.velocity;
			}
		}


		public override float Alpha
		{
			get
			{
				Mathf.Clamp01(base.AgeSecs * 10f);
				float num = 1f;
				float num2 = Mathf.Clamp01(1f - base.AgeSecs / (this.targetSize / this.velocity));
				return num * num2 * this.CalculatedIntensity();
			}
		}

		public void Initialize(Vector3 position, float size, float velocity)
		{
			this.exactPosition = position;
			this.targetSize = size;
			this.velocity = velocity;
			base.Scale = 0f;
		}

		protected override void TimeInterval(float deltaTime)
		{
			base.TimeInterval(deltaTime);
			if (base.Destroyed)
			{
				return;
			}
			float scale = base.AgeSecs * this.velocity;
			base.Scale = scale;
			//this.exactPosition += base.Map.waterInfo.GetWaterMovement(this.exactPosition) * deltaTime;
		}

		public float CalculatedIntensity()
		{
			return Mathf.Sqrt(this.targetSize) / 10f;
		}

		public float CalculatedShockwaveSpan()
		{
			return Mathf.Min(Mathf.Sqrt(this.targetSize) * 0.8f, this.exactScale.x) / this.exactScale.x;
		}



		private float targetSize;

		private float velocity;
	}
}
