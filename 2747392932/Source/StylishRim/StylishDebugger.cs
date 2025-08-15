using Verse;

namespace StylishRim
{
    public static class StylishDebugger
    {
        public static void LogError(string s)
        {
            Log.Error("[Stylish Rim] " + s);
        }
        public static void LogWarning(string s)
        {
            Log.Warning("[Stylish Rim] " + s);
        }
        public static void LogMessage(string s)
        {
            Log.Message("[Stylish Rim] " + s);
        }
    }
}
