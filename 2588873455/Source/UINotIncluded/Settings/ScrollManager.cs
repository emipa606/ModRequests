using System.Collections.Generic;
using UnityEngine;

namespace UINotIncluded
{
    public static class ScrollManager
    {
        private static readonly Dictionary<int, ScrollInstance> instances = new Dictionary<int, ScrollInstance>();

        public static ScrollInstance GetInstance(int i)
        {
            if (!instances.ContainsKey(i)) instances.Add(i, new ScrollInstance { pos = new Vector2() });
            return instances[i];
        }
    }

    public class ScrollInstance
    {
        public Vector2 pos;
    }
}