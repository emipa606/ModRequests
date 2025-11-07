using RimWorld.QuestGen;
using Verse;

namespace BillDoorsPredefinedCharacter
{
    public class QuestNode_RequireAppearanceMode : QuestNode
    {
        public PredefinedCharacterParmDef def;

        public string methodIdentifier;

        protected override void RunInt()
        {
        }

        protected override bool TestRunInt(Slate slate)
        {
            Log.Message($"contains key: {BDPDC_Mod.LoadedAppearModes.ContainsKey(def)}");
            Log.Message($"contains method: {BDPDC_Mod.LoadedAppearModes[def].Contains(methodIdentifier)}");
            return BDPDC_Mod.LoadedAppearModes.ContainsKey(def) && BDPDC_Mod.LoadedAppearModes[def].Contains(methodIdentifier);
        }
    }
    public class QuestNode_RequireCharacterAtHome : QuestNode
    {
        public PredefinedCharacterParmDef def;

        protected override void RunInt()
        {
        }

        protected override bool TestRunInt(Slate slate)
        {
            Log.Message($"at home: {PDCharacterUtil.AtHome(def)}");
            return PDCharacterUtil.AtHome(def);
        }
    }
}
