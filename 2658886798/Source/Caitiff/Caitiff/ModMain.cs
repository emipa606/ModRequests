using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vampire
{
	using UnityEngine;
	using Verse;

	namespace Vampire
	{

		public class ModMain : Mod
		{
			public ModMain(ModContentPack content) : base(content)
			{
			}

			public override string SettingsCategory() => "Rim of Madness - Improved Vampires";

			public override void DoSettingsWindowContents(Rect inRect)
			{
				if (Find.World?.GetComponent<WorldComponent_Improved_VampireSettings>() is WorldComponent_Improved_VampireSettings modSettings)
				{
					if (Widgets.ButtonText(new Rect(inRect.x, inRect.y, inRect.width * 0.25f, inRect.height * 0.15f), "ROMVC_VampireSettingsButton".Translate()))
					{
						Find.WindowStack.Add(new Dialog_Improved_VampireSetup());
					}
				}
				else
				{
					Widgets.Label(inRect, "ROMVC_VampireSettingsUnavailable".Translate());
				}
			}
		}
	}
}
