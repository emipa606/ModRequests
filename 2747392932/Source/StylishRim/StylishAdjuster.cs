using AlienRace;
using FacialAnimation;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using static AlienRace.AlienPartGenerator;
using Object = UnityEngine.Object;

namespace StylishRim
{
	public static class StylishAdjuster
	{
		// 作成したメッシュを格納しておくところ
		// ポートレートはまた別のサイズかもしれないので分けてるけど描画時とか計算時に調整できなくもないか？
		// あたまがおかしくなりそうだ
		private static Dictionary<string, GraphicMeshSet> _adjustedBodySet = new Dictionary<string, GraphicMeshSet>();

		private static Dictionary<string, GraphicMeshSet> _adjustedPortraitBodySet = new Dictionary<string, GraphicMeshSet>();
		/*
		// Facial Animationで使う避難用
		private static Dictionary<string, GraphicMeshSet> _adjustedHeadSetFAAverage = new Dictionary<string, GraphicMeshSet>();
		private static Dictionary<string, GraphicMeshSet> _adjustedHeadSetFANarrow = new Dictionary<string, GraphicMeshSet>();
		*/

		// ================= USES PATCH FIELDS ===================//

		/*
		private static bool isDrawHair = false;
		public static void BeginDrawHair()
		{
			isDrawHair = true;
		}
		public static void EndDrawHair()
		{
			isDrawHair = false;
		}

		public static GraphicMeshSet ApplyMeshSet(ref Dictionary<string, GraphicMeshSet> dic, string thingId_raceName, GraphicMeshSet meshSet)
		{
			// 作成したメッシュ格納するやつ
			// 参照渡し使ってるけどDictionaryで使う意味あるんかな
			// そもそも参照渡しな気がするけどとりあえず入れといて損は若干しかなかろうて
			if (dic == null) dic = new Dictionary<string, GraphicMeshSet>();
			if (!dic.ContainsKey(thingId_raceName))
			{
				dic.Add(thingId_raceName, meshSet);
			}
			else
			{
				dic[thingId_raceName] = meshSet;
			}
			return meshSet;
		}

		public static void RemoveMeshSet(string thingID_raceName, bool isPawn = false)
		{
			// 格納してあるメッシュセットを削除するやつ
			// 種族であれば個別設定してるポーンにも影響でるんで合わせて削除する
			DestroyMeshesByKey(thingID_raceName);
			RemoveBodySet(thingID_raceName);
			if (!isPawn)
			{
				foreach (string pawnKey in PawnStyleSet.Styles.Values.Where(x => x.raceName == thingID_raceName).Select(x => x.key))
				{
					RemoveMeshSet(pawnKey, true);
				}
			}
		}
		private static void DestroyMeshesByKey(string thingID_raceName)
		{
			// この処理いるのかしらないけど設定削除時に対応したメッシュセットをデストローイ
			// 多分メモリ使用量削減になるんじゃないかな
			DestroyMeshSet(thingID_raceName, ref _adjustedBodySet);
			DestroyMeshSet(thingID_raceName, ref _adjustedPortraitBodySet);
		}

		private static void DestroyMeshSet(string thingID_raceName, ref Dictionary<string, GraphicMeshSet> dic)
		{
			if (dic != null && dic.ContainsKey(thingID_raceName)) DestroyMeshes(dic[thingID_raceName]);
		}
		private static void DestroyMeshes(GraphicMeshSet gms)
		{
			// デストローイ！！！
			// foreach (Mesh mesh in AccessTools.FieldRefAccess<GraphicMeshSet, Mesh[]>(gms, "meshes")) Object.Destroy(mesh);
			foreach (Mesh mesh in Traverse.Create(gms).Field("meshes").GetValue<Mesh[]>()) Object.Destroy(mesh);
		}

		// ================= BODY ===================//
		public static GraphicMeshSet ApplyBodySet(string thingID_raceName, float bodySizeX, float bodySizeY, bool isPortrait = false)
		{
			return ApplyBodySet(thingID_raceName, new GraphicMeshSet(1.5f * bodySizeX, 1.5f * bodySizeY), isPortrait);
		}
		public static GraphicMeshSet ApplyBodySet(string thingID_raceName, GraphicMeshSet meshSet, bool isPortrait = false)
		{
			if (!isPortrait) return ApplyMeshSet(ref _adjustedBodySet, thingID_raceName, meshSet);
			return ApplyMeshSet(ref _adjustedPortraitBodySet, thingID_raceName, meshSet);
		}
		public static void RemoveBodySet(string thingID_raceName)
		{
			if (_adjustedBodySet != null && _adjustedBodySet.ContainsKey(thingID_raceName)) _adjustedBodySet.Remove(thingID_raceName);
			if (_adjustedPortraitBodySet != null && _adjustedPortraitBodySet.ContainsKey(thingID_raceName)) _adjustedPortraitBodySet.Remove(thingID_raceName);
		}
		
		*/


		// ================= USES PATCH METHODS ===================//
		public static bool adjustPortrait = false;
		private static Vector3 TempCameraAngle;
		public static void AdjustPortraitOffset(Pawn pawn, ref Rot4 rotation, ref Vector3 cameraOffset, ref float cameraZoom, ref bool renderHeadgear, ref bool renderClothes, bool stylingStation)
		{
			if (!adjustPortrait) return;
			// ポートレートの各種調整
			if (StylishModSettings.commonizationPortrait)
			{
				ThingDef_AlienRace def = pawn.def as ThingDef_AlienRace;
				if (def != null)
				{
					cameraZoom *= def.alienRace.generalSettings.alienPartGenerator.customPortraitDrawSize.y / def.alienRace.generalSettings.alienPartGenerator.customDrawSize.y;
				}
			}
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pss == null) return;
			cameraZoom *= pss.Calc.multPortraitSize * pss.PortraitSize;
			cameraOffset.x *= pss.Calc.multPortraitOffset.x;
			cameraOffset.x += pss.Calc.addPortraitOffset.x - pss.PortraitOffsetX / pss.PortraitSize;
			cameraOffset.z *= pss.Calc.multPortraitOffset.y;
			cameraOffset.z += pss.Calc.addPortraitOffset.y - pss.PortraitOffsetY / pss.PortraitSize;
			if (!stylingStation)
			{
				rotation = pss.PortraitRotation;
				renderHeadgear = !pss.adjustPortraitDisableHeadGear;
				renderClothes = !pss.adjustPortraitDisableClothes;
			}
		}
		public static void AdjustPortraitAngle(Transform transform, Pawn pawn, bool isPortrait, bool stylingStation)
		{
			if (!adjustPortrait) return;
			// ポートレートの角度調整すっだ
			TempCameraAngle = transform.eulerAngles;
			if (stylingStation) return;
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pss == null) return;
			if (isPortrait)
			{
				transform.RotateAround(transform.position, Vector3.up, pss.PortraitAngle);
			}
		}
		public static void RemovePortraitAngle(Camera pawnCacheCamera, bool stylingStation)
		{
			if (!adjustPortrait) return;
			pawnCacheCamera.transform.eulerAngles = TempCameraAngle;
		}

		public static void AddPawnOffsetBodyDiff(ref Vector3 drawLoc, Pawn pawn)
		{
			// ボディのサイズによって表示位置をずらすやつ
			// ついでにこれも調整可能にする
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pss == null) return;
			if (pss.Calc.CorrectionHasBodyType(pawn.story.bodyType.defName))
            {
				drawLoc += pss.Calc.GetBodyTypeCorrection(pawn.story.bodyType.defName).BodyOffsetByRot(pawn.Rotation);
            }
			else
            {
				drawLoc.z += pss.Calc.addBodyOffset.z;
            }
		}
		public static Vector3 AddAddonOffset(Vector3 v3)
		{

			// アドオンの位置調整
			if (HarmonyPatch_AlienRace.pss == null) return v3;
			if (HarmonyPatch_AlienRace.sa != null && HarmonyPatch_AlienRace.sa.Count > HarmonyPatch_AlienRace.addonIndex)
			{
				StylishAddon sa = HarmonyPatch_AlienRace.sa[HarmonyPatch_AlienRace.addonIndex];
				if (!((HarmonyPatch_AlienRace.flags & PawnRenderFlags.Portrait) != PawnRenderFlags.None))
				{
					if (sa.isHead)
					{
						v3.x *= HarmonyPatch_AlienRace.rot.IsHorizontal ? HarmonyPatch_AlienRace.pss.Calc.multHeadAddonOffset.x : 1;
						v3.z *= HarmonyPatch_AlienRace.pss.Calc.multHeadAddonOffset.y;
						//v3.x += rot.IsHorizontal ? -pss.Calc.addHeadAddonOffset.x - (pss.Calc.addons[index].alignWithHead ? 0 : pss.Calc.AddHeadOffset.x) : 0;
						//v3.z += pss.Calc.addHeadAddonOffset.y + (pss.Calc.addons[index].alignWithHead ? 0 : pss.Calc.AddHeadOffset.y);
					}
					else
					{
						v3.x *= HarmonyPatch_AlienRace.rot.IsHorizontal ? HarmonyPatch_AlienRace.pss.Calc.multBodyAddonOffset.x : 1;
						v3.z *= HarmonyPatch_AlienRace.pss.Calc.multBodyAddonOffset.y;
						//v3.x += rot.IsHorizontal ? -pss.Calc.addBodyAddonOffset.x : 0;
						//v3.z += pss.Calc.addBodyAddonOffset.y;
					}
				}
				else
				{
					if (sa.isHead)
					{
						v3.x *= HarmonyPatch_AlienRace.pss.PortraitRotation.IsHorizontal ? HarmonyPatch_AlienRace.pss.Calc.multPortraitHeadAddonOffset.x : 1;
						v3.z *= HarmonyPatch_AlienRace.pss.Calc.multPortraitHeadAddonOffset.y;
						//v3.x += pss.PortraitRotation.IsHorizontal ? -pss.Calc.addPortraitHeadAddonOffset.x - (pss.Calc.addons[index].alignWithHead ? 0 : pss.Calc.AddPortraitHeadOffset.x) : 0;
						//v3.z += pss.Calc.addPortraitHeadAddonOffset.y + (pss.Calc.addons[index].alignWithHead ? 0 : pss.Calc.AddPortraitHeadOffset.y);
					}
					else
					{
						v3.x *= HarmonyPatch_AlienRace.pss.PortraitRotation.IsHorizontal ? HarmonyPatch_AlienRace.pss.Calc.multPortraitBodyAddonOffset.x : 1;
						v3.z *= HarmonyPatch_AlienRace.pss.Calc.multPortraitBodyAddonOffset.y;
						//v3.x += pss.PortraitRotation.IsHorizontal ? -pss.Calc.addPortraitBodyAddonOffset.x : 0;
						//v3.z += pss.Calc.addPortraitBodyAddonOffset.y;
					}
				}
			}
            else
			{
				if (!((HarmonyPatch_AlienRace.flags & PawnRenderFlags.Portrait) != PawnRenderFlags.None))
				{
					if (HarmonyPatch_AlienRace.ba.alignWithHead)
					{
						v3.x *= HarmonyPatch_AlienRace.rot.IsHorizontal ? HarmonyPatch_AlienRace.pss.Calc.multHeadAddonOffset.x : 1;
						v3.z *= HarmonyPatch_AlienRace.pss.Calc.multHeadAddonOffset.y;
						//v3.x += rot.IsHorizontal ? -pss.Calc.addHeadAddonOffset.x - (pss.Calc.addons[index].alignWithHead ? 0 : pss.Calc.AddHeadOffset.x) : 0;
						//v3.z += pss.Calc.addHeadAddonOffset.y + (pss.Calc.addons[index].alignWithHead ? 0 : pss.Calc.AddHeadOffset.y);
					}
					else
					{
						v3.x *= HarmonyPatch_AlienRace.rot.IsHorizontal ? HarmonyPatch_AlienRace.pss.Calc.multBodyAddonOffset.x : 1;
						v3.z *= HarmonyPatch_AlienRace.pss.Calc.multBodyAddonOffset.y;
						//v3.x += rot.IsHorizontal ? -pss.Calc.addBodyAddonOffset.x : 0;
						//v3.z += pss.Calc.addBodyAddonOffset.y;
					}
				}
				else
				{
					if (HarmonyPatch_AlienRace.ba.alignWithHead)
					{
						v3.x *= HarmonyPatch_AlienRace.pss.PortraitRotation.IsHorizontal ? HarmonyPatch_AlienRace.pss.Calc.multPortraitHeadAddonOffset.x : 1;
						v3.z *= HarmonyPatch_AlienRace.pss.Calc.multPortraitHeadAddonOffset.y;
						//v3.x += pss.PortraitRotation.IsHorizontal ? -pss.Calc.addPortraitHeadAddonOffset.x - (pss.Calc.addons[index].alignWithHead ? 0 : pss.Calc.AddPortraitHeadOffset.x) : 0;
						//v3.z += pss.Calc.addPortraitHeadAddonOffset.y + (pss.Calc.addons[index].alignWithHead ? 0 : pss.Calc.AddPortraitHeadOffset.y);
					}
					else
					{
						v3.x *= HarmonyPatch_AlienRace.pss.PortraitRotation.IsHorizontal ? HarmonyPatch_AlienRace.pss.Calc.multPortraitBodyAddonOffset.x : 1;
						v3.z *= HarmonyPatch_AlienRace.pss.Calc.multPortraitBodyAddonOffset.y;
						//v3.x += pss.PortraitRotation.IsHorizontal ? -pss.Calc.addPortraitBodyAddonOffset.x : 0;
						//v3.z += pss.Calc.addPortraitBodyAddonOffset.y;
					}
				}
			}
			
			return v3;
		}
		public static void SetUpAddon(List<BodyAddon> list, Pawn pawn)
		{
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pss == null || pss.addons?.Count == list.Count) return;
			pss.SetUpAddon(list);
		}


		public static bool ShowHairSetting(bool show, Pawn pawn)
		{
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (show || pss == null) return show;
			return pss.ShowHair;
		}
		public static Vector2 AdjustBeltOffset(Vector2 offset, Pawn pawn)
		{
			// ベルトスロットの自動位置調整。
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pss == null || pss.disableUtilityAdjust) return offset;
			offset.x *= pss.Calc.multUtilityOffset.x;
			offset.y *= pss.Calc.multUtilityOffset.z;
			return offset;
		}
		/*
		// このヒゲ調整メソッドなんだっけ
		public static Vector3 OffsetBeard(Vector3 offset, Pawn pawn = null)
		{
			if (!PawnStyleSet.StylesContainsKey(pawn)) return offset;
			offset *= GetLargestValue(pawn);
			return offset;
		}
		public static Vector3 OffsetBeardNarrow(Rot4 headFacing, Vector3 beardLoc, Pawn pawn = null)
		{
			if (!PawnStyleSet.StylesContainsKey(pawn)) return beardLoc;
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pawn.story.crownType == CrownType.Narrow)
			{
				if (headFacing == Rot4.East)
				{
					beardLoc += (Vector3.right * -0.05f / pss.largestValue) - Vector3.right * -0.05f;
				}
				else if (headFacing == Rot4.West)
				{
					beardLoc += (Vector3.right * 0.05f / pss.largestValue) - Vector3.right * 0.05f;
				}
				beardLoc += (Vector3.forward * -0.05f / pss.largestValue) - Vector3.forward * -0.05f;
			return beardLoc;
		}
		public static GraphicMeshSet AdjustMeshSet(GraphicMeshSet gms, PawnRenderFlags renderFlags, Pawn pawn, bool wantsBody)
		{
			// 頭か身体かなんかゲットするやつ
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (!wantsBody || pss == null) return gms;
			if (StylishModSettings.commonizationPortrait || !renderFlags.FlagSet(PawnRenderFlags.Portrait))
			{
				if (_adjustedBodySet.ContainsKey(pss.key)) return _adjustedBodySet[pss.key];
				return ApplyBodySet(pss.key, AdjustMesh(pss.AlienDef.alienRace.generalSettings.alienPartGenerator.customDrawSize * pss.Calc.multBodySize));
			}
			else
			{
				if (_adjustedPortraitBodySet.ContainsKey(pss.key)) return _adjustedPortraitBodySet[pss.key];
				return ApplyBodySet(pss.key, AdjustMesh(pss.AlienDef.alienRace.generalSettings.alienPartGenerator.customPortraitDrawSize * pss.Calc.multBodySize), true);
			}
		}

		private static GraphicMeshSet AdjustMesh(Vector2 v2)
		{
			return new GraphicMeshSet(1.5f * v2.x, 1.5f * v2.y);
		}
		*/

		public static float ZoomAdjuster(Pawn pawn, float cameraZoom)
		{
			// カメラでサイズ調整するやつ
			// 良さげな処理だと思うけど競合しすぎィ
			if (StylishAtlasManager.forCorpseCache && pawn.Dead) return 1;
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pss == null) return cameraZoom;
			return 1f / pss.Calc.borderScale;
		}

		public static Vector2 GetHeadScale(Pawn pawn)
		{
			// メッシュの頭のサイズをゲットす……何このメソッド？
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pss == null) return Vector2.one;
			return pss.Calc.multHeadSize;
		}

		public static Pawn pawnFA = null;
		public static Rot4 rot4FA = default;
		public static void FacialAnimationAddScale(ref float scaleW, ref float scaleH)
		{
			// Facial Animation の何か二度かけてるサイズ補正
			// こっちだけでよかったりしねえかなぁ
			// よかった
			if (pawnFA == null) return;
			PawnStyleSet pss = PawnStyleSet.Get(pawnFA);
			if (pss != null)
			{
				Vector3 v3 = pss.Calc.HeadSizeByRot(rot4FA);
				scaleW *= v3.x;
				scaleH *= v3.z;
			}
		}

		/*
		public static void AdjustFacialAnimationHeadMesh(PawnStyleSet pss)
		{
			if (pss.raceName != null || pss.defaultMeshes) return;
			Type t = typeof(GraphicHelper);

			Vector2 v2 = ((from def in DefDatabase<ThingDef_AlienRace>.AllDefs where def.defName == pss.key select def).First()?.alienRace.generalSettings.alienPartGenerator.customHeadDrawSize ?? Vector2.one) * pss.Calc.multHeadSize;
			Vector2 size = v2 * (from def in DefDatabase<FaceAdjustmentDef>.AllDefs where def.RaceName == pss.key select def)?.First().Size ?? Vector2.one;

			Dictionary<string, GraphicMeshSet> dic = AccessTools.StaticFieldRefAccess<Dictionary<string, GraphicMeshSet>>(t, "headAverageMeshSetList");
            if (dic.ContainsKey(pss.key))
            {
				if (!_adjustedHeadSetFAAverage.ContainsKey(pss.key)) _adjustedHeadSetFAAverage.Add(pss.key, dic[pss.key]);
				dic[pss.key] = new GraphicMeshSet(size.x, size.y);
			}

			dic = AccessTools.StaticFieldRefAccess<Dictionary<string, GraphicMeshSet>>(t, "headNarrowMeshSetList");
			if (dic.ContainsKey(pss.key))
			{
				if (!_adjustedHeadSetFANarrow.ContainsKey(pss.key)) _adjustedHeadSetFANarrow.Add(pss.key, dic[pss.key]);
				dic[pss.key] = new GraphicMeshSet((float)((double)size.x / 1.086995905648755), size.y);
			}
		}
		*/
		public static int IgnoreHeadIndex(int i)
		{
			if (!StylishTextureAtlas.IgnoreHeadOnly || i < 4) return i;
			return i - 4;
		}

		public static Dictionary<string, float> pawnBorderScale = new Dictionary<string, float>();
		public static Dictionary<string, int> pawnAtlasScale = new Dictionary<string, int>();
		public static float GetPawnBorderScale(Pawn pawn)
		{
			// 描画範囲のセッティングされたバリューをゲットする
			// 描画範囲って言い方してるけど他にわかりやすい言い方ねえものか
			if (pawnBorderScale.ContainsKey(pawn.def.defName)) return pawnBorderScale[pawn.def.defName];
			if (pawnBorderScale.ContainsKey("_" + pawn.def.defName)) return pawnBorderScale["_" + pawn.def.defName];
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pss != null)
			{
				pawnBorderScale.Add(pss.raceName ?? pss.key, pss.AlienDef.alienRace.generalSettings.alienPartGenerator.borderScale);
				return pawnBorderScale[pss.raceName ?? pss.key];
			}
			pawnBorderScale.Add("_" + pawn.def.defName, (pawn.def as ThingDef_AlienRace)?.alienRace.generalSettings.alienPartGenerator.borderScale ?? PawnStyleSet.HumanDef.alienRace.generalSettings.alienPartGenerator.borderScale);
			return pawnBorderScale["_" + pawn.def.defName];
		}
		public static int GetPawnAtlasScale(Pawn pawn)
		{
			// アトラスのセッティングされたバリューをゲットする
			if (pawnAtlasScale.ContainsKey(pawn.def.defName)) return pawnAtlasScale[pawn.def.defName];
			if (pawnAtlasScale.ContainsKey("_" + pawn.def.defName)) return pawnAtlasScale["_" + pawn.def.defName];
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pss != null)
			{
				pawnAtlasScale.Add(pss.raceName ?? pss.key, pss.AlienDef.alienRace.generalSettings.alienPartGenerator.atlasScale);
				return pawnAtlasScale[pss.raceName ?? pss.key];
			}
			pawnAtlasScale.Add("_" + pawn.def.defName, (pawn.def as ThingDef_AlienRace)?.alienRace.generalSettings.alienPartGenerator.atlasScale ?? PawnStyleSet.HumanDef.alienRace.generalSettings.alienPartGenerator.atlasScale);
			return pawnAtlasScale["_" + pawn.def.defName];

			/*
			PawnStyleSet pss = PawnStyleSet.Get(pawn);
			if (pss != null && !pss.DefaultAtlasScale) return pss.Calc.atlasScale;
			return (pawn.def as ThingDef_AlienRace)?.alienRace.generalSettings.alienPartGenerator.atlasScale ?? 1;
			*/
		}
		public static void AddBorderScaleRecord(string raceName, float borderScale)
		{
			if (pawnBorderScale.ContainsKey(raceName))
			{
				pawnBorderScale[raceName] = borderScale;
			}
			else
			{
				pawnBorderScale.Add(raceName, borderScale);
			}
		}
		public static void AddAtlasScaleRecord(string raceName, int atlasScale)
		{
			if (pawnAtlasScale.ContainsKey(raceName))
			{
				pawnAtlasScale[raceName] = atlasScale;
			}
			else
			{
				pawnAtlasScale.Add(raceName, atlasScale);
			}
		}
		public static void RemoveBorderScaleRecord(string raceName)
		{
			if (pawnBorderScale.ContainsKey(raceName)) pawnBorderScale.Remove(raceName);
		}
		public static void RemoveAtlasScaleRecord(string raceName)
		{
			if (pawnAtlasScale.ContainsKey(raceName)) pawnAtlasScale.Remove(raceName);
		}
	}
}
