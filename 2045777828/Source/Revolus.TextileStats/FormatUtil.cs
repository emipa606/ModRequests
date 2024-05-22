using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace Revolus.TextileStats {
    public class FormatUtil {
        private static string FormatTemperatur(float v) {
            switch (Prefs.TemperatureMode) {
                case TemperatureDisplayMode.Fahrenheit:
                    return $"{v * (9f / 5f):0} °F";
                case TemperatureDisplayMode.Kelvin:
                    return $"{v:0} K";
                case TemperatureDisplayMode.Celsius:
                default:
                    return $"{v:0} °C";
            }
        }

        private static string FormatPercent(float v) => $"{v * 100:0} %";
        
        private static string FormatMass(float v) => $"{v:0.000} kg";
        
        private static string FormatSilver(float v) => $"{v:0.00} $";
        
        private static string FormatDecimal(float v) => $"{v:0.000}";

        private static string FormatDecimal1(float v) => $"{v:0.0}";

        private static string FormatInteger(float v) => $"{v:0}";

        private static string FormatScientific(float v) => $"{v:0.000E+00}";

        private static string FormatRatePerDay(float v) => $"{v:0.00} / d";

        private static string FormatSignedDecimal(float v) => $"{v:+0.000;0.000;0}";

        private static string FormatSignedDecimal1(float v) => $"{v:+0.0;0.0;0}";

        private static string FormatSeconds(float v) => $"{v:0.0} s";

        private static string FormatCentimeters(float v) {
            var absV = (int) (Math.Abs(v) + 0.5);
            if (absV < 10) {
                return $"{v * 100:0.0} cm";
            } else if (absV < 1000) {
                return $"{v:0.0} m";
            } else {
                return $"{v * (1f / 1000f):0.0} km";
            }
        }

        public static readonly Dictionary<string, Func<float, string>> Formatters = new Dictionary<string, Func<float, string>> {
            { "silver", FormatSilver },
            { "mass", FormatMass },
            { "percent", FormatPercent },
            { "temperature", FormatTemperatur },
            { "decimal", FormatDecimal },
            { "decimal1", FormatDecimal1 },
            { "integer", FormatInteger },
            { "scientific", FormatScientific },
            { "rate_per_day", FormatRatePerDay },
            { "signed_decimal", FormatSignedDecimal },
            { "signed_decimal1", FormatSignedDecimal1 },
            { "seconds", FormatSeconds },
            { "centimeters", FormatCentimeters },
        };

        public static string DoFormat(KindAndDef kindAndDef, float? v) {
            if (v is null) {
                return null;
            }
            
            var value = (float) v;
            var settings = TextileStatsMod.settings;

            if (kindAndDef is null) {
                return Formatters[settings.UnknownFormat](value);
            }

            var preferredFormatter = settings.StatFormatFor(kindAndDef);
            if (preferredFormatter != null && Formatters.TryGetValue(preferredFormatter, out var formatter)) {
                return formatter(value);
            }

            var def = (StatDef) kindAndDef.def;
            return GenText.ToStringByStyle(value, def.toStringStyle, def.toStringNumberSense);
        }
    }
}
