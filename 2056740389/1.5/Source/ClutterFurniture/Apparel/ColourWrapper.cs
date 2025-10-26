using System;
using UnityEngine;
namespace ApparelColorChange
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
