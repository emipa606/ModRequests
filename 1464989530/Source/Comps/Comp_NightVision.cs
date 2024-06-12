// Nightvision NightVision Comp_NightVision.cs
// 
// 16 05 2018
// 
// 21 07 2018

using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace NightVision
{
    public class Comp_NightVision : ThingComp
    {
        private bool _apparelNeedsChecking;
        private float _fullLightModifier = -1;


        private bool _hediffsNeedChecking;
        private Pawn _intParentPawn;
        private Race_LightModifiers _naturalLightModifiers;

        private int _numRemainingEyes = -1;
        private List<Hediff> _pawnsHediffs;
        public List<BodyPartRecord> _raceSightParts;
        private float _zeroLightModifier = -1;
        public bool ApparelGrantsNV;
        public bool ApparelNullsPS;
        public float[] HediffMods = new float[2];

        public int LastDarkTick;
        public float[] NvhediffMods = new float[2];

        public List<Apparel> PawnsNVApparel = new List<Apparel>();
        public Dictionary<string, List<HediffDef>> PawnsNVHediffs = new Dictionary<string, List<HediffDef>>();
        public float[] PshediffMods = new float[2];

        /// <summary>
        ///     The glow mods of the pawns races eyes
        ///     Null Checks, if fails gets eye glow mods normalised for race's number of eyes
        /// </summary>
        public Race_LightModifiers NaturalLightModifiers
        {
            get
            {
                if (_naturalLightModifiers == null)
                {
                    _naturalLightModifiers = Settings.Store.RaceLightMods.TryGetValue(ParentPawn.def);

                    if (_naturalLightModifiers == null)
                    {
                        _naturalLightModifiers =
                            new Race_LightModifiers(ParentPawn.def);
                        Settings.Store.RaceLightMods[ParentPawn.def] = _naturalLightModifiers;

                        Log.Message(
                            $"Natural Light Modifiers could not be found for {ParentPawn} of race: {ParentPawn.def}. An entry has been generated."
                        );

                        Log.Message(
                            "If BattleRoyale is running it is recommended that after it has completed you open and close the Night Vision mod settings menu to clear all extraneous entries.");
                    }

                    _zeroLightModifier = -1f;
                    _fullLightModifier = -1f;
                }

                return _naturalLightModifiers;
            }
        }

        public bool CanCheat => Props.CanCheat;

        public int TicksSinceLastDark => Find.TickManager.TicksGame - LastDarkTick;

        [UsedImplicitly] public CompProperties_NightVision Props => (CompProperties_NightVision)props;

        public Pawn ParentPawn => _intParentPawn ?? (_intParentPawn = parent as Pawn);

        public List<Hediff> PawnHediffs =>
            _pawnsHediffs ?? (_pawnsHediffs = ParentPawn?.health?.hediffSet?.hediffs);

        public int EyeCount => RaceSightParts.Count;

        public List<BodyPartRecord> RaceSightParts
        {
            get
            {
                if (_raceSightParts.NullOrEmpty())
                {
                    _raceSightParts = ParentPawn
                        ?.RaceProps.body.GetPartsWithTag(Defs_Rimworld.EyeTag)
                        .ToList();

                    if (_raceSightParts.NullOrEmpty())
                    {
                        Log.Message(
                            $"{ParentPawn?.LabelShort}'s race has no eyes. The NightVision Comp should not be attached.");
                        Log.Message(
                            "Creating an empty list for their races eyes but NV will not have any effect");
                        return new List<BodyPartRecord>();
                    }
                }

                return _raceSightParts;
            }
        }

        /// <summary>
        ///     Get: returns the number of pawns RACE's eyes if private float numRemainingEyes is negative
        ///     Set: If the value is less than 0 then will set = 0;
        /// </summary>
        public int NumberOfRemainingEyes
        {
            get
            {
                if (_numRemainingEyes < 0)
                {
                    _numRemainingEyes = RaceSightParts.Count;
                }

                return _numRemainingEyes;
            }
            set => _numRemainingEyes = value < 0 ? 0 : value;
        }

        public float ZeroLightModifier
        {
            get
            {
                //true on init, hediffs changed, apparel changed or settings changed
                if (_zeroLightModifier < -0.99f)
                {
                    //Also gets calculated every rare tick
                    if (_apparelNeedsChecking)
                    {
                        QuickRecheckApparel();
                        _apparelNeedsChecking = false;
                    }

                    //Also gets calculated every rare tick
                    if (_hediffsNeedChecking)
                    {
                        CalculateHediffMod();
                        _hediffsNeedChecking = false;
                    }

                    //Also gets calculated every rare tick
                    _zeroLightModifier = CalcZeroLightModifier();
                }

                if (ApparelGrantsNV && _zeroLightModifier + Constants.NV_EPSILON
                    < LightModifiersBase.NVLightModifiers[0])
                {
                    return LightModifiersBase.NVLightModifiers[0];
                }

                return _zeroLightModifier;
            }
        }

        public float FullLightModifier
        {
            get
            {
                //true on init, hediffs changed, apparel changed or settings changed
                if (_fullLightModifier < -0.99f)
                {
                    //Also gets calculated every rare tick
                    if (_apparelNeedsChecking)
                    {
                        QuickRecheckApparel();
                        _apparelNeedsChecking = false;
                    }

                    //Also gets calculated every rare tick
                    if (_hediffsNeedChecking)
                    {
                        CalculateHediffMod();
                        _hediffsNeedChecking = false;
                    }

                    //Also gets calculated every rare tick
                    _fullLightModifier = CalcFullLightModifier();
                }

                if (ApparelNullsPS && _fullLightModifier + Constants.NV_EPSILON < 0f)
                {
                    return 0f;
                }

                return _fullLightModifier;
            }
        }

        public void SetDirty()
        {
            _naturalLightModifiers = null;
            _zeroLightModifier = -1f;
            _fullLightModifier = -1f;
            _apparelNeedsChecking = true;
            _hediffsNeedChecking = true;
            ClearPsych();
        }

        /// <summary>
        ///     To calculate the effects of light on movement speed and work speed:
        /// </summary>
        /// <param name="glow">light level as float in [0,1]</param>
        /// <returns>a multiplier: (default + light offsets)</returns>
        public float FactorFromGlow(
            float glow)
        {
            //If glow is approx. 0%
            if (glow.IsTrivial())
            {
                return (float)Math.Round(Constants.DEFAULT_ZERO_LIGHT_MULTIPLIER + ZeroLightModifier,
                    Constants.NUMBER_OF_DIGITS);
            }
            //If glow is approx. 100% and the pawns full light modifier is not approx 0

            if (glow.ApproxEq(1f))
            {
                if (FullLightModifier.IsNonTrivial())
                {
                    return (float)Math.Round(
                        Constants.DEFAULT_FULL_LIGHT_MULTIPLIER + FullLightModifier,
                        Constants.NUMBER_OF_DIGITS);
                }

                return Constants.TRIVIAL_FACTOR;
            }
            //Else linear interpolation

            if (glow.GlowIsDarkness())
            {
                return (float)Math.Round(
                    1f + (Constants.MIN_GLOW_NO_GLOW - glow) * (ZeroLightModifier - 0.2f) / 0.3f,
                    Constants.NUMBER_OF_DIGITS);
            }

            if (glow.GlowIsBright() && FullLightModifier.IsNonTrivial())
            {
                return (float)Math.Round(
                    1f + (glow - Constants.MAX_GLOW_NO_GLOW) * FullLightModifier / 0.3f,
                    Constants.NUMBER_OF_DIGITS);
            }

            return Constants.TRIVIAL_FACTOR;
        }

        public override void CompTickRare()
        {
            if (!UpdateComp())
            {
                return;
            }
#if DEBUG
            if (DebugTab.LogPawnComps)
            {
                Log.Message(new string('*', 30));
                Log.Message("NightVisionComp: " + ParentPawn.Name);
                Log.Message("Hediffs: ");

                foreach (var hedifflist in PawnsNVHediffs)
                {
                    Log.Message(new string('-', 20));
                    Log.Message(hedifflist.Key + "has:");

                    foreach (var hediff in hedifflist.Value)
                    {
                        Log.Message(hediff.LabelCap);
                    }
                }

                Log.Message(new string('-', 20));
                Log.Message("NumberRemainingEyes: " + NumberOfRemainingEyes);
                Log.Message("HediffLightModifiers: zero = " + HediffMods[0] + PshediffMods[0]
                            + NvhediffMods[0] + ", full = " + HediffMods[0] + PshediffMods[0]
                            + NvhediffMods[0]);
                Log.Message("NaturalLightModifiers: zero = " + NaturalLightModifiers[0] + ", full = "
                            + NaturalLightModifiers[1]);
                Log.Message("Total Glow Mods: zero = " + ZeroLightModifier + ", full = "
                            + FullLightModifier);
                Log.Message(new string('*', 30));
            }
#endif
        }

        private bool UpdateComp()
        {
            if (!ParentPawn.Spawned || ParentPawn.Dead)
            {
                return false;
            }

            if (_apparelNeedsChecking)
            {
                QuickRecheckApparel();
                _apparelNeedsChecking = false;
            }

            if (_hediffsNeedChecking)
            {
                CalculateHediffMod();
                _hediffsNeedChecking = false;
            }

            if (_fullLightModifier < -0.99f)
            {
                _fullLightModifier = CalcFullLightModifier();
            }

            if (_zeroLightModifier < -0.99f)
            {
                _zeroLightModifier = CalcZeroLightModifier();
            }

            ClearPsych();
            return true;
        }

        //Note: we don't save this comp so this only gets called when spawning new pawn, i think
        public override void PostSpawnSetup(
            bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            InitPawnsHediffsAndCountEyes();
            InitPawnsApparel();
        }

        public override void Initialize(
            CompProperties compprops)
        {
            base.Initialize(compprops);

            //Note: Pawn.Dead is equivalent to ParentPawn.health.Dead, therefore null check
            if (parent == null || !ParentPawn.Spawned || ParentPawn.health == null || ParentPawn.Dead)
            {
                return;
            }

            InitPawnsHediffsAndCountEyes();
            InitPawnsApparel();
        }

        public void RemoveHediff(
            Hediff hediff,
            BodyPartRecord part = null)
        {
            if (!ParentPawn.Spawned || ParentPawn.health == null || ParentPawn.Dead)
            {
                return;
            }

            if (part != null && PawnsNVHediffs.ContainsKey(part.Label)
                             && PawnsNVHediffs[part.Label].Remove(hediff.def))
            {
                if (part.def.tags.Contains(Defs_Rimworld.EyeTag)
                    && (hediff is Hediff_MissingPart
                        || hediff.def.addedPartProps is AddedBodyPartProps abpp && abpp.solid))
                {
                    NumberOfRemainingEyes++;
                }

                CalculateHediffMod();
            }
            else if (PawnsNVHediffs[Str.BodyKey].Remove(hediff.def))
            {
                CalculateHediffMod();
            }
        }

        public void CheckAndAddHediff(
            Hediff hediff,
            BodyPartRecord part = null)
        {
            if (ParentPawn.Dead || !ParentPawn.Spawned)
            {
                return;
            }

            var allSightHediffs = Settings.Store.AllSightAffectingHediffs;

            if (part != null)
            {
                var partName = part.Label;

                if (PawnsNVHediffs.TryGetValue(partName, out var tempPartsHediffDefs))
                {
                    //Categorise the hediff:
                    //MissingPart overrides everything
                    var removedPart = false;

                    if (hediff is Hediff_MissingPart)
                    {
                        removedPart = true;
                        tempPartsHediffDefs = new List<HediffDef> { HediffDefOf.MissingBodyPart };
                    }
                    else
                    {
                        //Check if there is a setting for it
                        var hediffDef = hediff.def;

                        if (Settings.Store.HediffLightMods.ContainsKey(hediffDef))
                        {
                            if (hediffDef.addedPartProps is AddedBodyPartProps abpp
                                && abpp.solid)
                            {
                                removedPart = true;
                                tempPartsHediffDefs = new List<HediffDef> { hediffDef };
                            }
                            else if (!tempPartsHediffDefs.Contains(hediffDef))
                            {
                                tempPartsHediffDefs.Add(hediffDef);
                            }
                        }
                        //Last two checks are in case the user changes settings later
                        //Is it a solid replacement?
                        else if (hediffDef.addedPartProps is AddedBodyPartProps abpp
                                 && abpp.solid)
                        {
                            removedPart = true;
                            tempPartsHediffDefs = new List<HediffDef> { hediffDef };
                        }
                        //Check if it is a valid hediff
                        else if (allSightHediffs.Contains(hediffDef))
                        {
                            if (!tempPartsHediffDefs.Contains(hediffDef))
                            {
                                tempPartsHediffDefs.Add(hediffDef);
                            }
                        }
                    }

                    //remove an eye
                    if (removedPart && part.def.tags.Contains(Defs_Rimworld.EyeTag))
                    {
                        NumberOfRemainingEyes--;
                    }

                    PawnsNVHediffs[partName] = tempPartsHediffDefs;
                    CalculateHediffMod();
                }
                else if (allSightHediffs.Contains(hediff.def))
                {
                    PawnsNVHediffs[partName] = new List<HediffDef> { hediff.def };
                }
            }
            else if (allSightHediffs.Contains(hediff.def))
            {
                if (PawnsNVHediffs.TryGetValue(Str.BodyKey, out var value)
                    && !value.Contains(hediff.def))
                {
                    PawnsNVHediffs[Str.BodyKey].Add(hediff.def);
                    CalculateHediffMod();
                }
            }
        }

        public void CheckAndAddApparel(
            Apparel apparel)
        {
            if (ParentPawn.Dead || !ParentPawn.Spawned || apparel == null)
            {
                return;
            }

            if (Settings.Store.NVApparel.TryGetValue(apparel.def, out var value))
            {
                ApparelGrantsNV |= value.GrantsNV;
                ApparelNullsPS |= value.NullifiesPS;
                PawnsNVApparel.Add(apparel);
                ClearPsych();
            }
            //In case the user changes settings in game
            else if (Settings.Store.AllEyeCoveringHeadgearDefs.Contains(apparel.def))
            {
                PawnsNVApparel.Add(apparel);
            }
        }

        public void RemoveApparel(
            Apparel apparel)
        {
            if (ParentPawn.Dead || !ParentPawn.Spawned || apparel == null || !PawnsNVApparel.Contains(apparel))
            {
                return;
            }

            PawnsNVApparel.Remove(apparel);

            if (!(ApparelGrantsNV || ApparelNullsPS))
            {
                return;
            }

            QuickRecheckApparel();
        }

        private void CalculateHediffMod()
        {
            var zeroMod = 0f;
            var fullMod = 0f;
            var psZeroMod = 0f;
            var psFullMod = 0f;
            var nvZeroMod = 0f;
            var nvFullMod = 0f;
            var eyeCount = RaceSightParts.Count;

            var hediffLightMods = Settings.Store.HediffLightMods;

            foreach (var value in PawnsNVHediffs.Values)
            {
                if (value.NullOrEmpty())
                {
                    continue;
                }


                foreach (var hediffDef in value)
                {
                    if (hediffLightMods.TryGetValue(hediffDef,
                        out var hediffSetting))
                    {
                        var eyeNormalisingFactor = hediffSetting.AffectsEye ? eyeCount : 1;

                        switch (hediffSetting.Setting)
                        {
                            case VisionType.NVNone: continue;
                            case VisionType.NVCustom:
                                zeroMod += hediffSetting[0] / eyeNormalisingFactor;
                                fullMod += hediffSetting[1] / eyeNormalisingFactor;
                                continue;
                            case VisionType.NVNightVision:
                                nvZeroMod += hediffSetting[0] / eyeNormalisingFactor;
                                nvFullMod += hediffSetting[1] / eyeNormalisingFactor;
                                continue;
                            case VisionType.NVPhotosensitivity:
                                psZeroMod += hediffSetting[0] / eyeNormalisingFactor;
                                psFullMod += hediffSetting[1] / eyeNormalisingFactor;
                                continue;
                            default: continue;
                        }
                    }
                }
            }

            HediffMods[0] = zeroMod;
            NvhediffMods[0] = nvZeroMod;
            PshediffMods[0] = psZeroMod;

            HediffMods[1] = fullMod;
            NvhediffMods[1] = nvFullMod;
            PshediffMods[1] = psFullMod;

            _zeroLightModifier = -1f;
            _fullLightModifier = -1f;
            _hediffsNeedChecking = false;
            ClearPsych();
        }

        private float SumModifier(int index)
        {
            var nvMod = NvhediffMods[index];
            var psMod = PshediffMods[index];
            var hdMod = HediffMods[index];
            switch (NaturalLightModifiers.IntSetting)
            {
                case VisionType.NVNightVision: //Nightvision
                    nvMod += NaturalLightModifiers[index] * _numRemainingEyes;
                    break;
                case VisionType.NVPhotosensitivity: // Photosensitive
                    psMod += NaturalLightModifiers[index] * _numRemainingEyes;
                    break;
                case VisionType.NVCustom: //Custom
                    hdMod += NaturalLightModifiers[index] * _numRemainingEyes;
                    break;
            }

            var mod = index == 0 ? Constants.DEFAULT_ZERO_LIGHT_MULTIPLIER : Constants.DEFAULT_FULL_LIGHT_MULTIPLIER;
            return NVMaths.ClampMods(mod, nvMod, psMod, hdMod, LightModifiersBase.NVLightModifiers[index],
                LightModifiersBase.PSLightModifiers[index], CanCheat);
        }

        private float CalcZeroLightModifier()
        {
            return SumModifier(0);
        }

        private float CalcFullLightModifier()
        {
            return SumModifier(1);
        }

        /// <summary>
        ///     Builds a dictionary to store all the possibly relevant hediffs:
        ///     <para>
        ///         Keys: initially .Label of all sight parts & catch-all "wholebody" but will add other parts at runtime if
        ///         necessary
        ///     </para>
        ///     <para>Values: List of all the hediff defs that affect that part</para>
        /// </summary>
        private void InitPawnsHediffsAndCountEyes()
        {
            if (!ParentPawn.Spawned || ParentPawn.health == null || ParentPawn.Dead)
            {
                return;
            }

            NumberOfRemainingEyes = RaceSightParts.Count;
            PawnsNVHediffs.Clear();

            for (var i = 0; i < NumberOfRemainingEyes; i++)
            {
                PawnsNVHediffs[RaceSightParts[i].Label] = new List<HediffDef>();
            }

            PawnsNVHediffs[Str.BodyKey] = new List<HediffDef>();

            if (!PawnHediffs.NullOrEmpty())
            {
                foreach (var hediff in PawnHediffs)
                {
                    CheckAndAddHediff(hediff, hediff.Part);
                }
            }

            CalculateHediffMod();
        }

        private void InitPawnsApparel()
        {
            if (!ParentPawn.Spawned || ParentPawn.health == null || ParentPawn.Dead)
            {
                return;
            }

            ApparelGrantsNV = false;
            ApparelNullsPS = false;
            PawnsNVApparel = new List<Apparel>();

            if (!(ParentPawn.apparel?.WornApparel is List<Apparel> pawnsApparel))
            {
                return;
            }

            var nvApparel = Settings.Store.NVApparel;
            var allEyeGear = Settings.Store.AllEyeCoveringHeadgearDefs;

            foreach (var apparel in pawnsApparel)
            {
                if (nvApparel.TryGetValue(apparel.def, out var value))
                {
                    ApparelGrantsNV |= value.GrantsNV;
                    ApparelNullsPS |= value.NullifiesPS;
                    PawnsNVApparel.Add(apparel);
                }
                // proof against night vision settings being changed during play??
                else if (allEyeGear.Contains(apparel.def))
                {
                    PawnsNVApparel.Add(apparel);
                }
            }

            ClearPsych();
        }

        /// <summary>
        ///     This only works if PawnsNVApparel is correct
        /// </summary>
        private void QuickRecheckApparel()
        {
            ApparelGrantsNV = false;
            ApparelNullsPS = false;
            var nvApparel = Settings.Store.NVApparel;

            foreach (var apparel in PawnsNVApparel)
            {
                if (!nvApparel.TryGetValue(apparel.def, out var value))
                {
                    continue;
                }

                ApparelGrantsNV |= value.GrantsNV;
                ApparelNullsPS |= value.NullifiesPS;
            }

            _apparelNeedsChecking = false;
            ClearPsych();
        }

        /// <summary>
        ///     For ThoughtWorker_Dark patch
        /// </summary>
        /// <returns></returns>
        public VisionType PsychDark
        {
            get
            {
                if (DarknessPsych == null)
                {
                    DarknessPsych = Classifier.ClassifyModifier(ZeroLightModifier, true);
                }

                return (VisionType)DarknessPsych;
            }
        }

        private VisionType? BrightLightPsych;
        private VisionType? DarknessPsych;

        private void ClearPsych()
        {
            BrightLightPsych = null;
            DarknessPsych = null;
        }


        /// <summary>
        ///     For ThoughtWorker_TooBright
        /// </summary>
        public bool PsychBright
        {
            get
            {
                if (BrightLightPsych == null)
                {
                    BrightLightPsych = Classifier.ClassifyModifier(FullLightModifier, false);
                }

                return TicksSinceLastDark > Constants.THOUGHT_ACTIVE_TICKS_PAST
                       && BrightLightPsych == VisionType.NVPhotosensitivity;
            }
        }
    }
}