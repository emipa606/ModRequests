using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Atelier
{
    [StaticConstructorOnStartup]
    public static class Class1
    {
        static Class1()
        {
            new Harmony("Atelier.Mod").PatchAll();
		}

		public static bool IsCraftingRoom(this Room room, out List<Building_WorkTable> worktables)
        {
            if (room.Role != RoomRoleDefOf.None)
            {
                var buildings = room.ContainedAndAdjacentThings.OfType<Building_WorkTable>().Where(x => room.Cells.Contains(x.Position)).ToList();
                if (AtelierSettings.differentWorkbenchsCanBenefitQualityQonus)
                {
                    worktables = buildings;
                    return true;
                }
                else if (AtelierSettings.sameKindMultipleWorkbenchsCanBenefitQualityQonus)
                {
                    if (buildings.Any())
                    {
                        var firstKind = buildings[0];
                        worktables = buildings.Where(x => firstKind.def == x.def).ToList();
                        return true;
                    }
                }
                else if (buildings.Count == 1)
                {
                    worktables = new List<Building_WorkTable> { buildings[0] };
                    return true;
                }
            }
            worktables = null;
			return false;
		}
        public static bool IsQualityRoom(this Room room, Building_WorkTable checkedWorktable, out int qualityBonus)
        {
			qualityBonus = 0;
			if (room.IsCraftingRoom(out var worktables) && worktables.Contains(checkedWorktable))
			{
				var beauty = room.GetStat(RoomStatDefOf.Beauty);
				var stage = RoomStatDefOf.Beauty.GetScoreStageIndex(beauty);
				if (!room.ContainedBeds.Any())
				{
					switch (stage)
					{
						case 4: if (Rand.Chance(0.03f)) qualityBonus = 1; break;
						case 5: if (Rand.Chance(0.2f)) qualityBonus = 1; break;
						case 6:
							var value = Rand.Value;
							if (value <= 0.12f) qualityBonus = 2;
							else if (value <= 0.65f) qualityBonus = 1;
							break;
						case 7:
							value = Rand.Value;
							if (value <= 0.01f) qualityBonus = 3;
							else if (value <= 0.3f) qualityBonus = 2;
							else qualityBonus = 1;
							break;
						default: break;
					}
				}
				return true;
			}
			return false;
        }
    }

	[HarmonyPatch(typeof(QualityUtility))]
	[HarmonyPatch("GenerateQualityCreatedByPawn")]
	[HarmonyPatch(new Type[]
	{
			typeof(Pawn),
			typeof(SkillDef)
	}, new ArgumentType[]
	{
			0,
			0
	})]
	public static class GenerateQualityCreatedByPawn_Patch
	{
		private static void Postfix(ref QualityCategory __result, Pawn pawn, SkillDef relevantSkill, bool __state)
		{
			var room = pawn.GetRoom();
			var worktable = pawn.CurJob?.bill?.billStack?.billGiver as Building_WorkTable;
			if (worktable != null && room.IsQualityRoom(worktable, out var qualityBonus) && qualityBonus > 0)
            {
                __result = (QualityCategory)Mathf.Min((int)__result + qualityBonus, 6);
            }
        }
	}

	[HarmonyPatch(typeof(Room), "GetRoomRoleLabel")]
	public static class RoomLabelPatch
    {
		public static void Postfix(ref string __result, Room __instance)
        {
			if (__instance.IsCraftingRoom(out var buildings))
            {
                __result = string.Join(", ", buildings.Select(x => GenLabel.ThingLabel(x, 1)).Distinct()).CapitalizeFirst() + " " + __result;
            }
        }
	}

    public class AtelierMod : Mod
    {
        public static AtelierSettings settings;
        public AtelierMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<AtelierSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return Content.Name;
        }
    }

    public class AtelierSettings : ModSettings
    {
        public static bool sameKindMultipleWorkbenchsCanBenefitQualityQonus;
        public static bool differentWorkbenchsCanBenefitQualityQonus;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref sameKindMultipleWorkbenchsCanBenefitQualityQonus, "sameKindMultipleWorkbenchsCanBenefitQualityQonus");
            Scribe_Values.Look(ref differentWorkbenchsCanBenefitQualityQonus, "differentWorkbenchsCanBenefitQualityQonus");

        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            var ls = new Listing_Standard();
            ls.Begin(inRect);
            ls.CheckboxLabeled("Atelier.SameKindMultipleWorkbenchsCanBenefitQualityQonus".Translate(), ref sameKindMultipleWorkbenchsCanBenefitQualityQonus);
            ls.CheckboxLabeled("Atelier.DifferentWorkbenchsCanBenefitQualityQonus".Translate(), ref differentWorkbenchsCanBenefitQualityQonus);
            ls.End();
        }
    }
}
