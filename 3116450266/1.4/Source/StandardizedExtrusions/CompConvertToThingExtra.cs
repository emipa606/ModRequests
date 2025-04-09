using PipeSystem;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace PneumaticTubes
{
    /// <summary>
	/// Tap comp class, specifically for enabling plus/minus buttons at greater than 10
	/// </summary>
	public class CompConvertToThingExtra : CompConvertToThing
    {
        /// <summary>
		/// Action setting tap output to 100 less
		/// </summary>
		private Command_Action decrease100;

		/// <summary>
		/// Action setting tap output to 1000 less
		/// </summary>
		private Command_Action decrease1000;

		/// <summary>
		/// Action setting tap output to 100 more
		/// </summary>
		private Command_Action augment100;

		/// <summary>
		/// Action setting tap output to 1000 more
		/// </summary>
		private Command_Action augment1000;

        /// <summary>
		/// Create the actions
		/// </summary>
		/// <param name="respawningAfterLoad">Unused; passed through</param>
		public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

			decrease100 = new Command_Action
			{
				action = delegate
				{
					typeof(CompConvertToThing).GetField("maxHeldThingStackSize", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(
						this,
						((int)typeof(CompConvertToThing).GetField("maxHeldThingStackSize", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this)) - 100);

					if ((int)typeof(CompConvertToThing).GetField("maxHeldThingStackSize", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this) < 0)
					{
						typeof(CompConvertToThing).GetField("maxHeldThingStackSize", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, 0);
					}
				},
				defaultLabel = "StandardizedExtrusions.PipeSystem_DecreaseStack100".Translate(),
				defaultDesc = "StandardizedExtrusions.PipeSystem_DecreaseStackDesc100".Translate(),
				icon = (Texture2D)typeof(CompConvertToThing).GetField("less", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null)
			};

			decrease1000 = new Command_Action
			{
				action = delegate
				{
					typeof(CompConvertToThing).GetField("maxHeldThingStackSize", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(
						this,
						((int)typeof(CompConvertToThing).GetField("maxHeldThingStackSize", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this)) - 1000);

					if ((int)typeof(CompConvertToThing).GetField("maxHeldThingStackSize", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this) < 0)
					{
						typeof(CompConvertToThing).GetField("maxHeldThingStackSize", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, 0);
					}
				},
				defaultLabel = "StandardizedExtrusions.PipeSystem_DecreaseStack1000".Translate(),
				defaultDesc = "StandardizedExtrusions.PipeSystem_DecreaseStackDesc1000".Translate(),
				icon = (Texture2D)typeof(CompConvertToThing).GetField("less", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null)
			};

			augment100 = new Command_Action
			{
				action = delegate
				{
					typeof(CompConvertToThing).GetField("maxHeldThingStackSize", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(
						this,
						((int)typeof(CompConvertToThing).GetField("maxHeldThingStackSize", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this)) + 100);
				},
				defaultLabel = "StandardizedExtrusions.PipeSystem_AugmentStack100".Translate(),
				defaultDesc = "StandardizedExtrusions.PipeSystem_AugmentStackDesc100".Translate(),
				disabled = (Props.maxOutputStackSize != -1 && ((int)typeof(CompConvertToThing).GetField("maxHeldThingStackSize", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this)) + 100 > Props.maxOutputStackSize),
				icon = (Texture2D)typeof(CompConvertToThing).GetField("plus", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null)
			};

			augment1000 = new Command_Action
			{
				action = delegate
				{
					typeof(CompConvertToThing).GetField("maxHeldThingStackSize", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(
						this,
						((int)typeof(CompConvertToThing).GetField("maxHeldThingStackSize", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this)) + 1000);
				},
				defaultLabel = "StandardizedExtrusions.PipeSystem_AugmentStack1000".Translate(),
				defaultDesc = "StandardizedExtrusions.PipeSystem_AugmentStackDesc1000".Translate(),
				disabled = (Props.maxOutputStackSize != -1 && ((int)typeof(CompConvertToThing).GetField("maxHeldThingStackSize", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this)) + 1000 > Props.maxOutputStackSize),
				icon = (Texture2D)typeof(CompConvertToThing).GetField("plus", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null)
			};
		}

		/// <summary>
		/// Get the actions from the base class and then the 100/1000 gizmos from this class
		/// </summary>
		/// <returns>All the gizmos for the associated tap</returns>
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo item in base.CompGetGizmosExtra())
			{
				yield return item;
			}

			if (Props.maxOutputStackSize == -1 || Props.maxOutputStackSize > 100)
			{
				yield return decrease100;
				yield return augment100;
			}

			if (Props.maxOutputStackSize == -1 || Props.maxOutputStackSize > 1000)
			{
				yield return decrease1000;
				yield return augment1000;
			}
		}
	}
}
