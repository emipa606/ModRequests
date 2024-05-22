using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Verse;

namespace TakeOffIndoor
{
    public static class debug
    {
        static bool first = true;
        public static bool deb = false;

        public  static void WriteLine(string mes)
        {
            if (deb)
            {
                StreamWriter sw = new StreamWriter("TakeOffTest.log", !first);
                first = false;
                sw.WriteLine(DateTime.Now.ToString("HH:mm:ss ") + mes);
                sw.Close();

                //Log.Message(mes);
            }
            else
            {
                if (first)
                {
                    try
                    {
                        if (File.Exists("TakeOffTest.log"))
                        {
                            File.Delete("TakeOffTest.log");
                        }
                    }
                    catch
                    {

                    }
                    first = false;
                }
            }
        }
        //RealTime.frameCountでなにかできそう
    }
}
