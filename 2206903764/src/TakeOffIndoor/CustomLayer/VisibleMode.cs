using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using System.Reflection;
using System.Reflection.Emit;
using AlienRace;
using Garam_RaceAddon;
using System.Text.RegularExpressions;
using System.Diagnostics;
namespace TakeOffIndoor
{
    public enum VisibleMode
    {
        Never = 11,  //徴兵中:非表示　平時:非表示
        InvisibleNone = 10,  //徴兵中:非表示　平時:通常
        InvisibleForce = 12,  //徴兵中:非表示　平時:強制表示
        Invisible = 01,  //徴兵中:通常　平時:非表示
        None = 00,  //徴兵中:通常　平時:通常
        Visible = 02,  //徴兵中:通常　平時:強制表示
        ForceInvisible = 21,  //徴兵中:強制表示　平時:非表示
        ForceNone = 20,  //徴兵中:強制表示　平時:通常
        Force = 22  //徴兵中:強制表示　平時:強制表示
    }

    public enum VisibleModeSingle
    {
        None = 0,
        Invisible = 1,
        Force = 2,
    }

    public static class VMExt
    {
        public static VisibleModeSingle GetDrafted(this VisibleMode vm)
        {
            return (VisibleModeSingle)((int)vm / 10);
        }
        public static VisibleModeSingle GetNormal(this VisibleMode vm)
        {
            return (VisibleModeSingle)((int)vm % 10);
        }

        public static VisibleMode Change(this VisibleMode vm, VisibleModeSingle vms,bool drafted)
        {
            if (drafted)
            {
                return vm.ChangeDrafted(vms);
            }
            else
            {
                return vm.ChangeNotDrafted(vms);
            }
        }
        public static VisibleMode ChangeDrafted(this VisibleMode vm, VisibleModeSingle vms)
        {
            int tmp = (int)vm;
            tmp = 10 * (int)vms + (tmp % 10);
            return (VisibleMode)tmp;
        }
        public static VisibleMode ChangeNotDrafted(this VisibleMode vm, VisibleModeSingle vms)
        {
            int tmp = (int)vm;
            tmp = tmp / 10 * 10 + (int)vms;
            return (VisibleMode)tmp;
        }

        public static string GetLabel(this VisibleModeSingle vms)
        {
            switch (vms)
            {
                case VisibleModeSingle.Force:
                    return "TOC.Force".TranslateW("Force");
                case VisibleModeSingle.Invisible:
                    return "CommandHideZoneLabel".Translate();
                case VisibleModeSingle.None:
                    return "TOC.None".TranslateW("Normal");
            }
            return "";
        }

        public static string GetLabel(this VisibleMode vm)
        {
            string ret = "";
            ret += "TOC.Drafted".TranslateW("Drafted") + ":" + vm.GetDrafted().GetLabel();
            ret += " " + "TOC.NotDrafted".TranslateW("Not drafted") + ":" + vm.GetNormal().GetLabel();
            return ret;
        }
        public static string GetLabel(this VisibleMode vm,bool drafted)
        {
            string ret = "";
            if (drafted)
            {
                ret += "TOC.Drafted".TranslateW("Drafted") + ":" + vm.GetDrafted().GetLabel();
            }
            else
            {
                ret += " " + "TOC.NotDrafted".TranslateW("Not drafted") + ":" + vm.GetNormal().GetLabel();
            }
            return ret;
        }


        public static bool IsForce(this VisibleMode vm,bool drafted)
        {
            if (drafted)
            {
                if ((int)vm / 10 == 2)
                {
                    return true;
                }
            }
            else
            {
                if ((int)vm % 10 == 2)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool IsInvisible(this VisibleMode vm, bool drafted)
        {
            if (drafted)
            {
                if ((int)vm / 10 == 1)
                {
                    return true;
                }
            }
            else
            {
                if ((int)vm % 10 == 1)
                {
                    return true;
                }
            }
            return false;
        }
    }
}