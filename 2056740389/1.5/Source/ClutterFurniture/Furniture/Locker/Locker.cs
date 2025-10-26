using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;


namespace Clutter_Furniture
{
    [StaticConstructorOnStartup]
    class ClothLocker : Building
    {
        private static Texture2D UI_Button = ContentFinder<Texture2D>.Get("Clutter/Ui/Force_ChangeGear");
        private int counter = 0;
        private Pawn OwnerPawn;
        private int OwnerPawnID = 0;
        private Job LockerJob;
        private int TxtCounter = 0;
        private int txtpos = 0;
        public bool Interaction = false;
        private int num = 0;
        private int WornCount = 0;
        public bool GunChange = false;
        private List<Apparel> NewClothSetList = new List<Apparel>();
        public List<Apparel> StoredClothSetList = new List<Apparel>();
        public Thing StoredGun;
        public string OwnerName()
        {
            string name;
            if (OwnerPawn != null)
            {
                name = OwnerPawn.Name.ToStringShort;
            }
            else
            {
                name = "ClutterOwnedBy".Translate();
            }
            return name;
        }

        public List<string> EmptyTxt = new List<string>
		{
            "ClutterLockerEmptyTxt1".Translate(),
            "ClutterLockerEmptyTxt2".Translate(),
            "ClutterLockerEmptyTxt3".Translate(),
            "ClutterLockerEmptyTxt4".Translate()
            			
		};

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            LockerJob = new Job(DefDatabase<JobDef>.GetNamed("ClutterLockerBasic"), this, InteractionCell);
            if (StoredGun != null)
            {
                Log.Message(StoredGun.LabelCap);
            }
            LongEventHandler.ExecuteWhenFinished(randomeThingy);
            
        }

        public void randomeThingy()
        {
            txtpos = UnityEngine.Random.Range(1, EmptyTxt.Count);
        }

       
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref counter, "counter");
            Scribe_Values.Look<int>(ref txtpos, "txtpos");
            Scribe_Values.Look<int>(ref TxtCounter, "TxtCounter");
            Scribe_Values.Look<int>(ref num, "num");
            Scribe_Values.Look<int>(ref OwnerPawnID, "OwnerPawnID");
            Scribe_Collections.Look<Apparel>(ref NewClothSetList, "NewClothSetList", LookMode.Deep, null);
            Scribe_Collections.Look<Apparel>(ref StoredClothSetList, "StoredClothSetList", LookMode.Deep, null);
            Scribe_Deep.Look<Thing>(ref StoredGun, "StoredGun", new object[0]);
        }

        private void GunSwitch()
        {
            if (OwnerPawn.equipment.Primary != null)
            {
                ThingWithComps Gun = OwnerPawn.equipment.Primary;
                OwnerPawn.equipment.TryDropEquipment(Gun, out Gun, OwnerPawn.Position);
                ThingWithComps storedGun = StoredGun as ThingWithComps;
                if (StoredGun != null)
                {

                    OwnerPawn.equipment.MakeRoomFor(storedGun);
                    OwnerPawn.equipment.AddEquipment(storedGun);
                }
                StoredGun = Gun;
                Gun.DeSpawn();
            }
            else
            {
                if (StoredGun != null)
                {
                    ThingWithComps storedGun = StoredGun as ThingWithComps;
                    OwnerPawn.equipment.AddEquipment(storedGun);
                    StoredGun = null;
                }
            }
        }
               
        
        private void StoreCloths()
        {
            if (OwnerPawn.apparel.WornApparelCount > 0)
            {
                if (WornCount <= 0)
                {
                    WornCount = OwnerPawn.apparel.WornApparelCount;
                }
                Apparel cloths = OwnerPawn.apparel.WornApparel.First();
                NewClothSetList.Add(cloths);
                OwnerPawn.apparel.TryDrop(cloths, out cloths);
                cloths.DeSpawn();
                PuffThing();
            }
            if (WornCount > 0 && WornCount == NewClothSetList.Count)
            {
                StoredClothSetList = NewClothSetList;
                NewClothSetList = new List<Apparel>();
                WornCount = 0;
                Interaction = false;

            }
            OwnerPawn.Drawer.renderer.graphics.ResolveApparelGraphics();
        }
        private void ClothChange()
        {
            if (OwnerPawn.apparel.WornApparelCount > 0)
            {
                if (WornCount <= 0)
                {
                    WornCount = OwnerPawn.apparel.WornApparelCount;
                }
                Apparel cloths = OwnerPawn.apparel.WornApparel.First();
                NewClothSetList.Add(cloths);
                OwnerPawn.apparel.TryDrop(cloths, out cloths);
                cloths.DeSpawn();
                PuffThing();
            }
            else if (WornCount > 0 && WornCount == NewClothSetList.Count)
            {
                foreach (Apparel thing in StoredClothSetList)
                {
                    OwnerPawn.apparel.Wear(thing, false);
                }
                StoredClothSetList.Clear();
                StoredClothSetList = NewClothSetList;
                NewClothSetList = new List<Apparel>();
                WornCount = 0;
                Interaction = false;
            }
            else
            {
                foreach (Apparel thing in StoredClothSetList)
                {
                    OwnerPawn.apparel.Wear(thing, false);
                }
                StoredClothSetList.Clear();
                StoredClothSetList = NewClothSetList;
                NewClothSetList = new List<Apparel>();
                WornCount = 0;
                Interaction = false;
            }

            OwnerPawn.Drawer.renderer.graphics.ResolveApparelGraphics();
        }

        //Puff stuff
        public void PuffThing()
        {

            for (int i = 0; i < 30; i++)
            {
                MoteMaker.ThrowAirPuffUp(DrawPos,this.Map);
            }
        }


        public override void Tick()
        {

            base.Tick();
            if (OwnerPawn == null && OwnerPawnID != 0)
            {
                OwnerPawn = PapersPlease(OwnerPawnID);
            }
            if (OwnerPawn != null)
            {

                if (OwnerPawn.Position == InteractionCell && OwnerPawn.CurJob == LockerJob && Interaction)
                {
                    if (GunChange)
                    {
                        GunSwitch();
                        GunChange = false;
                    }
                    if (counter >= 20)
                    {
                        counter = 0;
                        if (num == 0)
                        {
                            StoreCloths();
                        }
                        else if (num == 1)
                        {
                            ClothChange();
                        }
                        OwnerPawn.Drawer.rotator.FaceCell(Position);
                    }
                    counter += 1;
                }

            }
            if (TxtCounter > 20000)
            {
                txtpos = UnityEngine.Random.Range(1, EmptyTxt.Count);
                TxtCounter = 0;
            }
            TxtCounter += 1;

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

                if (myPawn != OwnerPawn)
                {
                    Action action3 = delegate
                    {
                        OwnerPawn = myPawn;
                        OwnerPawnID = myPawn.thingIDNumber;
                    };
                    list.Add(new FloatMenuOption("ClutterMakeOwnerPart1".Translate() + myPawn.Name.ToStringShort + "ClutterMakeOwnerPart2".Translate(), action3));
                }



                if (StoredClothSetList.Count <= 0 && myPawn == OwnerPawn && !Interaction)
                {
                    Action action1 = delegate
                    {
                       
                            myPawn.jobs.TryTakeOrderedJob(LockerJob);
                        
                        OwnerPawn = myPawn;
                        myPawn.Reserve(this);
                        num = 0;

                    };
                    list.Add(new FloatMenuOption("ClutterStoreCloths".Translate(), action1));
                }
                if (StoredClothSetList.Count > 0 && myPawn == OwnerPawn && !Interaction)
                {
                    Action action2 = delegate
                    {
                        
                            myPawn.jobs.TryTakeOrderedJob(LockerJob);
                       
                        myPawn.Reserve(this);
                        num = 1;

                    };
                    list.Add(new FloatMenuOption("ClutterSwitchCloths".Translate(), action2));
                }

            }
            return list;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            IList<Gizmo> list = new List<Gizmo>();
            Command_Action command_Action = new Command_Action();

            if (OwnerPawn != null && OwnerPawn.CanReserveAndReach(this, PathEndMode.InteractionCell, Danger.Deadly))
            {
                command_Action.icon = UI_Button;
                command_Action.defaultDesc = "ClutterButtonForceChange".Translate();
                command_Action.action = ButtonStuff;
                command_Action.activateSound = SoundDef.Named("Click");
                command_Action.groupKey = 887767498;
                list.Add(command_Action);
            }
            IEnumerable<Gizmo> commands = base.GetGizmos();
            return (commands == null) ? list.AsEnumerable<Gizmo>() : list.AsEnumerable<Gizmo>().Concat(commands);
        }

        private void ButtonStuff()
        {
            if (!GunChange)
            {
                
                    OwnerPawn.jobs.TryTakeOrderedJob(LockerJob);
                              
                OwnerPawn.Reserve(this);
                num = 1;
                GunChange = true;
            }
        }


        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (OwnerPawn != null)
            {
                stringBuilder.Append(def.label + "ClutterStringOwnerIs".Translate() + OwnerPawn.Name.ToStringShort);
            }
            if (OwnerPawn == null)
            {
                stringBuilder.Append(def.label + "ClutterStringNoOwner".Translate());
            }

            stringBuilder.AppendLine();
            if (StoredGun != null)
            {
                stringBuilder.Append("ClutterStringStoredGun".Translate() + StoredGun.def.label);
                stringBuilder.AppendLine();
            }
            stringBuilder.Append("ClutterStringStoredApparel".Translate());
            stringBuilder.AppendLine();
            if (StoredClothSetList.Count > 0 && StoredClothSetList != null)
            {
                for (int i = 0; i < StoredClothSetList.Count; i++)
                {
                    stringBuilder.Append(StoredClothSetList.ElementAt(i).def.label + ", ");
                }
            }
            if (StoredClothSetList.Count <= 0 && StoredClothSetList != null)
            {
                stringBuilder.Append(EmptyTxt.ElementAt(txtpos));
            }
            return stringBuilder.ToString();
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (StoredClothSetList.Count > 0 && (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize))
            {

                foreach (Apparel thing in StoredClothSetList)
                {
                    thing.Destroy();

                }

            }
            base.Destroy(mode);
        }

        public override void DrawGUIOverlay()
        {
            if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
            {
                if (OwnerPawn != null)
                {
                    
                    GenMapUI.DrawThingLabel(this, OwnerPawn.Name.ToStringShort, new Color(1f, 1f, 1f, 0.75f));
                }


            }
        }

        private Pawn PapersPlease(int PawnId)
        {
            Pawn Owner = null;
            Owner = this.Map.mapPawns.FreeColonists.Where<Pawn>(s => s.thingIDNumber == PawnId).FirstOrDefault();

            return Owner;
        }

        //public override void Draw()
        //{
        //    base.Draw();
        //    if (StoredGun != null)
        //    {
        //        Matrix4x4 matrix = default(Matrix4x4);
        //        Vector3 s = new Vector3(0.8f, 1f, 0.8f);
        //        matrix.SetTRS(DrawPos + Altitudes.AltIncVect, this.Rotation.AsQuat, s);
        //        Graphics.DrawMesh(MeshPool.plane10, matrix, StoredGun.def.graphic.MatAt(base.Rotation, null), 0);
        //    }
        //}


    }
}
