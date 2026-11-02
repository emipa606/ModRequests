using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace JobEffects
{
    /// <summary>
    /// "Soft arms" — translucent, body-coloured limbs that bridge each shoulder to the SMYH hand
    /// gripping the animated tool. Drawn only while we draw hands (so only when Show Me Your Hands
    /// is loaded). Each arm is a tapered, feather-edged Bezier ribbon: one end pinned at a
    /// body-type-aware shoulder anchor, the other pinned to the wrist; the middle bows outward and
    /// down and FLEXES with the swing phase so it reads as a gestural reach, not a rigid bar.
    /// Colour is taken from the pawn's outermost torso garment (jacket > shirt > bare skin).
    /// </summary>
    [StaticConstructorOnStartup]
    public static class ArmRenderer
    {
        // ---- Sleeve texture (soft radial-feathered blob, tinted at runtime) ----
        private static readonly Texture2D SleeveTex = ContentFinder<Texture2D>.Get("UI/JE_ArmSleeve", false);

        // Material cache keyed by the baked sleeve colour (rgb + alpha = opacity slider).
        private static readonly Dictionary<Color, Material> mats = new Dictionary<Color, Material>();

        // ---- Mesh pool: Graphics.DrawMesh is deferred and references the Mesh, so each arm drawn
        // in a frame needs its own Mesh. We cycle a growing pool, resetting the cursor each frame. ----
        private const int Segments = 6;                 // bezier subdivisions (6 keeps the curve smooth while cutting the per-arm vertex/upload cost ~25% vs 8)
        private const int VertCount = (Segments + 1) * 2;
        private static readonly List<Mesh> pool = new List<Mesh>();
        private static int poolCursor;
        private static int lastFrame = -1;
        private static readonly int[] sharedTris = BuildTris();
        private static readonly Vector3[] vbuf = new Vector3[VertCount];
        private static readonly Vector2[] uvbuf = new Vector2[VertCount];

        private static int[] BuildTris()
        {
            int[] t = new int[Segments * 6];
            int k = 0;
            for (int i = 0; i < Segments; i++)
            {
                int l0 = i * 2, r0 = i * 2 + 1, l1 = (i + 1) * 2, r1 = (i + 1) * 2 + 1;
                // CCW-from-above winding to match MeshPool planes (front face toward the camera).
                t[k++] = l0; t[k++] = l1; t[k++] = r1;
                t[k++] = l0; t[k++] = r1; t[k++] = r0;
            }
            return t;
        }

        private static Mesh NextMesh()
        {
            int frame = Time.frameCount;
            if (frame != lastFrame) { lastFrame = frame; poolCursor = 0; }
            Mesh m;
            if (poolCursor < pool.Count) m = pool[poolCursor];
            else { m = new Mesh { name = "JE_Arm" }; m.MarkDynamic(); pool.Add(m); }
            poolCursor++;
            return m;
        }

        // ---- Per-body-type shoulder placement (in cells, scaled by the body draw width) ----
        // halfWidth = half the shoulder-to-shoulder span; lift = up-screen offset from the body
        // centre to the shoulder line; forward = how far the roots sit toward the front of the
        // torso (applied only in profile / E-W facings, where it reads). These are body-LOCAL and
        // get placed against the pawn's actual rendered FACING, so they weld to the body sprite.
        // halfWidth: deltoid outer edge from body centre (world cells, south sprite).
        // lift: shoulder height above the body draw centre.  forward: profile front-nudge.
        // widthMul: per-build arm THICKNESS multiplier (beefy Hulk/Fat vs slim Thin).
        // All are placed against the rendered facing + scaled by the body draw width, so they
        // track each body type's silhouette. The vanilla adult bodies all share the 1.5 mesh
        // width, so the per-type differences live entirely in this table.
        // The forearm size/placement tracks the pawn's ACTUAL rendered body size, not just the vanilla
        // 1.5-cell adult reference. On top of the life-stage mesh width (children), this folds in every
        // engine scale path BEYOND the mesh so genes/xenotypes/HAR that resize a vanilla-bodied pawn
        // scale the forearms with them: the body graphic's own drawSize (texture-resize mods), and the
        // body render node's debugScale + drawData scale (Biotech body-size genes, bodyTypeScales, size
        // mods). Every one of these is 1.0 on a stock adult, so there is zero change to the existing
        // per-body-type calibration. Read defensively — the render tree may not be resolved yet.
        private static float BodyDrawScale(Pawn pawn)
        {
            float factor = HumanlikeMeshPoolUtility.HumanlikeBodyWidthForPawn(pawn) / 1.5f;
            try
            {
                var rend = pawn.Drawer?.renderer;
                Graphic bg = rend?.BodyGraphic;
                if (bg != null && bg.drawSize.x > 0.01f) factor *= bg.drawSize.x;   // vanilla body graphic = (1,1)
                var rt = rend?.renderTree;
                if (rt != null && rt.TryGetNodeByTag(PawnRenderNodeTagDefOf.Body, out var bodyNode) && bodyNode != null)
                {
                    factor *= bodyNode.debugScale;
                    if (bodyNode.Props?.drawData != null) factor *= bodyNode.Props.drawData.ScaleFor(pawn);
                }
            }
            catch { /* render tree mid-resolve; fall back to the mesh-width factor */ }
            return Mathf.Clamp(factor, 0.3f, 3f);
        }

        // ---- Per-pawn cache: body draw scale (reflection into the render tree) + sleeve colour
        // (apparel scan). Both are stable frame-to-frame, so recomputing them every render frame for
        // every working pawn was pure waste (and BodyDrawScale was computed TWICE per pawn per frame:
        // once as ToolAnimator.currentBodyScale, once inside DrawArms). Refreshed at most once per
        // BodyCacheTtl game ticks (and dropped on despawn via Forget); the opacity slider still applies
        // live because SleeveMat folds the CURRENT ArmOpacity onto the cached rgb. ----
        private struct CachedBody { public float scale; public Color sleeve; public int tick; }
        private static readonly Dictionary<int, CachedBody> bodyCache = new Dictionary<int, CachedBody>();
        private const int BodyCacheTtl = 60;   // ticks (~1s at 1x); body/apparel barely change faster

        private static CachedBody GetCached(Pawn pawn)
        {
            int id = pawn.thingIDNumber;
            int now = Find.TickManager != null ? Find.TickManager.TicksGame : 0;
            if (bodyCache.TryGetValue(id, out CachedBody c) && now - c.tick < BodyCacheTtl) return c;
            c.scale = BodyDrawScale(pawn);
            c.sleeve = SleeveColor(pawn);
            c.tick = now;
            bodyCache[id] = c;
            return c;
        }

        // Public accessor so the holster (and any other body-welded prop) can scale to the pawn's
        // ACTUAL rendered size — 1.0 on a stock adult, smaller for children, larger for Biotech
        // body-size genes / HAR / texture-resize mods. Cached (see GetCached).
        public static float BodyScaleFor(Pawn pawn) => GetCached(pawn).scale;

        // ---- Per-pawn OWNED forearm meshes (used by ToolAnimator's pose throttle): 2 slots (main +
        // off arm), persistent so a captured draw entry stays valid when replayed on a skip frame
        // (the shared NextMesh() pool resets per frame and would be overwritten by another pawn). ----
        private static readonly Dictionary<int, Mesh[]> armMeshes = new Dictionary<int, Mesh[]>();
        private static Mesh[] OwnedArmMeshes(int id)
        {
            if (!armMeshes.TryGetValue(id, out Mesh[] arr))
            {
                arr = new Mesh[2];
                for (int i = 0; i < 2; i++) { arr[i] = new Mesh { name = "JE_ArmOwned" }; arr[i].MarkDynamic(); }
                armMeshes[id] = arr;
            }
            return arr;
        }

        // Only the stock vanilla body types have a calibrated shoulder shape; anything else is treated
        // as a (possibly oddly-shaped) modded body and gets no forearms. Child is included now that its
        // shoulder shape is calibrated against the Biotech Naked_Child sprites (see ShapeFor). Baby is
        // still excluded — babies can't be issued work/recreation jobs, so the forearm path never runs.
        private static bool IsVanillaBodyType(Pawn pawn)
        {
            string bt = pawn.story?.bodyType?.defName;
            switch (bt)
            {
                case "Male":
                case "Female":
                case "Thin":
                case "Fat":
                case "Hulk":
                case "Child":
                    return true;
                default:
                    return false;
            }
        }

        private struct ShoulderShape { public float halfWidth, lift, ewLift, forward, widthMul; }
        private static ShoulderShape ShapeFor(Pawn pawn)
        {
            // Thickness is UNIFORM (widthMul = 1) for every body type, but the shoulder POSITION is
            // body-type-specific so each forearm pivots toward the correct shoulder for that build.
            // RE-CALIBRATED against the vanilla Naked_<type>_south + Naked_<type>_east sprites by
            // decoding the actual silhouette: per-row opaque extents -> shoulder "knee" of the upper
            // torso, mapped 128px -> 1.5-cell quad with the texture centre = DrawPos. N/S uses the
            // south sprite (lift = shoulder-line height, halfWidth = deltoid outer edge); E/W uses the
            // east sprite SEPARATELY (ewLift = profile shoulder height, forward = front/back nudge of
            // the profile shoulder vs torso centre). Each anchor was visually confirmed to land on the
            // shoulder corner of every silhouette (see .modmixer/overlay_shoulders.py markups).
            //   halfWidth = lateral shoulder offset; lift = N/S height; ewLift = E/W height;
            //   forward = E/W depth nudge (toward FRONT when +, toward back when -).
            string bt = pawn.story?.bodyType?.defName;
            switch (bt)
            {
                case "Hulk":   return new ShoulderShape { halfWidth = 0.365f, lift = 0.175f, ewLift = 0.120f, forward = -0.026f, widthMul = 1f }; // broadest, square high shoulders
                case "Fat":    return new ShoulderShape { halfWidth = 0.325f, lift = 0.135f, ewLift = 0.110f, forward = -0.020f, widthMul = 1f }; // wide sloping shoulders above the belly
                case "Thin":   return new ShoulderShape { halfWidth = 0.145f, lift = 0.150f, ewLift = 0.100f, forward =  0.006f, widthMul = 1f }; // very narrow; profile leans slightly forward
                case "Female": return new ShoulderShape { halfWidth = 0.165f, lift = 0.195f, ewLift = 0.135f, forward = -0.022f, widthMul = 1f }; // narrow high shoulder plateau over wide hips
                case "Child":  return new ShoulderShape { halfWidth = 0.165f, lift = 0.115f, ewLift = 0.078f, forward = -0.010f, widthMul = 1f }; // calibrated vs Naked_Child south/east: narrow capsule torso, shoulder line near the top curve; runtime BodyDrawScale shrinks to the child mesh width
                case "Baby":   return new ShoulderShape { halfWidth = 0.150f, lift = 0.06f, ewLift = 0.040f, forward = -0.015f, widthMul = 1f };  // never drawn (IsVanillaBodyType excludes)
                default:       return new ShoulderShape { halfWidth = 0.275f, lift = 0.150f, ewLift = 0.095f, forward = -0.012f, widthMul = 1f }; // Male (the default body type)
            }
        }

        // ---- Per-pawn shoulder-binding memory ----
        // Re-deciding the shoulder every frame from per-frame geometry is what made arms teleport
        // between shoulders mid-swing. Instead each pawn remembers its binding and only rebinds
        // when (a) the body FACING changes (the sprite flips then, so a swap is invisible), or
        // (b) a different preference holds continuously for DebounceSeconds.
        private struct ArmBind
        {
            public int facing;         // pawn.Rotation.AsInt when bound
            public bool hasOne;        // one-hand binding initialized
            public bool oneOnA;        // one-hand: arm from shoulder A (pawn's right)
            public bool hasTwo;        // two-hand binding initialized
            public bool mainOnA;       // two-hand: main hand on shoulder A
            public bool offOnA;        // two-hand: off hand on shoulder A
            public bool pendingValid;  // a different preference is being debounced
            public float pendingSince; // Time.time when the pending preference first appeared
        }
        private static readonly Dictionary<int, ArmBind> binds = new Dictionary<int, ArmBind>();
        private const float DebounceSeconds = 0.3f;

        public static void Forget(int thingId)
        {
            binds.Remove(thingId);
            bodyCache.Remove(thingId);
            armMeshes.Remove(thingId);
        }

        public static void ResetTransientState()
        {
            binds.Clear();
            bodyCache.Clear();
            armMeshes.Clear();
            poolCursor = 0;
            lastFrame = -1;
        }

        // ---- Entry point, called from ToolAnimator.DrawHands ----
        // wristMain / wristOff are the exact fist anchors on the haft; missingOff suppresses the
        // off-hand arm. phase drives the flex. drawLoc is the body centre. dir (work direction) is
        // intentionally NOT used to place the shoulders — they lock to the body's facing instead.
        public static void DrawArms(Pawn pawn, Vector3 drawLoc, Vector3 dir,
            Vector3 wristMain, bool drawOff, Vector3 wristOff, bool missingOff, float phase, bool stir = false)
        {
            if (!JobEffectsSettings.DrawArms || SleeveTex == null) return;
            // Humanlikes only — never draw forearms on animals, mechs or other non-humanlike pawns.
            if (pawn == null || pawn.RaceProps == null || !pawn.RaceProps.Humanlike) return;
            // Vanilla body types only. The shoulder table is calibrated against the stock adult
            // silhouettes; a modded body could be any shape, so the welded forearm roots would land
            // wrong. Skip drawing entirely rather than guess on an unknown silhouette.
            if (!IsVanillaBodyType(pawn)) return;

            CachedBody cb = GetCached(pawn);
            Material mat = SleeveMat(cb.sleeve);
            if (mat == null) return;

            // ABSOLUTE SHOULDER LOCK: anchor to the pawn's actual rendered cardinal facing and the
            // authoritative body draw width (HumanlikeBodyWidthForPawn = the exact mesh width used
            // for the body sprite: 1.5 for adults, life-stage/gene-scaled for children & Biotech).
            // Using the discrete facing (not the continuous work dir) welds the roots to the body
            // sprite — they only move when the body sprite itself flips, never drift up to ~45 deg
            // off when the work target isn't cardinal.
            float scale = cb.scale;
            ShoulderShape sh = ShapeFor(pawn);
            // Per-pawn OWNED forearm meshes (≤2/frame) so a captured pose can be replayed safely.
            Mesh[] owned = OwnedArmMeshes(pawn.thingIDNumber);

            Vector3 facing = pawn.Rotation.FacingCell.ToVector3(); facing.y = 0f;
            if (facing.sqrMagnitude < 0.01f) facing = new Vector3(0f, 0f, -1f);   // in-place → assume South
            facing = facing.normalized;
            // lateral = the shoulder line AND the pawn's RIGHT-hand direction: Cross(up, facing) points
            // to screen-left when facing South, south when facing East, etc. — i.e. the pawn's right.
            Vector3 lateral = Vector3.Cross(Vector3.up, facing); lateral.y = 0f; lateral = lateral.normalized;
            Vector3 screenUp = new Vector3(0f, 0f, 1f);   // torso top is always up-screen, any facing

            // Per-facing DEPTH ordering against the body sprite (body parent draws at baseLayer ~20,
            // apparel shell ~30-40, head ~50). "Behind" tucks an arm under the body; "front" sits over
            // the torso/apparel but under the head.
            float pawnBase = AltitudeLayer.Pawn.AltitudeFor();
            float yBehind = pawnBase + PawnRenderUtility.AltitudeForLayer(12f);  // E/W rear arm: under apparel
            float yFront  = pawnBase + PawnRenderUtility.AltitudeForLayer(45f);  // over body, under head
            // North (back to camera): drop BOTH arms below the bare body sprite (layer 0) instead of
            // the old layer-12 (which sat over any exposed skin). Negative layer keeps them inside the
            // Pawn altitude band but fully under skin + clothes + head, matching the tool/hand stack.
            float yNorthBehind = pawnBase + PawnRenderUtility.AltitudeForLayer(-2f);
            float yRight, yLeft;
            if (facing.x > 0.5f) { yRight = yBehind; yLeft = yFront; }        // East: right arm behind, left in front
            else if (facing.x < -0.5f) { yRight = yFront; yLeft = yBehind; }  // West: mirror
            else if (facing.z > 0.5f) { yRight = yNorthBehind; yLeft = yNorthBehind; }  // North: both fully under the body
            else { yRight = yFront; yLeft = yFront; }                         // South: both over body, under head

            // Shoulder-line spread: full when both shoulders show side-on (N/S — facing the camera or
            // away), strongly FORESHORTENED in profile (E/W) where the two shoulders nearly overlap.
            // Without this the lower profile shoulder sinks ~0.3 cells to the hip and the arm reads as
            // sprouting from the waist. The forward nudge (profile only) pulls the roots to the front.
            bool profile = Mathf.Abs(facing.x) > 0.5f;
            float spread = sh.halfWidth * scale * (profile ? 0.42f : 1f);

            // Profile (E/W) uses its own calibrated shoulder height (ewLift); N/S keeps lift untouched.
            Vector3 ctr = drawLoc
                + screenUp * ((profile ? sh.ewLift : sh.lift) * scale)
                + facing * (sh.forward * scale * Mathf.Abs(facing.x));
            Vector3 shoulderA = ctr + lateral * spread;   // pawn's RIGHT shoulder
            Vector3 shoulderB = ctr - lateral * spread;   // pawn's LEFT shoulder

            if (stir)
            {
                // Stirring orbits the hand in a circle, so a nearest-shoulder bind flips sides every
                // half-rotation. Lock to ONE shoulder (the pawn's right) and draw a single forearm so
                // it reads as one hand stirring rather than the arm teleporting between shoulders.
                DrawArm(shoulderA, wristMain, drawLoc, phase, scale, sh.widthMul, mat, yRight, owned[0]);
                return;
            }

            // Per-pawn binding memory: facing change = instant reset (the body sprite flips at
            // that moment, so a rebind is invisible); anything else must out-vote the current
            // binding for DebounceSeconds straight before a switch happens.
            int bindId = pawn.thingIDNumber;
            binds.TryGetValue(bindId, out ArmBind bind);
            int face = pawn.Rotation.AsInt;
            if (bind.facing != face)
            {
                bind.facing = face;
                bind.hasOne = false;
                bind.hasTwo = false;
                bind.pendingValid = false;
            }
            Vector3 dirFlat = new Vector3(dir.x, 0f, dir.z);
            dirFlat = dirFlat.sqrMagnitude > 0.0001f ? dirFlat.normalized : facing;
            float haftLat = Vector3.Dot(dirFlat, lateral);

            if (!drawOff)
            {
                // Single-hand tool: decide the side from the STABLE work direction (constant for
                // the whole job target), never from the per-frame wrist — the wrist swings across
                // the body's midline every stroke, which is exactly what made the arm teleport
                // between shoulders. Dead zone (working straight up/down screen) keeps whatever
                // side we already had, defaulting to the pawn's right.
                bool strong = Mathf.Abs(haftLat) > 0.25f;
                bool prefA = haftLat > 0f;   // working toward the pawn's right → right shoulder
                if (!bind.hasOne)
                {
                    bind.hasOne = true;
                    bind.oneOnA = strong ? prefA : true;
                    bind.pendingValid = false;
                }
                else if (strong && prefA != bind.oneOnA)
                {
                    if (!bind.pendingValid) { bind.pendingValid = true; bind.pendingSince = Time.time; }
                    else if (Time.time - bind.pendingSince >= DebounceSeconds)
                    {
                        bind.oneOnA = prefA;
                        bind.pendingValid = false;
                    }
                }
                else bind.pendingValid = false;
                binds[bindId] = bind;

                bool onA = bind.oneOnA;
                DrawArm(onA ? shoulderA : shoulderB, wristMain, drawLoc, phase, scale, sh.widthMul, mat,
                    onA ? yRight : yLeft, owned[0]);
                return;
            }

            // Two-handed: both fists ride the haft TOWARD the work, so for any tool that reaches /
            // levers off to one side (a crowbar's pry, diagonal mining, etc.) BOTH fists sit on the
            // same side of the body — they do NOT straddle the midline. The old logic read the
            // steep haft angle as a left/right straddle and sent the MAIN arm crossing the whole
            // torso to the far shoulder. Instead, decide from the STABLE work direction (haftLat,
            // constant for the job — never the per-frame swung wrist, so no teleporting):
            //   haftLat >  Cluster -> both fists toward the pawn's RIGHT : both arms from shoulder A
            //   haftLat < -Cluster -> both fists toward the pawn's LEFT  : both arms from shoulder B
            //   |haftLat| small    -> a near-centred forward grip        : symmetric main->A, off->B
            // Changes are debounced exactly like the one-hand path.
            const float Cluster = 0.30f;
            bool prefMainOnA, prefOffOnA;
            if (haftLat > Cluster)       { prefMainOnA = true;  prefOffOnA = true; }   // both to the near (right) shoulder
            else if (haftLat < -Cluster) { prefMainOnA = false; prefOffOnA = false; }  // both to the near (left) shoulder
            else                         { prefMainOnA = true;  prefOffOnA = false; }  // centred: one arm per shoulder
            if (!bind.hasTwo)
            {
                bind.hasTwo = true;
                bind.mainOnA = prefMainOnA;
                bind.offOnA = prefOffOnA;
                bind.pendingValid = false;
            }
            else if (prefMainOnA != bind.mainOnA || prefOffOnA != bind.offOnA)
            {
                if (!bind.pendingValid) { bind.pendingValid = true; bind.pendingSince = Time.time; }
                else if (Time.time - bind.pendingSince >= DebounceSeconds)
                {
                    bind.mainOnA = prefMainOnA;
                    bind.offOnA = prefOffOnA;
                    bind.pendingValid = false;
                }
            }
            else bind.pendingValid = false;
            binds[bindId] = bind;

            bool mainOnA = bind.mainOnA;
            bool offOnA = bind.offOnA;

            DrawArm(mainOnA ? shoulderA : shoulderB, wristMain, drawLoc, phase, scale, sh.widthMul, mat,
                mainOnA ? yRight : yLeft, owned[0]);
            if (!missingOff)
                DrawArm(offOnA ? shoulderA : shoulderB, wristOff, drawLoc, phase, scale, sh.widthMul, mat,
                    offOnA ? yRight : yLeft, owned[1]);
        }

        // FOREARM ONLY. `shoulder` is used purely as a DIRECTION hint (toward the elbow/body); we draw a
        // short ribbon from the wrist toward the elbow and let the texture fade it out before the elbow,
        // so the exact shoulder position never matters. You see the hand + a forearm dissolving at mid-arm.
        // ---- Forearm on an arbitrary SMYH hand (weapons / carried items / idle) ----
        // Given a world hand position SMYH just drew, attach a forearm pointing at the nearest
        // shoulder. Used by SmyhArmHook so the "forearms on all hands" option reuses the exact same
        // look as the work-tool forearms. No swing phase here (static slight bow).
        public static void DrawForearmAtHand(Pawn pawn, Vector3 handPos)
        {
            if (!JobEffectsSettings.ForearmsOnHands || SleeveTex == null) return;
            if (pawn == null || pawn.RaceProps == null || !pawn.RaceProps.Humanlike || !pawn.Spawned) return;
            if (!IsVanillaBodyType(pawn)) return;
            CachedBody cb = GetCached(pawn);
            Material mat = SleeveMat(cb.sleeve);
            if (mat == null) return;

            float scale = cb.scale;
            ShoulderShape sh = ShapeFor(pawn);
            Vector3 drawLoc = pawn.DrawPos;

            Vector3 facing = pawn.Rotation.FacingCell.ToVector3(); facing.y = 0f;
            if (facing.sqrMagnitude < 0.01f) facing = new Vector3(0f, 0f, -1f);
            facing = facing.normalized;
            Vector3 lateral = Vector3.Cross(Vector3.up, facing); lateral.y = 0f; lateral = lateral.normalized;
            Vector3 screenUp = new Vector3(0f, 0f, 1f);
            bool profile = Mathf.Abs(facing.x) > 0.5f;
            float spread = sh.halfWidth * scale * (profile ? 0.42f : 1f);
            Vector3 ctr = drawLoc + screenUp * ((profile ? sh.ewLift : sh.lift) * scale) + facing * (sh.forward * scale * Mathf.Abs(facing.x));
            Vector3 shoulderA = ctr + lateral * spread;
            Vector3 shoulderB = ctr - lateral * spread;

            Vector3 hXZ = new Vector3(handPos.x, 0f, handPos.z);
            bool onA = (hXZ - new Vector3(shoulderA.x, 0f, shoulderA.z)).sqrMagnitude
                     <= (hXZ - new Vector3(shoulderB.x, 0f, shoulderB.z)).sqrMagnitude;

            // Sit just under the hand SMYH drew (it's already at the weapon/carry altitude) so the
            // fist caps the forearm; depth ordering is relative to that hand, not the body band.
            float y = handPos.y - 0.02f;
            DrawArm(onA ? shoulderA : shoulderB, handPos, drawLoc, 0f, scale, sh.widthMul, mat, y, NextMesh());
        }

        private static void DrawArm(Vector3 shoulder, Vector3 wrist, Vector3 bodyCtr, float phase, float scale, float widthMul, Material mat, float y, Mesh m)
        {
            wrist.y = y;
            Vector3 toShoulder = shoulder - wrist; toShoulder.y = 0f;
            float armLen = toShoulder.magnitude;
            if (armLen < 0.05f) return;
            Vector3 armDir = toShoulder / armLen;                 // wrist -> elbow -> shoulder

            // Run from the hand ~0.6x of the way to the rough shoulder so the ribbon TIP sits at the
            // arm's midpoint (the elbow); the texture's lengthwise fade then dissolves it over the last
            // ~45% so nothing is drawn past the elbow. Clamped so a far reach can't grow a giant forearm.
            float foreLen = Mathf.Clamp(armLen * 0.6f, 0.26f * scale, 0.55f * scale);
            Vector3 elbow = wrist + armDir * foreLen; elbow.y = y;

            // Slight natural bend (outward + down) so it isn't a dead-straight stick; flexes with the swing.
            Vector3 outward = Vector3.Cross(Vector3.up, armDir).normalized;
            Vector3 mid = (wrist + elbow) * 0.5f;
            if (Vector3.Dot(outward, mid - bodyCtr) < 0f) outward = -outward;     // bow away from the body
            Vector3 bowDir = (outward * 0.5f + new Vector3(0f, 0f, -0.5f)).normalized;
            float flex = 0.10f + 0.05f * Mathf.Sin(phase * Mathf.PI * 2f);
            Vector3 ctrl = mid + bowDir * (foreLen * flex); ctrl.y = y;

            // Width: 40% thinner than before (slim forearm). Wrist a touch slimmer than the elbow end
            // (which fades out anyway). widthMul is uniform across body types.
            float wWrist = 0.078f * scale * widthMul;
            float wElbow = 0.096f * scale * widthMul;

            // s = wrist (solid centre of the texture), w = elbow (faded edge). See FillRibbon.
            FillRibbon(wrist, ctrl, elbow, wWrist, wElbow);
            m.Clear();
            m.vertices = vbuf;
            m.uv = uvbuf;
            m.triangles = sharedTris;
            // Lift the forearm above a construction frame's MetaOverlay fill when the tool is doing
            // so (see ToolAnimator.QueueAdjust); otherwise this is a no-op pass-through. Funnels
            // through Emit so it's captured into the pawn's pose buffer under the throttle.
            ToolAnimator.Emit(m, Matrix4x4.identity, ToolAnimator.QueueAdjust(mat), 0);
        }

        // Build a tapered ribbon along the quadratic bezier s(wrist)->ctrl->w(elbow). u runs 0..1 across
        // the width (black borders + side feather). v maps the WRIST end (t=0) to the texture centre 0.5
        // (fully solid) and the ELBOW end (t=1) to the edge 0.0 (transparent), so the forearm is opaque
        // at the hand and dissolves before the elbow. The texture's v-alpha is symmetric, so 0.5*(1-t)
        // reliably picks the solid->faded ramp regardless of texture v orientation.
        private static void FillRibbon(Vector3 s, Vector3 c, Vector3 w, float wS, float wW)
        {
            for (int i = 0; i <= Segments; i++)
            {
                float t = (float)i / Segments;
                float mt = 1f - t;
                Vector3 p = mt * mt * s + 2f * mt * t * c + t * t * w;          // B(t)
                Vector3 tan = 2f * mt * (c - s) + 2f * t * (w - c);             // B'(t)
                tan.y = 0f;
                if (tan.sqrMagnitude < 1e-6f) tan = (w - s);
                Vector3 perp = Vector3.Cross(Vector3.up, tan.normalized).normalized;
                float hw = Mathf.Lerp(wS, wW, t);
                float v = 0.5f * (1f - t);                                      // wrist 0.5 (solid) -> elbow 0.0 (faded)
                int li = i * 2, ri = i * 2 + 1;
                vbuf[li] = p - perp * hw; vbuf[li].y = s.y;
                vbuf[ri] = p + perp * hw; vbuf[ri].y = s.y;
                uvbuf[li] = new Vector2(0f, v);
                uvbuf[ri] = new Vector2(1f, v);
            }
        }

        /// <summary>
        /// Pre-create the arm sleeve material and the first few pooled meshes, off the render path.
        /// The per-colour sleeve variants are still created on demand (there's no way to enumerate
        /// every jacket colour in the colony up front), but the Transparent shader variant and the
        /// mesh pool's initial growth are the parts that would otherwise land on a gameplay frame.
        /// </summary>
        public static void WarmCache()
        {
            if (SleeveTex == null) return;
            _ = SleeveMat(Color.white);
            // Grow the pool to a typical simultaneous-arm count so the first busy frame doesn't
            // allocate a run of Meshes mid-draw.
            while (pool.Count < 8)
            {
                Mesh m = new Mesh { name = "JE_Arm" };
                m.MarkDynamic();
                pool.Add(m);
            }
        }

        // ---- Sleeve colour: jacket (Shell) > shirt (Middle/OnSkin) > bare skin ----
        private static Material SleeveMat(Color baseColor)
        {
            Color c = baseColor;
            c.a = JobEffectsSettings.ArmOpacity;
            if (!mats.TryGetValue(c, out Material m))
            {
                m = MaterialPool.MatFrom(SleeveTex, ShaderDatabase.Transparent, c);
                mats[c] = m;
            }
            return m;
        }

        private static Color SleeveColor(Pawn pawn)
        {
            Apparel best = null;
            int bestRank = 0;
            var worn = pawn.apparel?.WornApparel;
            if (worn != null)
            {
                for (int i = 0; i < worn.Count; i++)
                {
                    Apparel ap = worn[i];
                    var ad = ap.def.apparel;
                    if (ad == null || ad.bodyPartGroups == null || !ad.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
                        continue;
                    int rank = LayerRank(ad.LastLayer);
                    if (rank > bestRank) { bestRank = rank; best = ap; }
                }
            }
            Color baseC = best != null ? best.DrawColor
                        : (pawn.story != null ? pawn.story.SkinColor : Color.white);
            // Slight shade so the limb reads as a sleeve rather than a flat clone of the torso.
            return new Color(baseC.r * 0.9f, baseC.g * 0.9f, baseC.b * 0.9f, 1f);
        }

        private static int LayerRank(ApparelLayerDef layer)
        {
            if (layer == ApparelLayerDefOf.Shell) return 3;   // jacket
            if (layer == ApparelLayerDefOf.Middle) return 2;  // shirt/duster
            if (layer == ApparelLayerDefOf.OnSkin) return 1;  // t-shirt
            return 0;
        }
    }
}
