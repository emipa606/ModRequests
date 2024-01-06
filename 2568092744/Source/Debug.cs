namespace EatAnything
{
    public static class Debug
    {
        public static void Log(string message)
        {
#if DEBUG
            Verse.Log.Message($"[{EatAnythingMod.PACKAGE_NAME}] {message}");
#endif
        }
    }
}
