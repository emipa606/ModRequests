using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Clutter_Furniture
{
    internal class ClutterThingDefsFurniture : ThingDef
    {
        //Bed
        public string PrisonerBedTexture = "";
        public string MedicalBedTexture = "";
        public string MedicalPrisonerBedTexture = "";
        public string Ui_buttonTextureOn = "";
        public string Ui_buttonTextureOff = "";
        public List<string> RandomBedTex = new List<string>();

        //Books
        public List<string> BookText = new List<string>();
        public List<ThingDef> BooksList = new List<ThingDef>();
        public bool IsABook = false;
        public string CloseTexture = "";

        //Lamps
        public string LampOffTexture = "";

        //Tables
        public string UiButtonTex = "";
        public List<string> SwitchTexPath = new List<string>();

        //Freezer
        public string FreezerBase = "";

        //Hydroponic
        public string Ui_ButtonHydro = "";

    }
}
