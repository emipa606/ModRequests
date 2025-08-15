using AlienRace;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using static AlienRace.AlienPartGenerator;

namespace StylishRim
{
	public class StylishAddon : IExposable
	{

		public readonly static string REG_HEAD_PARTS = "[hH]ead|[eE]ar|[hH]orn|[eE]ye|[aA]ntennae|[aA]ntler";
		public readonly static string REG_HAIR = "[hH]air";
		private readonly static string RACE_EPONA = "Alien_Epona";
		private readonly static string RACE_DESTRIER = "Alien_Destrier";
		private readonly static string RACE_UNICORN = "Alien_Unicorn";

		private const string REG_EPONA_EAR = "[hH]orse_[eE]ar";

		private readonly static Dictionary<string, string[]> ExceptionalRaceRegPair = new Dictionary<string, string[]>() { { RACE_EPONA, new string[] { REG_EPONA_EAR } }, { RACE_DESTRIER, new string[] { REG_EPONA_EAR } }, { RACE_UNICORN, new string[] { REG_EPONA_EAR } }, };
		public static bool HasPart(BodyAddon ba, string reg, bool inPath = true)
		{
			// 正規表現でアドオンが頭か身体かを区別するやつ
			// そもそもがワードでチョイスってやばいよなぁ
			if (ba == null) return false;
			if (ba.bodyPart != null)
			{
				if (Regex.IsMatch(ba.bodyPart.defName, reg)) return true;
			}
			return inPath && (HasPartInPath(ba, reg) || HasPartInHediffPath(ba, reg));
		}
		public static bool HasPartBySomeRace(string raceName, BodyAddon ba)
		{
			// 例外的な設定持ってる種族への個別対応
			// 今はフィールドとして直接宣言させてるけどそのうち外部ファイルから読み込みたい
			if (ExceptionalRaceRegPair.ContainsKey(raceName))
			{
				foreach (string reg in ExceptionalRaceRegPair[raceName])
				{
					if (HasPartInPath(ba, reg)) return true;
				}
				foreach (string reg in ExceptionalRaceRegPair[raceName])
				{
					if (HasPartInHediffPath(ba, reg)) return true;
				}
			}
			return false;
		}
		public static bool HasPartInPath(BodyAddon ba, string reg)
		{
			// どうにかアドオンの情報を取得するための特殊なやつ
			// グラフィックの格納パスを正規表現でマッチさせてる
			return ba.path != null && Regex.IsMatch(ba.path, reg);
		}
		public static bool HasPartInHediffPath(BodyAddon ba, string reg)
		{
			// 更に特殊なやつ。なんか特定のパーツの格納パスから
			// というか種族設定が作者によってバラバラ過ぎて拾うのきっついんだけど
			if (ba.hediffGraphics != null && ba.hediffGraphics.Count > 0)
			{
				foreach (ExtendedHediffGraphic part in ba.hediffGraphics)
				{
					if (part.path != null && Regex.IsMatch(part.path, reg)) return true;
				}
			}
			if (ba.bodytypeGraphics != null && ba.bodytypeGraphics.Count > 0)
			{
				foreach (ExtendedBodytypeGraphic part in ba.bodytypeGraphics)
				{
					if (part.path != null && Regex.IsMatch(part.path, reg)) return true;
				}
			}
			if (ba.headtypeGraphics != null && ba.headtypeGraphics.Count > 0)
			{
				foreach (ExtendedHeadtypeGraphic part in ba.headtypeGraphics)
				{
					if (part.path != null && Regex.IsMatch(part.path, reg)) return true;
				}
			}
			return false;
		}

		public static void SetUpAddon(ref Dictionary<int, StylishAddon> addons, ThingDef_AlienRace def) => SetUpAddon(ref addons, def?.alienRace.generalSettings.alienPartGenerator.bodyAddons, def?.defName);
		public static void SetUpAddon(ref Dictionary<int, StylishAddon> addons, List<BodyAddon> list, string raceName)
		{
			if (addons == null) addons = new Dictionary<int, StylishAddon>();
			if (list == null) return;
			for (int i = 0; i < list.Count; i++)
			{
				if (addons.ContainsKey(i))
				{
					if (addons[i] != null)
					{
						if (list[i].alignWithHead)
						{
							addons[i].alignWithHead = addons[i].isHead = true;
						}
					}
					else
					{
						StylishAddon sa = new StylishAddon(i, list[i]);
						if (HasPart(list[i], REG_HAIR))
						{
							sa.isHead = true;
							sa.isHair = true;
						}
						else if (HasPart(list[i], REG_HEAD_PARTS) || HasPartBySomeRace(raceName, list[i]))
						{
							sa.isHead = true;
						}
						addons[i] = sa;
					}
				}
				else
				{
					StylishAddon sa = new StylishAddon(i, list[i]);
					if (HasPart(list[i], REG_HAIR))
					{
						sa.isHead = true;
						sa.isHair = true;
					}
					else if (HasPart(list[i], REG_HEAD_PARTS) || HasPartBySomeRace(raceName, list[i]))
					{
						sa.isHead = true;
					}
					addons.Add(i, sa);
				}
			}
		}

		public Vector3 size = Vector3.one;
		public Vector4 offset = Vector4.zero;
		private Vector3 calcSize = Vector3.one;
		private Vector3 calcOffsetNorth = Vector3.zero;
		private Vector3 calcOffsetEast = Vector3.zero;
		private Vector3 calcOffsetSouth = Vector3.zero;
		private Vector3 calcOffsetWest = Vector3.zero;
		private Vector3 calcPortraitOffsetNorth = Vector3.zero;
		private Vector3 calcPortraitOffsetEast = Vector3.zero;
		private Vector3 calcPortraitOffsetSouth = Vector3.zero;
		private Vector3 calcPortraitOffsetWest = Vector3.zero;
		public Vector2 angle = Vector2.zero;

		public int key = -1;
		public bool isHead = false;
		public bool isHair = false;
		public bool hide = false;
		public bool alignWithHead = false;

		public void Calculate(Vector3 scale, Vector3 loc, Vector3 portraitLoc, Vector2 drawSize, Vector2 portraitDrawSize, Vector2 headVerticalOffset)
		{
			calcSize = Vector3.Scale(scale, size);
			Vector4 calcOffset = loc;
			Vector4 calcPortraitOffset = portraitLoc;
			calcOffset.w = calcOffset.y;
			calcPortraitOffset.w = calcPortraitOffset.y;
			calcOffset.x = offset.x * drawSize.x;
			calcOffset.z += offset.z * drawSize.x;
			calcOffset.y += offset.y * drawSize.y;
			calcOffset.w += offset.w * drawSize.y;
			calcPortraitOffset.x = offset.x * portraitDrawSize.x;
			calcPortraitOffset.z += offset.z * portraitDrawSize.x;
			calcPortraitOffset.y += offset.y * portraitDrawSize.y;
			calcPortraitOffset.w += offset.w * portraitDrawSize.y;
			calcOffsetNorth = OffsetNorth(calcOffset, headVerticalOffset.y * drawSize.y);
			calcOffsetEast = OffsetEast(calcOffset, headVerticalOffset.x * drawSize.y);
			calcOffsetSouth = OffsetSouth(calcOffset);
			calcOffsetWest = OffsetWest(calcOffset, headVerticalOffset.x * drawSize.y);
			calcPortraitOffsetNorth = OffsetNorth(calcPortraitOffset, headVerticalOffset.y * portraitDrawSize.y);
			calcPortraitOffsetEast = OffsetEast(calcPortraitOffset, headVerticalOffset.x * portraitDrawSize.y);
			calcPortraitOffsetSouth = OffsetSouth(calcPortraitOffset);
			calcPortraitOffsetWest = OffsetWest(calcPortraitOffset, headVerticalOffset.x * portraitDrawSize.y);
		}
		private Vector3 SizeNS => new Vector3(calcSize.x, 1f, calcSize.y);
		private Vector3 SizeWE => new Vector3(calcSize.z, 1f, calcSize.y);
		private Vector3 OffsetNorth(Vector4 v4, float offsetDiff = 0) => new Vector3(-v4.x, 0, v4.y + offsetDiff);
		private Vector3 OffsetEast(Vector4 v4, float offsetDiff = 0) => new Vector3(v4.z, 0, v4.w + offsetDiff);
		private Vector3 OffsetSouth(Vector4 v4, float offsetDiff = 0) => new Vector3(v4.x, 0, v4.y + offsetDiff);
		private Vector3 OffsetWest(Vector4 v4, float offsetDiff = 0) => new Vector3(-v4.z, 0, v4.w + offsetDiff);
		public float AngleNorth => 360 - angle.x;
		public float AngleEast => angle.y;
		public float AngleSouth => angle.x;
		public float AngleWest => 360 - angle.y;

		public Vector3 SizeByRot(Rot4 rot)
		{
			return rot.IsHorizontal ? SizeWE : SizeNS;
		}
		public Vector3 OffsetByRot(Rot4 rot, bool isPortrait = false)
		{
			switch (rot.AsInt)
			{
				case 0: return isPortrait ? calcPortraitOffsetNorth : calcOffsetNorth;
				case 1: return isPortrait ? calcPortraitOffsetEast : calcOffsetEast;
				case 2: return isPortrait ? calcPortraitOffsetSouth : calcOffsetSouth;
				default: return isPortrait ? calcPortraitOffsetWest : calcOffsetWest;
			}
		}
		public Quaternion AngleQuatByRot(Rot4 rot)
		{
			return Quaternion.AngleAxis(rot == Rot4.North ? AngleNorth : rot == Rot4.East ? AngleEast : rot == Rot4.South ? AngleSouth : AngleWest, Vector3.up);
		}


		public StylishAddon() { }
		public StylishAddon(int key) { this.key = key; }
		public StylishAddon(int key, BodyAddon ba)
		{
			this.key = key;
			if (ba.alignWithHead)
			{
				alignWithHead = true;
				isHead = true;
			}
		}
		private StylishAddon(StylishAddon sa)
		{
			key = sa.key;
			size = sa.size;
			offset = sa.offset;
			angle = sa.angle;
			isHead = sa.isHead;
			isHair = sa.isHair;
			hide = sa.hide;
			alignWithHead = sa.alignWithHead;
		}
		public StylishAddon Clone => new StylishAddon(this);
		public StylishAddon Combine(StylishAddon sa)
		{
			StylishAddon temp = Clone;
			temp.size.Scale(sa.size);
			temp.offset += sa.offset;
			temp.angle.x += sa.angle.x;
			temp.angle.x %= 360;
			temp.angle.y += sa.angle.y;
			temp.angle.y %= 360;
			if (sa.isHead) temp.isHead = true;
			if (sa.hide) temp.hide = true;
			return temp;
		}
		public void ExposeData()
		{
			Scribe_Values.Look<float>(ref size.x, "sizeX", 1f);
			Scribe_Values.Look<float>(ref size.y, "sizeY", 1f);
			Scribe_Values.Look<float>(ref size.z, "sizeZ", 1f);
			Scribe_Values.Look<float>(ref offset.x, "offsetX", 0f);
			Scribe_Values.Look<float>(ref offset.y, "offsetY", 0f);
			Scribe_Values.Look<float>(ref offset.z, "offsetZ", 0f);
			Scribe_Values.Look<float>(ref offset.w, "offsetW", 0f);
			Scribe_Values.Look<float>(ref angle.x, "angleX", 0f);
			Scribe_Values.Look<float>(ref angle.y, "angleY", 0f);
			Scribe_Values.Look<int>(ref key, nameof(key), -1);
			Scribe_Values.Look<bool>(ref isHead, nameof(isHead), false);
			Scribe_Values.Look<bool>(ref isHair, nameof(isHair), false);
			Scribe_Values.Look<bool>(ref hide, nameof(hide), false);
			Scribe_Values.Look<bool>(ref alignWithHead, nameof(alignWithHead), false);
		}
	}
}
