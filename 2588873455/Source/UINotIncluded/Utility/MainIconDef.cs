using UnityEngine;
using Verse;

namespace UINotIncluded
{
    public class MainIconDef : Def
    {
        private Texture2D _icon;
        public string path;

        public Texture2D Icon
        {
            get
            {
                if (_icon == null) _icon = ContentFinder<Texture2D>.Get(path);
                return _icon;
            }
        }
    }
}