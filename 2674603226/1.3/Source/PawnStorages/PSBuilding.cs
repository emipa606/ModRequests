using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PawnStorages
{
    public class PSExtension : DefModExtension
    {
        public GraphicData extraGraphicData;
        public Vector3 statueOffset = Vector3.zero;
    }

    public class PSBuilding : Building
    {
        private PSExtension defExtension;
        private Graphic alternativeGraphicInt;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            defExtension = def.GetModExtension<PSExtension>();
            
        }

        //
        protected bool HasExtension => defExtension != null;
        public virtual bool ShouldUseAlternative => HasExtension && defExtension.extraGraphicData != null;

        protected Vector3 StatueOffset => HasExtension ? defExtension.statueOffset : Vector3.zero;
        protected Graphic AlternateGraphic
        {
            get
            {
                if (alternativeGraphicInt == null)
                {
                    if (!HasExtension || defExtension.extraGraphicData == null)
                    {
                        return BaseContent.BadGraphic;
                    }
                    alternativeGraphicInt = defExtension.extraGraphicData.GraphicColoredFor(this);
                }
                return alternativeGraphicInt;
            }
        }

        public override Graphic Graphic => ShouldUseAlternative ? AlternateGraphic : base.Graphic;
    }
}
