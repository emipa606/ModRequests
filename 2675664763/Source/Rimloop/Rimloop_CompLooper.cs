using System;
using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace RimloopMod
{
    //good luck
    public class CompLooper : ThingComp
    {
        bool looping = false;
        int ticksInLoop;
        int ticksElapsed;
        int loopSeedOffset;

        public CompProperties_Looper Props => (CompProperties_Looper)this.props;

        #region Saving
        //vanilla SaveGame function won't do because it doesn't properly accept a full path
        private string SaveGameToFolder(string gameName, string prefix)
        {
            //create rimloop directory if doesn't already exist
            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(GenFilePaths.SaveDataFolderPath, "Saves", "Rimloop_Saves"));

            string fileName = GenFilePaths.FilePathForSavedGame(Path.Combine("Rimloop_Saves", prefix + gameName));
            if (!directoryInfo.Exists) { directoryInfo.Create(); }

            try
            {
                Log.Message("Rimloop: Saving loop file to " + fileName);
                SafeSaver.Save(fileName, "savegame", delegate
                {
                    ScribeMetaHeaderUtility.WriteMetaHeader();
                    Game game = Current.Game;
                    Scribe_Deep.Look<Game>(ref game, "game", Array.Empty<object>());
                }, false);

            }
            catch (Exception arg)
            {
                Log.Error("Exception while saving Rimloop save file: " + arg);
            }
            return fileName;
        }
        private void SpliceXML(string SOLFile, string EOLFile)
        {
            try
            {
                XmlDocument SOLDoc = new XmlDocument();
                XmlDocument EOLDoc = new XmlDocument();
                SOLDoc.Load(GenFilePaths.FilePathForSavedGame(SOLFile));
                EOLDoc.Load(EOLFile);
                XmlNode SOLGame = SOLDoc.SelectSingleNode("savegame/game");
                XmlNode EOLGame = SOLDoc.ImportNode(EOLDoc.SelectSingleNode("savegame/game"), true);

                //do once on the save
                foreach (string path in Props.spliceXPath)
                {
                    if (EOLGame.SelectSingleNode(path) != null)
                    {
                        XmlNode EOLElement = EOLGame.SelectSingleNode(path);
                        XmlNode SOLElement = SOLGame.SelectSingleNode(path);
                        SOLGame.ReplaceChild(EOLElement, SOLElement);
                    }
                    else { Log.Warning("Rimloop: Tried to splice XML with path " + path + " but it did not exist! Skipping."); }
                }

                //select all pawns with awareness
                XmlNodeList EOLAwareness = EOLGame.SelectNodes("//def[.='Rimloop_LoopAwareness']/../../../../..");
                XmlNodeList SOLAwareness = SOLGame.SelectNodes("//def[.='Rimloop_LoopAwareness']/../../../../..");

                //put them in a dictionary based on ids to make sure we're splicing the right pawn
                Dictionary<int, XmlNode> EOLAwarenessDict = new Dictionary<int, XmlNode>();
                Dictionary<int, XmlNode> SOLAwarenessDict = new Dictionary<int, XmlNode>();
                foreach (XmlNode node in EOLAwareness)
                {
                    EOLAwarenessDict.Add(node.SelectSingleNode("id").InnerText.GetHashCode(), node);
                }
                foreach (XmlNode node in SOLAwareness)
                {
                    SOLAwarenessDict.Add(node.SelectSingleNode("id").InnerText.GetHashCode(), node);
                }

                //xml node for death thought, need to assign null to prevent compiler error
                XmlNode deathNode = null;
                if (Props.giveDiedThought)
                {
                    deathNode = SOLDoc.CreateNode(XmlNodeType.Element, "li", "");
                    deathNode.AppendChild(SOLDoc.CreateNode(XmlNodeType.Element, "def", "")).InnerText = Props.diedThought.defName;
                    deathNode.AppendChild(SOLDoc.CreateNode(XmlNodeType.Element, "otherPawn", "")).InnerText = "null";
                    deathNode.AppendChild(SOLDoc.CreateNode(XmlNodeType.Element, "age", "")).InnerText = "0";
                }

                //do per pawn with awareness
                foreach (int pawn in SOLAwarenessDict.Keys)
                {
                    if (EOLAwarenessDict.ContainsKey(pawn) && SOLAwarenessDict.ContainsKey(pawn))
                    {
                        //check if pawn is dead in EOL but not in SOL, then give died thought
                        if (Props.giveDiedThought && EOLAwarenessDict[pawn].SelectSingleNode("healthTracker/healthState[.='Dead']") != null && SOLAwarenessDict[pawn].SelectSingleNode("healthTracker/healthState[.='Dead']") == null)
                        {
                            SOLAwarenessDict[pawn].SelectSingleNode("needs/needs/li[@Class='Need_Mood']/thoughts/memories/memories").AppendChild(deathNode);
                        }
                        //do pawn paths
                        foreach (string path in Props.spliceXPathPerPawn)
                        {
                            if (EOLAwarenessDict[pawn].SelectSingleNode(path) != null && SOLAwarenessDict[pawn].SelectSingleNode(path) != null)
                            {
                                SOLAwarenessDict[pawn].SelectSingleNode(path + "/..").ReplaceChild(EOLAwarenessDict[pawn].SelectSingleNode(path), SOLAwarenessDict[pawn].SelectSingleNode(path));
                            }
                            else { Log.Warning("Rimloop: Tried to splice XML with path " + path + " on pawn " + pawn + " but it did not exist! Skipping."); }
                        }
                    }
                    else { Log.Warning("Rimloop: Tried to splice XML for pawn " + pawn + " but they did not exist in both EOL and SOL files! This was most likely a result of their brain being destroyed or a new person joining with awareness. Skipping."); }
                }

                SOLDoc.Save(GenFilePaths.FilePathForSavedGame(SOLFile));
            }
            catch (Exception arg)
            {
                Log.Error("Exception while splicing Rimloop save file: " + arg);
            }
        }
        #endregion

        #region Loop Controls
        private void ResetLoop()
        {
            string gameName = Faction.OfPlayer.Name;
            ScreenFader.SetColor(Props.loopColor);
            HandleImplant();
            string EOLFile = SaveGameToFolder(gameName, "EndOfLoop_");
            string SOLFile = Path.Combine("Rimloop_Saves", "LoopStart_" + gameName);
            SpliceXML(SOLFile, EOLFile);
            GameDataSaveLoader.LoadGame(SOLFile);
        }

        private void EndLoop()
        {
            looping = false;
            ticksElapsed = 0;
            HandleImplant();
            Log.Message("Rimloop: Loop ended.");
        }

        private void StartLoop()
        {
            loopSeedOffset = DateTime.Now.GetHashCode();
            looping = true;
            HandleImplant();
            SetLoopTime(ticksInLoop);
            SaveGameToFolder(Faction.OfPlayer.Name, "LoopStart_");
            Log.Message("Rimloop: Loop started.");
        }
        #endregion

        private void HandleImplant()
        {
            //give ALL pawns the awareness hediff if they have the implant, otherwise remove awarness if they have it
            foreach (Pawn pawn in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead.Concat(PawnsFinder.AllCaravansAndTravelingTransportPods_AliveOrDead))
            {
                if (pawn.health.hediffSet.HasHediff(Props.awarenessImplant))
                {
                    pawn.health.AddHediff(Props.awarenessHediff, pawn.health.hediffSet.GetBrain());
                }
                else if (pawn.health.hediffSet.HasHediff(Props.awarenessHediff))
                {
                    pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(Props.awarenessHediff));
                }
            }
        }

        private void SetLoopTime(int time)
        {
            if (time - this.ticksElapsed <= 0) { return; }
            this.ticksInLoop = Mathf.Clamp(time, 60000, int.MaxValue);
            this.parent.GetComp<CompPowerTrader>().PowerOutput = -(Mathf.Pow(2.5f, this.ticksInLoop / 60000) * 50000);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.ticksInLoop, "ticksInLoop", 0, false);
            Scribe_Values.Look<int>(ref this.ticksElapsed, "ticksElapsed", 0, true);
            Scribe_Values.Look<bool>(ref this.looping, "looping", false, true);
            Scribe_Values.Look<int>(ref this.loopSeedOffset, "loopSeedOffset", 0, false);
        }

        #region Comp
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (ticksInLoop == 0) { SetLoopTime(Props.defaultLoopTime); }
            else { SetLoopTime(ticksInLoop); }
        }

        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);
            if (signal == "PowerTurnedOn") { StartLoop(); }
            else if (signal == "PowerTurnedOff") { EndLoop(); }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (looping)
            {
                if (this.ticksInLoop - this.ticksElapsed <= 0 || !this.parent.Spawned)
                {
                    return;
                }
                this.ticksElapsed++;
                //Rand.Seed = Find.TickManager.TicksAbs + loopSeedOffset;
                if (this.ticksInLoop - this.ticksElapsed == 100) { ScreenFader.StartFade(Props.loopColor, 0.5f); }
                else if (this.ticksInLoop - this.ticksElapsed <= 0) { ResetLoop(); }
            }
        }

        public override string CompInspectStringExtra()
        {
            if (!this.parent.Spawned) { return null; }
            return "Time to full charge: " + (this.ticksInLoop - this.ticksElapsed).ToStringTicksToPeriod();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            Command_Action command = new Command_Action
            {
                action = delegate ()
                {
                    this.SetLoopTime(ticksInLoop - 60000);
                },
                defaultLabel = "-1 day",
                defaultDesc = "Lower loop interval by 1 day.",
                hotKey = KeyBindingDefOf.Misc4,
                icon = ContentFinder<Texture2D>.Get("UI/Commands/TempLower", true)
            };
            if (looping) { command.Disable("Time loop interval cannot be changed while device is active."); }
            yield return command;

            command = new Command_Action
            {
                action = delegate ()
                {
                    this.SetLoopTime(ticksInLoop + 60000);
                },
                defaultLabel = "+1 day",
                defaultDesc = "Raise loop interval by 1 day.",
                hotKey = KeyBindingDefOf.Misc2,
                icon = ContentFinder<Texture2D>.Get("UI/Commands/TempRaise", true)
            };
            if (looping) { command.Disable("Time loop interval cannot be changed while device is active."); }
            yield return command;
        }

        #endregion
    }

    public class CompProperties_Looper : CompProperties
    {
        public CompProperties_Looper()
        {
            this.compClass = typeof(CompLooper);
        }

        public int defaultLoopTime;
        public Color loopColor;
        public HediffDef awarenessImplant;
        public HediffDef awarenessHediff;
        public bool giveDiedThought;
        public ThoughtDef diedThought;
        public List<string> spliceXPath;
        public List<string> spliceXPathPerPawn;
    }
}
