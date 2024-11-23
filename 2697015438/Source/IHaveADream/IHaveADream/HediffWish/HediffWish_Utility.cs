using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;

namespace HDream
{
    public static class HediffWish_Utility
    {

        public static List<BodyPartRecord> AllChildPartRecordRecursive(BodyPartRecord bodyPart, bool addPrimePart = false)
        {
            List<BodyPartRecord> parts = new List<BodyPartRecord>();
            if(addPrimePart) parts.Add(bodyPart);
            if (bodyPart.parts != null)
            {
                for (int j = 0; j < bodyPart.parts.Count; j++)
                {
                    parts.Add(bodyPart.parts[j]);
                    parts.AddRange(AllChildPartRecordRecursive(bodyPart.parts[j]));
                }
            }
            return parts;
        }
        public static List<BodyPartRecord> AllParentPartRecordRecursive(BodyPartRecord bodyPart, bool addPrimePart = false)
        {
            List<BodyPartRecord> parts = new List<BodyPartRecord>();
            if (addPrimePart) parts.Add(bodyPart);
            if (bodyPart.parent != null)
            {
                parts.Add(bodyPart.parent);
                parts.AddRange(AllParentPartRecordRecursive(bodyPart.parent));
            }
            return parts;
        }
        public static List<BodyPartDef> AllChildPartDefRecursive(BodyPartRecord bodyPart, bool addPrimePart = false)
        {
            List<BodyPartDef> parts = new List<BodyPartDef>();
            if (addPrimePart) parts.Add(bodyPart.def);
            if (bodyPart.parts != null)
            {
                for (int j = 0; j < bodyPart.parts.Count; j++)
                {
                    parts.Add(bodyPart.parts[j].def);
                    parts.AddRange(AllChildPartDefRecursive(bodyPart.parts[j]));
                }
            }
            return parts;
        }
        public static List<BodyPartDef> AllParentPartDefRecursive(BodyPartRecord bodyPart, bool addPrimePart = false)
        {
            List<BodyPartDef> parts = new List<BodyPartDef>();
            if (addPrimePart) parts.Add(bodyPart.def);
            if (bodyPart.parent != null)
            {
                parts.Add(bodyPart.parent.def);
                parts.AddRange(AllParentPartDefRecursive(bodyPart.parent));
            }
            return parts;
        }
    }
}
