using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace JobEffects
{
    /// <summary>
    /// Resolves a representative colour for whatever a colonist is working, so chips/shards
    /// can be tinted to match (granite grey, marble pale, jade green, wood species, …).
    /// Falls back through Stuff colour → the thing's drawn colour, then lifts very dark values
    /// so the tinted pale-chip mote stays visible against the ground.
    /// </summary>
    public static class MaterialColor
    {
        public static bool TryResolve(Thing t, out Color col)
        {
            col = Color.white;
            if (t == null) return false;

            // Stuffed things (buildings, blueprints, furniture being deconstructed/smoothed).
            ThingDef stuff = t.Stuff;
            if (stuff?.stuffProps != null)
            {
                col = Lift(stuff.stuffProps.color);
                return true;
            }

            // Mineable rock, plants, and anything else: use the rendered colour.
            try
            {
                Color dc = t.DrawColor;
                // Near-pure-white DrawColor usually means "no meaningful colour" — skip it.
                if (dc.r > 0.96f && dc.g > 0.96f && dc.b > 0.96f) return false;
                col = Lift(dc);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Raise value so a dark material doesn't multiply the pale chip into invisibility,
        // while keeping the hue/saturation that makes it read as "that material".
        private static Color Lift(Color c)
        {
            Color.RGBToHSV(c, out float h, out float s, out float v);
            if (v < 0.50f) v = 0.50f;
            if (s > 0.92f) s = 0.92f;
            Color outc = Color.HSVToRGB(h, s, v);
            outc.a = 1f;
            return outc;
        }

        // --- Foliage colour sampling -------------------------------------------------
        // Trees & crops are pre-coloured Graphic_Random sprites (graphicData.color is white),
        // so DrawColor tells us nothing. To tint chopped leaves to the ACTUAL species colour we
        // sample the plant's texture once and cache a green-biased average per texture.
        private static readonly Dictionary<string, Color?> foliageCache = new Dictionary<string, Color?>();

        /// <summary>
        /// Representative leaf/foliage colour of a plant being worked (oak green, pine deep-green,
        /// rice paddy green, …). Returns false for non-plants or when the texture can't be read,
        /// so callers can fall back to a seasonal tint.
        /// </summary>
        public static bool TryResolveFoliage(Thing t, out Color col)
        {
            col = Color.white;
            if (t == null || !(t is Plant)) return false;

            Material mat;
            try { mat = t.Graphic?.MatSingleFor(t); }
            catch { mat = null; }
            Texture tex = mat?.mainTexture;
            if (tex == null) return false;

            string key = string.IsNullOrEmpty(tex.name) ? t.def.defName : tex.name;
            if (foliageCache.TryGetValue(key, out Color? cached))
            {
                if (!cached.HasValue) return false;
                col = cached.Value;
                return true;
            }

            if (SampleAverage(tex, foliageBias: true, out Color avg))
            {
                avg = Lift(avg);
                foliageCache[key] = avg;
                col = avg;
                return true;
            }
            foliageCache[key] = null;
            return false;
        }

        /// <summary>
        /// Representative TRUNK/wood colour of a tree being felled (oak tan, pine amber, teak
        /// dark-brown, ...). Samples the tree texture weighting brown/woody pixels so the canopy is
        /// ignored and chopped wood chips can be tinted to the real species wood. Trees only;
        /// returns false for crops/non-plants or when the texture can't be read.
        /// </summary>
        public static bool TryResolveTrunk(Thing t, out Color col)
        {
            col = Color.white;
            if (!(t is Plant plant) || plant.def?.plant == null || !plant.def.plant.IsTree) return false;

            Material mat;
            try { mat = t.Graphic?.MatSingleFor(t); }
            catch { mat = null; }
            Texture tex = mat?.mainTexture;
            if (tex == null) return false;

            string key = "trunk:" + (string.IsNullOrEmpty(tex.name) ? t.def.defName : tex.name);
            if (foliageCache.TryGetValue(key, out Color? cached))
            {
                if (!cached.HasValue) return false;
                col = cached.Value;
                return true;
            }

            if (SampleAverage(tex, foliageBias: false, trunkBias: true, out Color avg))
            {
                avg = Lift(avg);
                foliageCache[key] = avg;
                col = avg;
                return true;
            }
            foliageCache[key] = null;
            return false;
        }

        // Blit the texture into a small temporary RenderTexture so we can ReadPixels even when the
        // source Texture2D isn't CPU-readable (RimWorld assets aren't). One-time per texture; the
        // result is cached by the caller. foliageBias weights greener pixels so the canopy wins
        // over the trunk; trunkBias weights brown/woody pixels so the trunk wins over the canopy.
        private static bool SampleAverage(Texture tex, bool foliageBias, out Color result)
            => SampleAverage(tex, foliageBias, false, out result);

        private static bool SampleAverage(Texture tex, bool foliageBias, bool trunkBias, out Color result)
        {
            result = Color.white;
            int w = Mathf.Clamp(tex.width, 1, 48);
            int h = Mathf.Clamp(tex.height, 1, 48);
            RenderTexture prev = RenderTexture.active;
            RenderTexture rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);
            try
            {
                Graphics.Blit(tex, rt);
                RenderTexture.active = rt;
                var readable = new Texture2D(w, h, TextureFormat.RGBA32, false);
                readable.ReadPixels(new Rect(0f, 0f, w, h), 0, 0);
                readable.Apply(false);
                Color32[] px = readable.GetPixels32();
                Object.Destroy(readable);

                double rs = 0, gs = 0, bs = 0, ws = 0;
                for (int i = 0; i < px.Length; i++)
                {
                    Color32 p = px[i];
                    if (p.a < 128) continue;                     // skip transparent background
                    float pr = p.r / 255f, pg = p.g / 255f, pb = p.b / 255f;
                    float weight = 1f;
                    if (foliageBias)
                    {
                        float green = pg - 0.5f * (pr + pb);    // how green is this pixel
                        weight = Mathf.Max(0f, green) + 0.04f;   // small floor keeps non-green trees
                    }
                    else if (trunkBias)
                    {
                        // Woody pixels are brown: red leads, green is mid, blue is low, and they are
                        // NOT green-dominant. Weight by brownness so the canopy is rejected and we
                        // land on the real trunk colour. Small floor keeps bare/leafless trunks.
                        float brown = (pr - pb) - Mathf.Max(0f, pg - pr);
                        weight = Mathf.Max(0f, brown) + 0.03f;
                    }
                    rs += pr * weight; gs += pg * weight; bs += pb * weight; ws += weight;
                }
                if (ws <= 0.0001) return false;
                result = new Color((float)(rs / ws), (float)(gs / ws), (float)(bs / ws), 1f);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);
            }
        }
    }
}
