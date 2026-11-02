using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace JobEffects
{
    /// <summary>
    /// Optional feature: draw our translucent forearm on EVERY hand Show Me Your Hands renders
    /// (equipped weapons, carried items, idle/resting hands) — not just our animated work tools.
    ///
    /// SMYH draws each hand with Graphics.DrawMesh(mesh, pos, rot, mat, 0) inside three private
    /// HandDrawer methods. We transpile those methods to route each of those DrawMesh calls through
    /// SmyhHandHook, which forwards the hand draw unchanged and then (only when the setting is on)
    /// draws a forearm at that exact hand position pointing toward the pawn's shoulder. A prefix on
    /// each method stashes the pawn so the hook knows whose shoulder to aim at.
    ///
    /// Always installed when SMYH is loaded (the hook is a no-op for SMYH when the setting is off),
    /// so toggling the setting at runtime needs no re-patch. Fully guarded: if anything fails the
    /// methods stay unpatched and SMYH renders exactly as normal.
    /// </summary>
    public static class SmyhArmHook
    {
        // Set by the per-method prefix for the duration of that SMYH draw call (main render thread).
        private static Pawn currentPawn;

        private static MethodInfo drawMeshMI;
        private static MethodInfo hookMI;

        public static void Apply(Harmony h)
        {
            if (!JobEffectsSettings.SmyhActive) return;
            try
            {
                Type drawer = AccessTools.TypeByName("ShowMeYourHands.HandDrawer");
                if (drawer == null) return;

                drawMeshMI = AccessTools.Method(typeof(Graphics), "DrawMesh",
                    new[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(int) });
                hookMI = AccessTools.Method(typeof(SmyhArmHook), nameof(SmyhHandHook));
                if (drawMeshMI == null || hookMI == null) return;

                var pre = new HarmonyMethod(typeof(SmyhArmHook), nameof(SetPawn));
                var post = new HarmonyMethod(typeof(SmyhArmHook), nameof(ClearPawn));
                var trans = new HarmonyMethod(typeof(SmyhArmHook), nameof(Transpiler));

                Patch(h, drawer, "DrawHandsOnWeapon",
                    new[] { typeof(Thing), typeof(float), typeof(Pawn), typeof(Thing), typeof(bool), typeof(bool) },
                    pre, post, trans);
                Patch(h, drawer, "DrawHandsOnItem", new[] { typeof(Pawn) }, pre, post, trans);
                Patch(h, drawer, "drawHandsAllTheTime", new[] { typeof(Pawn) }, pre, post, trans);
            }
            catch (Exception e)
            {
                Log.Warning("[Show Me Your Tools] forearms-on-hands hook failed to install: " + e.Message);
            }
        }

        private static void Patch(Harmony h, Type t, string name, Type[] args,
            HarmonyMethod pre, HarmonyMethod post, HarmonyMethod trans)
        {
            try
            {
                MethodInfo mi = AccessTools.Method(t, name, args);
                if (mi == null) { Log.Warning($"[Show Me Your Tools] SMYH method {name} not found; weapon forearms skipped for it."); return; }
                h.Patch(mi, prefix: pre, postfix: post, transpiler: trans);
            }
            catch (Exception e)
            {
                Log.Warning($"[Show Me Your Tools] could not patch SMYH {name}: {e.Message}");
            }
        }

        // Harmony injects the target's `pawn` argument by name (all three methods name it `pawn`).
        public static void SetPawn(Pawn pawn) { currentPawn = pawn; }
        public static void ClearPawn() { currentPawn = null; }

        // Replace every `Graphics.DrawMesh(Mesh,Vector3,Quaternion,Material,int)` call in the SMYH
        // method with a call to SmyhHandHook (identical signature → trivial operand swap).
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction ci in instructions)
            {
                if ((ci.opcode == OpCodes.Call || ci.opcode == OpCodes.Callvirt)
                    && ci.operand is MethodInfo mi && mi == drawMeshMI)
                {
                    yield return new CodeInstruction(OpCodes.Call, hookMI);
                }
                else
                {
                    yield return ci;
                }
            }
        }

        // Stand-in for SMYH's hand DrawMesh: draw the hand exactly as SMYH would, then add a forearm.
        public static void SmyhHandHook(Mesh mesh, Vector3 pos, Quaternion rot, Material mat, int layer)
        {
            Graphics.DrawMesh(mesh, pos, rot, mat, layer);   // the hand, unchanged
            Pawn p = currentPawn;
            if (p != null && JobEffectsSettings.ForearmsOnHands)
            {
                try { ArmRenderer.DrawForearmAtHand(p, pos); } catch { }
            }
        }
    }
}
