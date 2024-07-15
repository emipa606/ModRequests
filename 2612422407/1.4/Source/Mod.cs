using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RealNamesInfinite {

	public class RealNamesInfiniteMod : Mod {

		public static RealNamesInfiniteSettings Settings;

		public override string SettingsCategory() { return "RealNamesInfiniteText.Name".Translate(); }

		public override void DoSettingsWindowContents(Rect canvas) { Settings.DoWindowContents(canvas); }
		
		public RealNamesInfiniteMod(ModContentPack content) : base(content) {
			Settings = GetSettings<RealNamesInfiniteSettings>();
		}

	}

	public class ReplacementSet : IExposable {

		public string Original = "";
		public string Replacement = "";
		public bool OnlyReplaceForDefWithSameLabel = true;
		public bool IsEnabled = true;

		public ReplacementSet() { }

		public ReplacementSet(string original = "", string replacement = "", bool onlyReplaceForDefWithSameLabel = false, bool isEnabled = true) {
			Original = original;
			Replacement = replacement;
			OnlyReplaceForDefWithSameLabel = onlyReplaceForDefWithSameLabel;
			IsEnabled = isEnabled;
		}

		public void ExposeData() {
            Scribe_Values.Look(ref Original, "original", "");
            Scribe_Values.Look(ref Replacement, "replacement", "");
			Scribe_Values.Look(ref OnlyReplaceForDefWithSameLabel, "onlyReplaceForDefWithSameLabel", true);
			Scribe_Values.Look(ref IsEnabled, "isEnabled", true);
        }

	}

	public class RealNamesInfiniteSettings : ModSettings {

		private const float ScrollbarWidth = 12f;
		private static Vector2 scrollPosition = Vector2.zero;
		private float scrollBarHeight = 0;

		public List<ReplacementSet> Replacements = null;
		private List<ReplacementSet> newReplacements = null;

		private bool initialized = false;
		private bool loadDefaults = false;

		public bool FirstTime = false;

		public void DoWindowContents(Rect canvas) {

			Text.Font = GameFont.Tiny;
			float yMargin = Text.LineHeight * 0.5f;

			if(loadDefaults) {
				ResetToDefaults();
				loadDefaults = false;
				initialized = false;
			}

			if(!initialized || Replacements == null) {
				if(Replacements == null){
					Replacements = new List<ReplacementSet>();
				}
				scrollBarHeight = 0;
				scrollBarHeight += ((Text.LineHeight * 2) + (yMargin * 3)) * Replacements.Count;
				initialized = true;
			}

			if(newReplacements != null) {
				Replacements = new List<ReplacementSet>(newReplacements);
				newReplacements = null;
			}

			Rect containingRect = new Rect(0, 64f + yMargin, canvas.width, canvas.height - 60f);
			Rect viewRect = new Rect(0, 64f + yMargin, containingRect.width - 20f, scrollBarHeight + 20f);

			float y = viewRect.y;

			if (Widgets.ButtonText(new Rect(canvas.width-170, 0, 140, 28), "RealNamesInfiniteText.ResetDefaults".Translate())) {
				Dialog_MessageBox dialog_MessageBox = Dialog_MessageBox.CreateConfirmation("RealNamesInfiniteText.ConfirmResetDefaults".Translate(), delegate {
					loadDefaults = true;
				}, true, null);

				dialog_MessageBox.buttonAText = "RealNamesInfiniteText.Reset".Translate();
				Find.WindowStack.Add(dialog_MessageBox);
			}

			if (Widgets.ButtonText(new Rect(canvas.width-170, 32, 140, 28), "RealNamesInfiniteText.AddReplacement".Translate())) {
				AddSet((Text.LineHeight * 2) + (yMargin * 3));
			}

			if (Widgets.ButtonText(new Rect(canvas.width-320, 0, 140, 28), "RealNamesInfiniteText.ReplaceWords".Translate())) {
				ReplaceWords();
			}

			GUI.color = new Color(1f, 0.3f, 0.35f);
			if (Widgets.ButtonText(new Rect(canvas.width-320, 32, 140, 28), "RealNamesInfiniteText.RemoveAll".Translate())) {
				GUI.color = Color.white;
				Dialog_MessageBox dialog_MessageBox = Dialog_MessageBox.CreateConfirmation("RealNamesInfiniteText.ConfirmRemoveAll".Translate(), delegate {
					RemoveAll();
				}, true, null);

				dialog_MessageBox.buttonAText = "RealNamesInfiniteText.Remove".Translate();
				Find.WindowStack.Add(dialog_MessageBox);
			}
			GUI.color = Color.white;

			Widgets.Label(new Rect(167, 36, 500, Text.LineHeight), "(" + "RealNamesInfiniteText.RestartNeededTip".Translate() + ")");

			Widgets.BeginScrollView(containingRect, ref scrollPosition, viewRect, true);

			for(int x = 0; x < Replacements.Count; x++) {

				Widgets.Label(new Rect(containingRect.x, y, 80, Text.LineHeight), "RealNamesInfiniteText.ToReplace".Translate() + ":");
				Replacements[x].Original = Widgets.TextField(new Rect(containingRect.x + 100, y, containingRect.width * 0.6f, Text.LineHeight), Replacements[x].Original.ToString());
				
				GUI.color = new Color(1f, 0.3f, 0.35f);
				if (Widgets.ButtonText(new Rect(containingRect.width * .86f, y, 100, Text.LineHeight * 2 + yMargin), "RealNamesInfiniteText.Remove".Translate())) {
					RemoveSet(x, (Text.LineHeight * 2) + (yMargin * 3));
				}
				GUI.color = Color.white;

				Widgets.Checkbox(new Vector2(containingRect.width * .75f, (y + (Text.LineHeight / 2))), ref Replacements[x].OnlyReplaceForDefWithSameLabel);
				TooltipHandler.TipRegion(new Rect(containingRect.width * .75f, (y + (Text.LineHeight / 2)), 30f, 30f), "RealNamesInfiniteText.OnlyReplaceForDefWithSameLabelTooltip".Translate());
				Widgets.Checkbox(new Vector2(containingRect.width * .8f, (y + (Text.LineHeight / 2))), ref Replacements[x].IsEnabled);
				TooltipHandler.TipRegion(new Rect(containingRect.width * .8f, (y + (Text.LineHeight / 2)), 30f, 30f), "RealNamesInfiniteText.IsEnabledTooltip".Translate());

				y += Text.LineHeight + yMargin;

				Widgets.Label(new Rect(containingRect.x, y, 80, Text.LineHeight), "RealNamesInfiniteText.Replacement".Translate() + ":");
				Replacements[x].Replacement = Widgets.TextField(new Rect(containingRect.x + 100, y, containingRect.width * 0.6f, Text.LineHeight), Replacements[x].Replacement.ToString());
				
				y += Text.LineHeight + yMargin * 2;
			}

			Widgets.EndScrollView();

		}

		private void CreateHeader() {

		}

		private void AddSet(float scrollBarHeightIncrease) {
			Replacements.Add(new ReplacementSet());
			scrollBarHeight += scrollBarHeightIncrease;
		}

		private void RemoveSet(int setIndex, float scrollBarHeightDecrease) {
			newReplacements = new List<ReplacementSet>(Replacements);
			newReplacements.RemoveAt(setIndex);
			scrollBarHeight -= scrollBarHeightDecrease;
		}

		private void ReplaceWords() {
			Dialog_MessageBox dialog_MessageBox = Dialog_MessageBox.CreateConfirmation("RealNamesInfiniteText.ReplaceWordsConfirmation".Translate(), delegate {
				Tools.ReplaceWords(ref Replacements);
			}, true, null);

			Find.WindowStack.Add(dialog_MessageBox);
		}

		private void RemoveAll() {
			Replacements = new List<ReplacementSet>();
			newReplacements = null;
			initialized = false;
		}

		public void ResetToDefaults() {

			Replacements = new List<ReplacementSet>();
			newReplacements = null;

			//drugs
			Replacements.Add(new ReplacementSet("smokeleaf", "cannabis"));
			Replacements.Add(new ReplacementSet("wake-up powder", "amphetamine powder"));
			Replacements.Add(new ReplacementSet("wake-up", "amphetamines"));
			Replacements.Add(new ReplacementSet("psychoid plant", "coca plant"));
			Replacements.Add(new ReplacementSet("psychoid leaves", "coca leaves"));
			Replacements.Add(new ReplacementSet("psychoid", "coca"));
			Replacements.Add(new ReplacementSet("psychite tea", "coca pekoe"));
			Replacements.Add(new ReplacementSet("psychite", "cocaine"));
			Replacements.Add(new ReplacementSet("yayo", "cocaine"));
			Replacements.Add(new ReplacementSet("flake", "crack cocaine"));

			//guns
			
			Replacements.Add(new ReplacementSet("autopistol", "M1911A1", true));
			Replacements.Add(new ReplacementSet("revolver", "S&W Triple Lock",true));
			Replacements.Add(new ReplacementSet("machine pistol", "Tec-9", true));
			Replacements.Add(new ReplacementSet("heavy smg", "FAMAE SAF", true));
			Replacements.Add(new ReplacementSet("pump shotgun", "Remington 870", true));
			Replacements.Add(new ReplacementSet("chain shotgun", "Saiga-12", true));
			Replacements.Add(new ReplacementSet("bolt-action rifle", "Lee-Enfield", true));
			Replacements.Add(new ReplacementSet("sniper rifle", "M24", true));
			Replacements.Add(new ReplacementSet("assualt rifle", "M16A2", true));
			Replacements.Add(new ReplacementSet("lmg", "DP-28", true));
			Replacements.Add(new ReplacementSet("incendiary launcher", "T-9 incendiary launcher", true));
			Replacements.Add(new ReplacementSet("minigun", "M134G minigun", true));
		}

		public override void ExposeData() {
			base.ExposeData();
			Scribe_Collections.Look(ref Replacements, "replacements", LookMode.Deep, new object[0]);
			Scribe_Values.Look(ref FirstTime, "firstTime", true);
		}

	}

} 
