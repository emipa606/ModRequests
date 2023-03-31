namespace AnimalFilthDontCare
{
    public static class Debug
    {
        public static void Log(string message)
        {
#if DEBUG
            Verse.Log.Message($"[{AnimalFilthDontCareMod.PACKAGE_NAME}] {message}");
#endif
        }
    }
}
