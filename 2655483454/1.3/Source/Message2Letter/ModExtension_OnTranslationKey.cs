using System;
using Verse;
using System.Collections.Generic;

namespace Message2Letter
{
    public class ModExtension_OnTranslationKey : DefModExtension
    {
        /// <summary>
        /// List of Incidents to be converted to Letter
        /// </summary>
        public List<string> eventKeyList;
    }
}
