using Verse;

namespace BillDoorsFramework
{
    public class LogMessager
    {
        int i = 0;

        public LogMessager()
        {
            Log.Message("============");
        }

        public LogMessager(string s)
        {
            Log.Message($"======{s}======");
        }

        public void DoLog()
        {
            DoLog("");
        }

        public void DoLog(string s)
        {
            i++;
            Log.Message(">" + s + i.ToString());
        }
    }
}
