using System;
using System.Collections.Generic;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Callouts
{
    public class MoteAttachedText : MoteAttached
    {
        public string text;

        public Color textColor = Color.white;

        public float overrideTimeBeforeStartFadeout = -1f;

        protected float TimeBeforeStartFadeout
        {
            get
            {
                if (!(overrideTimeBeforeStartFadeout >= 0f))
                {
                    return base.SolidTime;
                }
                return overrideTimeBeforeStartFadeout;
            }
        }

        protected override bool EndOfLife => base.AgeSecs >= TimeBeforeStartFadeout + def.mote.fadeOutTime;

        public override void Draw()
        {
        }

        public override void DrawGUIOverlay()
        {
            float a = 1f - (base.AgeSecs - TimeBeforeStartFadeout) / def.mote.fadeOutTime;
            CalloutUtility.DrawText(new Vector2(exactPosition.x, exactPosition.z), text, new Color(textColor.r, textColor.g, textColor.b, a));
        }
    }
}
