using RimTrust.Trade.Ext;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimTrust.Core.Interactive
{
    public static class FloatMenuManager
    {
        internal static Dictionary<string, Action<Pawn>> rawItems;

        internal static Dictionary<string, bool> usesDefaultJobDriver;

        internal static Action<Pawn> currentAction;

        internal static Dictionary<string, Action> shiftKeyItems;

        static FloatMenuManager()
        {
            rawItems = new Dictionary<string, Action<Pawn>>();
            usesDefaultJobDriver = new Dictionary<string, bool>();
            currentAction = null;
            shiftKeyItems = new Dictionary<string, Action>();
            Add("FloatMenuCaptionExchange".Translate(), delegate (Pawn p)
            {
                ExtUtil.PrepareVirtualTrade(p, new Trader_BankNoteExchange());
            }, usesDefaultJobDriver: true);
            AddShiftKeyItem("FloatMenuCaptionRemoveAll".Translate(), delegate
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("DlgRemoveModContents".Translate(), delegate
                {
                    int num = 0;
                    num += ModContentRemover.RemoveAllModContentsFromWorldObjects();
                    num += ModContentRemover.RemoveAllModContentsFromPassingShips();
                    num += ModContentRemover.RemoveAllModContentsFromMaps();
                    num += ModContentRemover.RemoveAllModContentsFromAllPawns();
                    Messages.Message("MsgModContentsRemoved".Translate(num), MessageTypeDefOf.PositiveEvent);
                }, destructive: true, title: "DlgTitleRemoveModContents".Translate()));
            });
        }

        public static void Add(string str, Action<Pawn> action, bool usesDefaultJobDriver = false)
        {
            rawItems.Add(str, action);
            FloatMenuManager.usesDefaultJobDriver.Add(str, usesDefaultJobDriver);
        }

        public static void AddShiftKeyItem(string str, Action action)
        {
            shiftKeyItems.Add(str, action);
        }

        public static void Remove(string name)
        {
            rawItems.Remove(name);
            usesDefaultJobDriver.Remove(name);
            shiftKeyItems.Remove(name);
        }

        public static IEnumerable<FloatMenuOption> RequestBuild(Building target, Pawn pawn)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (KeyValuePair<string, Action<Pawn>> pair in rawItems)
            {
                list.Add(new FloatMenuOption(pair.Key, delegate
                {
                    if (usesDefaultJobDriver[pair.Key])
                    {
                        currentAction = pair.Value;
                        Job job = new Job(CoreDefOf.UseBankTerminal, target);
                        pawn.jobs.TryTakeOrderedJob(job);
                    }
                    else
                    {
                        pair.Value(pawn);
                    }
                }));
            }
             if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                foreach (KeyValuePair<string, Action> shiftKeyItem in shiftKeyItems)
                {
                    list.Add(new FloatMenuOption(shiftKeyItem.Key, shiftKeyItem.Value));
                }
                return list;
            } 
            return list;
        }
    }
}