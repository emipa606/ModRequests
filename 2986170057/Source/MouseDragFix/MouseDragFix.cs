using HarmonyLib;
using System.Reflection;
using UnityEngine;
using Verse;
using System;

namespace MouseDragFix
{
    [StaticConstructorOnStartup]
    public class MouseDragFixBase
    {
        static MouseDragFixBase() {
            (new Harmony("Almantuxas.MouseDragFix")).PatchAll(Assembly.GetExecutingAssembly());
        }

        [HarmonyPatch(typeof(Root), nameof(Root.OnGUI))]
        internal class Harmony_Root_OnGUI
        {
            public static bool allowMouseDrag = false;
            public static bool allowLayout = true;

            public static bool Prefix() {
                // RimWorld already implemented a fix for inconsistent mouse positions reported by events,
                // except it is triggered at a slightly later point during UI rendering than this prefix.
                // Still, calling it sooner does not break anything.
                Vector2 currentMousePos = UI.MousePositionOnUIInverted;
                Vector2 eventMousePos = Event.current.mousePosition;
                Event.current.mousePosition = currentMousePos;

                switch (Event.current.type) {
                    case (EventType.MouseDown): // Allow mouse drag event to pass through on mouse down
                        allowMouseDrag = true;
                        return true;

                    case (EventType.Repaint):   // Allow mouse drag event to pass through on repaint
                        allowMouseDrag = true;
                        return true;

                    case (EventType.MouseDrag):
                        Event.current.delta += currentMousePos - eventMousePos; // Update the delta after the position was overwritten
                        if (!allowMouseDrag) {  // Disallow the next layout event (which always follows after a mouse drag event)
                            allowLayout = false;
                            return false;
                        }
                        allowMouseDrag = false;
                        return true;

                    case (EventType.Layout):    // Excessive layout events appear to cause lag when expanding zones
                        if (allowLayout) {
                            return true;
                        }
                        allowLayout = true;
                        return false;
                }

                return true;
            }
        }
    }
}
