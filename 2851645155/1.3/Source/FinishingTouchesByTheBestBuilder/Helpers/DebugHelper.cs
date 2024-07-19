#if DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinishingTouchesByTheBestBuilder
{
    internal static class DebugHelper
    {
        public static void AddLog(string text)
        {
            WriteLog(text);
        }
        public static void AddLogEntry()
        {

            WriteLog($"{Environment.NewLine}======== {DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} ========");
        }

        static void WriteLog(string text)
        {
            using (StreamWriter sw = new StreamWriter("FT_DEBUG.txt", true, Encoding.Default))
            {
                sw.WriteLine(text);
            }
        }
    }
}
#endif