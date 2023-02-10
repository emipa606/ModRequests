using HugsLib;
using HugsLib.Utils;
using SearchAndDestroy.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchAndDestroy
{
    class Base : ModBase
    {
        public override string ModIdentifier => "SearchAndDestroy";
        public static Base Instance { get; private set; }
        private ExtendedDataStorage _extendedDataStorage;

        public Base()
        {
            Instance = this;
        }
        public ExtendedDataStorage GetExtendedDataStorage()
        {
            return _extendedDataStorage;
        }
        public override void WorldLoaded()
        {
            _extendedDataStorage = UtilityWorldObjectManager.GetUtilityWorldObject<ExtendedDataStorage>();
            base.WorldLoaded();
        }
    }


}
