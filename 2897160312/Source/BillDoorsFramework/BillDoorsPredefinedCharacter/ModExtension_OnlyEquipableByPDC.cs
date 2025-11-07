using Verse;
using BillDoorsFramework;
using System.Collections.Generic;

namespace BillDoorsPredefinedCharacter
{
    public class ModExtension_PDCDef : DefModExtension
    {
        public PredefinedCharacterParmDef def;
    }
    public class ModExtension_PDCDefs : DefModExtension
    {
        public List<PredefinedCharacterParmDef> defs = new List<PredefinedCharacterParmDef>();
    }

    public class ModExtension_OnlyEquipableByPDC : ModExtension_PDCDef, IEquipRestriction
    {
        public bool CannotEquip(Pawn pawn, out string reason)
        {
            if (BDPDC_Mod.Tracker.ContainsKey(def) && BDPDC_Mod.Tracker[def] == pawn)
            {
                reason = "";
                return false;
            }
            reason = "BDPDC_PawnNotPDC".Translate(pawn.Name.ToStringShort, def.label);
            return true;
        }
    }
}
