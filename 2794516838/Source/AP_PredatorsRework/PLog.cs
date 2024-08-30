using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AP_PredatorsRework
{
    public static class PLog
    {
        public static bool predDebug = false;
        public static void M(string t)
        {
            if(predDebug==true)
            {
                Verse.Log.Message("[DEBUG][Predators Unleashed] "+t);
            }
            
        }
    }
}
