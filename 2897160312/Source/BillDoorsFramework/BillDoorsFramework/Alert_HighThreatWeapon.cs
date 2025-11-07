using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using static HarmonyLib.Code;

namespace BillDoorsFramework
{
    public class Alert_HighThreatWeapon : Alert
    {
        public static void AddThing(Thing thing, Pawn p)
        {
            Things.AddDistinct(thing, p);
        }

        public static void RemoveThing(Thing thing)
        {
            Things.Remove(thing);
        }

        public static readonly List<Thing> removeThings = new List<Thing>();
        public static readonly Dictionary<Thing, Pawn> Things = new Dictionary<Thing, Pawn>();
        StringBuilder stringBuilder = new StringBuilder();

        public Alert_HighThreatWeapon()
        {
            defaultLabel = "Enemies holding dangerous weapon";
        }

        public override AlertReport GetReport()
        {
            SelfCheck();
            return AlertReport.CulpritsAre(Things.Values.ToList());
        }

        public override TaggedString GetExplanation()
        {
            return "BDF_HighThreatWeaponAlert".Translate() + ":\n" + WeaponHolderExpl() + "\n\n" + "BDF_HighThreatWeaponAlertDesc".Translate();
        }

        public void SelfCheck()
        {
            foreach (var v in Things)
            {
                if (v.Key.ParentHolder == null)
                {
                    removeThings.Add(v.Key);
                    continue;
                }
                if (!v.Value.Spawned)
                {
                    removeThings.Add(v.Key);
                    continue;
                }
            }
            foreach (var r in removeThings)
            {
                Things.Remove(r);
            }
            removeThings.Clear();
        }

        string WeaponHolderExpl()
        {
            stringBuilder.Clear();
            foreach (var v in Things)
            {
                stringBuilder.AppendLine("BDF_HighThreatWeaponAlertLine".Translate(v.Value.NameShortColored, v.Key.Label));
            }
            return stringBuilder.ToString();
        }

        public override string GetLabel()
        {
            if (Things.NullOrEmpty()) return defaultLabel;
            var str = Things.Count > 1 ? "BDF_HighThreatWeaponAlertEtc".Translate() : null;
            return "BDF_HighThreatWeaponAlertLabel".Translate(Things.First().Key.Label) + str;
        }
    }
}
