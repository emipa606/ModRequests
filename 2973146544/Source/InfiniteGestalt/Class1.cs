using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GestaltEngine;
using RimWorld;
using UnityEngine;
using Verse;

using HarmonyLib;
using System.Reflection;

namespace InfiniteGestaltEngine
{
    public class UpgradeSettings: DefModExtension
    {
        public Upgrade init;
        public Upgrade offset;
        public Upgrade max;
    }

    public static class Utils
    {
        public static Dictionary<int, Upgrade> cache = new Dictionary<int, Upgrade>();
        public static UpgradeSettings settings = ThingDef.Named("RM_GestaltEngine").GetModExtension<UpgradeSettings>();
        public static MethodInfo SetLevel = AccessTools.Method(typeof(CompGestaltEngine), "SetLevel");
        public static Upgrade UP(int level)
        {
            if (!cache.ContainsKey(level))
            {
                cache[level] = new Upgrade
                {
                    overlayGraphic = new GraphicData
                    {
                        texPath = $"Things/Building/Biotech/Tier{(level > 4 ? 4 : level)}_GestaltEngine",
                        graphicClass = typeof(Graphic_Single),
                        drawSize = new Vector2(11, 11),
                    },
                    upgradeCooldownTicks = Mathf.Min(settings.init.upgradeCooldownTicks + level * settings.offset.upgradeCooldownTicks, settings.max.upgradeCooldownTicks),
                    upgradeDurationTicks = Mathf.Min(settings.init.upgradeDurationTicks + level * settings.offset.upgradeDurationTicks, settings.max.upgradeDurationTicks),
                    powerConsumption = Mathf.Min(settings.init.powerConsumption + level * settings.offset.powerConsumption, settings.max.powerConsumption),
                    heatPerSecond = Mathf.Min(settings.init.heatPerSecond + level * settings.offset.heatPerSecond, settings.max.heatPerSecond),
                    researchPointsPerSecond = Mathf.Min(settings.init.researchPointsPerSecond + level * settings.offset.researchPointsPerSecond, settings.max.researchPointsPerSecond),
                    totalMechBandwidth = Mathf.Min(settings.init.totalMechBandwidth + level * settings.offset.totalMechBandwidth, settings.max.totalMechBandwidth),
                    totalControlGroups = Mathf.Min(settings.init.totalControlGroups + level * settings.offset.totalControlGroups, settings.max.totalControlGroups),
                    allowCaravans = true,
                };
            }
            return cache[level];
        }
    }

    [HarmonyPatch(typeof(CompUpgradeable))]
    public static class CompPatch
    {
        [HarmonyPatch(nameof(CompUpgradeable.CurrentUpgrade), MethodType.Getter)]
        [HarmonyPrefix]
        public static bool Prefix1(CompUpgradeable __instance, ref Upgrade __result)
        {
            if (__instance.parent.def.defName != "RM_GestaltEngine") return true; // only apply on gestalt
            ///
            __result = Utils.UP(__instance.level);
            return false;
        }

        [HarmonyPatch(nameof(CompUpgradeable.MaxLevel), MethodType.Getter)]
        [HarmonyPrefix]
        public static bool Prefix2(CompUpgradeable __instance, ref int __result)
        {
            if (__instance.parent.def.defName != "RM_GestaltEngine") return true; // only apply on gestalt
            ///
            __result = int.MaxValue;
            return false;
        }

        [HarmonyPatch(nameof(CompUpgradeable.CompInspectStringExtra))]
        [HarmonyPostfix]
        public static void Postfix(CompUpgradeable __instance, ref string __result)
        {
            if (__instance.parent.def.defName != "RM_GestaltEngine") return; // only apply on gestalt
            ///
            if (__instance.GetType() != typeof(CompGestaltEngine))
            {
                return;
            }
            int used = (__instance as CompGestaltEngine).dummyPawn.mechanitor.UsedBandwidth;
            string str = "\n" + "Bandwidth".Translate() + ": " + used + " / " + __instance.CurrentUpgrade.totalMechBandwidth;
            __result += str;
        }

        [HarmonyPatch(nameof(CompUpgradeable.CompGetGizmosExtra))]
        [HarmonyPrefix]
        public static bool Gizmo_Prefix(CompUpgradeable __instance, out List<Gizmo> __state)
        {
            __state = new List<Gizmo>();
            if (__instance.parent.def.defName != "RM_GestaltEngine") return true; // only apply on gestalt
            ///
            if (__instance.parent.Faction == Faction.OfPlayer)
            {
                Command_Action upgrade = new Command_Action
                {
                    defaultLabel = "RM.Upgrade".Translate(),
                    defaultDesc = "RM.UpgradeDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Buttons/GestaltUpgrade"),
                    action = delegate
                    {
                        __instance.StartUpgrade(1);
                    }
                };
                Command_Action downgrade = new Command_Action
                {
                    defaultLabel = "RM.Downgrade".Translate(),
                    defaultDesc = "RM.DowngradeDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Buttons/GestaltDowngrade"),
                    action = delegate
                    {
                        __instance.StartUpgrade(-1);
                    }
                };
                Command_Action upgradeInstant = new Command_Action
                {
                    defaultLabel = "DEV: Instant upgrade",
                    defaultDesc = "RM.UpgradeDesc".Translate(),
                    action = delegate
                    {
                        __instance.StartUpgrade(1);
                        //__instance.SetLevel();
                        Utils.SetLevel.Invoke(__instance, new Type[] { });
                    }
                };
                Command_Action downgradeInstant = new Command_Action
                {
                    defaultLabel = "DEV: Instant downgrade",
                    defaultDesc = "RM.DowngradeDesc".Translate(),
                    action = delegate
                    {
                        __instance.StartUpgrade(-1);
                        //__instance.SetLevel();
                        Utils.SetLevel.Invoke(__instance, new Type[] { });
                    }
                };
                if (!__instance.compPower.PowerOn)
                {
                    upgrade.Disable("NoPower".Translate());
                    downgrade.Disable("NoPower".Translate());
                }
                else if (__instance.Upgrading)
                {
                    upgrade.Disable("RM.Upgrading".Translate());
                    downgrade.Disable("RM.Upgrading".Translate());
                }
                else if (__instance.Downgrading)
                {
                    upgrade.Disable("RM.Downgrading".Translate());
                    downgrade.Disable("RM.Downgrading".Translate());
                }
                else if (__instance.OnCooldown)
                {
                    upgrade.Disable("RM.OnCooldown".Translate((__instance.cooldownPeriod - Find.TickManager.TicksGame).ToStringTicksToPeriod()));
                    downgrade.Disable("RM.OnCooldown".Translate((__instance.cooldownPeriod - Find.TickManager.TicksGame).ToStringTicksToPeriod()));
                }
                if (__instance.level == __instance.MinLevel)
                {
                    downgrade.Disabled = true;
                    downgradeInstant.Disabled = true;
                }
                else
                {
                    downgrade.defaultDesc = downgrade.defaultDesc + "\n\n" + Utils.UP(__instance.level - 1).UpgradeDesc();
                    downgradeInstant.defaultDesc = downgradeInstant.defaultDesc + "\n\n" + Utils.UP(__instance.level - 1).UpgradeDesc();
                }
                if (__instance.level == __instance.MaxLevel)
                {
                    upgrade.Disabled = true;
                    upgradeInstant.Disabled = true;
                }
                else
                {
                    upgrade.defaultDesc = upgrade.defaultDesc + "\n\n" + Utils.UP(__instance.level + 1).UpgradeDesc();
                    upgradeInstant.defaultDesc = upgradeInstant.defaultDesc + "\n\n" + Utils.UP(__instance.level + 1).UpgradeDesc();
                }
                __state.Add(upgrade);
                __state.Add(downgrade);
                if (Prefs.DevMode)
                {
                    __state.Add(upgradeInstant);
                    __state.Add(downgradeInstant);
                }
            }
            return false;
        }

        [HarmonyPatch(nameof(CompUpgradeable.CompGetGizmosExtra))]
        [HarmonyPostfix]
        public static IEnumerable<Gizmo> Gizmo_Postfix(IEnumerable<Gizmo> gizmos, List<Gizmo> __state)
        {
            if (!gizmos.EnumerableNullOrEmpty()) return gizmos;
            return __state.AsEnumerable();
        }
    }

    [StaticConstructorOnStartup]
    public static class Patch
    {
        static Patch()
        {
            if (
                ModLister.HasActiveModWithName("Gestalt Engine (Continued)")
                || ModLister.HasActiveModWithName("Reinforced Mechanoid 2 (Continued)")
            ){

                new Harmony("cedaro.InfiniteGestaltEngine").PatchAll();
            }
            else
            {
                Log.Error("Infinite Gestalt Engine require either Gestalt Engine or Reinforced Mechanoid 2 as dependency. Seems none of them is active.");
            }
        }
    }
}
