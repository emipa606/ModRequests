using System;
using UnityEngine;
namespace HairStyling
{
    public class ColourWrapper
    {
        public Color Color
        {
            get;
            set;
        }
        public ColourWrapper(Color color)
        {
            this.Color = color;
        }
    }
}
