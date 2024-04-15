using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace SR
{
    public class CompMeltable : ThingComp
	{
		public float? floatHealth = 0;
		protected static int hoursToMeltStart = 1;
		protected float meltMultiplier = .01f;
		protected float meltRate = 0;
		protected float meltRatePerHour = 0;
		protected int meltBufferTicks = 0;
		protected int meltBufferTicksMax = Mathf.RoundToInt(hoursToMeltStart * 2500f);

		public MeltStage Stage
		{
			get
			{
				return parent.AmbientTemperature <= 0 ? MeltStage.Solid: MeltStage.Melting;
			}
		}

		public override void CompTick()
		{
			Tick(1);
		}

		public override void CompTickRare()
		{
			Tick(250);
		}

		private void Tick(int interval)
		{
			UpdateFloatHealthToRealHealth();
			if (Stage == MeltStage.Melting)
			{
				if (meltBufferTicks < meltBufferTicksMax) //If the buffer hasn't expired..
					meltBufferTicks++; //Progress the buffer..
				var coolingRate = GenTemperature.RotRateAtTemperature(parent.AmbientTemperature);
				meltRate = coolingRate * meltMultiplier / (parent.stackCount / 2) ; //Update melt rate.
				meltRatePerHour = meltRate * 2500f;
				var meltRateOverInterval = meltRate * interval;
				var coolingRateOverInterval = coolingRate * interval;
				if (parent.MapHeld != null) //Don't push heat if we're not on a map (caravans).
					GenTemperature.PushHeat(parent.PositionHeld, parent.MapHeld, -coolingRateOverInterval); //Cool area, ceiling round it so that it's never by zero.
				if (parent.stackCount > 1 && floatHealth <= meltRateOverInterval) //If it's a stack and we're about to run out of HP..
				{
					parent.stackCount--; //Decrement stack.
					meltRateOverInterval -= floatHealth.Value; //Reduce damage by hitpoints to not be kind, we want to reserve it and apply it still.
					floatHealth = parent.MaxHitPoints; //Reset HP instead.
				}
				else //Not about to run out of HP..
					floatHealth -= meltRateOverInterval; //Do damage.
				UpdateRealHealthToFloatHealth();
			}
			else
				meltBufferTicks = 0; //Reset the buffer.
		}

		public void UpdateFloatHealthToRealHealth()
        {
			if (floatHealth == null || parent.HitPoints < floatHealth)
				floatHealth = parent.HitPoints;
		}

		public void UpdateRealHealthToFloatHealth()
		{
			parent.HitPoints = Mathf.RoundToInt(floatHealth.Value);
			if (parent.HitPoints <= 0)
				parent.Destroy(DestroyMode.Vanish);
		}

		public override void PreAbsorbStack(Thing otherStack, int count)
		{
			UpdateFloatHealthToRealHealth();
			float t = (float)count / (float)(parent.stackCount + count);
			floatHealth = Mathf.Lerp(floatHealth.Value, (float)parent.MaxHitPoints, t);
			UpdateRealHealthToFloatHealth();
		}

		public override string CompInspectStringExtra()
		{
			StringBuilder stringBuilder = new StringBuilder();
			switch (Stage)
			{
				case MeltStage.Solid:
					stringBuilder.Append("MeltStateSolid".Translate() + ".");
					break;
				case MeltStage.Melting:
					stringBuilder.Append("MeltStateMelting".Translate() + "." + (meltRate > 0 ? ("\nMelt rate: " + Math.Round(meltRatePerHour, 2) + " / hour") : ""));
					break;
			}
			return stringBuilder.ToString();
		}
	}
}