using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace CompToggleFireMode
{
	// Token: 0x02000004 RID: 4
	public class Command_ActionActive : Command_Action
	{
		// Token: 0x0600000D RID: 13 RVA: 0x00002284 File Offset: 0x00000484
		public override void GizmoUpdateOnMouseover()
		{
			base.GizmoUpdateOnMouseover();
			float range = this.verbProps.range;
			bool flag = range < 90f;
			if (flag)
			{
				GenDraw.DrawRadiusRing(this.pawn.Position, range);
			}
		}

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            var result = base.GizmoOnGUI(topLeft, maxWidth, parms);
			if (comp.drawMenu)
            {
				var otherVerbs = comp.Equippable.AllVerbs.Except(comp.ActiveVerb).Where(x => x.verbProps?.defaultProjectile != null).ToList();
				var baseHeight = 85f;
				var height = otherVerbs.Count * baseHeight;
				var rect = new Rect(topLeft.x, topLeft.y - height, this.GetWidth(maxWidth), height);
				for (var i = 0; i < otherVerbs.Count; i++)
				{
					var otherVerb = otherVerbs[i];
					var command = new Command_Action
					{
						icon = otherVerb.verbProps.defaultProjectile.uiIcon,
						defaultLabel = otherVerb.verbProps.label,
						action = () =>
						{
							comp.SwitchFireMode(comp.Equippable.AllVerbs.IndexOf(otherVerb));
							comp.drawMenu = false;
						}
					};
					var state = command.GizmoOnGUI(new Vector2(rect.x, rect.y + (i * baseHeight)), maxWidth, parms);
					if (state.State == GizmoState.Interacted)
                    {
						command.ProcessInput(state.InteractEvent);
					}
				}
			}
			return result;
        }

        // Token: 0x04000007 RID: 7
        public VerbProperties verbProps;

		// Token: 0x04000008 RID: 8
		public Thing pawn;

		public CompToggleFireMode comp;
	}
}
