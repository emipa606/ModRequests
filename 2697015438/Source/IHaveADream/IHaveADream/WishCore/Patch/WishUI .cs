using RimWorld;
using HarmonyLib;
using Verse;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;

namespace HDream
{

	[HarmonyPatch(typeof(NeedsCardUtility), "DrawThoughtListing")]
	public static class AddWishListing
	{
		public const float rectWidth = 280f;

		public const float titleOffsetX = 5f;
		public const float titleHeight = 30f;
		public const float blockOffset = 10f;
		public const float rectOffsetY = 24f;
		public static float offsetY = 0;
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var codes = new List<CodeInstruction>(instructions);
			MethodInfo m_MyExtraMethod;
			bool foundHeigthPart = false;

			for (int i = 0; i < codes.Count; i++)
			{
				yield return codes[i];
				if (codes[i].opcode == OpCodes.Ldsfld && codes[i].operand.ToString().Contains("thoughtGroupsPresent")) foundHeigthPart = true;
				if (foundHeigthPart && codes[i].opcode == OpCodes.Stloc_0)
				{
					m_MyExtraMethod = SymbolExtensions.GetMethodInfo(() => ResizeScroll(null, -1));
					yield return new CodeInstruction(OpCodes.Ldarg_1);
					yield return new CodeInstruction(OpCodes.Ldloc_0);
					yield return new CodeInstruction(OpCodes.Call, m_MyExtraMethod);
					yield return new CodeInstruction(OpCodes.Stloc_0);
					foundHeigthPart = false;
					
					for (int j = i + 1; j < codes.Count; j++)
					{
						yield return codes[j];
						if (codes[j].opcode == OpCodes.Call && codes[j].operand.ToString().Contains("BeginScrollView"))
						{
							m_MyExtraMethod = SymbolExtensions.GetMethodInfo(() => AddTitleAndWish(new Rect(), null));
							yield return new CodeInstruction(OpCodes.Ldarg_0);
							yield return new CodeInstruction(OpCodes.Ldarg_1);
							yield return new CodeInstruction(OpCodes.Call, m_MyExtraMethod);
							for (int k = j + 1; k < codes.Count; k++) yield return codes[k];
							break;
						}
					}
					break;
				}
			}
		}

		public static float ResizeScroll(Pawn pawn, float height)
		{
			if (!WishUtility.CanHaveWish(pawn) || pawn.wishes() == null) return height;
			height += titleHeight * 2 + blockOffset;
			if (!pawn.wishes().wishes.NullOrEmpty()) height += (float)pawn.wishes().wishes.Count * rectOffsetY;
			else height += rectOffsetY;
			return height;
		}
		public static void AddTitleAndWish(Rect listingRect, Pawn pawn)
		{
			if (!WishUtility.CanHaveWish(pawn) || pawn.wishes() == null) return;
			offsetY = 0f;
			AddTitle(new Rect(titleOffsetX, offsetY, listingRect.width, titleHeight), TranslationKey.WISH_TITLE.Translate());

			Text.Anchor = TextAnchor.MiddleLeft;
			if (pawn.wishes().wishes.NullOrEmpty())
			{
				GUI.color = Color.gray;
				Widgets.Label(new Rect(15f, offsetY, listingRect.width - 21f, 20f), TranslationKey.WISH_NONE.Translate());
				GUI.color = Color.white;
				offsetY += rectOffsetY;
			}
			else
			{
				for (int i = 0; i < pawn.wishes().wishes.Count; i++)
				{
					if (DrawWish(new Rect(0f, offsetY, listingRect.width - 16f, 20f), pawn.wishes().wishes[i], pawn))
					{
						offsetY += rectOffsetY;
					}
				}

			}

			offsetY += blockOffset;
			AddTitle(new Rect(titleOffsetX, offsetY, listingRect.width, titleHeight), TranslationKey.MOOD_TITLE.Translate());

			Text.Anchor = TextAnchor.UpperLeft;
		}
		
		public static void Postfix()
		{
			offsetY = 0f;
		}
		private static void AddTitle(Rect rect, string title)
		{
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.Label(new Rect(rect.x, rect.y, rectWidth, rect.height), title);
			Text.Font = GameFont.Small;
			offsetY += titleHeight;
		}

		private static bool DrawWish(Rect rect, Wish wish, Pawn pawn)
		{
			try
			{
				if (Mouse.IsOver(rect))
				{
					Widgets.DrawHighlight(rect);
					if (Input.GetMouseButtonUp(1))
					{
						Find.LetterStack.LettersListForReading.RemoveAll((Letter l)=> (l as ChoiceLetter_Wish)?.wish == wish);
						ChoiceLetter_Wish letter = (ChoiceLetter_Wish)LetterMaker.MakeLetter(TranslationKey.RECEIVE_WISH_LETTER.Translate(WishUtility.Def.tierSingular[wish.TierIndex], pawn.LabelShort, wish.LabelCap),
													WishDescription(wish),
													HDLetterDefOf.Wish,
													wish.pawn);
						letter.wish = wish;
						Find.LetterStack.ReceiveLetter(letter);
					}
				}
				if (Mouse.IsOver(rect))
				{

					TooltipHandler.TipRegion(rect, new TipSignal(WishDescription(wish), 7291));
				}	
				
				Text.WordWrap = false;
				Text.Anchor = TextAnchor.MiddleLeft;
				Rect rect2 = new Rect(rect.x + 10f, rect.y, rectWidth, rect.height);
				rect2.yMin -= 3f;
				rect2.yMax += 3f;
				string text = wish.LabelCap;

				Widgets.Label(rect2, text);
				Text.Anchor = TextAnchor.MiddleCenter;
				Text.WordWrap = true;
			}
			catch (Exception ex)
			{
				Log.ErrorOnce(string.Concat("HDream : Exception in DrawThoughtGroup for ", wish.def, " on ", pawn, ": ", ex.ToString()), 3452698);
			}
			return true;
		}

		private static string WishDescription(Wish wish)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(wish.DescriptionTitle);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			stringBuilder.Append(wish.DescriptionToFulfill);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			stringBuilder.Append(wish.DescriptionTime);

			return stringBuilder.ToString();
		}
	}


	[HarmonyPatch(typeof(NeedsCardUtility), "DrawThoughtGroup")]
	public static class OffsetThougth
	{
		public static void Prefix(ref Rect rect)
		{
			rect.y += AddWishListing.offsetY;
		}
	}
}
