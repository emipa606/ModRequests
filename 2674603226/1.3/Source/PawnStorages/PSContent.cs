using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PawnStorages
{
    [StaticConstructorOnStartup]
    public static class PSContent
    {
        public static Texture2D DirectionArrow = ContentFinder<Texture2D>.Get("UI/Direction");
    }
}
