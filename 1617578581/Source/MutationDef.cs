﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Radiology
{
    public class MutationDef : HediffDef
    {
        public MutationDef()
        {
            hediffClass = typeof(Mutation);
        }

        public string exclusive;
        public string exclusiveGlobal;
        public List<string> exclusives;
        public List<string> exclusivesGlobal;

        public List<BodyPartDef> relatedParts;
        public List<BodyPartDef> affectedParts;

        /// this decides whether all body part records with specified def are affected by mutation, or just one (ie two lungs vs one lung)
        public bool affectsAllParts = false;

        public float likelihood = 1.0f;
        public int beauty = 0;

        public RadiologyEffectSpawnerDef spawnEffect;
        public RadiologyEffectSpawnerDef spawnEffectFemale;
        public RadiologyEffectSpawnerDef SpawnEffect(Pawn pawn) => (pawn.gender == Gender.Female && spawnEffectFemale != null) ? spawnEffectFemale : spawnEffect;

        public HediffStage stage;

        public List<ThingDef> apparel;

        public GraphicData icon = new GraphicData();

        public override void PostLoad()
        {
            base.PostLoad();

            if (exclusives == null)
                exclusives = new List<string>();

            if (exclusive != null)
                exclusives.Add(exclusive);

            if (exclusivesGlobal == null)
                exclusivesGlobal = new List<string>();

            if (exclusiveGlobal != null)
                exclusivesGlobal.Add(exclusiveGlobal);

            if (stage != null)
            {
                if (stages == null)
                    stages = new List<HediffStage>();

                stages.Add(stage);
            }

            icon.graphicClass = typeof(Graphic_Single);
            icon.texPath = "Radiology/Mutations/" + defName;
        }
    }
}
