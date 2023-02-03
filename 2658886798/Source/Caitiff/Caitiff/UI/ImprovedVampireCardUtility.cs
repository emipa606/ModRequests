using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Vampire
{

	public class ImprovedVampireCardUtility : VampireCardUtility
	{

		public static List<VitaeAbilityDef> orderedDisciplines(List<VitaeAbilityDef> abilityL)
		{
			int count = 0;
			SortedDictionary<string, VitaeAbilityDef> disciplines = new SortedDictionary<string, VitaeAbilityDef>();
			List<VitaeAbilityDef> returnDisciplines = new List<VitaeAbilityDef>();

			while (count < abilityL.Count)
			{
				disciplines.Add(abilityL[count].ToString(), abilityL[count]);
				count++;
			}
			disciplines.OrderBy(key => abilityL[0].ToString());
			count = 0;
			while (count < disciplines.Count)
			{
				returnDisciplines.Add(disciplines[count.ToString()]);
				count++;
			}
			return returnDisciplines;
		}

		public static List<VitaeAbilityDef> ordered = new List<VitaeAbilityDef>();

		#region PowersGUI


		public static void PowersGUIHandler(Rect inRect, CompVampire compVampire, Discipline discipline)
		{
			float buttonXOffset = inRect.x;
			if (discipline?.Def?.abilities is List<VitaeAbilityDef> abilities && !abilities.NullOrEmpty())
			{
				ordered = discipline.Def.abilities;
				ordered.OrderBy(key => ordered[ordered.Count - 1]);
				Rect rectLabel = new Rect(inRect.x, inRect.y, inRect.width * 0.7f, TextSize);
				Text.Font = GameFont.Small;
				Widgets.Label(rectLabel, discipline.Def.LabelCap);
				Text.Font = GameFont.Small;
				int count = 0;
				int pntCount = 0;

				foreach (VitaeAbilityDef ability in ordered)
				{

					Rect buttonRect = new Rect(buttonXOffset, rectLabel.yMax, VampButtonSize, VampButtonSize);
					TooltipHandler.TipRegion(buttonRect, () => ability.LabelCap + "\n\n" + ability.description, 398452); //"\n\n" + "PJ_CheckStarsForMoreInfo".Translate()


					Texture2D abilityTex = ability.uiIcon;
					bool disabledForGhouls =
						compVampire.IsGhoul && (int)compVampire.GhoulHediff.ghoulPower < discipline.Level;
					if (disabledForGhouls)
						GUI.color = Color.gray;
					if (compVampire.AbilityPoints == 0 || discipline.Level >= 4)
					{
						Widgets.DrawTextureFitted(buttonRect, abilityTex, 1.0f);
					}
					else if (Widgets.ButtonImage(buttonRect, abilityTex) && compVampire.AbilityUser.Faction == Faction.OfPlayer)
					{
						if (compVampire.AbilityUser.story != null && compVampire.AbilityUser.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Violent) && ability.MainVerb.isViolent)
						{
							Messages.Message("IsIncapableOfViolenceLower".Translate(new object[]
							{
							compVampire.parent.LabelShort
							}), MessageTypeDefOf.RejectInput);
							return;
						}
						if (disabledForGhouls)
						{
							Messages.Message("ROMV_DomitorVitaeIsTooWeak".Translate(new object[]
							{
								compVampire.parent.LabelShort
							}), MessageTypeDefOf.RejectInput);
							return;
						}

						discipline.Notify_PointsInvested(1); //LevelUpPower(power);
						compVampire.Notify_UpdateAbilities();
						compVampire.AbilityPoints -= 1; //powerDef.abilityPoints;
					}

					float drawXOffset = buttonXOffset;
					float modifier = 0f;
					switch (count)
					{
						case 0: break;
						case 1: modifier = 0.75f; break;
						case 2: modifier = 0.72f; break;
						case 3: modifier = 0.60f; break;
					}
					if (count != 0) drawXOffset -= VampButtonPointSize * count * modifier;
					else drawXOffset -= 2;

					for (int j = 0; j < count + 1; j++)
					{

						++pntCount;
						float drawYOffset = VampButtonSize + TextSize + Padding;
						Rect powerRegion = new Rect(inRect.x + drawXOffset + VampButtonPointSize * j, inRect.y + drawYOffset, VampButtonPointSize, VampButtonPointSize);
						if (discipline.Points >= pntCount)
						{
							Widgets.DrawTextureFitted(powerRegion, TexButton.ROMV_PointFull, 1.0f);
						}
						else
						{
							Widgets.DrawTextureFitted(powerRegion, TexButton.ROMV_PointEmpty, 1.0f);
						}

						TooltipHandler.TipRegion(powerRegion, () => ability.GetDescription() + "\n" + compVampire.PostAbilityVerbCompDesc(ability.MainVerb), 398462);
					}
					++count;
					buttonXOffset += ButtonSize * 3f + Padding;
					if (disabledForGhouls)
						GUI.color = Color.white;
				}
			}
		}
		#endregion PowersGUI
	}
}
