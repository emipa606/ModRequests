using RimTrust.Trade;
using RimTrust.Trade.Ext;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimTrust.Core.Interactive
{
    
    public static class FloatMenuManagerNeuralChair
    {

        internal static Dictionary<string, Action<Pawn>> rawItems;

        internal static Dictionary<string, bool> usesDefaultJobDriver;

        internal static Action<Pawn> currentAction;

        internal static Dictionary<string, Action> shiftKeyItems;

        public static HediffDef NeuralOverdose;
        

        static FloatMenuManagerNeuralChair()
        {
            rawItems = new Dictionary<string, Action<Pawn>>();
            usesDefaultJobDriver = new Dictionary<string, bool>();
            currentAction = null;
            shiftKeyItems = new Dictionary<string, Action>();
            Add("FloatMenuCaptionLegacySkills".Translate(), delegate (Pawn p)
            {
                string msg = "";
                int index = 0;
                List<string> initSkillRecord = new List<string>() { "Shooting", "Melee", "Construction", "Mining", "Cooking", "Plants", "Animals", "Crafting", "Artistic", "Medical", "Social", "Intellectual" };
                foreach (string item in initSkillRecord)
                    {
                    msg += item + ": " + Methods.LegacySkills[index] + "\n";
                    index++;
                    }
                msg += "\n\n" + "Summe aus Methods.LegacySkills:  " + Methods.LegacySkills.Sum() + " (" + Math.Round(((double)Methods.LegacySkills.Sum() / (double)3564000) * 100, 4) + "%)";
                DiaNode diaNode = new DiaNode(msg);
                DiaOption diaOption = new DiaOption("GameOverKeepPlaying".Translate());
                diaOption.resolveTree = true;
                diaNode.options.Add(diaOption);
                Dialog_NodeTree dialog_NodeTree = new Dialog_NodeTree(diaNode, true, false, null);
                Find.WindowStack.Add(dialog_NodeTree);
            }, usesDefaultJobDriver: false);
            Add("FloatMenuCaptionNeuralChair".Translate(), delegate (Pawn p)
            {
                string selectedSkill = Methods.SelectSkillToSafeNIC(p);
                SkillRecord selectedSkillRecord = Methods.SelectSkillRecordToSafeNIC(p);
                int selectedSkillXP = Methods.SkillXPTotal(selectedSkillRecord);

                List<string> initSkillRecord = new List<string>() { "Shooting", "Melee", "Construction", "Mining", "Cooking", "Plants", "Animals", "Crafting", "Artistic", "Medical", "Social", "Intellectual" };
                int index = initSkillRecord.FindIndex(x => x == selectedSkill);
                Methods.UpdateLegacySkills(selectedSkill, p, 1);
                Methods.SaveLegacySkills();
                Methods.NicOverdoseToPawn(p);
            }, usesDefaultJobDriver: true);
        }

        public static void Add(string str, Action<Pawn> action, bool usesDefaultJobDriver = false)
        {
            rawItems.Add(str, action);
            FloatMenuManagerNeuralChair.usesDefaultJobDriver.Add(str, usesDefaultJobDriver);
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
                        Job job = new Job(CoreDefOf.UseNeuralChair, target);
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