﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

/// <summary>
/// Thank you to RunAndGun author "roolo" on Steam for allowing me to use his code!
/// Anything below this point is his code merely slightly changed to work with my assembly!
/// </summary>

namespace UseYourGun
{
    public static class Extensions
    {
        public static bool Contains(this Rect rect, Rect otherRect)
        {
            if (!rect.Contains(new Vector2(otherRect.xMin, otherRect.yMin)))
                return false;
            if (!rect.Contains(new Vector2(otherRect.xMin, otherRect.yMax)))
                return false;
            if (!rect.Contains(new Vector2(otherRect.xMax, otherRect.yMax)))
                return false;
            if (!rect.Contains(new Vector2(otherRect.xMax, otherRect.yMin)))
                return false;
            return true;
        }

    }
}
