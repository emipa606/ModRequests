using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CustomPlayerIcon
{
    public class CustomPlayerIconComponent : GameComponent
    {
        public static CustomPlayerIconComponent Instance;
        public Color? chosenColor;
        public string chosenTexPath;
		public CustomPlayerIconComponent(Game game)
        {

        }

        public override void StartedNewGame()
		{
			Instance = this;
			base.StartedNewGame();
        }
        public override void LoadedGame()
        {
			Instance = this;
			base.LoadedGame();
        }
        public override void FinalizeInit()
        {
            Instance = this;
            base.FinalizeInit();
            ApplyIcon();
        }

        public void ApplyIcon()
        {
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				if (!chosenTexPath.NullOrEmpty())
				{
					Faction.OfPlayer.def.factionIcon = ContentFinder<Texture2D>.Get(chosenTexPath);
				}
			});
		}
        public override void ExposeData()
        {
            Instance = this;
            base.ExposeData();
            Scribe_Values.Look(ref chosenColor, "chosenColor");
            Scribe_Values.Look(ref chosenTexPath, "chosenTexPath");
        }
    }

    [StaticConstructorOnStartup]
    public static class Core
    {
        public static List<Color> colors = new List<Color> // copied from Ideology defs
        {
                new ColorInt(245, 245, 245).ToColor,
                new ColorInt(192, 192, 192).ToColor,
                new ColorInt(128, 128, 128).ToColor,
                new ColorInt(255, 115, 115).ToColor,
                new ColorInt(191, 86, 86).ToColor,
                new ColorInt(148, 67, 67).ToColor,
                new ColorInt(255, 199, 115).ToColor,
                new ColorInt(191, 149, 86).ToColor,
                new ColorInt(148, 115, 67).ToColor,
                new ColorInt(227, 255, 115).ToColor,
                new ColorInt(170, 191, 86).ToColor,
                new ColorInt(132, 148, 67).ToColor,
                new ColorInt(143, 255, 115).ToColor,
                new ColorInt(107, 191, 86).ToColor,
                new ColorInt(83, 148, 67).ToColor,
                new ColorInt(115, 255, 171).ToColor,
                new ColorInt(86, 191, 128).ToColor,
                new ColorInt(67, 148, 99).ToColor,
                new ColorInt(115, 255, 255).ToColor,
                new ColorInt(86, 191, 191).ToColor,
                new ColorInt(67, 148, 148).ToColor,
                new ColorInt(115, 171, 255).ToColor,
                new ColorInt(86, 128, 191).ToColor,
                new ColorInt(67, 99, 148).ToColor,
                new ColorInt(143, 115, 255).ToColor,
                new ColorInt(107, 86, 191).ToColor,
                new ColorInt(83, 67, 148).ToColor,
                new ColorInt(227, 115, 255).ToColor,
                new ColorInt(170, 86, 191).ToColor,
                new ColorInt(132, 67, 148).ToColor,
                new ColorInt(255, 115, 199).ToColor,
                new ColorInt(191, 86, 149).ToColor,
                new ColorInt(148, 67, 115).ToColor,
                new Color(0.1f, 0.1f, 0.1f),
                new Color(0.2f, 0.2f, 0.2f),
                new Color(0.31f, 0.28f, 0.26f),
                new Color(0.25f, 0.2f, 0.15f),
                new Color(0.3f, 0.2f, 0.1f),
                new ColorInt(90, 58, 32).ToColor,
                new ColorInt(132, 83, 47).ToColor,
                new ColorInt(193, 146, 85).ToColor,
                new ColorInt(237, 202, 156).ToColor
        };
		public static HashSet<string> allIconPaths = new HashSet<string>();
        static Core()
        {
			colors.SortByColor(x => x);
			foreach (var t in ContentFinder<Texture2D>.GetAllInFolder("UI/Ideoligions"))
			{
				var path = "UI/Ideoligions/" + t.name;
				if (ContentFinder<Texture2D>.Get(path, false) != null)
				{
					allIconPaths.Add(path);
				}
			}
			foreach (var def in DefDatabase<IdeoIconDef>.AllDefs)
            {
				allIconPaths.Add(def.iconPath);
			}

			foreach (var t in ContentFinder<Texture2D>.GetAllInFolder("CustomPlayerIcons"))
			{
				var path = "CustomPlayerIcons/" + t.name;
				if (ContentFinder<Texture2D>.Get(path, false) != null)
                {
					allIconPaths.Add(path);
				}
			}
			new Harmony("CustomPlayerIcon.Mod").PatchAll();
        }
	}

    [HarmonyPatch(typeof(Faction), nameof(Faction.Color), MethodType.Getter)]
    public static class FactionColorPatch
    {
        public static bool Prefix(Faction __instance, ref Color __result)
        {
            if (CustomPlayerIconComponent.Instance != null && __instance.IsPlayer && CustomPlayerIconComponent.Instance.chosenColor.HasValue)
            {
                __result = CustomPlayerIconComponent.Instance.chosenColor.Value;
                return false;
            }
            return true;
        }
    }

	[HarmonyPatch(typeof(WorldObject), nameof(WorldObject.ExpandingIconColor), MethodType.Getter)]
	public static class WorldObjectExpandingIconColorPatch
	{
		public static bool Prefix(WorldObject __instance, ref Color __result)
		{
			if (CustomPlayerIconComponent.Instance != null && __instance is Settlement settlement && settlement.Faction != null 
				&& settlement.Faction.IsPlayer && CustomPlayerIconComponent.Instance.chosenColor.HasValue)
			{
				__result = CustomPlayerIconComponent.Instance.chosenColor.Value;
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Settlement), "GetGizmos")]
    public static class SettlementGetGizmosPatch
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Settlement __instance)
        {
            foreach (Gizmo gizmo in __result)
            {
                yield return gizmo;
            }
            if (__instance.Faction != null && __instance.Faction.IsPlayer)
            {
                yield return new Command_Action
                {
                    defaultLabel = "ChangePlayerIcon".Translate(),
                    defaultDesc = "ChangePlayerIconDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/ChangePlayerIcon"),
                    action = delegate
                    {
						Find.WindowStack.Add(new Dialog_ChoosePlayerIcon());
                    }
                };
            }
        }
    }
	public class Dialog_ChoosePlayerIcon : Window
	{
		private static string newIconPath;

		private static Color newColor;

		private Vector2 scrollPos;

		private float viewHeight;

		private static readonly Vector2 ButSize = new Vector2(150f, 38f);
		public override Vector2 InitialSize => new Vector2(740f, 700f);

		public static bool firstTime = true;
		public Dialog_ChoosePlayerIcon()
		{
			absorbInputAroundWindow = true;
			if (firstTime)
            {
				newColor = Core.colors.First();
				newIconPath = Core.allIconPaths.First();
				firstTime = false;
			}
		}

		public override void OnAcceptKeyPressed()
		{
			TryAccept();
			Event.current.Use();
		}

		public override void DoWindowContents(Rect rect)
		{
			Rect rect2 = rect;
			rect2.height -= Window.CloseButSize.y;
			Text.Font = GameFont.Medium;
			Text.Font = GameFont.Small;
			float y = rect2.y;
			Rect mainRect = rect2;
			mainRect.yMax -= 4f;
			Widgets.Label(mainRect.x, ref y, mainRect.width, "Icon".Translate());
			mainRect.yMin = y;
			DoColorSelector(mainRect, ref y);
			mainRect.yMin = y;
			DoIconSelector(mainRect);
			if (Widgets.ButtonText(new Rect(0f, rect.height - ButSize.y, ButSize.x, ButSize.y), "Back".Translate()))
			{
				Close();
			}
			if (Widgets.ButtonText(new Rect(rect.width - ButSize.x, rect.height - ButSize.y, ButSize.x, ButSize.y), "DoneButton".Translate()))
			{
				TryAccept();
			}
		}

		private void DoIconSelector(Rect mainRect)
		{
			int num = 50;
			Rect viewRect = new Rect(0f, 0f, mainRect.width - 16f, viewHeight);
			Widgets.BeginScrollView(mainRect, ref scrollPos, viewRect);
			IEnumerable<string> allDefs = Core.allIconPaths;
			int num2 = Mathf.FloorToInt(viewRect.width / (float)(num + 5));
			int num3 = allDefs.Count();
			int num4 = 0;
			foreach (var item in allDefs)
			{
				int num5 = num4 / num2;
				int num6 = num4 % num2;
				int num7 = ((num4 >= num3 - num3 % num2) ? (num3 % num2) : num2);
				float num8 = (viewRect.width - (float)(num7 * num) - (float)((num7 - 1) * 5)) / 2f;
				Rect rect = new Rect(num8 + (float)(num6 * num) + (float)(num6 * 5), num5 * num + num5 * 5, num, num);
				Widgets.DrawLightHighlight(rect);
				Widgets.DrawHighlightIfMouseover(rect);
				if (item == newIconPath)
				{
					Widgets.DrawBox(rect);
				}
				GUI.color = newColor;
				GUI.DrawTexture(new Rect(rect.x + 5f, rect.y + 5f, 40f, 40f), ContentFinder<Texture2D>.Get(item));
				GUI.color = Color.white;
				if (Widgets.ButtonInvisible(rect))
				{
					newIconPath = item;
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
				}
				viewHeight = Mathf.Max(viewHeight, rect.yMax);
				num4++;
			}
			GUI.color = Color.white;
			Widgets.EndScrollView();
		}

		private void DoColorSelector(Rect mainRect, ref float curY)
		{
			int num = 26;
			float num2 = 98f;
			int num3 = Mathf.FloorToInt((mainRect.width - num2) / (float)(num + 2));
			int num4 = Mathf.CeilToInt((float)Core.colors.Count / (float)num3);
			GUI.BeginGroup(mainRect);
			GUI.color = newColor;
			GUI.DrawTexture(new Rect(5f, 5f, 88f, 88f), ContentFinder< Texture2D>.Get(newIconPath));
			GUI.color = Color.white;
			curY += num2;
			int num5 = 0;
			foreach (Color item in Core.colors)
			{
				int num6 = num5 / num3;
				int num7 = num5 % num3;
				float num8 = (num2 - (float)(num * num4) - 2f) / 2f;
				Rect rect = new Rect(num2 + (float)(num7 * num) + (float)(num7 * 2), num8 + (float)(num6 * num) + (float)(num6 * 2), num, num);
				Widgets.DrawLightHighlight(rect);
				Widgets.DrawHighlightIfMouseover(rect);
				if (newColor == item)
				{
					Widgets.DrawBox(rect);
				}
				Widgets.DrawBoxSolid(new Rect(rect.x + 2f, rect.y + 2f, 22f, 22f), item);
				if (Widgets.ButtonInvisible(rect))
				{
					newColor = item;
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
				}
				curY = Mathf.Max(curY, mainRect.yMin + rect.yMax);
				num5++;
			}
			GUI.EndGroup();
			curY += 4f;
		}

		private void TryAccept()
		{
			CustomPlayerIconComponent.Instance.chosenColor = newColor;
			CustomPlayerIconComponent.Instance.chosenTexPath = newIconPath;
			CustomPlayerIconComponent.Instance.ApplyIcon();
			Close();
		}
	}
}
