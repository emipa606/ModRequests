using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using System.Reflection.Emit;
using System.Reflection;
using UnityEngine.UI;

namespace DTechprinting
{
	public class Patch_ResearchNodeDraw_Transpiler
	{

		/* Code to delete:
		if (flag)
				{
					Text.Anchor = TextAnchor.UpperRight;
					Text.Font = ((this.Research.CostApparent > 1000000f) ? GameFont.Tiny : GameFont.Small);
		// -----------------------------------------------------------------------------------------------------------------------------------------------
					Widgets.Label(base.CostLabelRect, this.Research.CostApparent.ToStringByStyle(ToStringStyle.Integer, ToStringNumberSense.Absolute));
					GUI.DrawTexture(base.CostIconRect, (!this.Completed && !this.Available) ? Assets.Lock : Assets.ResearchIcon, ScaleMode.ScaleToFit);
		// -----------------------------------------------------------------------------------------------------------------------------------------------
				}
				Text.WordWrap = true;
		 */

		// Code to replace in IL (between dashes):

		///* 0x0000169E 6FA900000A   */ IL_0182: callvirt  instance float32 ['Assembly-CSharp']Verse.ResearchProjectDef::get_CostApparent()
		///* 0x000016A3 2200247449   */ IL_0187: ldc.r4    1000000
		///* 0x000016A8 3003         */ IL_018C: bgt.s     IL_0191

		///* 0x000016AA 17           */ IL_018E: ldc.i4.1
		///* 0x000016AB 2B01         */ IL_018F: br.s      IL_0192

		///* 0x000016AD 16           */ IL_0191: ldc.i4.0

		///* 0x000016AE 28A700000A   */ IL_0192: call      void ['Assembly-CSharp']Verse.Text::set_Font(valuetype ['Assembly-CSharp']Verse.GameFont)
		// -----------------------------------------------------------------------------------------------------------------------------------------------
		///* 0x000016B3 02           */ IL_0197: ldarg.0
		///* 0x000016B4 2876000006   */ IL_0198: call      instance valuetype [UnityEngine.CoreModule]UnityEngine.Rect FluffyResearchTree.Node::get_CostLabelRect()
		///* 0x000016B9 02           */ IL_019D: ldarg.0
		///* 0x000016BA 7B25000004   */ IL_019E: ldfld     class ['Assembly-CSharp']Verse.ResearchProjectDef FluffyResearchTree.ResearchNode::Research
		///* 0x000016BF 6FA900000A   */ IL_01A3: callvirt  instance float32 ['Assembly-CSharp']Verse.ResearchProjectDef::get_CostApparent()
		///* 0x000016C4 16           */ IL_01A8: ldc.i4.0
		///* 0x000016C5 17           */ IL_01A9: ldc.i4.1
		///* 0x000016C6 28AA00000A   */ IL_01AA: call      string ['Assembly-CSharp']Verse.GenText::ToStringByStyle(float32, valuetype ['Assembly-CSharp']Verse.ToStringStyle, valuetype ['Assembly-CSharp']Verse.ToStringNumberSense)
		///* 0x000016CB 28AB00000A   */ IL_01AF: call      void ['Assembly-CSharp']Verse.Widgets::Label(valuetype [UnityEngine.CoreModule]UnityEngine.Rect, string)
		///* 0x000016D0 02           */ IL_01B4: ldarg.0
		///* 0x000016D1 2875000006   */ IL_01B5: call      instance valuetype [UnityEngine.CoreModule]UnityEngine.Rect FluffyResearchTree.Node::get_CostIconRect()
		///* 0x000016D6 02           */ IL_01BA: ldarg.0
		///* 0x000016D7 6F89000006   */ IL_01BB: callvirt  instance bool FluffyResearchTree.Node::get_Completed()
		///* 0x000016DC 2D08         */ IL_01C0: brtrue.s  IL_01CA

		///* 0x000016DE 02           */ IL_01C2: ldarg.0
		///* 0x000016DF 6F8A000006   */ IL_01C3: callvirt  instance bool FluffyResearchTree.Node::get_Available()
		///* 0x000016E4 2C07         */ IL_01C8: brfalse.s IL_01D1

		///* 0x000016E6 7E05000004   */ IL_01CA: ldsfld    class [UnityEngine.CoreModule]UnityEngine.Texture2D FluffyResearchTree.Assets::ResearchIcon
		///* 0x000016EB 2B05         */ IL_01CF: br.s      IL_01D6

		///* 0x000016ED 7E07000004   */ IL_01D1: ldsfld    class [UnityEngine.CoreModule]UnityEngine.Texture2D FluffyResearchTree.Assets::Lock

		///* 0x000016F2 18           */ IL_01D6: ldc.i4.2
		///* 0x000016F3 283400000A   */ IL_01D7: call      void [UnityEngine.IMGUIModule]UnityEngine.GUI::DrawTexture(valuetype [UnityEngine.CoreModule]UnityEngine.Rect, class [UnityEngine.CoreModule]UnityEngine.Texture, valuetype [UnityEngine.IMGUIModule]UnityEngine.ScaleMode)

		// -----------------------------------------------------------------------------------------------------------------------------------------------
		///* 0x000016F8 17           */ IL_01DC: ldc.i4.1
		///* 0x000016F9 28A600000A   */ IL_01DD: call      void ['Assembly-CSharp']Verse.Text::set_WordWrap(bool)



		private const int startOffset = 7;
		private const int endOffset = -2;

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var found = false;
			int startIndex = -1, endIndex = -1;
			var codes = new List<CodeInstruction>(instructions);
			for (int i = 0; i < codes.Count; i++)
			{
				if (startIndex == -1 && codes[i].opcode == OpCodes.Callvirt)
				{
					var type = codes[i].operand as System.Reflection.MethodInfo;
					if (type != null)
					{
						if (type.Name == "get_CostApparent")
						{
							startIndex = i + startOffset;
						}
					}
				}
				else if (startIndex > -1 && codes[i].opcode == OpCodes.Call)
				{
					var type = codes[i].operand as System.Reflection.MethodInfo;
					if (type != null)
					{
						if (type.Name == "set_WordWrap")
						{
							endIndex = i + endOffset;
							found = true;
							break;
						}
					}

				}
			}
			if (found)
			{
				CodeInstruction loadThis = new CodeInstruction(OpCodes.Ldarg_0);
				MethodInfo drawlabel = AccessTools.Method(typeof(PatchResearchPalBase), nameof(PatchResearchPalBase.DrawLabel));
				CodeInstruction tpAvailable = new CodeInstruction(OpCodes.Call, drawlabel);

				codes[startIndex] = loadThis;
				codes[startIndex + 1] = tpAvailable;
				for (var x = startIndex + 2; x <= endIndex; x++)
				{
					codes[x].opcode = OpCodes.Nop;
				}
			}
			return codes.AsEnumerable();
		}

	}
}
