using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.AI;
namespace Clutter_Furniture
{
    [StaticConstructorOnStartup]
    class Lamps : Building
    {
        public ClutterThingDefsFurniture OffTexturePath;
        private static Graphic OffTexture;
        private CompGlower glowerComp;
        private CompPowerTrader powerComp;
        private bool YellowLight = false;
        private bool RedLight = false;
        private bool GreenLight = false;
        private bool BlueLight = false;
        private bool OrangeLight = false;
        private bool VoiletLight = false;
        private int LampNumber = 0;
        private Pawn pawn = null;
        private Job jobPawn = null;
        private Thing thing;


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            LongEventHandler.ExecuteWhenFinished(GetGraphics);
            powerComp = GetComp<CompPowerTrader>();
            glowerComp = GetComp<CompGlower>();
            LampDetect();


        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        private void GetGraphics()
        {
            OffTexturePath = (def as ClutterThingDefsFurniture);

            OffTexture = GraphicDatabase.Get<Graphic_Single>(OffTexturePath.LampOffTexture);
        }

        public override Graphic Graphic
        {
            get
            {
                                
                if (!powerComp.PowerOn)
                {
                    return OffTexture;
                }
                else
                {
                    return def.graphic;
                }

            }
        }

        private void LampDetect()
        {
            if (def.defName == "ClutterLampYellow")
            {
                YellowLight = true;
            }
            else if (def.defName == "ClutterLampBlue")
            {
                BlueLight = true;
            }
            else if (def.defName == "ClutterLampRed")
            {
                RedLight = true;
            }
            else if (def.defName == "ClutterLampGreen")
            {
                GreenLight = true;
            }
            else if (def.defName == "ClutterLampOrange")
            {
                OrangeLight = true;
            }
            else if (def.defName == "ClutterLampVoilet")
            {
                VoiletLight = true;
            }

        }

        public override void Tick()
        {
            if (LampNumber != 0)
            {
                base.Tick();


                PawnDetector();
                if (pawn.CurJob != jobPawn)
                {
                    LampNumber = 0;
                }



            }

        }

        private void PawnDetector()
        {
            if (pawn.Position.InHorDistOf(Position, 1.5f))

            //IEnumerable<Pawn> enumerable =
            //    from p in Find.ListerPawns.AllPawns
            //    where p == pawn && p.Position.WithinHorizontalDistanceOf(base.Position, 2.5f)
            //    select p;
            //foreach (Pawn current in enumerable)
            {

                if (LampNumber == 1)
                {
                    thing = ThingMaker.MakeThing(ThingDef.Named("ClutterLampYellow"));
                }
                else if (LampNumber == 2)
                {
                    thing = ThingMaker.MakeThing(ThingDef.Named("ClutterLampBlue"));
                }
                else if (LampNumber == 3)
                {
                    thing = ThingMaker.MakeThing(ThingDef.Named("ClutterLampRed"));
                }
                else if (LampNumber == 4)
                {
                    thing = ThingMaker.MakeThing(ThingDef.Named("ClutterLampGreen"));
                }
                else if (LampNumber == 5)
                {
                    thing = ThingMaker.MakeThing(ThingDef.Named("ClutterLampOrange"));
                }
                else if (LampNumber == 6)
                {
                    thing = ThingMaker.MakeThing(ThingDef.Named("ClutterLampVoilet"));
                }
                Map mapzie = this.Map;
                pawn.jobs.StopAll();
                LampNumber = 0;
                thing.SetFactionDirect(Faction);
                Destroy();
                GenSpawn.Spawn(thing, Position, mapzie, Rotation);
            }

        }



        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {



            List<FloatMenuOption> list = new List<FloatMenuOption>();

            {
                if (!myPawn.CanReserve(this))
                {

                    FloatMenuOption item = new FloatMenuOption("CannotUseReserved".Translate(), null);
                    return new List<FloatMenuOption>
			    	{
			    		item
			    	};
                }
                if (!myPawn.CanReach(this, PathEndMode.Touch, Danger.Some))
                {
                    FloatMenuOption item2 = new FloatMenuOption("CannotUseNoPath".Translate(), null);
                    return new List<FloatMenuOption>
			    	{
			    		item2
			    	};

                }
                if (this.Map.GameConditionManager.ConditionIsActive(GameConditionDef.Named("SolarFlare")))
                {
                    FloatMenuOption item3 = new FloatMenuOption("CannotUseSolarFlare".Translate(), null);
                    return new List<FloatMenuOption>
				    {
				    	item3
				    };
                }
                if (!powerComp.PowerOn)
                {
                    FloatMenuOption item4 = new FloatMenuOption("CannotUseNoPower".Translate(), null);
                    return new List<FloatMenuOption>
				    {
				    	item4
			    	};
                }
                if (!ResearchProjectDef.Named("ColoredLights").IsFinished)
                {
                    FloatMenuOption item5 = new FloatMenuOption("ClutterRequireResearch".Translate(), null);
                    return new List<FloatMenuOption>
				    {
				    	item5
			    	};
                }



                if (!YellowLight)
                {                  
                    Action action1 = delegate
                    {


                        Job job = new Job(JobDefOf.Goto, this);
                        myPawn.jobs.TryTakeOrderedJob(job);
                        LampNumber = 1;
                        pawn = myPawn; ;
                        jobPawn = job;



                    };
                    list.Add(new FloatMenuOption("ClutterLampYellow".Translate(), action1));
                }

                if (!BlueLight)
                {
                    Action action2 = delegate
                    {


                        Job job = new Job(JobDefOf.Goto, this);
                        myPawn.jobs.TryTakeOrderedJob(job);
                        LampNumber = 2;
                        pawn = myPawn;
                        jobPawn = job;


                    };
                    list.Add(new FloatMenuOption("ClutterLampBlue".Translate(), action2));
                }

                if (!RedLight)
                {
                    Action action3 = delegate
                    {


                        Job job = new Job(JobDefOf.Goto, this);
                        myPawn.jobs.TryTakeOrderedJob(job);
                        LampNumber = 3;
                        pawn = myPawn;
                        jobPawn = job;


                    };
                    list.Add(new FloatMenuOption("ClutterLampRed".Translate(), action3));
                }

                if (!GreenLight)
                {
                    Action action4 = delegate
                    {


                        Job job = new Job(JobDefOf.Goto, this);
                        myPawn.jobs.TryTakeOrderedJob(job);
                        LampNumber = 4;
                        pawn = myPawn;
                        jobPawn = job;


                    };
                    list.Add(new FloatMenuOption("ClutterLampGreen".Translate(), action4));
                }

                if (!OrangeLight)
                {
                    Action action5 = delegate
                    {


                        Job job = new Job(JobDefOf.Goto, this);
                        myPawn.jobs.TryTakeOrderedJob(job);
                        LampNumber = 5;
                        pawn = myPawn;
                        jobPawn = job;


                    };
                    list.Add(new FloatMenuOption("ClutterLampOrange".Translate(), action5));
                }
                if (!VoiletLight)
                {
                    Action action6 = delegate
                    {


                        Job job = new Job(JobDefOf.Goto, this);
                        myPawn.jobs.TryTakeOrderedJob(job);
                        LampNumber = 6;
                        pawn = myPawn;
                        jobPawn = job;


                    };
                    list.Add(new FloatMenuOption("ClutterLampVoilet".Translate(), action6));
                }
                //TEST--------------------------------------------------------
                if (Debug.developerConsoleVisible)
                {
                    Action action7 = delegate
                    {
                        CCLTEst();


                    };
                    list.Add(new FloatMenuOption("TestMode", action7));
                }

            }
            return list;
        }


        private void CCLTEst()
        {
        //    CompProperties_Glower compColor = GetComp<CompGlower>().Props;
        //    compColor.glowColor.r = 164;
        //    compColor.glowColor.r = 255;
        //    compColor.glowColor.r = 0;
        //    powerComp.PowerOn = false;
        //    glowerComp.UpdateLit();
        //    powerComp.PowerOn = true;
        //    glowerComp.UpdateLit();
        }
    }
}
