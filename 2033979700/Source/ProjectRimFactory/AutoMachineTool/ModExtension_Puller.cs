﻿using Verse;

namespace ProjectRimFactory.AutoMachineTool
{
    public class ModExtension_Puller : DefModExtension
    {
        public bool outputSides = false;


        public IntVec3 GetOutputCell(IntVec3 pos, Rot4 rot, bool is_right = false)
        {
            if (outputSides)
            {
                RotationDirection dir = RotationDirection.Clockwise;
                if (!is_right)
                {
                    dir = RotationDirection.Counterclockwise;
                }
                return pos + rot.RotateAsNew(dir).FacingCell;
            }
            else
            {
                return pos + rot.FacingCell;
            }
        }
    }
}
