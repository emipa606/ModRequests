using HugsLib;
using HugsLib.Settings;

namespace OneBigFamily
{
    public class ModBaseOneBigFamily : ModBase
    {
        public override string ModIdentifier => "OneBigFamily";

        public static SettingHandle<int> minRelationships;
        public static SettingHandle<int> avgRelationships;
        public static SettingHandle<int> maxRelationships;
        public static SettingHandle<bool> allowColonists;
        
        public override void DefsLoaded()
        {
            minRelationships = Settings.GetHandle("minRelationships", "Minimum relationships", "How many relationships a new pawn will at least have (default = 1).\nNote that often only a certain amount of relationships is possible. Also, increasing this number can significantly slow down creating new pawns.", 1, Validate);
            
            maxRelationships = Settings.GetHandle("maxRelationships", "Maximum relationships", "How many relationships a new pawn will have at max (default = 5).\nNote that some extra relationships are automatic and can't be prevented.", 5, Validate);
            
            avgRelationships = Settings.GetHandle("avgRelationships", "Average relationships", "How many relationships a new pawn will have on average (default = 1).", 1);
            
            allowColonists = Settings.GetHandle("allowColonists", "Allow colonists", "If checked, colonists can receive relationships with new pawns (default = true).\nThis is not realistic and can cause unexpected relationships.", true);
        }

        private static bool Validate(string value)
        {
            if (!int.TryParse(value, out var val)) return false;
            if (val < 0) return false;
            if (val > 50) return false;
            
            return true;
        }
    }
}