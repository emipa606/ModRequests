using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Message2Letter
{
    // Patch of this makes the following assumptions:
    // 1) Translated string is immediately/close-to-immediately consumed
    // 2) no other message will be made before the message is consumed
    [HarmonyPatch(typeof(TranslatorFormattedStringExtensions))]
    public static class Patch_TranslatorFormattedStringExtensions
    {
        public static void Postfix_Translate(ref TaggedString __result, string key, NamedArgument[] args)
        {
            if (TranslationKeyObserver.ShouldBeObservingNow && TranslationKeyObserver.translateToIncident.TryGetValue(key, out IncidentDef incident))
            {
                TranslationKeyObserver.currentIncident = new CurrentIncident
                {
                    incidentDef = incident,
                    key = key,
                    namedArguments = args,
                };
            }
        }

        // Code below intercepts all translate call and then call `Postfix_Translate`
        // static method above
        // Reasoning:
        // 1) Since it is the most common method called before message is
        //    used/consumed.
        // 2) In theory (not yet tested) this will allow for compatibility
        //    for other languages as well.

        [HarmonyPostfix]
        [HarmonyPatch("Translate")]
        [HarmonyPatch(
            new Type[] {
                typeof(string),
                typeof(NamedArgument), // arg1
            },
            new ArgumentType[] {
                ArgumentType.Normal,
                ArgumentType.Normal,   // arg1
            }
        )]
        public static void Postfix_1(ref TaggedString __result, string key, NamedArgument arg1)
        {
            Postfix_Translate(ref __result, key, new NamedArgument[] { arg1 });
        }

        [HarmonyPostfix]
        [HarmonyPatch("Translate")]
        [HarmonyPatch(
            new Type[] {
                typeof(string),
                typeof(NamedArgument), // arg1
                typeof(NamedArgument),
            },
            new ArgumentType[] {
                ArgumentType.Normal,
                ArgumentType.Normal,   // arg1
                ArgumentType.Normal,
            }
        )]
        public static void Postfix_2(ref TaggedString __result, string key, NamedArgument arg1, NamedArgument arg2)
        {
            Postfix_Translate(ref __result, key, new NamedArgument[] { arg1, arg2 });
        }

        [HarmonyPostfix]
        [HarmonyPatch("Translate")]
        [HarmonyPatch(
            new Type[] {
                typeof(string),
                typeof(NamedArgument), // arg1
                typeof(NamedArgument),
                typeof(NamedArgument),
            },
            new ArgumentType[] {
                ArgumentType.Normal,
                ArgumentType.Normal,   // arg1
                ArgumentType.Normal,
                ArgumentType.Normal,
            }
        )]
        public static void Postfix_3(ref TaggedString __result, string key, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3)
        {
            Postfix_Translate(ref __result, key, new NamedArgument[] { arg1, arg2, arg3 });
        }

        [HarmonyPostfix]
        [HarmonyPatch("Translate")]
        [HarmonyPatch(
            new Type[] {
                typeof(string),
                typeof(NamedArgument), // arg1
                typeof(NamedArgument),
                typeof(NamedArgument),
                typeof(NamedArgument),
            },
            new ArgumentType[] {
                ArgumentType.Normal,
                ArgumentType.Normal,   // arg1
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,
            }
        )]
        public static void Postfix_4(ref TaggedString __result, string key, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4)
        {
            Postfix_Translate(ref __result, key, new NamedArgument[] { arg1, arg2, arg3, arg4 });
        }

        [HarmonyPostfix]
        [HarmonyPatch("Translate")]
        [HarmonyPatch(
            new Type[] {
                typeof(string),
                typeof(NamedArgument), // arg1
                typeof(NamedArgument),
                typeof(NamedArgument),
                typeof(NamedArgument),
                typeof(NamedArgument), // arg5
            },
            new ArgumentType[] {
                ArgumentType.Normal,
                ArgumentType.Normal,   // arg1
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,   // arg5
            }
        )]
        public static void Postfix_5(ref TaggedString __result, string key, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4, NamedArgument arg5)
        {
            Postfix_Translate(ref __result, key, new NamedArgument[] { arg1, arg2, arg3, arg4, arg5 });
        }

        [HarmonyPostfix]
        [HarmonyPatch("Translate")]
        [HarmonyPatch(
            new Type[] {
                typeof(string),
                typeof(NamedArgument), // arg1
                typeof(NamedArgument),
                typeof(NamedArgument),
                typeof(NamedArgument),
                typeof(NamedArgument), // arg5
                typeof(NamedArgument),
            },
            new ArgumentType[] {
                ArgumentType.Normal,
                ArgumentType.Normal,   // arg1
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,   // arg5
                ArgumentType.Normal,
            }
        )]
        public static void Postfix_6(ref TaggedString __result, string key, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4, NamedArgument arg5, NamedArgument arg6)
        {
            Postfix_Translate(ref __result, key, new NamedArgument[] { arg1, arg2, arg3, arg4, arg5, arg6 });
        }

        [HarmonyPostfix]
        [HarmonyPatch("Translate")]
        [HarmonyPatch(
            new Type[] {
                typeof(string),
                typeof(NamedArgument), // arg1
                typeof(NamedArgument),
                typeof(NamedArgument),
                typeof(NamedArgument),
                typeof(NamedArgument), // arg5
                typeof(NamedArgument),
                typeof(NamedArgument),
            },
            new ArgumentType[] {
                ArgumentType.Normal,
                ArgumentType.Normal,   // arg1
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,   // arg5
                ArgumentType.Normal,
                ArgumentType.Normal,
            }
        )]
        public static void Postfix_7(ref TaggedString __result, string key, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4, NamedArgument arg5, NamedArgument arg6, NamedArgument arg7)
        {
            Postfix_Translate(ref __result, key, new NamedArgument[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7 });
        }

        [HarmonyPostfix]
        [HarmonyPatch("Translate")]
        [HarmonyPatch(
            new Type[] {
                typeof(string),
                typeof(NamedArgument), // arg1
                typeof(NamedArgument),
                typeof(NamedArgument),
                typeof(NamedArgument),
                typeof(NamedArgument), // arg5
                typeof(NamedArgument),
                typeof(NamedArgument),
                typeof(NamedArgument),
            },
            new ArgumentType[] {
                ArgumentType.Normal,
                ArgumentType.Normal,   // arg1
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,   // arg5
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,
            }
        )]
        public static void Postfix_8(ref TaggedString __result, string key, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4, NamedArgument arg5, NamedArgument arg6, NamedArgument arg7, NamedArgument arg8)
        {
            Postfix_Translate(ref __result, key, new NamedArgument[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 });
        }

        [HarmonyPostfix]
        [HarmonyPatch("Translate")]
        [HarmonyPatch(
            new Type[] {
                typeof(string),
                typeof(NamedArgument[]), // arg1
            },
            new ArgumentType[] {
                ArgumentType.Normal,
                ArgumentType.Normal,   // arg1
            }
        )]
        public static void Postfix_9(ref TaggedString __result, string key, NamedArgument[] args)
        {
            Postfix_Translate(ref __result, key, args);
        }
    }
}
