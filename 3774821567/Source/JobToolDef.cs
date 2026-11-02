using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace JobEffects
{
    public enum SwingStyle
    {
        Chop,   // wind-up then strike (pickaxe, axe, hammer, sickle, chisel)
        Sweep,  // smooth side-to-side oscillation (broom, trowel)
        Stir,   // tip orbits a small circle over the target (wooden spoon)
        Stab,   // quick thrust along the aim axis (sewing needle)
        Pry,    // tip planted in the target, handle levers around it (crowbar)
        Beat,   // flat pad raised and slammed onto the target twice per cycle (fire blanket)
        Saw,    // reciprocating back-and-forth along the work axis (bone saw)
        Dig,    // drive the head into the ground, then scoop/lift up (shovel, trowel)
        Crank,  // head planted on the target, handle rotates around it (wrench tightening)
        Load,   // feed a shell forward into the target; it seats and fades, then repeats (turret reload)
        Spray,  // held forward, nozzle aimed at the target; pulses a forward cone of mist + recoils (fire extinguisher)
        Hold,   // held stationary in front of the pawn; optional frame-based animation
        Read,   // notepad: lifted up to read, held, lowered to jot, repeat (no strikes)
        Scratch // no tool at all - the pawn occasionally raises a hand and scratches their head
    }

    /// <summary>
    /// Data-driven mapping: a vanilla JobDef (or several) -> an animated tool sprite
    /// drawn over the working colonist, plus the impact particles fired at each strike.
    /// Tunable entirely from XML so the swing feel can be iterated without recompiling.
    /// </summary>
    public class JobToolDef : Def
    {
        // Which JobDefs this tool applies to (by defName).
        public List<string> jobDefs = new List<string>();

        // If true, only animate when the job's A-target is a tree (chop/harvest only on trees).
        public bool treeOnly = false;

        // If true, only animate when the A-target is a NON-tree plant (crops/bushes).
        public bool nonTreeOnly = false;

        // For DoBill jobs: only animate when the workbench (target A) def is in this list.
        public List<string> workbenchDefs;

        // For FinishFrame jobs ONLY: the frame's stuff must belong to one of these stuff
        // categories ("Woody" / "Stony" / "Metallic" / "Fabric" / "Leathery"). The special
        // entry "None" matches UNSTUFFED frames - machines and other fixed-cost buildings
        // (which are steel/component builds, i.e. welder work). Jobs other than FinishFrame
        // ignore this filter entirely, so the welder can claim every Repair while only the
        // metal FinishFrames pick it up. Splits construction by material: hammer = wood,
        // chisel + mallet = stone, welder = metal.
        public List<string> frameStuffCategories;

        // For FinishFrame jobs that build TERRAIN (tilled soil / plowed fields / special floors):
        // the frame's entityDefToBuild must be a TerrainDef whose defName is in this list. Normal
        // build tools (hammer/chisel/welder) EXCLUDE every TerrainDef frame so they never "weld a
        // floor", which leaves terrain builds matching NO tool - a tool that sets buildTerrainDefs
        // fills exactly that gap (e.g. a hoe tilling Medieval Overhaul's plowed soil) without
        // colliding with the material build tools. A tool with this set matches nothing else.
        public List<string> buildTerrainDefs;

        // Electricity tech-gate. When the "Gate modern tools behind Electricity research" setting
        // is on AND the Electricity research is NOT yet finished, this tool is treated as
        // anachronistic for a pre-industrial colony: it is either SWAPPED to a period-correct tool
        // (preElectricityTool, by defName) or, if none is given, SUPPRESSED (nothing drawn -> vanilla
        // behavior). Once Electricity is researched, the tool behaves normally. Tools without this
        // flag are unaffected. The swap only changes what's DRAWN; the original tool's job/bench
        // match is what selected it.
        public bool requiresElectricity = false;
        public string preElectricityTool;

        // Surgery mode: match a DoBill whose A-target is a Pawn carrying a Bill_Medical (an
        // operation), instead of a workbench building. Use this for surgical tools. Crafting
        // DoBills target a building and never trip this; surgical DoBills target the patient pawn.
        public bool surgeryOnly = false;

        // Optional recipe filter for surgeryOnly tools: only match when the operation's RecipeDef
        // defName contains one of these substrings (case-insensitive). Empty/null = match any
        // surgery. Lets a bonesaw claim amputations ("RemoveBodyPart", "Amputate") while a scalpel
        // with no keywords catches every other operation. List keyworded tools FIRST so they win.
        public List<string> recipeNameKeywords;

        // Motion style of the swing.
        public SwingStyle swingStyle = SwingStyle.Chop;

        // When true, the swing is driven off the vanilla mining hit countdown
        // (JobDriver_Mine.ticksToPickHit) instead of free real-time, so the strike lands exactly
        // on the vanilla pick-hit (and its PickHit sound) every ~round(100/MiningSpeed) ticks.
        // The tool winds up slowly across the interval and strikes on the hit.
        public bool syncToMineHit = false;

        // When true, a Chop tool lands its strike on each butchering "hit" sound. Butchering is a
        // continuous DoBill (no discrete tick event like mining), so the only real per-hit signal is
        // the recipe sustainer re-firing its meat-chop grain. A Harmony hook notifies the animator on
        // each grain start; the swing predicts the next hit from the measured inter-hit interval and
        // times the downstroke to land on it. Real-time based, so it stays synced at any game speed.
        public bool syncToButcherHit = false;

        // When true (the bench chisels), the strike LOCKS onto the workbench recipe's WORK SOUND:
        // each time the recipe sustainer re-fires its PRIMARY grain (the audible "chink"/scrape),
        // a Harmony hook on SubSustainer.StartSample notifies the animator, which measures the
        // inter-hit interval and times the chisel's mallet rap to land ON the next predicted hit -
        // so the visible chisel impact coincides with the bench's hit noise. Wall-clock based (the
        // work sound plays in real time), so it stays synced at any game speed. Applies to any
        // workbench the chisel works (vanilla or modded), driven purely by that bench's own sound.
        public bool syncToBenchSound = false;

        // When true (default), a Chop swing draws a few faded ghost trails on the fast downstroke
        // (motion blur). Set false to disable the blur on a specific Chop tool.
        public bool motionBlur = true;

        // How many hands grip the haft (Show Me Your Hands integration). 2 = both hands on a
        // long tool (pickaxe, broom); 1 = a one-handed tool (chisel, needle, spoon).
        public int handCount = 2;

        // Show Me Your Hands placement. Where the gripping hand(s) sit along the haft, as a
        // fraction from the grip anchor (0 = handle butt / near end, where the sprite's bottom
        // edge renders) to the tool tip (1 = head, the sprite's top edge). Authored per-tool so
        // each fist sits on the actual handle drawn in that tool's texture. handPosA is the
        // primary / lower hand (always drawn); handPosB is the upper hand (only when handCount>=2).
        public float handPosA = 0.18f;
        public float handPosB = 0.42f;

        // Per-tool multiplier on the drawn hand size. The animator applies SMYH's own sizing
        // underneath (0.8 x pawn body size), so handScale = 1.0 matches SMYH's native hands.
        public float handScale = 1f;

        // Horizontally mirror the sprite (about the handle axis). Use for asymmetric heads
        // (axe blade / pickaxe point) authored facing the wrong side, so the business end
        // leads the swing. Composes with the automatic left/right facing mirror.
        public bool flipHead = false;

        // For single-edged tools authored blade-up (knife): when working east/west the default
        // mirror leaves the cutting edge pointing UP (reads as upside down). Set true to flip
        // across the haft axis whenever the tool lies horizontal, so the edge faces DOWN. Only
        // affects the profile (E/W) facings; N/S are untouched. Same correction the Saw style
        // applies automatically for its toothed edge.
        public bool flipEdgeProfile = false;

        // For Stir style: radius (cells) the tool tip orbits over the target.
        public float stirRadius = 0.13f;

        // For Stab style: how far (cells) the tool thrusts forward at full extension.
        // Also used as the dig-in reach for the Dig style.
        public float stabDistance = 0.24f;

        // For Stab style: phase (0..1) at which the strike lands. Default 0.3 matches the
        // moment full extension is first reached; raise to 0.5 to fire at the end of the hold.
        public float stabStrikePhase = 0.3f;

        // For Crank style: where on the sprite the contact head (the jaw that grips the bolt) sits,
        // so it - not the sprite's leading edge - is pinned on the pivot and stays planted through
        // the whole turn. crankHeadDepth is the head's position along the handle in mesh-Z
        // (0.5 = sprite top edge, the old default; lower = inset toward the grip). crankHeadLateral
        // is its sideways offset from the sprite centerline (sprite-space, mirrors with facing).
        public float crankHeadDepth = 0.5f;
        public float crankHeadLateral = 0f;

        // For Crank style: degrees the handle sweeps to turn the bolt before snapping back to
        // re-grip. The wrench's big 85 reads as a hard wrench-pull; a screwdriver wants a gentler
        // ~40 twist so the motion is subtle. Applies to the turn arc only; the re-grip snap and
        // strike phase are unchanged.
        public float crankSweepAngle = 85f;

        // For Stab style: vary each stab cycle (sewing) - a per-cycle random angle offset and a
        // small lateral shift so repeated stitches come in from slightly different directions
        // and spots instead of piling on one point.
        public bool stabVaried = false;

        // For Stir style: cooking variation - a long macro-cycle that stirs laps at slightly
        // different speeds with short pauses in between. The whole sequence spans one period,
        // so set period to the full macro-cycle length (~4-5s).
        public bool stirVaried = false;

        // Sprite-space offset (same convention as impactTipOffset: x = lateral in scale units,
        // y = along-haft fraction from the sprite tip, negative = down the haft) applied to the
        // tip-glow / tip-ember spawn point. Lets the glow sit exactly on the authored hot end
        // (rod tip glow, torch flame heart) when the texture has padding above it.
        public Vector2 tipGlowOffset = Vector2.zero;

        // Facing-specific override of tipGlowOffset used ONLY when the straight north/south art
        // (texPathNorthSouth) is active. The 3/4 profile torch has its hot end up-and-to-the-side
        // (so tipGlowOffset carries lateral), but the straight front/back art has a centred
        // electrode, so the lateral component would push the glow off the tip. Sentinel x<-900
        // means "unset - fall back to tipGlowOffset".
        public Vector2 tipGlowOffsetNorthSouth = new Vector2(-999f, -999f);

        // For Stab style: optional small hand hammer (mallet) drawn in the OFF hand. When set,
        // the main tool stays PLANTED against the target (chisel against the rock) and the mallet
        // winds back and raps its butt on each strike. Path + relative size (fraction of scale).
        public string malletTexPath;
        public float malletScale = 0.55f;

        // Off-hand FACE SHIELD (welding mask): when set, the OFF hand leaves the haft and holds
        // this sprite raised over the pawn's FACE for the whole job. Anchored to the pawn's head
        // (not the work point) so the tool's wander doesn't drag it around: square over the face
        // when facing the camera, ahead of the profile when working east/west, and tucked behind
        // the head when facing north (where SetDrawAltitudes already layers tool + mask + arms
        // UNDER the body sprite). Works with any swing style.
        public string maskTexPath;
        public float maskScale = 0.62f;

        // Optional per-facing mask art. The mask sprite follows the BODY facing (it covers the
        // face the body sprite is showing): East/West are authored profiles, North is the back
        // of the shell. Any missing entry falls back to maskTexPath (the front view), which
        // then gets the legacy mirror/cant treatment. Dedicated art is drawn unrotated.
        public string maskTexPathEast;
        public string maskTexPathWest;
        public string maskTexPathNorth;

        // Mask scale override for the PROFILE facings (east/west) only; <= 0 means "use
        // maskScale". The profile shell reads slimmer than the front view at equal scale,
        // so it can be bumped independently.
        public float maskScaleProfile = -1f;

        // --- Off-hand CARRIED prop (forage / seed basket) ---
        // A static container held LOW in the OFF hand for the whole job, while the MAIN hand
        // works the tool (trowel = seed basket alongside the dig). Anchored to the body + body
        // facing (not the wandering work point), so it stays put at the hip; drawn UNDER the body
        // when facing north (QueueAdjust routes it to the under-body queue). The off hand grips
        // the handle. Works with any swing style; scoped to tools that set offhandTexPath.
        public string offhandTexPath;
        public float offhandScale = 0.5f;          // drawn size (cells)
        // FRONT/BACK (south/north) facings, relative to the body centre:
        public float offhandLateral = 0.24f;       // out to the off-hand (anatomical-left) side
        public float offhandForward = 0.06f;       // along the facing (front when south, behind when north)
        public float offhandDrop = 0.20f;          // down toward the hip
        // PROFILE (east/west) facings: held forward (toward the work) and low, on the near side,
        // so it reads in front of the body instead of buried in it (no left/right asymmetry).
        public float offhandForwardProfile = 0.16f;
        public float offhandDropProfile = 0.22f;
        // Off-hand grip: fraction of offhandScale UP from the basket centre (the art's handle apex
        // sits high) so the fist lands on the handle, not the rim.
        public float offhandGripRaise = 0.4f;

        // For Chop style: an off-hand work prop at the strike point.
        //  propMode "plant": the prop (a nail) stands upright at the strike point; the off hand
        //    pinches beside it and retreats just before each blow lands, returning after.
        //  propMode "hold": the off hand holds the prop (tongs + billet) from the side with its
        //    business end ON the strike point, steady through the blows.
        public string propTexPath;
        public float propScale = 0.2f;
        public string propMode = "plant";

        // For Sweep style: a side prop held in the off hand, with a special cycle every
        // sidePropEvery sweeps.
        //  sidePropMode "dip": (paint pot) prop held at the hip; on the special cycle the brush
        //    arcs over to it, dips in, and returns - strikes are suppressed on dip cycles.
        //  sidePropMode "pan": (dustpan) prop only appears on the special cycle, set down ahead
        //    of the pawn, off hand on its handle, the sweep biased toward it.
        public string sidePropTexPath;
        public float sidePropScale = 0.3f;
        public string sidePropMode = "dip";
        public int sidePropEvery = 5;

        // Periodic alternate sprite (scalpel ↔ forceps): of every altEvery cycles, the LAST
        // altFor cycles draw altTexPath instead of texPath. 0 = disabled.
        public string altTexPath;
        public int altEvery = 0;
        public int altFor = 2;

        // Strike sprite (e.g. shears snapping CLOSED on the snip): for swing styles, this texture
        // replaces texPath for a short window AROUND each strike, so the tool reads as opening
        // (texPath) during the windup and closing (strikeTexPath) on the cut. Unlike altTexPath
        // (a slow cycle-counted swap), this is phase-synced WITHIN a single swing. null = static.
        public string strikeTexPath;

        // Off hand pressed flat on the worked target (bonesaw on the patient, sickle grabbing
        // the stalks) with a small bob synced to the motion.
        public bool braceOffHand = false;

        // Periodic brow wipe (axe/pickaxe): every ~16-26s the tool drops to the hip for ~1.6s
        // while the main hand wipes across the forehead, flicking sweat drops.
        public bool browWipe = false;

        // Occasionally the tool sticks in the target on a strike: the swing freezes at the
        // impact pose and wiggles for a beat before pulling free (pickaxe in rock).
        public bool canStick = false;

        // For Stir style: trace a figure-8 (1:2 Lissajous) instead of a circle (torch waving).
        public bool stirFigure8 = false;

        // For Stir style: every Nth cycle is a QUENCH - the rod pushes forward/down and a burst
        // of steam erupts from the hot end (tipGlowOffset). 0 = disabled.
        public int quenchEvery = 0;

        // For Stir style: a slow, deliberate PLUNGE every cycle instead of the cook's-stir orbit -
        // push the tool forward (and slightly down) into the work, HOLD it there for a beat, then
        // withdraw, repeating. No fast laps. Used by the smelt rod. plungeForward/plungeDown set
        // the reach (cells) of the dip; the in/hold/out timing follows QuenchCurve over one period.
        public bool stirPlunge = false;
        public float plungeForward = 0.30f;
        public float plungeDown = 0.10f;

        // For Saw style: number of back-and-forth strokes per cycle and the reach (cells) of each.
        public int sawStrokes = 4;
        public float sawAmplitude = 0.12f;

        // For Beat style: how high (cells, up the screen) the pad is raised overhead before each
        // downbeat (NOT a forward lunge), and the degrees it rocks side-to-side as it beats.
        public float beatLunge = 0.22f;
        public float beatRock = 7f;

        // Optional "hot tip" glow emitted from the tool tip on a steady cadence
        // (e.g. a smelting rod). Works with any swing style.
        public string tipGlowFleck;
        public float tipGlowInterval = 0.2f;   // real seconds between glows at 1x
        public float tipGlowScale = 1f;

        // Optional rising ember thrown from the tip alongside each tip glow (hot tools, e.g. smelt rod).
        public string tipEmberFleck;
        // Per-glow chance to throw an ember, and how many to throw when it does. Defaults reproduce
        // the old "occasional single ember" ambience; the welder bumps these for a constant shower.
        public float tipEmberChance = 0.4f;
        public int tipEmberCount = 1;

        // Optional ONE-SHOT sound played at each strike crossing, synced to the visual swing.
        // Use this for percussive hand tools (pickaxe, chisel) so the work sound matches the
        // animation cadence instead of the sparse vanilla gameplay-hit / sustainer cadence.
        public string strikeSound;

        // Stir only: plant the tool's TIP on the work point and keep it there; the pawn makes
        // small, slow angle corrections from the HANDLE end instead of waving the whole tool
        // (an arc welder holding one weld spot). Replaces the stir orbit and the generic sway.
        public bool tipPlanted;

        // Stir-style tools never cross a strike point, so a plain strikeSound stays silent on
        // them. When > 0, the strikeSound fires on this fixed interval instead (seconds of
        // worked time, scaled with game speed like the phase clock). Works for any style;
        // Stir tools without an interval fall back to one pulse per completed stir cycle.
        public float strikeSoundInterval = -1f;

        // Optional emote rising above the pawn's head (heart while cooking, sweat while
        // doing heavy labor). Emitted on a steady cadence while the tool is active.
        public string headFleck;
        public float headInterval = 0.7f;
        public float headScale = 0.8f;

        // Optional "thread" wisps that rise from the tool TIP on each strike (sewing needle).
        // Two populations:
        //   - threadFleck   = little strands that launch upward and float down under gravity.
        //   - threadSnipFleck = larger snipped ends that stay at the impact site and fade slowly.
        // Both are gated by the tool's per-tool effects toggle and scaled by intensity + debris.
        public string threadFleck;
        public int threadCount = 4;
        public FloatRange threadScale = new FloatRange(0.3f, 0.45f);
        public FloatRange threadSpeed = new FloatRange(0.3f, 0.55f);
        public string threadSnipFleck;
        public int threadSnipCount = 2;
        public FloatRange threadSnipScale = new FloatRange(0.6f, 0.85f);

        // Tool texture. Authored with the GRIP at the texture center, head pointing "up" (+Z).
        public string texPath;

        // Optional NORTH/SOUTH-facing override art for the ACTIVE tool draw. When the pawn's BODY
        // faces north or south (the front-on / back-on views) this replaces texPath; east/west
        // (the profile facings) keep texPath. Used by the welder: a straight, vertical welder
        // reads better head-on than the 3/4 torch, which stays for the side profile. The holster
        // (idle hip) draw is unaffected. Falls back to texPath when unset.
        public string texPathNorthSouth;

        // For frame-animated tools (Hold style): list of texture paths, played in order once
        // per job activation, then frozen on the final frame until the job ends.
        public List<string> frameTexPaths;
        public float frameDuration = 0.12f;

        // When true, frameTexPaths play on a continuous LOOP (wrap back to frame 0) for the whole
        // job instead of advancing once and freezing on the final frame. Use for an animation that
        // is itself a complete cycle (a looping motion that repeats), so it reads as the colonist
        // repeatedly working until the job finishes. Ignored when
        // depletionTexPaths is set (that path takes over after the opening pass).
        public bool loopFrames = false;

        // Optional second frame set played AFTER the frameTexPaths "opening" animation finishes.
        // Instead of advancing on a real-time clock, these frames are selected by the live progress
        // of the active job toil (0 -> 1), so the sequence stretches/compresses to finish exactly
        // when the job does. Used for the herbal bundle "emptying" pass: once it has bloomed open
        // (frameTexPaths), it depletes from full to empty across the remaining tend, reaching the
        // last frame as the tend completes. Leave null for tools that just freeze on the final
        // opening frame.
        public List<string> depletionTexPaths;

        // When true, frame-animated draws respect each frame's native pixel aspect ratio and scale
        // every frame against the SET's largest dimension (FrameRefSize), instead of stretching each
        // frame to fill the square quad. This keeps an animation whose frames are individually
        // bounding-box cropped (varying width/height) from distorting/jittering: the content's
        // bounding-box centre stays pinned to the quad centre and its relative size (growth) is
        // preserved. Leave false for kits authored at a uniform square size.
        public bool aspectCorrectFrames = false;
        private float frameRefSize = -1f;

        // Signed rotation offset (deg) applied to the Hold pose, letting a tool sit at an angle
        // relative to the work direction (e.g. a kit held sideways).
        public float holdAngleOffset = 0f;

        // Hold style only: lift the held item up the screen (cells) on top of the default chest
        // height - e.g. raise a spyglass to eye level. (The Hold pose also mirrors holdAngleOffset
        // by facing side, so a tilt - spyglass pointing up to the sky - leans the same way at E/W.)
        public float holdRaise = 0f;

        // Hold style: draw the held item as a TALL vertical item with both hands STACKED up its
        // central axis (recorder, spyglass) instead of side-by-side along the bottom edge (book,
        // medicine kit). grip = low on the item, tip = high, so handPosA/handPosB stack the fists.
        public bool holdVertical = false;

        // Hold (wide item) / Read pose: the two hands grip the item's LOWER corners. These set how
        // far out from centre (half-width) and how far below centre (drop) those corners sit, in
        // scale units, so a NARROW portrait book/folder (TeachersBook) can pull its grip corners
        // inboard onto the painted edges instead of floating in the canvas padding. Defaults
        // reproduce the original wide open-book / medicine-kit corners, so existing Hold tools are
        // unaffected.
        public float bookGripHalfWidth = 0.40f;
        public float bookGripDrop = 0.32f;

        // Hold style: pin the TOP of the sprite (the mouthpiece) to the pawn's mouth and let the
        // barrel hang DOWN and OUT - used for a recorder/flute so the colonist reads as blowing
        // into it rather than holding a pole over their face. Rotation (holdAngleOffset) pivots
        // about the mouthpiece, so the bell swings away from the body without lifting the
        // mouthpiece off the lips. Overrides holdVertical's centred draw. Scoped (default off) so
        // it never disturbs the medicine kits / book / spyglass Hold poses.
        public bool holdMouthAnchor = false;

        // Hold style: shift the held item sideways (cells) toward the facing/playing side, so a
        // recorder is held OUT past the face instead of dead-centre over it. Mirrors with facing.
        public float holdLateral = 0f;

        // Hold style: pin the BOTTOM of the sprite (the small eyepiece end) to the pawn's eye and
        // raise the barrel UP toward the sky - used for a spyglass so the colonist reads as
        // squinting through it at the stars rather than holding a pole over their face. Rotation
        // (holdAngleOffset) pivots about the eyepiece, so the big objective end swings up/out
        // without lifting the eyepiece off the eye. One hand grips the eyepiece (low) end
        // (use handCount=1). Overrides holdVertical's centred draw. Scoped (default off).
        public bool holdEyeAnchor = false;

        // Hold style: "fishing" pose for a rod held in BOTH hands out over the water (the Odyssey
        // Fish job, whose target A is the water cell the pawn faces). The long blank rises up-and-
        // out toward the water at holdAngleOffset; the whole rod pivots about the REAR hand at the
        // butt so the tip sweeps. A calm loop works the lure - a slow continuous up/down sweep of
        // the tip (jigSweep) plus a quick "set the hook" twitch once per cycle (jigTwitch) - with a
        // gentle bob + sway. handPosA = rear hand at the butt, handPosB = front hand at the reel.
        // animTime-driven (constant-1x, pause-frozen). Scoped (default off). Overrides the generic
        // centred Hold draw.
        public bool holdFishing = false;
        // Degrees the rod tip slowly sweeps up/down about the base lean as the angler works the lure.
        public float jigSweep = 6f;
        // Degrees the rod tip jerks UP (toward vertical) on the quick "set the hook" twitch each cycle.
        public float jigTwitch = 22f;

        // Hold style: prayer-beads pose with PER-FACING art and PER-FACING hand placement.
        //  - Facing SOUTH/NORTH: draws texPath (a symmetric necklace), held up in front with a
        //    hand on EACH side of the bead strands (grip = upper-left, tip = upper-right).
        //  - Facing EAST/WEST: draws texPathProfile (a coiled loop) cupped low in BOTH hands
        //    clasped near centre (grip/tip pulled in close together).
        // handPosA/handPosB still slide the two fists along the per-facing grip->tip span, so the
        // same fractions read as "wide, one per side" front-on and "narrow, cupped" in profile.
        // Scoped (default off). Overrides the generic centred Hold draw.
        public bool holdRosary = false;

        // Profile (east/west) texture for holdRosary. Falls back to texPath when unset.
        public string texPathProfile;

        // Hold style: "offer" pose for a feeding bowl held OUT toward the work target (taming an
        // animal). Both hands cup the bowl's lower side edges; it is kept SCREEN-UPRIGHT (the bowl
        // never spins to point at the target) and held out in front, extended toward the animal.
        // A slow coaxing cycle gently pushes the bowl further toward the animal and draws it back
        // (offerReach) with a soft bob, as if enticing the animal to approach and eat. The two-hand
        // grip corners use bookGripHalfWidth/bookGripDrop. Scoped (default off). Overrides the
        // generic centred Hold draw.
        public bool holdOffer = false;
        // Extra reach (cells) the bowl extends toward the target at the peak of the coaxing cycle.
        public float offerReach = 0.12f;

        // Hold style: "bottle" pose for an adult bottle-feeding a baby (Biotech BottleFeedBaby).
        // The carer cradles the baby in front - the baby is CARRIED, so the work target rides on the
        // carrier and the work dir is unreliable; the pose anchors to the clean BODY facing instead.
        // The bottle is held low and forward toward the cradle and tilts nipple-down with a gentle
        // feeding rock. holdAngleOffset = how far it tilts over; pourGripFrac = the pivot point up
        // the sprite (the hand on the bottle body). Scoped (default off).
        public bool holdBottle = false;

        // Hold style: "toy" pose for a baby holding a toy while being played with (Biotech BabyPlay).
        // Two cute motions selected by toyMotion: "shake" (a rattle waved in quick swelling bursts,
        // pivoting about its handle so the head whips side to side, then a short rest) and "cuddle"
        // (a plush hugged to the chest with a soft bob + slow rock + squash). Eased toward the
        // playmate (work dir, valid: the baby is spawned during play). Scoped (default off).
        public bool holdToy = false;
        public string toyMotion = "shake";

        // Hold style: "pour" pose for a flask / test tube (drug lab). The item is held upright
        // and, as the job progresses, tilts over to empty like a jug - reaching fully inverted
        // (mouth pointing down) at the pourPeak fraction of the bill (default 0.72: a long, slow
        // pour, then a quick right-up over the short remainder), then righting itself back to
        // vertical by completion. At the inverted peak the sprite swaps from texPath (the FULL
        // flask) to pourEmptyTexPath (the EMPTY flask), so the tube finishes upright and empty.
        // Progress is read live from the DoBill workLeft, so the pour paces with the real craft
        // time at any game speed. Overrides the generic centred Hold draw. Scoped (default off).
        public bool holdPour = false;
        // Empty-flask art shown after the inverted halfway swap. Falls back to texPath when unset.
        public string pourEmptyTexPath;
        // Degrees the tube is tilted at the inverted (pour) peak. 180 = mouth straight down.
        public float pourMaxTilt = 170f;
        // Pivot point up the sprite (0 = base, 1 = mouth) - where the hand grips and the tube
        // rotates about. ~0.45 cups the lower body of an Erlenmeyer flask.
        public float pourGripFrac = 0.45f;
        // Fraction of the bill's progress at which the flask reaches full inversion (the pour
        // peak) and swaps to the empty sprite. The down-tilt fills prog [0, pourPeak] and the
        // right-up fills prog [pourPeak, 1], so a value > 0.5 pours slowly then rights quickly.
        public float pourPeak = 0.72f;
        // Acid-pour stream: while the flask is meaningfully tilted, dribble these droplets out of
        // the (inverted) mouth and curl this vapor up off the pour. Both optional FleckDef names;
        // emit only fires once the tilt passes the threshold, paced by pourEmitInterval seconds.
        public string pourDripFleck;
        public string pourSmokeFleck;
        // Real-time seconds between pour-stream emissions while tilted (drip + vapor pulse).
        public float pourEmitInterval = 0.16f;

        // Medicine filter for tend tools (matched against the medicine Thing in job.targetB).
        // requireMedicineDef:  only match when the tend medicine's defName equals this.
        // forbidMedicineDefs:  only match when it is NOT one of these.
        // Either filter implies medicine must be present: a no-medicine (bare-handed) tend matches none.
        // Lets the herbal- and glitterworld-specific kits and the default kit split TendPatient by
        // medicine type.
        public string requireMedicineDef;
        public List<string> forbidMedicineDefs;

        // Size of the drawn tool quad, in cells.
        public float scale = 1.1f;

        // Distance of the grip from the pawn's center, toward the work target (cells).
        public float reach = 0.32f;

        // Anchor the work point ON the worked thing instead of at fixed `reach`: re-aim at the
        // target's occupied cell nearest the pawn and extend the grip (clamped) so the painted
        // tool tip lands on top of the target's sprite. The arc flash / spark shower / impact
        // burst all ride the tip, so they land on the building too. Most visible on diagonal
        // work, where a fixed reach leaves the tip welding thin air a half-cell short.
        public bool beadOnTarget = false;

        // Suppress the vanilla recipe/work effecter's sprayer motes (e.g. the smithy "Smith"
        // effecter's stone-bit sparks) while this tool is actively drawing for the pawn. Those
        // motes spawn BetweenPositions - biased ~0.6 toward the worker - so on a bench they erupt
        // from the pawn's feet, disconnected from the animated tool. We already throw our own
        // bright sparks at the electrode tip (impactBitFleck / tipEmber / tipGlow), so for the
        // welder we cut the vanilla feet-sparks and let the tip do the work.
        public bool suppressWorkMotes = false;

        // Real-time seconds for one full swing cycle at 1x game speed.
        public float period = 0.46f;

        // Degrees the tool is wound back at the top of the wind-up.
        public float windupAngle = 62f;

        // Degrees the tool follows through past the target on the strike.
        public float followThroughAngle = 16f;

        // Signed angle (deg) the head sits at the INSTANT of impact, REPLACING the followThrough
        // overshoot when set (NaN = use followThroughAngle as before). Use this to land an
        // off-axis working tip (a pickaxe point / axe edge that juts to one side of the haft)
        // directly on the forward line: the haft cants back by this angle so the cutting tip stops
        // ON target instead of swinging past it. The swing eases down to this pose and holds, then
        // recovers to 0 - it never rotates beyond it (no past-front overshoot).
        public float strikeAngle = float.NaN;
        public bool HasStrikeAngle => !float.IsNaN(strikeAngle);

        // --- Impact particles (fired once per swing, at the strike) ---
        public string impactBitFleck;                 // thrown debris (shards / chips)
        public int impactBitCount = 4;
        public FloatRange impactBitSpeed = new FloatRange(6f, 13f);
        public FloatRange impactBitScale = new FloatRange(0.5f, 0.9f);
        public float impactSpread = 55f;              // degrees of spread around the rebound direction
        public string impactFlashFleck;               // bright flash at impact
        public float impactFlashScale = 3f;
        public string impactDustFleck;                // soft ground dust kicked up at the strike (stone/build tools)

        // Anchor the whole impact burst (dust / flash / thrown debris) at the tool's TIP at the
        // moment of the strike, instead of the generic point out in front of the pawn. Use for
        // stab/point tools (chisel, pickaxe) so the effect reads as the very tip biting the rock.
        // impactTipOffset is a sprite-space delta from the geometric tip (x = image-lateral,
        // y = along the handle axis toward/past the head),
        // letting the burst sit on the actual cutting point rather than the top-centre of the quad.
        public bool impactAtTip = false;
        public UnityEngine.Vector2 impactTipOffset = UnityEngine.Vector2.zero;
        // Facing-specific override of impactTipOffset used ONLY when the straight north/south art
        // (texPathNorthSouth) is active - same rationale as tipGlowOffsetNorthSouth. Sentinel
        // x<-900 means "unset - fall back to impactTipOffset".
        public UnityEngine.Vector2 impactTipOffsetNorthSouth = new UnityEngine.Vector2(-999f, -999f);
        // DRAW-ONLY shift of the whole tool sprite + hands, in the SAME haft-space convention as
        // impactTipOffset (x = image-lateral, flipHead-aware; y = fraction of scale along the haft
        // toward/past the head). It moves what you SEE but is subtracted back out before the impact
        // burst is spawned, so the spark/flecks stay anchored to the un-shifted strike pose. Use it
        // to slide a sprite feature (e.g. the pickaxe's lower point) onto the impact spark without
        // dragging the spark along. Default (0,0) = no shift.
        public UnityEngine.Vector2 strikeLift = UnityEngine.Vector2.zero;
        // Extra WORLD screen-down (south, -Z) nudge applied to the impact burst, in world units.
        // Unlike impactTipOffset (haft-relative - its lateral term flips sign and goes unstable at
        // pure N/S facings, so it stays zero on symmetric crosswise heads), this is a plain screen-
        // down drop: it seats the burst on a crosswise head's LOWER striking face in the E/W profile
        // views, and only nudges harmlessly toward the haft at N/S. Default 0.
        public float impactDrop = 0f;

        // --- Immersion: seasonal + material tinting of the thrown bits ---
        // When true, the impact bits are recolored by the current season (leaf/soil tools).
        public bool seasonTintBits = false;
        // When true, the impact bits take the color of the worked thing (mining/masonry/build/butcher).
        public bool materialTintBits = false;

        // --- Optional SECOND thrown-debris burst on the same strike ---
        // Lets one tool fling two kinds of debris per hit (e.g. an axe throwing both wood chips
        // AND leaves shaken loose from the canopy). Emitted from the same strike point as the
        // primary bits, so it is also hit-synced. Leave impactBitFleck2 empty to skip.
        public string impactBitFleck2;
        public int impactBitCount2 = 3;
        public FloatRange impactBitSpeed2 = new FloatRange(2f, 5f);
        public FloatRange impactBitScale2 = new FloatRange(0.35f, 0.6f);
        public float impactSpread2 = 75f;
        public bool seasonTintBits2 = false;
        public bool materialTintBits2 = false;

        // --- Immersion: settling debris that lands near the work and fades out ---
        public string restFleck;                       // FleckDef thrown to the ground at the strike
        public float restChance = 0.5f;                // chance per strike to drop any litter
        public int restCountMax = 2;                   // up to this many flecks per drop (x intensity)

        // When true, this tool is drawn ONLY during the active work animation: it is never
        // carried on the belt while the pawn walks TO the job, and never left to linger/fade at
        // the hip AFTER the job ends. Use it where vanilla already renders the real carried item
        // (the three medicine kits - vanilla draws the Medicine the doctor carries en route, and
        // a holster pose would double-draw / leave a phantom kit on the belt afterward).
        public bool noHolster = false;

        // When true, this tool ALWAYS draws in front of the pawn (over the body AND the world, floated
        // at MoteOverhead) for every facing EXCEPT north, where it still tucks under the body like
        // every tool. Use for tools that must read clearly and never be world-occluded — the welder
        // (its face mask), the medicine kits, cleaver, crowbar, smelt rod, hacking device. Overrides
        // the default hybrid "share the colonist's depth" world-sort for non-north facings.
        public bool alwaysInFront = false;
 
        // When true, the mod draws NO grip hands on this tool. Use for items where adult-sized SMYH
        // hand billboards would read oversized and the animated item alone sells the pose.
        public bool noHands = false;

        // Like noHands, but ONLY for the Baby body type. A baby being played with is tiny (bodySize
        // 0.2), so hands would dwarf it; a CHILD holding the same toy (e.g. our gallery/lineup, or a
        // mod that lets older kids play) is big enough that the calibrated child shoulders + forearms
        // + proportionally-scaled hands read correctly. Use on the baby toys so babies stay hand-less
        // while children get the full hand+forearm grip.
        public bool noHandsBabyOnly = false;

        // Optional variant group: when several JobToolDefs match the SAME job AND share a non-empty
        // variantGroup, ONE is chosen at random per job activation (stable for that job instance via
        // its loadID) instead of always taking the first XML match. Lets a job show a different prop
        // each time (e.g. a baby grabbing the rattle one play session, the plush the next). Empty
        // (the default) keeps the deterministic first-match behavior every other tool relies on.
        public string variantGroup;

        // --- Immersion: idle holster pose (tool tucks into the belt after work, then fades) ---
        // All values are SCREEN-relative (not the work vector): the tool sits on the pawn's
        // sprite at a fixed hip/waist spot regardless of which way the last job faced.
        public float idleHipOffset = 0.10f;            // screen-X offset out to the hip (cells) - tucked close to the body
        public float idleBeltDrop  = -0.06f;           // screen-Z offset down to the belt line (cells; negative = toward feet)
        public float idleAngle = 150f;                 // screen-relative tilt: head hangs down-and-out (deg; 180 = straight down)
        public float idleScale = 0.85f;                // holster size multiplier on top of <scale>
        public float idleGripAnchor = 0.20f;           // 0 = tool centered on belt, 0.5 = grip end pinned to belt (head hangs below)

        [Unsaved(false)]
        private Material cachedMat;

        [Unsaved(false)]
        private Material cachedMatTransparent;

        [Unsaved(false)]
        private Material cachedMalletMat;

        [Unsaved(false)]
        private Material cachedMaskMat;

        [Unsaved(false)]
        private Material cachedMaskMatEast;

        [Unsaved(false)]
        private Material cachedMaskMatWest;

        [Unsaved(false)]
        private Material cachedMaskMatNorth;

        [Unsaved(false)]
        private Material cachedPropMat;

        [Unsaved(false)]
        private Material cachedSidePropMat;

        [Unsaved(false)]
        private Material cachedAltMat;

        [Unsaved(false)]
        private Material cachedStrikeMat;

        [Unsaved(false)]
        private Material cachedOffhandMat;

        [Unsaved(false)]
        private FleckDef cachedRest;

        [Unsaved(false)]
        private FleckDef cachedBitFleck;

        [Unsaved(false)]
        private FleckDef cachedBitFleck2;

        [Unsaved(false)]
        private FleckDef cachedFlashFleck;

        [Unsaved(false)]
        private FleckDef cachedTipGlow;

        [Unsaved(false)]
        private FleckDef cachedTipEmber;

        [Unsaved(false)]
        private FleckDef cachedDust;

        [Unsaved(false)]
        private FleckDef cachedHead;
        private FleckDef cachedPourDrip;
        private FleckDef cachedPourSmoke;

        [Unsaved(false)]
        private FleckDef cachedThread;

        [Unsaved(false)]
        private FleckDef cachedThreadSnip;

        [Unsaved(false)]
        private Material[] cachedFrameMats;

        [Unsaved(false)]
        private Material[] cachedFrameMatsTransparent;

        [Unsaved(false)]
        private Material[] cachedDepletionMats;

        [Unsaved(false)]
        private SoundDef cachedStrikeSound;

        [Unsaved(false)]
        private bool strikeSoundResolved;

        [Unsaved(false)]
        private bool fleckResolved;

        // Human-readable name for the settings list. Tools carry no <label>, so derive one from
        // the defName: strip the "JE_Tool_" prefix and space out the CamelCase ("SmithHammer" ->
        // "Smith hammer"). Honors an authored <label> if one is ever added.
        [Unsaved(false)]
        private string cachedLabelNice;
        public string LabelNice
        {
            get
            {
                if (cachedLabelNice != null) return cachedLabelNice;
                if (!label.NullOrEmpty()) { cachedLabelNice = label; return cachedLabelNice; }
                string s = defName ?? "";
                if (s.StartsWith("JE_Tool_")) s = s.Substring("JE_Tool_".Length);
                var sb = new System.Text.StringBuilder(s.Length + 4);
                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];
                    if (i > 0 && char.IsUpper(c) && !char.IsUpper(s[i - 1]))
                        sb.Append(' ');
                    sb.Append(i == 0 ? char.ToUpper(c) : char.ToLower(c));
                }
                cachedLabelNice = sb.ToString();
                return cachedLabelNice;
            }
        }

        public Material Material
        {
            get
            {
                if (cachedMat == null && !texPath.NullOrEmpty())
                {
                    cachedMat = MaterialPool.MatFrom(texPath, ShaderDatabase.Cutout);
                }
                return cachedMat;
            }
        }

        // North/South-facing override art (welder straight view); falls back to the front Material.
        private Material cachedNorthSouthMat;
        public Material NorthSouthMaterial
        {
            get
            {
                if (texPathNorthSouth.NullOrEmpty()) return Material;
                if (cachedNorthSouthMat == null)
                    cachedNorthSouthMat = MaterialPool.MatFrom(texPathNorthSouth, ShaderDatabase.Cutout);
                return cachedNorthSouthMat ?? Material;
            }
        }

        // Profile (east/west) art for holdRosary; falls back to the front-view Material.
        private Material cachedProfileMat;
        public Material ProfileMaterial
        {
            get
            {
                if (texPathProfile.NullOrEmpty()) return Material;
                if (cachedProfileMat == null)
                    cachedProfileMat = MaterialPool.MatFrom(texPathProfile, ShaderDatabase.Cutout);
                return cachedProfileMat ?? Material;
            }
        }

        // Empty-flask art for holdPour (shown after the halfway inversion); falls back to Material.
        private Material cachedPourEmptyMat;
        public Material PourEmptyMaterial
        {
            get
            {
                if (pourEmptyTexPath.NullOrEmpty()) return Material;
                if (cachedPourEmptyMat == null)
                    cachedPourEmptyMat = MaterialPool.MatFrom(pourEmptyTexPath, ShaderDatabase.Cutout);
                return cachedPourEmptyMat ?? Material;
            }
        }

        public Material MalletMaterial
        {
            get
            {
                if (cachedMalletMat == null && !malletTexPath.NullOrEmpty())
                {
                    cachedMalletMat = MaterialPool.MatFrom(malletTexPath, ShaderDatabase.Cutout);
                }
                return cachedMalletMat;
            }
        }

        public Material MaskMaterial
        {
            get
            {
                if (cachedMaskMat == null && !maskTexPath.NullOrEmpty())
                    cachedMaskMat = MaterialPool.MatFrom(maskTexPath, ShaderDatabase.Cutout);
                return cachedMaskMat;
            }
        }

        // Facing-specific mask material. `dedicated` reports whether directional art was used
        // (drawn as authored - no mirror/cant) or the front-view fallback (legacy treatment).
        public Material MaskMaterialFor(Rot4 facing, out bool dedicated)
        {
            dedicated = true;
            if (facing == Rot4.East && !maskTexPathEast.NullOrEmpty())
            {
                if (cachedMaskMatEast == null)
                    cachedMaskMatEast = MaterialPool.MatFrom(maskTexPathEast, ShaderDatabase.Cutout);
                if (cachedMaskMatEast != null) return cachedMaskMatEast;
            }
            else if (facing == Rot4.West && !maskTexPathWest.NullOrEmpty())
            {
                if (cachedMaskMatWest == null)
                    cachedMaskMatWest = MaterialPool.MatFrom(maskTexPathWest, ShaderDatabase.Cutout);
                if (cachedMaskMatWest != null) return cachedMaskMatWest;
            }
            else if (facing == Rot4.North && !maskTexPathNorth.NullOrEmpty())
            {
                if (cachedMaskMatNorth == null)
                    cachedMaskMatNorth = MaterialPool.MatFrom(maskTexPathNorth, ShaderDatabase.Cutout);
                if (cachedMaskMatNorth != null) return cachedMaskMatNorth;
            }
            dedicated = false;
            return MaskMaterial;
        }

        public Material PropMaterial
        {
            get
            {
                if (cachedPropMat == null && !propTexPath.NullOrEmpty())
                    cachedPropMat = MaterialPool.MatFrom(propTexPath, ShaderDatabase.Cutout);
                return cachedPropMat;
            }
        }

        public Material SidePropMaterial
        {
            get
            {
                if (cachedSidePropMat == null && !sidePropTexPath.NullOrEmpty())
                    cachedSidePropMat = MaterialPool.MatFrom(sidePropTexPath, ShaderDatabase.Cutout);
                return cachedSidePropMat;
            }
        }

        public Material AltMaterial
        {
            get
            {
                if (cachedAltMat == null && !altTexPath.NullOrEmpty())
                    cachedAltMat = MaterialPool.MatFrom(altTexPath, ShaderDatabase.Cutout);
                return cachedAltMat;
            }
        }

        public Material StrikeMaterial
        {
            get
            {
                if (cachedStrikeMat == null && !strikeTexPath.NullOrEmpty())
                    cachedStrikeMat = MaterialPool.MatFrom(strikeTexPath, ShaderDatabase.Cutout);
                return cachedStrikeMat;
            }
        }

        // Off-hand carried prop (forage / seed basket); null when unset.
        public Material OffhandMaterial
        {
            get
            {
                if (cachedOffhandMat == null && !offhandTexPath.NullOrEmpty())
                    cachedOffhandMat = MaterialPool.MatFrom(offhandTexPath, ShaderDatabase.Cutout);
                return cachedOffhandMat;
            }
        }

        // Alpha-blended variant of the tool material, used only for the holster fade-out.
        // Cutout can't fade smoothly (binary alpha test), so the resting tool uses Transparent.
        public Material MaterialTransparent
        {
            get
            {
                if (cachedMatTransparent == null && !texPath.NullOrEmpty())
                {
                    cachedMatTransparent = MaterialPool.MatFrom(texPath, ShaderDatabase.Transparent);
                }
                return cachedMatTransparent;
            }
        }

        public Material FrameMaterial(int frame)
        {
            if (frameTexPaths == null || frame < 0 || frame >= frameTexPaths.Count) return Material;
            if (cachedFrameMats == null) cachedFrameMats = new Material[frameTexPaths.Count];
            if (cachedFrameMats[frame] == null && !frameTexPaths[frame].NullOrEmpty())
                cachedFrameMats[frame] = MaterialPool.MatFrom(frameTexPaths[frame], ShaderDatabase.Cutout);
            return cachedFrameMats[frame] ?? Material;
        }

        // Largest pixel dimension across every frame's texture (width or height), computed once and
        // cached. Used by aspectCorrectFrames to scale all frames against a common reference so the
        // longest-side frame fills the quad and every other frame keeps its true relative size.
        // Frame materials must be loaded first; we force-load them here via FrameMaterial.
        public float FrameRefSize()
        {
            if (frameRefSize > 0f) return frameRefSize;
            float m = 1f;
            if (frameTexPaths != null)
            {
                for (int i = 0; i < frameTexPaths.Count; i++)
                {
                    Texture t = FrameMaterial(i)?.mainTexture;
                    if (t != null) m = Mathf.Max(m, Mathf.Max(t.width, t.height));
                }
            }
            // Only cache once we actually saw a loaded texture (>1), so an early call before the
            // content loads doesn't lock in the fallback of 1.
            if (m > 1f) frameRefSize = m;
            return m;
        }

        public Material DepletionMaterial(int frame)
        {
            if (depletionTexPaths == null || frame < 0 || frame >= depletionTexPaths.Count) return Material;
            if (cachedDepletionMats == null) cachedDepletionMats = new Material[depletionTexPaths.Count];
            if (cachedDepletionMats[frame] == null && !depletionTexPaths[frame].NullOrEmpty())
                cachedDepletionMats[frame] = MaterialPool.MatFrom(depletionTexPaths[frame], ShaderDatabase.Cutout);
            return cachedDepletionMats[frame] ?? Material;
        }

        public Material FrameMaterialTransparent(int frame)
        {
            if (frameTexPaths == null || frame < 0 || frame >= frameTexPaths.Count) return MaterialTransparent;
            if (cachedFrameMatsTransparent == null) cachedFrameMatsTransparent = new Material[frameTexPaths.Count];
            if (cachedFrameMatsTransparent[frame] == null && !frameTexPaths[frame].NullOrEmpty())
                cachedFrameMatsTransparent[frame] = MaterialPool.MatFrom(frameTexPaths[frame], ShaderDatabase.Transparent);
            return cachedFrameMatsTransparent[frame] ?? MaterialTransparent;
        }

        public FleckDef BitFleck { get { ResolveFlecks(); return cachedBitFleck; } }
        public FleckDef BitFleck2 { get { ResolveFlecks(); return cachedBitFleck2; } }
        public FleckDef FlashFleck { get { ResolveFlecks(); return cachedFlashFleck; } }
        public FleckDef TipGlow { get { ResolveFlecks(); return cachedTipGlow; } }
        public FleckDef TipEmber { get { ResolveFlecks(); return cachedTipEmber; } }
        public FleckDef ImpactDust { get { ResolveFlecks(); return cachedDust; } }
        public FleckDef HeadFleck { get { ResolveFlecks(); return cachedHead; } }
        public FleckDef PourDripFleck { get { ResolveFlecks(); return cachedPourDrip; } }
        public FleckDef PourSmokeFleck { get { ResolveFlecks(); return cachedPourSmoke; } }
        public FleckDef RestFleck { get { ResolveFlecks(); return cachedRest; } }
        public FleckDef ThreadFleck { get { ResolveFlecks(); return cachedThread; } }
        public FleckDef ThreadSnipFleck { get { ResolveFlecks(); return cachedThreadSnip; } }

        public SoundDef StrikeSound
        {
            get
            {
                if (!strikeSoundResolved)
                {
                    strikeSoundResolved = true;
                    if (!strikeSound.NullOrEmpty())
                        cachedStrikeSound = DefDatabase<SoundDef>.GetNamedSilentFail(strikeSound);
                }
                return cachedStrikeSound;
            }
        }

        /// <summary>
        /// Force every lazily-created Material / FleckDef / SoundDef on this def into existence.
        ///
        /// Called off the render path by <see cref="ToolWarmup"/>, a couple of defs per frame. Each
        /// of the getters below is a "create on first access" cache, so without this the FIRST frame
        /// a colonist is drawn holding a given tool pays for all of them at once, inside the render
        /// postfix — which is exactly the 17.5 ms max-frame the profiler was reporting against an
        /// 0.044 ms average. Warming is pure cache population: no behavior changes, and a second
        /// call is free.
        /// </summary>
        public void WarmCache()
        {
            // Cutout + Transparent (holster fade) variants of the main sprite.
            _ = Material;
            _ = MaterialTransparent;
            // Facing / pose overrides.
            _ = NorthSouthMaterial;
            _ = ProfileMaterial;
            _ = PourEmptyMaterial;
            // Secondary sprites.
            _ = MalletMaterial;
            _ = MaskMaterial;
            _ = MaskMaterialFor(Rot4.East, out _);
            _ = MaskMaterialFor(Rot4.West, out _);
            _ = MaskMaterialFor(Rot4.North, out _);
            _ = PropMaterial;
            _ = SidePropMaterial;
            _ = AltMaterial;
            _ = StrikeMaterial;
            _ = OffhandMaterial;
            // Frame animations + the depletion strip. These are the big ones: a frame-animated tool
            // can hold a dozen textures, every one of them created on the frame it first ticks over.
            if (frameTexPaths != null)
            {
                for (int i = 0; i < frameTexPaths.Count; i++)
                {
                    _ = FrameMaterial(i);
                    _ = FrameMaterialTransparent(i);
                }
                _ = FrameRefSize();   // reads every frame's mainTexture; must come after the loads
            }
            if (depletionTexPaths != null)
                for (int i = 0; i < depletionTexPaths.Count; i++)
                    _ = DepletionMaterial(i);
            // Def lookups (one-shot flags, but they're DefDatabase hits on the hot path otherwise).
            ResolveFlecks();
            _ = StrikeSound;
        }

        private void ResolveFlecks()
        {
            if (fleckResolved) return;
            fleckResolved = true;
            if (!impactBitFleck.NullOrEmpty())
                cachedBitFleck = DefDatabase<FleckDef>.GetNamedSilentFail(impactBitFleck);
            if (!impactBitFleck2.NullOrEmpty())
                cachedBitFleck2 = DefDatabase<FleckDef>.GetNamedSilentFail(impactBitFleck2);
            if (!impactFlashFleck.NullOrEmpty())
                cachedFlashFleck = DefDatabase<FleckDef>.GetNamedSilentFail(impactFlashFleck);
            if (!tipGlowFleck.NullOrEmpty())
                cachedTipGlow = DefDatabase<FleckDef>.GetNamedSilentFail(tipGlowFleck);
            if (!tipEmberFleck.NullOrEmpty())
                cachedTipEmber = DefDatabase<FleckDef>.GetNamedSilentFail(tipEmberFleck);
            if (!impactDustFleck.NullOrEmpty())
                cachedDust = DefDatabase<FleckDef>.GetNamedSilentFail(impactDustFleck);
            if (!headFleck.NullOrEmpty())
                cachedHead = DefDatabase<FleckDef>.GetNamedSilentFail(headFleck);
            if (!pourDripFleck.NullOrEmpty())
                cachedPourDrip = DefDatabase<FleckDef>.GetNamedSilentFail(pourDripFleck);
            if (!pourSmokeFleck.NullOrEmpty())
                cachedPourSmoke = DefDatabase<FleckDef>.GetNamedSilentFail(pourSmokeFleck);
            if (!threadFleck.NullOrEmpty())
                cachedThread = DefDatabase<FleckDef>.GetNamedSilentFail(threadFleck);
            if (!threadSnipFleck.NullOrEmpty())
                cachedThreadSnip = DefDatabase<FleckDef>.GetNamedSilentFail(threadSnipFleck);
            if (!restFleck.NullOrEmpty())
                cachedRest = DefDatabase<FleckDef>.GetNamedSilentFail(restFleck);
        }
    }
}
