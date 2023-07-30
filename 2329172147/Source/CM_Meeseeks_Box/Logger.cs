﻿using System;

using Verse;

namespace CM_Meeseeks_Box
{
    public static class Logger
    {
        public static bool WarningEnabled = true;
        public static bool ErrorEnabled = true;

        public static void MessageFormat(object caller, string message, params object[] stuff)
        {
            if (MeeseeksMod.settings.showDebugLogMessages)
            {
                message = caller.GetType().ToString() + "." + (new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod().Name + " - " + message;
                Log.Message(String.Format(message, stuff));
            }
        }

        public static void MessageFormat(string message, params object[] stuff)
        {
            if (MeeseeksMod.settings.showDebugLogMessages)
            {
                message = (new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod().Name + " - " + message;
                Log.Message(String.Format(message, stuff));
            }
        }

        public static void WarningFormat(object caller, string message, params object[] stuff)
        {
            if (Logger.WarningEnabled)
            {
                message = caller.GetType().ToString() + "." + (new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod().Name + " - " + message;
                Log.Warning(String.Format(message, stuff));
            }
        }

        public static void ErrorFormat(object caller, string message, params object[] stuff)
        {
            if (Logger.ErrorEnabled)
            {
                message = caller.GetType().ToString() + "." + (new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod().Name + " - " + message;
                Log.Error(String.Format(message, stuff));
            }
        }
    }
}
