using HarmonyLib;
using System.Linq;
using Verse;

namespace SettlementQuests
{
    public class SettlementQuestsMod : Mod
    {
        public SettlementQuestsMod(ModContentPack pack) : base(pack)
        {
			new Harmony("SettlementQuestsMod").PatchAll();
        }
    }
}
