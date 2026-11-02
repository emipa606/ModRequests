using Verse;
using UnityEngine;

namespace JobEffects
{
    /// <summary>
    /// Small shared perf primitives (Stage 2). These consolidate two patterns that were being
    /// hand-rolled in several places, so new features inherit the correct behaviour instead of
    /// re-implementing (and subtly mis-implementing) them.
    ///
    /// Deliberately NOT included: a <c>ConditionalWeakTable&lt;Map,T&gt;</c> map-comp resolver and a
    /// generic stamped memo cache. This mod has no MapComponent and no un-served derived-math memo
    /// site, so adding them now would be dead code. Add them at the first real call site.
    /// </summary>
    ///
    /// <summary>
    /// "Is this the first call this Unity frame?" Alloc-free, default-safe. Replaces the
    /// <c>static int fooFrame = -1; if (f != fooFrame) { fooFrame = f; … }</c> idiom.
    ///
    /// Internally stores <c>Time.frameCount + 1</c> so that a default(FrameGate) (== 0) always reads
    /// as "never advanced" — frameCount+1 is always ≥ 1 — without needing a -1 initializer that a
    /// struct's zero-default can't express.
    /// </summary>
    public struct FrameGate
    {
        private int stamp;   // Time.frameCount + 1; 0 = never advanced

        /// <summary>Returns true exactly once per Unity frame (the first caller); false afterwards.</summary>
        public bool Advance()
        {
            int s = Time.frameCount + 1;
            if (s == stamp) return false;
            stamp = s;
            return true;
        }

        /// <summary>Peek whether the next <see cref="Advance"/> would return true, without consuming it.</summary>
        public bool WouldAdvance => stamp != Time.frameCount + 1;
    }

    /// <summary>
    /// Lazily-resolved, cached Def lookup. Replaces the repeated
    /// <c>static Foo x; static bool xResolved; if (!xResolved) { xResolved = true; x = DefDatabase…; }</c>
    /// idiom with a single field. A class (not a struct) on purpose: one tiny heap alloc at static
    /// init avoids the "property getter mutates a struct copy → never caches" footgun, and def
    /// lookups are cold (startup / first-use), so the alloc is irrelevant.
    ///
    /// Note: use this for SINGLE defs. A batch of related defs behind one shared <c>bool resolved</c>
    /// guard (see ToolAnimator.ResolvePale) is intentionally left as-is — one guard for ten lookups
    /// is cheaper than ten LazyDef fields.
    /// </summary>
    public sealed class LazyDef<T> where T : Def
    {
        private readonly string defName;
        private T value;
        private bool resolved;

        public LazyDef(string defName) { this.defName = defName; }

        public T Value
        {
            get
            {
                if (!resolved)
                {
                    resolved = true;
                    value = DefDatabase<T>.GetNamedSilentFail(defName);
                }
                return value;
            }
        }

        public static implicit operator T(LazyDef<T> d) => d?.Value;
    }
}
