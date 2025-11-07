using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BillDoorsPredefinedCharacter
{
    public class PawnRenderNode_PDCEyes : PawnRenderNode_AttachmentHead
    {
        public PawnRenderNode_PDCEyes(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {
        }

        protected override string TexPathFor(Pawn pawn)
        {
            foreach (var p in BDPDC_Mod.Tracker)
            {
                if (p.Key.eyeTexPath != null && p.Value == pawn) return p.Key.eyeTexPath;
            }

            return base.TexPathFor(pawn);
        }
    }
    public class PawnRenderNodeWorker_PDCEyes : PawnRenderNodeWorker_Eye
    {
        bool? canDraw;

        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            if (canDraw == null && node.Props is PawnRenderNodeProperties_PDCEyes PPDC)
            {
                canDraw = true;
                foreach (var id in PPDC.MayNotRequireMods)
                {
                    if (ModLister.GetActiveModWithIdentifier(id) != null)
                    {
                        canDraw = false;
                        break;
                    }
                }
            }

            return canDraw ?? true && !parms.pawn.IsDessicated();
        }

        public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms)
        {
            if (parms.pawn.IsDessicated())
            {
                return Vector3.zero;
            }
            return base.ScaleFor(node, parms);
        }
    }

    public class PawnRenderNodeProperties_PDCEyes : PawnRenderNodeProperties
    {
        public List<string> MayNotRequireMods = new List<string>();

        public PawnRenderNodeProperties_PDCEyes()
        {
            visibleFacing = new List<Rot4>
        {
            Rot4.East,
            Rot4.South,
            Rot4.West
        };
            workerClass = typeof(PawnRenderNodeWorker_PDCEyes);
            nodeClass = typeof(PawnRenderNode_PDCEyes);
        }

        public override void ResolveReferences()
        {
            skipFlag = RenderSkipFlagDefOf.Eyes;
        }
    }
}
