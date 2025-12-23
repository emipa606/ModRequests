using System.Runtime.CompilerServices;
using Verse;

namespace VPEArchocaster
{
    public class CachedResult<T>
    {
        public int lastTickCheck;
        private T _result;
        public bool CacheExpired
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return lastTickCheck == 0 || Find.TickManager.TicksGame > lastTickCheck + 60;
            }
        }

        public T Result
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _result;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _result = value;
                lastTickCheck = Find.TickManager.TicksGame;
            }
        }
    }
}
