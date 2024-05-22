using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using FloatMenuWrapper;

namespace TakeOffIndoor
{
    public enum RuleType
    {
        //翻訳の都合が良いのでTabRaceを使用
        Trait = 0, 
        TabRace = 1,
        Gender=2
    }

    public class GenderWrapperDef:Def
    {
        public Gender gender;
        public static GenderWrapperDef Male
        {
            get
            {
                var ret = new GenderWrapperDef();
                ret.gender = Gender.Male;
                ret.defName = ret.gender.ToString();
                ret.label = ret.gender.GetLabel();
                return ret;
            }
        }
        public static GenderWrapperDef Female
        {
            get
            {
                var ret = new GenderWrapperDef();
                ret.gender = Gender.Female;
                ret.defName = ret.gender.ToString();
                ret.label = ret.gender.GetLabel();
                return ret;
            }
        }
        public static GenderWrapperDef None
        {
            get
            {
                var ret = new GenderWrapperDef();
                ret.gender = Gender.None;
                ret.defName = ret.gender.ToString();
                ret.label = ret.gender.GetLabel();
                return ret;
            }
        }

        public static GenderWrapperDef GetGender(string genderStr)
        {
            switch (genderStr)
            {
                case "Male":
                    return Male;
                case "Female":
                    return Female;
                case "None":
                    return None;
            }
            return None;
        }

        public static bool Evaluate(string genderStr)
        {
            return genderStr == GetGender(genderStr).defName;
        }
    }
    public class RuleFilter : IExposable
    {
        public RuleType type;

        private string itemValue;
        public bool logicOr = false; //falseならand,trueならor
        public bool logicNot = false; //trueなら結果を反転させる
        private Def targetDef;

        private bool first = true;
        private bool noDef = false;


        private bool menuFirst = true;

        public static List<string> typeNameList = new List<string>(new string[] {RuleType.Trait.ToString(), RuleType.TabRace.ToString(),RuleType.Gender.ToString() });

        FloatMenuW fmwType = new FloatMenuW();
        FloatMenuW fmwRace = new FloatMenuW();
        FloatMenuW fmwTrait = new FloatMenuW();
        FloatMenuW fmwGender = new FloatMenuW();


        public static List<string> raceDefList = new List<string>();
        public static List<string> raceLabelList = new List<string>();
        public static List<TraitDef> traitList = new List<TraitDef>();
        public static List<Gender> genderList = new List<Gender>();

        public static bool firstStatic = true;

        public RuleFilter()
        {

        }
        public RuleFilter(RuleType _type, string _value, bool _or = false, bool _not = false)
        {
            type = _type;
            itemValue = _value;
            logicOr = _or;
            logicNot = _not;

            targetDef = GetDef();
            if (targetDef == null)
            {
                noDef = true;
            }
        }

        private static void InitializeList()
        {
            if (firstStatic)
            {
                if (ModUtil.ExistHAR)
                {
                    HARWrapper.GetHARNames(out List<string> raceDefName, out List<string> raceLabel);
                    raceDefList.AddRange(raceDefName);
                    raceLabelList.AddRange(raceLabel);
                }

                foreach (TraitDef trait in DefDatabase<TraitDef>.AllDefsListForReading)
                {
                    traitList.Add(trait);
                }

                genderList.AddRange(new Gender[] { Gender.Male, Gender.Female, Gender.None });

                firstStatic = false;
            }
        }
        private void InitializeMenu()
        {
            if (menuFirst)
            {
                menuFirst = false;
                int cnt = 0;

                fmwType.AddMenu(RuleType.Trait.ToString().Translate(), cnt++);
                if (ModUtil.ExistHAR)
                {
                    fmwType.AddMenu(RuleType.TabRace.ToString().Translate(), cnt++);
                }
                else cnt++;
                fmwType.AddMenu(RuleType.Gender.ToString().Translate(), cnt++);
                fmwType.SetAction(TypeListCommand);

                if (ModUtil.ExistHAR)
                {
                    for (int i = 0; i < raceLabelList.Count; i++)
                    {
                        fmwRace.AddMenu(raceLabelList[i], i);
                    }
                    fmwRace.SetAction(RaceListCommand);
                }

                cnt = 0;
                foreach (TraitDef trait in traitList)
                {
                    if(trait.degreeDatas!=null && trait.degreeDatas.First() != null)
                    {
                        fmwTrait.AddMenu(trait.degreeDatas.First().label, cnt++);
                    }
                }
                fmwTrait.SetAction(TraitListCommand);

                cnt = 0;
                foreach (Gender gender in genderList)
                {
                    fmwGender.AddMenu(gender.GetLabel(), cnt++);
                }
                fmwGender.SetAction(GenderListCommand);

            }
        }

        void RaceListCommand(int num)
        {
            Value = raceDefList[num];
        }
        void TraitListCommand(int num)
        {
            Value = traitList[num].defName;
        }

        void TypeListCommand(int num)
        {
            TypeName = typeNameList[num];
        }

        void GenderListCommand(int num)
        {
            Value = genderList[num].ToString();
        }


        public void ShowValueMenu()
        {
            switch (type)
            {
                case RuleType.TabRace:
                    ShowRaceMenu();
                    debug.WriteLine("cr showf race");
                    break;
                case RuleType.Trait:
                    ShowTraitMenu();
                    debug.WriteLine("cr showf trait");
                    break;
                case RuleType.Gender:
                    ShowGenderMenu();
                    debug.WriteLine("cr showf gender");
                    break;
            }
        }
        public void ShowTypeMenu()
        {
            InitializeList();
            InitializeMenu();
            fmwType.Show();
        }

        void ShowRaceMenu()
        {
            if (ModUtil.ExistHAR)
            {
                InitializeList();
                InitializeMenu();
                fmwRace.Show();
            }
        }

        void ShowTraitMenu()
        {
            InitializeList();
            InitializeMenu();
            fmwTrait.Show();
        }
        void ShowGenderMenu()
        {
            InitializeList();
            InitializeMenu();
            fmwGender.Show();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref type, "Type");
            Scribe_Values.Look(ref itemValue, "Value");
            Scribe_Values.Look(ref logicOr, "Or", false);
            Scribe_Values.Look(ref logicNot, "Not", false);
        }

        public string Label
        {
            get
            {
                if (first && !noDef)
                {
                    targetDef = GetDef();
                    if (targetDef == null)
                    {
                        noDef = true;
                    }
                }
                if (targetDef != null)
                {
                    if (type == RuleType.Trait)
                    {
                        return ((TraitDef)targetDef).degreeDatas.First().label;
                    }
                    else
                    {
                        return targetDef.label;
                    }
                }
                return itemValue;
            }
        }
        public string DefName
        {
            get
            {
                return itemValue;
            }
        }
        private Def GetDef()
        {
            first = false;
            switch (type)
            {
                case RuleType.TabRace:
                    if (ModUtil.ExistHAR)
                    {
                        return HARWrapper.GetByName(itemValue);
                    }
                    break;
                case RuleType.Trait:
                    return DefDatabase<TraitDef>.GetNamed(itemValue, false);
                case RuleType.Gender:
                    return GenderWrapperDef.GetGender(itemValue);
            }
            return null;
        }

        public string Value
        {
            get
            {
                return itemValue;
            }
            set
            {
                itemValue = value;
                targetDef = GetDef();
                if (targetDef == null)
                {
                    noDef = true;
                }
                else
                {
                    noDef = false;
                }
            }
        }
        public string TypeName
        {
            get
            {
                return type.ToString();
            }
            set
            {
                if(Enum.TryParse<RuleType>(value, out type))
                {
                    targetDef = GetDef();
                    if (targetDef == null)
                    {
                        noDef = true;
                    }
                    else
                    {
                        noDef = false;
                    }
                }
                else
                {
                    type = 0;
                }
            }
        }
        public bool Evaluate(Pawn pawn, bool prevFlg = true)
        {
            if (pawn == null) return false;
            bool flg = false;
            switch (type)
            {
                case RuleType.TabRace:
                    if (pawn.def.defName == itemValue)
                    {
                        flg = true;
                    }
                    break;
                case RuleType.Trait:
                    if(pawn.story!=null && pawn.story.traits!=null && pawn.story.traits.allTraits != null)
                    {
                        foreach (Trait trait in pawn.story.traits.allTraits)
                        {
                            if (trait.def.defName == itemValue)
                            {
                                flg = true;
                                break;
                            }
                        }
                    }
                    break;
                case RuleType.Gender:
                    flg = GenderWrapperDef.Evaluate(itemValue);
                    break;
            }
            flg = flg ^ logicNot;
            if (logicOr)
            {
                return flg || prevFlg;
            }
            else
            {
                return flg && prevFlg;
            }
        }

        public string LogicString
        {
            get
            {
                if (logicNot)
                {
                    return "Not";
                }
                else
                {
                    if (logicOr)
                    {
                        return "Or";
                    }
                    else
                    {
                        return "And";
                    }
                }
            }
            set
            {
                if (value=="Not")
                {
                    logicNot = true;
                    logicOr = false;
                }
                else
                {
                    logicNot = false;
                    if (value == "Or")
                    {
                        logicOr = true;
                    }
                    else
                    {
                        logicOr = false;
                    }
                }
            }
        }
    }
}
