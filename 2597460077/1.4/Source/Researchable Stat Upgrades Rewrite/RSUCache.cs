using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchableStatUpgrades
{
    public static class RSUCache
    {
        public static void TryChangeOrAddValue(string str,object val1)
        {
            if (cache.ContainsKey(str))
            {
                cache[str] = val1;
            }
            else
            {
                cache.Add(str, val1);
            }
        }

        public const string DefEditingResearchManagerSaveString = "WC_DERM_Dictionary";
        public static Dictionary<string, object> cache = new Dictionary<string, object>();
    }
}
