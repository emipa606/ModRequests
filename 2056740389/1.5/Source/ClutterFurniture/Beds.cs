using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse.AI;
using RimWorld;
using Verse;
using Verse.Sound;

namespace Clutter_Furniture
{
    class Beds : Building_Bed
    {

        public ClutterThingDefsFurniture BedTextures = null;
        public string PrisonerBedTexture;
        public string MedicalBedTexture;
        public string MediPrisonerBedTexture;
        public string RandPrimary;
        public bool usePrimaryTex=false;
        public Graphic SecondaryGraphic;
        public Graphic PrimaryGraphic;
        public Graphic MedicalGraphic;
        public Graphic MedicalSecondaryGraphic;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            LongEventHandler.ExecuteWhenFinished(GraphicHandler);
        }
        private void GraphicHandler()
        {
            ReadFromXML();
            GetGraphics();
        }
        

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<string>(ref RandPrimary, "RandPrimary");
            Scribe_Values.Look<bool>(ref usePrimaryTex, "usePrimaryTex");
            ReadFromXML();
        }

        public override Graphic Graphic
        {
            get
            {
                Graphic graphic;
                Graphic result;
                if (!ForPrisoners)
                {
                    if (Medical)
                    {
                        if (MedicalGraphic == null)
                        {
                            GetGraphics();
                            if (MedicalGraphic == null)
                            {
                                graphic = base.Graphic;
                                result = graphic;
                                return result;
                            }
                        }
                        graphic = MedicalGraphic;
                    }
                    else
                    {
                        if (PrimaryGraphic == null)
                        {
                            GetGraphics();
                            if (PrimaryGraphic == null)
                            {
                                graphic = base.Graphic;
                                result = graphic;
                                return result;
                            }
                        }
                        graphic = PrimaryGraphic;
                    }
                }
                else
                {
                    if (Medical)
                    {
                        if (MedicalSecondaryGraphic == null)
                        {
                            GetGraphics();
                            if (MedicalSecondaryGraphic == null)
                            {
                                graphic = base.Graphic;
                                result = graphic;
                                return result;
                            }
                        }
                        graphic = MedicalSecondaryGraphic;
                    }
                    else
                    {
                        if (SecondaryGraphic == null)
                        {
                            GetGraphics();
                            if (MedicalSecondaryGraphic == null)
                            {
                                graphic = base.Graphic;
                                result = graphic;
                                return result;
                            }
                        }
                        graphic = SecondaryGraphic;
                    }
                }
                result = graphic;
                return result;
            }
        }


        private void GetGraphics()
        {
            if (PrisonerBedTexture.NullOrEmpty())
            {
                ReadFromXML();
            }
            if (BedTextures == null)
            {
                BedTextures = (ClutterThingDefsFurniture)def;
            }
            if (PrimaryGraphic == null && !def.graphicData.texPath.NullOrEmpty())
            {
                if (!this.RandPrimary.NullOrEmpty())
                {
                    PrimaryGraphic = GraphicDatabase.Get<Graphic_Multi>(this.RandPrimary);
                }
                else
                {
                    PrimaryGraphic = GraphicDatabase.Get<Graphic_Multi>(def.graphicData.texPath);
                }
                PrimaryGraphic.drawSize = def.graphicData.drawSize;
            }
            if (SecondaryGraphic == null && !PrisonerBedTexture.NullOrEmpty())
            {
                SecondaryGraphic = GraphicDatabase.Get<Graphic_Multi>(PrisonerBedTexture);
                SecondaryGraphic.drawSize = def.graphicData.drawSize;
            }
            if (MedicalGraphic == null && !MedicalBedTexture.NullOrEmpty())
            {
                MedicalGraphic = GraphicDatabase.Get<Graphic_Multi>(MedicalBedTexture);
                MedicalGraphic.drawSize = def.graphicData.drawSize;
            }
            if (MedicalSecondaryGraphic == null)
            {
                if (!MediPrisonerBedTexture.NullOrEmpty())
                {
                    MedicalSecondaryGraphic = GraphicDatabase.Get<Graphic_Multi>(MediPrisonerBedTexture);
                    MedicalSecondaryGraphic.drawSize = def.graphicData.drawSize;
                }
                else
                {
                    if (!MedicalBedTexture.NullOrEmpty())
                    {
                        MedicalGraphic = GraphicDatabase.Get<Graphic_Multi>(MedicalBedTexture);
                        MedicalGraphic.drawSize = def.graphicData.drawSize;
                    }
                }
            }
        }
        private void ReadFromXML()
        {
            BedTextures = (ClutterThingDefsFurniture)def;
            if (!BedTextures.PrisonerBedTexture.NullOrEmpty())
            {
                PrisonerBedTexture = BedTextures.PrisonerBedTexture;
                MedicalBedTexture = BedTextures.MedicalBedTexture;
                MediPrisonerBedTexture = BedTextures.MedicalPrisonerBedTexture;
                if(BedTextures.RandomBedTex.Count>0 && PrimaryGraphic==null && !usePrimaryTex)
                {
                    float num = UnityEngine.Random.Range(0, 100);
                    if(num<10)
                    {
                        this.RandPrimary = BedTextures.RandomBedTex.RandomElement();
                                            
                    }
                    else 
                    {
                        this.usePrimaryTex = true;
                    }
                }

            }
        }
    }
}
