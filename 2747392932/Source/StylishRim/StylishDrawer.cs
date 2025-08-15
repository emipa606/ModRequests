using RimWorld;
using System.Reflection;
using UnityEngine;
using Verse;
using static AlienRace.AlienPartGenerator;

namespace StylishRim
{
	public static class StylishDrawer
	{
		/*
		 * body or head 
		body north or head y	= 0.0231660213
		body or head north y	= 0.02027027
			body apparel y		+= 0.008687258f + (0.00289575267f)x
			body tatoo y		+= 0.00144787633

		head tatoo y			= 0.0241660213
		head tatoo north y		= 0.0221660213
		head part y				= 0.0289575271
			beard y				+= 0
			hair y				+= 0
			apparel front y		+= 0
		apparel behind head y	= 0.0221660212
		apparel head y			= 0.03185328
		apparel head north y	= 0.00289575267
		 */
		public static PawnStyleSet spss = null;
		public static Pawn pawn = null;
		public static Vector3 AdjustHeadOffset(Vector3 headOffset, Pawn pawn, bool isPortrait, Rot4 bodyFacing)
		{
			// 頭の位置調整
			// 大体苦戦してるのはこいつ。
			// やっぱり種族ごとに基準値バラバラである程度投げざるを得ない
			spss = PawnStyleSet.Get(pawn);
			if (spss == null) return headOffset;
			headOffset.x *= spss.Calc.multHeadOffset.x;
			headOffset.z *= isPortrait ? spss.Calc.multPortraitHeadOffset.y : spss.Calc.multHeadOffset.y;
			headOffset += spss.Calc.AddHeadOffsetByRot(bodyFacing, isPortrait, pawn?.story?.bodyType.defName);
			/*
			if (bodyFacing.IsHorizontal)
			{
				if (Rot4.East == bodyFacing) headOffset.x += isPortrait ? spss.Calc.AddPortraitHeadOffset.x : spss.Calc.AddHeadOffset.x;
				else headOffset.x -= isPortrait ? spss.Calc.AddPortraitHeadOffset.x : spss.Calc.AddHeadOffset.x;
			}
			headOffset.z += isPortrait ? spss.Calc.AddPortraitHeadOffset.y : spss.Calc.AddHeadOffset.y;
			*/
			return headOffset;
		}
		public static void DrawAdjustHead(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow, Rot4 bodyFacing)
		{
			//
			if (spss != null)
			{
				GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quat) * Matrix4x4.Scale(spss.Calc.HeadSizeByRot(bodyFacing)), mat, drawNow);
			}
			else
			{
				GenDraw.DrawMeshNowOrLater(mesh, loc, quat, mat, drawNow);
			}
		}
		public static void DrawAdjustApparelHead(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow, ApparelGraphicRecord agr, Rot4 headFacing, PawnRenderFlags flags)
		{
			DrawAdjustApparelHeadBody(mesh, loc, quat, mat, drawNow, agr.sourceApparel, headFacing);
		}
		public static void DrawAdjustApparelHeadBody(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow, Apparel apparel, Rot4 headFacing)
		{
			if (apparel.def.apparel.LastLayer == ApparelLayerDefOf.EyeCover)
			{
				loc.y -= 0.0015f;
			}
			else if (apparel.def.apparel.forceRenderUnderHair)
			{
				loc.y -= 0.0005f;
			}
			else
			{
				loc.y += 0.001f;
			}
			if (spss != null)
			{
				StylishSet ass = spss.GetApparel(apparel, true);
				if (ass != null)
				{
					if (ass.hide) return;
					GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quat * ass.AngleQuatByRot(headFacing)) * Matrix4x4.Translate(ass.OffsetByRot(headFacing)) * Matrix4x4.Scale(ass.SizeByRot(headFacing)), mat, drawNow);
				}
				else
				{
					GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quat) * Matrix4x4.Scale(spss.Calc.HeadSizeByRot(headFacing)), mat, drawNow);
				}
			}
			else
			{
				GenDraw.DrawMeshNowOrLater(mesh, loc, quat, mat, drawNow);
			}
		}
		public static void DrawAdjustHair(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow, Rot4 headFacing, PawnRenderFlags flags)
		{
			loc.y -= 0.001f;
			if (spss != null)
			{
				DrawAdjustHeadHair(mesh, loc, quat, mat, drawNow, headFacing, flags, spss.GetHair(pawn.story.hairDef.modContentPack.Name));
			}
			else
			{
				GenDraw.DrawMeshNowOrLater(mesh, loc, quat, mat, drawNow);
			}
		}
		public static void DrawAdjustBeard(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow, Rot4 headFacing, PawnRenderFlags flags)
		{
			loc.y -= 0.0015f;
			if (spss != null)
			{
				DrawAdjustHeadHair(mesh, loc, quat, mat, drawNow, headFacing, flags, spss.GetHair(pawn.style.beardDef.modContentPack.Name));
			}
			else
			{
				GenDraw.DrawMeshNowOrLater(mesh, loc, quat, mat, drawNow);
			}
		}
		public static void DrawAdjustFaceTatoo(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow, Rot4 headFacing, PawnRenderFlags flags)
		{
			loc.y -= 0.001f;
			if (spss != null)
			{
				GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quat) * Matrix4x4.Scale(spss.Calc.HeadSizeByRot(headFacing)), mat, drawNow);
			}
			else
			{
				GenDraw.DrawMeshNowOrLater(mesh, loc, quat, mat, drawNow);
			}
		}
		public static void DrawAdjustHeadHair(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow, Rot4 headFacing, PawnRenderFlags flags, StylishSet ss)
		{
			if (ss != null)
			{
				/*if (pawn.story.headType.narrow)
				{
					GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quat * ss.AngleQuatByRot(headFacing)) * Matrix4x4.Translate(ss.OffsetByRot(headFacing)) * Matrix4x4.Scale(ss.SizeByRot(headFacing).ToNarrow()), mat, drawNow);
				}
				else
				{
					GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quat * ss.AngleQuatByRot(headFacing)) * Matrix4x4.Translate(ss.OffsetByRot(headFacing)) * Matrix4x4.Scale(ss.SizeByRot(headFacing)), mat, drawNow);
				}*/
				GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quat * ss.AngleQuatByRot(headFacing)) * Matrix4x4.Translate(ss.OffsetByRot(headFacing)) * Matrix4x4.Scale(ss.SizeByRot(headFacing)), mat, drawNow);
			}
			else
			{
				/*if (pawn.story.headType.narrow)
				{
					GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quat) * Matrix4x4.Scale(spss.Calc.HeadSizeByRot(headFacing).ToNarrow()), mat, drawNow);
				}
                else
				{
					GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quat) * Matrix4x4.Scale(spss.Calc.HeadSizeByRot(headFacing)), mat, drawNow);
				}*/
				GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quat) * Matrix4x4.Scale(spss.Calc.HeadSizeByRot(headFacing)), mat, drawNow);
			}
		}
		public static void DrawAdjustHeadExtraEyes(Mesh mesh, Matrix4x4 matrix, Material mat, bool drawNow, Rot4 headFacing)
		{
			if (spss != null)
			{
				GenDraw.DrawMeshNowOrLater(mesh, matrix * Matrix4x4.Scale(spss.Calc.HeadSizeByRot(headFacing)), mat, drawNow);
			}
			else
			{
				GenDraw.DrawMeshNowOrLater(mesh, matrix, mat, drawNow);
			}
		}
		public static void DrawAdjustHeadGenes(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow, Rot4 headFacing, GeneGraphicRecord geneRecord)
		{
			if (spss != null)
			{
				StylishSet ss = spss.GetGene(geneRecord.sourceGene.def.defName);
				if (ss != null)
				{
					if (ss.hide) return;
					GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quat * ss.AngleQuatByRot(headFacing)) * Matrix4x4.Translate(ss.OffsetByRot(headFacing)) * Matrix4x4.Scale(spss.Calc.HeadSizeByRot(headFacing)) * Matrix4x4.Scale(ss.SizeByRot(headFacing)), mat, drawNow);
				}
                else
				{
					GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quat) * Matrix4x4.Scale(spss.Calc.HeadSizeByRot(headFacing)), mat, drawNow);
				}
			}
			else
			{
				GenDraw.DrawMeshNowOrLater(mesh, loc, quat, mat, drawNow);
			}
		}
		public static Vector3 ToNarrow(this Vector3 v3)
		{
			return new Vector3(v3.x / 1.5f * 1.3f, v3.y, v3.z);
		}


		public static bool isDrawPawnBody = false;
		public static void ApparelAdjusterInit(Pawn pawn, bool drawClothes)
		{
			//if (!isDrawPawnBody) return;
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pss != null)
			{
				pss.ApparelAdjusterInit(pawn.ThingID);
			}
		}
		public static ApparelGraphicRecord AddApparelAdjuster(ApparelGraphicRecord agr, Pawn pawn)
		{
			//if (!isDrawPawnBody) return agr;
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pss != null)
			{
				if (pss.Calc.apparels != null)
				{
					pss.AddApparelAdjuster(pawn.ThingID, agr.sourceApparel);
				}
			}
			return agr;
		}
		public static void DrawAdjustApparel(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow, Pawn pawn, Rot4 bodyFacing, PawnRenderFlags flags)
		{
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pss != null)
			{
				if (flags.FlagSet(PawnRenderFlags.Clothes))
				{
					StylishSet ass = pss.GetAdjuster(pawn.ThingID);
					if (pss.CountUp(pawn.ThingID) == 0)
					{
						pss.locY = loc.y;
						GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quat * pss.Calc.ChangeBodyAngleQuatByRot(bodyFacing)) * Matrix4x4.Translate(pss.Calc.ChangeBodyOffsetByRot(bodyFacing)) * Matrix4x4.Scale(pss.Calc.ChangeBodySizeByRot(bodyFacing)), mat, drawNow);
					}
					else
					{
						if (ass != null)
						{
							if (ass.hide) return;
							GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.Translate(ass.ChangePriority ? pss.ToBodyLocY(loc) : loc) * Matrix4x4.Rotate(quat * ass.AngleQuatByRot(bodyFacing)) * Matrix4x4.Translate(ass.OffsetByRot(bodyFacing)) * Matrix4x4.Scale(ass.SizeByRot(bodyFacing)), mat, drawNow);
						}
						else
						{
							GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quat) * Matrix4x4.Scale(pss.Calc.BodySizeByRot(bodyFacing)), mat, drawNow);
						}
					}
				}
				else
				{
					GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quat * pss.Calc.ChangeBodyAngleQuatByRot(bodyFacing)) * Matrix4x4.Translate(pss.Calc.ChangeBodyOffsetByRot(bodyFacing)) * Matrix4x4.Scale(pss.Calc.ChangeBodySizeByRot(bodyFacing)), mat, drawNow);
				}
			}
			else
			{
				GenDraw.DrawMeshNowOrLater(mesh, loc, quat, mat, drawNow);
			}
		}
		public static void DrawAdjustApparelShell(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow, Pawn pawn, ApparelGraphicRecord agr, Rot4 bodyFacing)
		{
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pss != null)
			{
				StylishSet ass = pss.GetApparel(agr.sourceApparel);
				if (ass != null)
				{
					if (ass.hide) return;
					GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quat * ass.AngleQuatByRot(bodyFacing)) * Matrix4x4.Translate(ass.OffsetByRot(bodyFacing)) * Matrix4x4.Scale(ass.SizeByRot(bodyFacing)), mat, drawNow);
					return;
				}
				else
				{
					GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quat) * Matrix4x4.Scale(pss.Calc.BodySizeByRot(bodyFacing)), mat, drawNow);
				}
			}
			else
			{
				GenDraw.DrawMeshNowOrLater(mesh, loc, quat, mat, drawNow);
			}
		}
		public static void DrawAdjustBelt(Mesh mesh, Matrix4x4 matrix, Material mat, bool drawNow, Pawn pawn, ApparelGraphicRecord agr, Rot4 bodyFacing)
		{
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			Matrix4x4 m4 = matrix;
			if (pss != null)
			{
				StylishSet ass = pss.GetApparel(agr.sourceApparel);
				if (ass != null)
				{
					if (ass.hide) return;
					m4 *= Matrix4x4.Rotate(ass.AngleQuatByRot(bodyFacing)) * Matrix4x4.Translate(ass.OffsetByRot(bodyFacing)) * Matrix4x4.Scale(ass.SizeByRot(bodyFacing));
				}
				if (!pss.disableUtilityAdjust) m4 *= Matrix4x4.Scale(pss.Calc.multUtilitySize);
			}
			GenDraw.DrawMeshNowOrLater(mesh, m4, mat, drawNow);
		}
		public static void DrawAdjustBodyTatoo(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow, Pawn pawn, Rot4 bodyFacing, PawnRenderFlags flags)
		{
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pss != null)
			{
				GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quat) * Matrix4x4.Scale(pss.Calc.BodySizeByRot(bodyFacing)), mat, drawNow);
			}
			else
			{
				GenDraw.DrawMeshNowOrLater(mesh, loc, quat, mat, drawNow);
			}
		}
		public static void DrawAdjustBodyGenes(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow, Pawn pawn, Rot4 bodyFacing, PawnRenderFlags flags, GeneGraphicRecord geneRecord)
		{
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pss != null)
			{
				StylishSet ss = pss.GetGene(geneRecord.sourceGene.def.defName);
				if (ss != null)
				{
					if (ss.hide) return;
					GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quat * ss.AngleQuatByRot(bodyFacing)) * Matrix4x4.Translate(ss.OffsetByRot(bodyFacing)) * Matrix4x4.Scale(pss.Calc.BodySizeByRot(bodyFacing)) * Matrix4x4.Scale(ss.SizeByRot(bodyFacing)), mat, drawNow);
				}
                else
				{
					GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quat) * Matrix4x4.Scale(pss.Calc.BodySizeByRot(bodyFacing)), mat, drawNow);
				}
			}
			else
			{
				GenDraw.DrawMeshNowOrLater(mesh, loc, quat, mat, drawNow);
			}
		}
		public static void DrawAdjustPawnFur(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow, Pawn pawn, Rot4 bodyFacing, PawnRenderFlags flags)
		{
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pss != null)
			{
				GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quat) * Matrix4x4.Scale(pss.Calc.BodySizeByRot(bodyFacing)), mat, drawNow);
			}
			else
			{
				GenDraw.DrawMeshNowOrLater(mesh, loc, quat, mat, drawNow);
			}
		}
		public static void DrawAdjustAddons(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow)
		{
			if (HarmonyPatch_AlienRace.pss != null)
			{
				if (HarmonyPatch_AlienRace.sa != null && HarmonyPatch_AlienRace.sa.Count > HarmonyPatch_AlienRace.addonIndex)
				{
					StylishAddon sa = HarmonyPatch_AlienRace.sa[HarmonyPatch_AlienRace.addonIndex];
					if (sa.hide) return;
					GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quat * sa.AngleQuatByRot(HarmonyPatch_AlienRace.rot)) * Matrix4x4.Translate(sa.OffsetByRot(HarmonyPatch_AlienRace.rot, HarmonyPatch_AlienRace.flags.FlagSet(PawnRenderFlags.Portrait))) * Matrix4x4.Scale(sa.SizeByRot(HarmonyPatch_AlienRace.rot)), mat, drawNow);
				}
                else
                {
					GenDraw.DrawMeshNowOrLater(mesh, Matrix4x4.Translate(loc) * Matrix4x4.Rotate(quat) * (HarmonyPatch_AlienRace.ba.alignWithHead ? Matrix4x4.Scale(HarmonyPatch_AlienRace.pss.Calc.BodySizeByRot(HarmonyPatch_AlienRace.rot)) : Matrix4x4.Scale(HarmonyPatch_AlienRace.pss.Calc.HeadSizeByRot(HarmonyPatch_AlienRace.rot))), mat, drawNow);
				}
			}
			else
			{
				GenDraw.DrawMeshNowOrLater(mesh, loc, quat, mat, drawNow);
			}
			HarmonyPatch_AlienRace.addonIndex = 99;
			HarmonyPatch_AlienRace.ba = null;
		}

		/*
		 * 
			// アドオンのサイズ調整
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pss != null)
			{
				if (HasPart(ba, REG_HAIR))
				{
					drawSize *= pss.ShowHair ? Vector2.zero : rot.IsHorizontal ? pss.Calc.HeadAddonSizeV2ByRot(rot).Rotated() : pss.Calc.HeadAddonSizeV2ByRot(rot);
				}
				else if(HasPart(ba, REG_HEAD_PARTS) || HasPartBySomeRace(pawn, ba))
				{
					drawSize *= rot.IsHorizontal ? pss.Calc.HeadAddonSizeV2ByRot(rot).Rotated() : pss.Calc.HeadAddonSizeV2ByRot(rot);
				}
				else
				{
					drawSize *= rot.IsHorizontal ? pss.Calc.BodyAddonSizeV2ByRot(rot).Rotated() : pss.Calc.BodyAddonSizeV2ByRot(rot);
				}
			}
			return drawSize;
		*/
		static Rot4 rot;
		static bool aiming;
		static bool isMelee;
		static StylishSet ess;
		static StylishSet com;

		static bool isOffHand = false;
		public static bool isDynamicPartAdjust => com != null;
		public static void DynamicPartStloc(Pawn pawn, Rot4 rotation)
		{
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pss != null)
			{
				ess = pss.GetEquipment(pawn.equipment?.Primary);
				com = pss.GetEquipmentCommon;
				rot = rotation;
				isMelee = pawn.equipment?.Primary?.def.IsMeleeWeapon ?? false;
				aiming = (pawn.stances?.curStance as Stance_Busy)?.focusTarg.IsValid ?? false;
			}
		}
		public static bool DynamicPartDualWield(bool isAiming, Pawn pawn, ThingWithComps eq)
		{
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pss != null)
			{
				ess = pss.GetEquipment(eq);
				aiming = isAiming;
				isMelee = eq?.def.IsMeleeWeapon ?? false;
				isOffHand = true;
			}
			return isAiming;
		}
		public static void DynamicPartPostfix()
		{
			aiming = false;
			ess = null;
			com = null;
			isOffHand = false;
			isMelee = false;
		}
		public static void DrawAdjustDynamicPartWithScale(Mesh mesh, Vector3 position, Quaternion rotation, Vector3 scale, Material material, int layer)
		{
			if (isDynamicPartAdjust)
			{
				DrawAdjustDynamicPartBody(mesh, position, rotation, material, layer, scale);
			}
			else
			{
				Graphics.DrawMesh(mesh, Matrix4x4.TRS(position, rotation, scale), material, layer);
			}
		}
		public static void DrawAdjustDynamicPart(Mesh mesh, Matrix4x4 matrix, Material material, int layer)
		{
			if (isDynamicPartAdjust)
			{
				DrawAdjustDynamicPartBody(mesh, matrix, material, layer);
			}
			else
			{
				Graphics.DrawMesh(mesh, matrix, material, layer);
			}
		}
		private static bool isFlip1_4 = false;
		public static Matrix4x4 DrawAdjustDynamicPart1_4(Vector3 position, Quaternion rotation, Vector3 scale)
		{
			if (isDynamicPartAdjust)
			{
				if (ess != null)
				{
					scale.Scale(ess.SizeByRot(rot));
					isFlip1_4 = (isMelee || !aiming) && ess.FlipByRot(rot);
					rotation *= aiming ? ess.UsageAngleQuatByRot(rot) : ess.AngleQuatByRot(rot);
					if (aiming && ess.useCommonWhenAiming) ess = com;
				}
				else
				{
					ess = com;
					isFlip1_4 = !aiming && ess.FlipByRot(rot);
					rotation *= aiming ? ess.UsageAngleQuatByRot(rot) : ess.AngleQuatByRot(rot);
				}
				return Matrix4x4.Translate(position + ess.OffsetOffHandByRot(rot)) * Matrix4x4.Rotate(isFlip1_4 ? rotation.Inverse() : rotation) * Matrix4x4.Scale(scale);
			}
			isFlip1_4 = false;
			return Matrix4x4.TRS(position, rotation, scale);
		}
		public static Mesh DrawAdjustDynamicPart1_4FlipMesh(Mesh mesh)
        {
			return isFlip1_4 ? Flip(mesh) : mesh;
        }

		/*
		public static void DrawAdjustDynamicPart_yayoAni(Mesh mesh, Vector3 position, Quaternion rotation, Material material, int layer, Pawn pawn)
		{
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pss != null)
			{
				if (ess == null) ess = pss.GetEquipment(pawn.equipment?.Primary);
				if (com == null) com = pss.GetEquipmentCommon;
				if (!aiming) aiming = (pawn.stances.curStance as Stance_Busy)?.focusTarg.IsValid ?? false;
				if (isDynamicPartAdjust)
				{
					isMelee = pawn.equipment?.Primary?.def.IsMeleeWeapon ?? false;
					DrawAdjustDynamicPartBody(mesh, position, rotation, material, layer);
					return;
				}
			}
			Graphics.DrawMesh(mesh, position, rotation, material, layer);
		}
		*/
		public static Matrix4x4 DrawAdjustDynamicPart_yayoAni(Vector3 position, Quaternion quat, Vector3 scale, Pawn pawn)
		{
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pss != null)
			{
				ess = pss.GetEquipment(pawn.equipment?.Primary);
				com = pss.GetEquipmentCommon;
				if (!aiming) aiming = (pawn.stances.curStance as Stance_Busy)?.focusTarg.IsValid ?? false;
				if (isDynamicPartAdjust)
				{
					isMelee = pawn.equipment?.Primary?.def.IsMeleeWeapon ?? false;
					return DrawAdjustDynamicPart1_4(position, quat, scale);
				}
			}
			return Matrix4x4.TRS(position, quat, scale);
		}
		public static void DrawAdjustDynamicPartOffHand(Mesh mesh, Vector3 position, Quaternion rotation, Material material, int layer)
		{
			if (isDynamicPartAdjust)
			{
				if (isOffHand)
				{
					Vector3 scale = Vector3.one;
					bool flip;
					if (ess != null)
					{
						scale.Scale(ess.SizeByRot(rot));
						flip = (isMelee || !aiming) && ess.FlipByRot(rot);
						rotation *= aiming ? ess.UsageAngleQuatOffHandByRot(rot) : ess.AngleQuatOffHandByRot(rot);
						if (aiming && ess.useCommonWhenAiming) ess = com;
					}
					else
					{
						ess = com;
						flip = !aiming && ess.FlipByRot(rot);
						rotation *= aiming ? ess.UsageAngleQuatOffHandByRot(rot) : ess.AngleQuatOffHandByRot(rot);
					}
					Graphics.DrawMesh(flip ? Flip(mesh) : mesh, Matrix4x4.Translate(position + ess.OffsetOffHandByRot(rot)) * Matrix4x4.Rotate(flip ? rotation.Inverse() : rotation) * Matrix4x4.Scale(scale), material, layer);
					isOffHand = false;
				}
				else
				{
					DrawAdjustDynamicPartBody(mesh, position, rotation, material, layer);
				}
			}
			else
			{
				Graphics.DrawMesh(mesh, position, rotation, material, layer);
			}
		}

		public static void DrawExtras1_Ratkin(Pawn pawn)
		{
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pss != null)
			{
				ess = pss.GetEquipmentExtra1;
				if (ess != null)
				{
					rot = pawn.Rotation;
					aiming = false;
				}
			}
		}
		public static void DrawExtras2_Ratkin(Pawn pawn)
		{
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pss != null)
			{
				ess = pss.GetEquipmentExtra2;
				if (ess != null)
				{
					rot = pawn.Rotation;
					aiming = false;
				}
			}
		}
		public static void DrawAdjustDynamicPart_Ratkin(Mesh mesh, Vector3 position, Quaternion rotation, Material material, int layer)
		{
			if (ess != null)
			{
				Vector3 size = ess.SizeByRot(rot);
				bool flip = !aiming && ess.FlipByRot(rot);
				Quaternion quat = rotation * ess.AngleQuatByRot(rot);
				Graphics.DrawMesh(flip ? Flip(mesh) : mesh, Matrix4x4.Translate(position + ess.OffsetByRotInvertPriorityEast(rot)) * Matrix4x4.Rotate(flip ? quat.Inverse() : quat) * Matrix4x4.Scale(size), material, layer);
			}
			else
			{
				Graphics.DrawMesh(mesh, position, rotation, material, layer);
			}
		}
		public static void DrawAdjustDynamicPartBody(Mesh mesh, Vector3 position, Quaternion rotation, Material material, int layer, Vector3 scale = default)
		{
/*
			if (scale == default) scale = Vector3.one;
			bool flip;
			if (ess != null)
			{
				scale.Scale(ess.SizeByRot(rot));
				flip = (isMelee || !aiming) && ess.FlipByRot(rot);
				rotation *= aiming ? ess.UsageAngleQuatByRot(rot) : ess.AngleQuatByRot(rot);
				if (aiming && ess.useCommonWhenAiming) ess = com;
			}
			else
			{
				ess = com;
				flip = !aiming && ess.FlipByRot(rot);
				rotation *= aiming ? ess.UsageAngleQuatByRot(rot) : ess.AngleQuatByRot(rot);
			}
			Graphics.DrawMesh(flip ? Flip(mesh) : mesh, Matrix4x4.Translate(position + ess.OffsetByRotInvertPriority(rot)) * Matrix4x4.Rotate(flip ? rotation.Inverse() : rotation) * Matrix4x4.Scale(scale), material, layer);
*/
			DrawAdjustDynamicPartBody(mesh, Matrix4x4.TRS(position, rotation, Vector3.one), material, layer, scale);
		}
		public static void DrawAdjustDynamicPartBody(Mesh mesh, Matrix4x4 matrix, Material material, int layer, Vector3 scale = default)
		{
			if (scale == default) scale = Vector3.one;
			Quaternion rotation;
			bool flip;
			if (ess != null)
			{
				scale.Scale(ess.SizeByRot(rot));
				flip = (isMelee || !aiming) && ess.FlipByRot(rot);
				rotation = aiming ? ess.UsageAngleQuatByRot(rot) : ess.AngleQuatByRot(rot);
				if (aiming && ess.useCommonWhenAiming) ess = com;
			}
			else
			{
				ess = com;
				flip = !aiming && ess.FlipByRot(rot);
				rotation = aiming ? ess.UsageAngleQuatByRot(rot) : ess.AngleQuatByRot(rot);
			}
			Graphics.DrawMesh(flip ? Flip(mesh) : mesh, matrix * Matrix4x4.Translate(ess.OffsetByRotInvertPriority(rot)) * (flip ? Matrix4x4.Rotate(rotation).inverse : Matrix4x4.Rotate(rotation)) * Matrix4x4.Scale(scale), material, layer);
		}
		private static Mesh Flip(Mesh m) => m == MeshPool.plane10 ? MeshPool.plane10Flip : MeshPool.plane10;
		private static Quaternion Inverse(this Quaternion quat) => Quaternion.Inverse(quat);
	}
}
