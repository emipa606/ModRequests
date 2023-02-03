using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ChickenExplosives
{
    public class CompRemoteTrigger : ThingComp
    {
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Command_Action comp = GizmoManager.remoteTriggerGizmos[base.parent.def];
            comp.action = delegate
            {
                Thing toDetonate = base.parent;
                CompProjectileSprayer proj = toDetonate.TryGetComp<CompProjectileSprayer>();
                if (!(proj?.fired ?? true)) proj?.Fire();
                CompExplosive expl = toDetonate.TryGetComp<CompExplosive>();
                if (!(expl?.wickStarted ?? true)) expl.StartWick();
            };
            yield return comp;
        }
    }
    public class CompProperties_RemoteTrigger : CompProperties
    {
        public string labelKey, descKey, iconPath;
        public KeyBindingDef keyBinding = null;
        public SoundDef activateSound = null;

        public CompProperties_RemoteTrigger()
        {
            base.compClass = typeof(CompRemoteTrigger);
        }
    }
}
