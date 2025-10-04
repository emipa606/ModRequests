using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using Verse;

namespace PropagandaAndManifestos
{
    [DefOf]
    public static class PropagandaDef
    {
        public static ThingDef PropagandaSheet;
        static PropagandaDef()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PropagandaDef));
        }
    }
    [DefOf]
    public static class PropagandaRaidDef
    {
        public static QuestScriptDef PropagandaRaid;
        static PropagandaRaidDef()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PropagandaRaidDef));
        }
    }

    [StaticConstructorOnStartup]
    public static class PropagandaPatch
    {
        private static readonly Type patchType = typeof(PropagandaPatch);
        static PropagandaPatch()
        {
            var harmony = new Harmony("rimworld.Chaoticbloom.PropagandaAndManifestos.Manifesto");
            harmony.Patch(AccessTools.Method(typeof(FactionGiftUtility), "GetBaseGoodwillChange"),
            prefix: new HarmonyMethod(patchType, nameof(Prefix)));
        }

        public static bool Prefix(ref float __result, Thing anyThing, int count, Faction theirFaction)
        {
            int num;
            if (anyThing.def.Equals(PropagandaDef.PropagandaSheet))
            {
                num = count;
                //maybe some kind of reverse curve to neutralize other curve and always give +1
                __result = (float)num + .25f;
                return false;
            }
            return true;
        }

    }
    public class CompProperties_Propaganda : CompProperties
    {
        public CompProperties_Propaganda()
        {
            compClass = typeof(CompPropaganda);
        }
    }
    public class CompPropaganda : ThingComp
    {

        private CompProperties_Propaganda Props => (CompProperties_Propaganda)props;
        private void TryPropagandaRaidStart(int amount, int tile)
        {
            if (Rand.Chance(.02f))
            {
                Settlement settlement = Find.WorldObjects.SettlementAt(tile);
                if (settlement != null && settlement.Faction.HostileTo(Faction.OfPlayer))
                {
                    Slate slate = new Slate();
                    Map map = QuestGen_Get.GetMap();
                    slate.Set("map", map);
                    slate.Set("enemyFaction", settlement.Faction);
                    slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(map));
                    QuestUtility.GenerateQuestAndMakeAvailable(PropagandaRaidDef.PropagandaRaid, slate);
                }
            }
        }
        public override void Notify_AbandonedAtTile(int tile)
        {
            TryPropagandaRaidStart(parent.stackCount, tile);
        }
    }
}
