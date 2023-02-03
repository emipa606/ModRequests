using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EBTools
{
    public static class EBExtensions
    {
        public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        public static List<T> OrEmptyIfNull<T>(this List<T> source)
        {
            return source ?? new List<T>();
        }

        public static int AsInt(this bool source)
        {
            return source? 1 : 0;
        }

        
    }
}
