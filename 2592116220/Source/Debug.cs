namespace BuriedAlive
{
    public static class Debug
    {
        public static void Log(string message)
        {
#if DEBUG
            Verse.Log.Message($"[{BuriedAliveMod.PACKAGE_NAME}] {message}");
#endif
        }
    }
}
