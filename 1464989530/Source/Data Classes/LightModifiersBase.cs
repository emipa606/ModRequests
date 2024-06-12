// Nightvision NightVision LightModifiers.cs
// 
// 16 05 2018
// 
// 21 07 2018

using JetBrains.Annotations;
using System;
using System.Linq;
using Verse;

namespace NightVision
{
    public class LightModifiersBase : IExposable, ISaveCheck
    {
        public static LightModifiersBase NVLightModifiers = new LightModifiersBase
        {
            Offsets = new[]
            {
                Constants.NVDefaultOffsets[0],
                Constants.NVDefaultOffsets[1]
            },
            Initialised = true
        };

        public static LightModifiersBase PSLightModifiers = new LightModifiersBase
        {
            Offsets = new[]
            {
                Constants.PSDefaultOffsets[0],
                Constants.PSDefaultOffsets[1]
            },
            Initialised = true
        };

        public bool Initialised;

        public float[] Offsets = new float[2];

        public LightModifiersBase()
        {
        }

        [UsedImplicitly]
        public LightModifiersBase(
            bool isPhotosensitiveLm,
            bool isNightVisionLm
        )
        {
            if (isPhotosensitiveLm)
            {
                PSLightModifiers = this;
            }
            else if (isNightVisionLm)
            {
                NVLightModifiers = this;
            }
        }

        public virtual float this[
            int index
        ]
        {
            get => Offsets[index];
            set => Offsets[index] = value;
        }


        public virtual Def ParentDef => null;

        public virtual VisionType Setting
        {
            get => VisionType.NVNone;
            set { }
        }

        public virtual float[] DefaultOffsets
        {
            get
            {
                if (this == NVLightModifiers)
                {
                    return Constants.NVDefaultOffsets;
                }

                if (this == PSLightModifiers)
                {
                    return Constants.PSDefaultOffsets;
                }

                return new float[2];
            }
        }


        /// <summary>
        ///     Save and load: in this class only used for NV and PS settings
        /// </summary>
        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref Offsets[0], "ZeroOffset", forceSave: true);
            Scribe_Values.Look(ref Offsets[1], "FullOffset", forceSave: true);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (Offsets == null)
                {
                    if (this == NVLightModifiers)
                    {
                        Offsets = Constants.NVDefaultOffsets.ToArray();
                    }

                    else if (this == PSLightModifiers)
                    {
                        Offsets = Constants.PSDefaultOffsets.ToArray();
                    }
                    else
                    {
                        Offsets = new float[2];
                    }
                }

                Initialised = true;
            }
        }

        public virtual bool ShouldBeSaved()
        {
            return true;
        }

        public virtual float GetEffectAtGlow(
            float glow,
            int numEyesNormalisedFor = 1
        )
        {
            if (glow < 0.001)
            {
                return (float) Math.Round(
                    this[0] / numEyesNormalisedFor,
                    Constants.NUMBER_OF_DIGITS,
                    Constants.ROUNDING
                );
            }

            if (glow > 0.999)
            {
                return (float) Math.Round(
                    this[1] / numEyesNormalisedFor,
                    Constants.NUMBER_OF_DIGITS,
                    Constants.ROUNDING
                );
            }

            if (glow.GlowIsDarkness())
            {
                return (float) Math.Round(
                    this[0] / numEyesNormalisedFor * (0.3f - glow) / 0.3f,
                    Constants.NUMBER_OF_DIGITS,
                    Constants.ROUNDING
                );
            }

            if (glow.GlowIsBright())
            {
                return (float) Math.Round(
                    this[1] / numEyesNormalisedFor * (glow - 0.7f) / 0.3f,
                    Constants.NUMBER_OF_DIGITS,
                    Constants.ROUNDING
                );
            }

            return 0;
        }

        public static float[] GetCapsAtGlow(
            float glow
        )
        {
            float mincap;
            float maxcap;
            float nvcap;
            float pscap;
            var caps = Settings.Store.MultiplierCaps;

            if (glow.GlowIsDarkness())
            {
                mincap = (caps.min - Constants.DEFAULT_ZERO_LIGHT_MULTIPLIER)
                       * (0.3f - glow)
                       / 0.3f;

                maxcap = (caps.max - Constants.DEFAULT_ZERO_LIGHT_MULTIPLIER)
                       * (0.3f - glow)
                       / 0.3f;

                pscap = PSLightModifiers[0] * (0.3f - glow) / 0.3f;
                nvcap = NVLightModifiers[0] * (0.3f - glow) / 0.3f;
            }
            else
            {
                mincap = (caps.min - Constants.DEFAULT_FULL_LIGHT_MULTIPLIER)
                       * (glow - 0.7f)
                       / 0.3f;

                maxcap = (caps.max - Constants.DEFAULT_FULL_LIGHT_MULTIPLIER)
                       * (glow - 0.7f)
                       / 0.3f;

                pscap = PSLightModifiers[1] * (glow - 0.7f) / 0.3f;
                nvcap = NVLightModifiers[1] * (glow - 0.7f) / 0.3f;
            }

            return new[]
            {
                (float) Math.Round(maxcap, Constants.NUMBER_OF_DIGITS, Constants.ROUNDING),
                (float) Math.Round(mincap, Constants.NUMBER_OF_DIGITS, Constants.ROUNDING),
                (float) Math.Round(nvcap, Constants.NUMBER_OF_DIGITS, Constants.ROUNDING),
                (float) Math.Round(pscap, Constants.NUMBER_OF_DIGITS, Constants.ROUNDING)
            };
        }

        public bool HasAnyModifier()
        {
            return Math.Abs(this[0]) > 0.001 || Math.Abs(this[1]) > 0.001;
        }

        public bool HasAnyCustomModifier()
        {
            return Math.Abs(Offsets[0]) > 0.001 || Math.Abs(Offsets[1]) > 0.001;
        }


        public bool IsCustom()
        {
            return Setting == VisionType.NVCustom;
        }

        public void ChangeSetting(
            VisionType newsetting
        )
        {
            if (Setting != newsetting)
            {
                if (newsetting == VisionType.NVCustom && !HasAnyCustomModifier())
                {
                    var defaultValues = DefaultOffsets;

                    if (Math.Abs(defaultValues[0]) > 0.001 && Math.Abs(defaultValues[1]) > 0.001)
                    {
                        Offsets = DefaultOffsets.ToArray();
                    }
                    else if (Setting == VisionType.NVNightVision)
                    {
                        Offsets = NVLightModifiers.Offsets.ToArray();
                    }
                    else if (Setting == VisionType.NVPhotosensitivity)
                    {
                        Offsets = PSLightModifiers.Offsets.ToArray();
                    }
                }
            }

            Setting = newsetting;
        }


        /// <inheritdoc />
        public override string ToString()
        {
            return $"LightMod::[ 0%: {Offsets[0]}, 100%: {Offsets[1]}]";
        }
    }
}