using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RedAlert
{
    public class CompRedAlertToggleable : ThingComp
    {
        public bool toggled = true;
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Toggle
            {
                defaultLabel = "Toggle alerts",
                isActive = () => toggled,
                icon = ContentFinder<Texture2D>.Get("UI/alert_toggle"),
                toggleAction = () => 
                { 
                    toggled = !toggled;
                    if (!toggled)
                    {
                        var mapComp = this.parent.Map.GetComponent<MapComponent_RedAlert>();
                        foreach (var savedLamp in mapComp.savedColorsPerLamps.ToList())
                        {
                            if (savedLamp.Key == this.parent)
                            {
                                mapComp.ResetLamp(savedLamp);
                            }
                        }
                    }
                }
            };
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref toggled, "toggled", true);
        }
    }
}
