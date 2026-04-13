using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Verse;
using Verse.Noise;

namespace BionicleKanohiMasksOfPower
{
	[StaticConstructorOnStartup]
	public class Command_Invisibility : Command_Action
	{
		private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(9, 203, 4, 64));

		private Apparel_Huna apparel;
		public Command_Invisibility(Apparel_Huna apparel)
		{
			this.apparel = apparel;
			order = 5f;
		}

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
			if (apparel.lastUsedTick > 0)
			{
				var cooldownTicksRemaining = Find.TickManager.TicksGame - apparel.lastUsedTick;
				if (cooldownTicksRemaining < Apparel_Huna.InvisibilityCooldownTicks)
				{
					float num = Mathf.InverseLerp(Apparel_Huna.InvisibilityCooldownTicks, 0, cooldownTicksRemaining);
					Widgets.FillableBar(rect, Mathf.Clamp01(num), cooldownBarTex, null, doBorder: false);
				}
			}
			if (result.State == GizmoState.Interacted)
			{
				return result;
			}
			return new GizmoResult(result.State);
        }
	}
	public class Apparel_Huna : Apparel
    {
		public int lastUsedTick;
		public const int InvisibilityCooldownTicks = 1740;//cooldown of 29 seconds, uptime of 30 seconds
		public override IEnumerable<Gizmo> GetWornGizmos()
        {
            foreach (var g in base.GetWornGizmos())
            {
                yield return g;
            }
            if (Wearer.IsColonistPlayerControlled && this.IsMasterworkOrLegendary())
            {
				yield return new Command_Invisibility(this)
				{
					defaultLabel = "Bionicle.Invisibility".Translate(),
					defaultDesc = "Bionicle.InvisibilityDesc".Translate(),
					action = delegate
					{
						var hediff = HediffMaker.MakeHediff(BionicleDefOf.BKMOP_Invisibility, Wearer);
						Wearer.health.AddHediff(hediff);
						lastUsedTick = Find.TickManager.TicksGame;
					},
					icon = this.def.uiIcon,
					disabled = lastUsedTick + InvisibilityCooldownTicks > Find.TickManager.TicksGame
				};
			}
        }

        public override void ExposeData()
        {
            base.ExposeData();
			Scribe_Values.Look(ref lastUsedTick, "lastUsedTick");
        }
    }
}