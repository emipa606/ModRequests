using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
namespace Clutter_Furniture
{
    internal class ReadableBooks : ThingWithComps
    {
        public Pawn currentReader = null;
        private List<string> BookText = new List<string>();
        private int LifeSpam = 1500;
        public bool TexChange = false;
        private Graphic OpenBook;
        private List<string> DefaultText = new List<string>
		{
			"It was a dark and stormy night.",
			"Suddenly, a shot rang out!",
			"A door slammed.",
			"The maid screamed.",
			"Suddenly, a pirate ship appeared on the horizon!",
			"While millions of people were starving, the king lived in luxury.",
			"Meanwhile, on a small farm in Kansas, a boy was growing up.",
			"A light snow was falling....",
			"and the little girl with the tattered shawl had not sold a violet all day.",
			"At that very moment...",
			"a young intern at City Hospital was making an important discovery.",
			"The mysterious patient in Room 213 had finally awakened",
			"She moaned softly",
			"Could it be that she was the sister of the boy in Kansas...",
			"who loved the girl with the tattered shawl",
			"who was the daughter of the maid who had escaped from the pirates?",
			"The intern frowned."
		};
        public override Graphic Graphic
        {
            get
            {
                ReadFormXML();
                Graphic result;
                if (!TexChange && OpenBook != null)
                {
                    result = OpenBook;
                }
                else
                {
                    result = base.Graphic;
                }
                return result;
            }
        }
        public List<string> PrepareText()
        {
            ClutterThingDefsFurniture clutterThingDefs = (ClutterThingDefsFurniture)def;
            BookText = clutterThingDefs.BookText;
            List<string> result;
            if (BookText.Count > 0)
            {
                result = TextChooping(BookText);
            }
            else
            {
                result = TextChooping(DefaultText);
            }
            return result;
        }
        public override void Tick()
        {
            base.Tick();
            if (currentReader == null)
            {
                if (this.Map.reservationManager.IsReserved(this,factionInt))
                {
                    List<Pawn> bookWorms = new List<Pawn>();
                    bookWorms = this.Map.mapPawns.FreeHumanlikesOfFaction(factionInt).ToList();
                    foreach( Pawn BookWorm in bookWorms)
                    {
                        if(this.Map.reservationManager.ReservedBy(this,BookWorm))
                        {
                            if (this.Map.reservationManager.IsReservedByAnyoneWhoseReservationsRespects(this, BookWorm))
                            {
                                currentReader = BookWorm;
                                break;
                            }
                        }
                    }
                                      
                }
                if (LifeSpam <= 0)
                {
                    FeedBackPulse();
                }
                LifeSpam--;
            }
            else
            {
                if (currentReader.CurJob.def.defName != "ClutterSitAndRead")
                {
                    FeedBackPulse();
                }
            }
        }
        public void FeedBackPulse()
        {
            foreach (Thing current in this.Map.listerThings.AllThings)
            {
                if (current.def.defName == "ClutterBookShelf")
                {
                    Bookshelf bookshelf = current as Bookshelf;
                    if (bookshelf.MissingBooksList.Contains(def))
                    {
                        if (bookshelf.StoredBooks.Count < 3)
                        {
                            bookshelf.MissingBooksList.Remove(def);
                            bookshelf.StoredBooks.Add(def);
                            if (Spawned)
                            {
                                Destroy();
                            }
                            break;
                        }
                    }
                }
            }
            if (Spawned)
            {
                Destroy();
            }
        }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            LongEventHandler.ExecuteWhenFinished(SS2);
          
        }

        public void SS2()
        {
            ReadFormXML();
            if (OpenBook == null)
            {
                OpenBook = GraphicDatabase.Get<Graphic_Single>("Clutter/Books/CBook_Blue");
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<string>(ref BookText, "BookText", LookMode.Undefined, null);
            Scribe_References.Look<Pawn>(ref currentReader, "currentReader");
        }
        private List<string> TextChooping(List<string> textlist)
        {
            List<string> list = new List<string>();
            int num = Rand.RangeInclusive(0, textlist.Count - 7);
            int num2 = 0;
            for (int i = num; i < textlist.Count; i++)
            {
                list.Add(textlist[i]);
                num2++;
                if (num2 >= 7)
                {
                    break;
                }
            }
            return list;
        }
        private void ReadFormXML()
        {
            List<string> list = new List<string>();
            ClutterThingDefsFurniture clutterThingDefs = (ClutterThingDefsFurniture)def;
            if (clutterThingDefs.BookText.Count > 0)
            {
                list = clutterThingDefs.BookText;
            }
            if (!clutterThingDefs.CloseTexture.NullOrEmpty())
            {
                OpenBook = GraphicDatabase.Get<Graphic_Single>(clutterThingDefs.CloseTexture);
            }
        }
        

        public void Thoughts(Pawn reader)
        {
            int num = UnityEngine.Random.Range(1, 100);
            string goodMemory = "ReadBookGood";
            string badMemory = "ReadBookBad";
            string mindBomb=null;
            if(num<10)
            {
                mindBomb = badMemory;
                if(DebugSettings.godMode)
                {
                    Log.Message("Bad Event:" + reader.Name.ToStringShort);
                }
            }
            else if(num>70)
            {
                mindBomb = goodMemory;
                if (DebugSettings.godMode)
                {
                    Log.Message("Good Event:" + reader.Name.ToStringShort);
                }
            }
            if (!mindBomb.NullOrEmpty())
            {
                Thought Brainwashing = ThoughtMaker.MakeThought(ThoughtDef.Named(mindBomb));
                List<Thought> thinkTank = new List<Thought>();
                reader.needs.mood.thoughts.GetDistinctMoodThoughtGroups(thinkTank);
                foreach(Thought current in thinkTank)
                {
                    if (Brainwashing.def.stackLimit>1)
                    {
                        reader.needs.mood.thoughts.memories.TryGainMemory(Brainwashing.def);
                    }
                    else if(!thinkTank.Contains(Brainwashing))
                    {                                            
                        reader.needs.mood.thoughts.memories.TryGainMemory(Brainwashing.def);
                       
                    }
                }
    
            }
                           
            FeedBackPulse();
        }
    }
}
