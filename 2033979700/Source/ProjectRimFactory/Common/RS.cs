﻿using UnityEngine;
using Verse;

namespace ProjectRimFactory
{
    [StaticConstructorOnStartup]
    public static class RS
    {
        static RS()
        {
            PregnantIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Pregnant", true);
            BondIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Bond", true);
            MaleIcon = GenderUtility.GetIcon(Gender.Male);
            FemaleIcon = GenderUtility.GetIcon(Gender.Female);
            SlaughterIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Slaughter", true);
            TrainedIcon = ContentFinder<Texture2D>.Get("UI/Icons/Trainables/Obedience", true);
            YoungIcon = ContentFinder<Texture2D>.Get("UI/Icons/LifeStage/Young", true);
            AdultIcon = ContentFinder<Texture2D>.Get("UI/Icons/LifeStage/Adult", true);

            ForbidOn = ContentFinder<Texture2D>.Get("UI/Designators/ForbidOn", true);
            ForbidOff = ContentFinder<Texture2D>.Get("UI/Designators/ForbidOff", true);


            OutputDirectionIcon = ContentFinder<Texture2D>.Get("PRFUi/OutputDirection", true);
            ForbidIcon = ContentFinder<Texture2D>.Get("PRFUi/Forbid", true);
            PlayIcon = ContentFinder<Texture2D>.Get("PRFUi/Play", true);

            SplitterDisabeld = ContentFinder<Texture2D>.Get("PRFUi/ForbidIcon", true);
            SplitterArrow_Up = ContentFinder<Texture2D>.Get("PRFUi/UpArrow", true);
            SplitterArrow_Right = ContentFinder<Texture2D>.Get("PRFUi/RightArrow", true);
            SplitterArrow_Left = ContentFinder<Texture2D>.Get("PRFUi/LeftArrow", true);
            SplitterArrow_Down = ContentFinder<Texture2D>.Get("PRFUi/DownArrow", true);


            DeleteX = ContentFinder<Texture2D>.Get("UI/Buttons/Delete", true);

            Arrow = ContentFinder<Texture2D>.Get("UI/Overlays/Arrow", true);
            // Initialize graphics for SpecialSculptures:
            foreach (var s in ProjectRimFactory.Common.
                     ProjectRimFactory_ModComponent.availableSpecialSculptures)
                s.Init();
        }

        public static readonly Texture2D SplitterArrow_Up;
        public static readonly Texture2D SplitterArrow_Right;
        public static readonly Texture2D SplitterArrow_Left;
        public static readonly Texture2D SplitterArrow_Down;
        public static readonly Texture2D SplitterDisabeld;

        public static readonly Texture2D ForbidOn;
        public static readonly Texture2D ForbidOff;

        public static readonly Texture2D PregnantIcon;
        public static readonly Texture2D BondIcon;
        public static readonly Texture2D MaleIcon;
        public static readonly Texture2D FemaleIcon;

        public static readonly Texture2D SlaughterIcon;
        public static readonly Texture2D TrainedIcon;
        public static readonly Texture2D YoungIcon;
        public static readonly Texture2D AdultIcon;

        public static readonly Texture2D OutputDirectionIcon;
        public static readonly Texture2D ForbidIcon;
        public static readonly Texture2D PlayIcon;

        public static readonly Texture2D DeleteX;

        public static readonly Texture2D Arrow;
    }
}
