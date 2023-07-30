using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using RimWorld;
using Verse;

namespace CM_Meeseeks_Box
{
    public class CompProperties_HasButton : CompProperties
    {
        [NoTranslate]
        public string commandTexture = "";

        [NoTranslate]
        public string commandLabelKey = "CM_Meeseeks_Box_PressButtonLabel";

        [NoTranslate]
        public string commandDescKey = "CM_Meeseeks_Box_PressButtonDescription";


        public int useDuration = 100;

        [NoTranslate]
        // Signal to broadcast when turned 'on'
        public string onSignal = "CM_Meeseeks_Box_Button_On";

        [NoTranslate]
        // Signal to broadcast when turned 'off'
        public string offSignal = "CM_Meeseeks_Box_Button_Off";

        // Should an extra signal be sent with the presser's id
        public bool sendPresserSignal = false;

        // Prefix to add on to signal sent when signalling presser
        public string presserSignalPrefix = "CM_Meeseeks_Box_Button_Presser";

        // Should the command button show the on/off state of the box?
        public bool toggleable = true;

        public CompProperties_HasButton()
        {
            compClass = typeof(CompHasButton);
        }
    }
}
