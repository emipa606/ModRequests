using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using UnityEngine;

namespace zed_0xff.CPS;

[HarmonyPatch]
public static class Patch_GenClamor_lambda {
    public static MethodBase TargetMethod()
    {
        Type[] nestedTypes = typeof(GenClamor).GetNestedTypes(AccessTools.all);

        // try exact match first
        foreach( var t in nestedTypes ){
            foreach( var m in t.GetMethods(AccessTools.all)){
                var param = m.GetParameters();
                if( m.ReturnType == typeof(bool)
                        && param.Length == 2
                        && param[0].ParameterType == typeof(Region)
                        && param[1].ParameterType == typeof(Region)
                        && m.Name == "<DoClamor>b__4_0"
                  ){
                    return m;
                }
            }
        }

        // something changed, try more relaxed match
        foreach( var t in nestedTypes ){
            foreach( var m in t.GetMethods(AccessTools.all)){
                var param = m.GetParameters();
                if( m.ReturnType == typeof(bool)
                        && param.Length == 2
                        && param[0].ParameterType == typeof(Region)
                        && param[1].ParameterType == typeof(Region)
                        && m.Name == "<DoClamor>b__"
                  ){
                    return m;
                }
            }
        }

        // failed to find anything :(
        return null;
    }

    static bool isCabin(Region r){
        return Cache.Get(r.Cells.First(), r.Map) is Building_Cabin c
            && r.CellCount <= c.def.size.x * c.def.size.z;
    }

    // return true if sound can travel from region 'from' to region 'r'
    // XXX not catches all cases, I guess it's due to inlining
    static bool Postfix(bool result, Region from, Region r){
        if( !result ) return result;

        bool c1 = isCabin(from);
        bool c2 = isCabin(r);

        bool res = !(c1 || c2);

//        Log.Warning("[d] " + from.id + " -> " + r.id + " (" + c1 + "," + c2 + ") => " + res);

        return res;
    }
}
