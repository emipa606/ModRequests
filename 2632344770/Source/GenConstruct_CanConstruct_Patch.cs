using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace FixedIdeoStyle
{
    [HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.CanConstruct), new Type[] { typeof(Thing), typeof(Pawn), typeof(bool), typeof(bool) })]
    static class GenConstruct_CanConstruct_Patch
    {
        private static readonly List<string> tmpIdeoMemberNames = new List<string>();

        public static void Postfix(ref bool __result, Thing t, Pawn p, bool checkSkills, bool forced)
        {
            if (!__result)
            {
                return;
            }

            var frame = t as Frame;
            if (frame == null || frame.Faction != Faction.OfPlayer)
            {
                return;
            }

            var thingDef = frame.def.entityDefToBuild as ThingDef;
            if (thingDef == null)
            {
                return;
            }

            var frameStyle = frame.StyleDef;
            var builderStyle = p.Ideo?.GetStyleFor(thingDef);

            if (frameStyle == builderStyle)
            {
                return;
            }

            __result = false;

            tmpIdeoMemberNames.Clear();
            foreach (var ideo in Find.IdeoManager.IdeosListForReading)
            {
                if (ideo.GetStyleFor(thingDef) == frameStyle)
                {
                    tmpIdeoMemberNames.Add(ideo.memberName);
                }
            }
            if (tmpIdeoMemberNames.Any())
            {
                JobFailReason.Is("OnlyMembersCanBuild".Translate(tmpIdeoMemberNames.ToCommaList(useAnd: true)));
            }
        }
    }
}
