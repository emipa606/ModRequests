using RimWorld;
using System.Text;
using UnityEngine;
using Verse;

namespace BillDoorsLootsAndShelves
{
    public class DisplayOffsetInfo : IExposable
    {
        public float drawSizeMult = 1;
        public float drawRot;
        public Vector3 drawOffset;
        public bool shouldFlip;
        public string description;
        public bool useSingleText;
        public bool showInnerContainer;

        public Rot4 rotOverride = Rot4.Invalid;
        public bool hideTop = false;

        public DisplayOffsetInfo()
        {
        }

        public DisplayOffsetInfo(LockerExtension extension)
        {
            if (extension != null)
            {
                drawSizeMult = extension.drawSizeMult;
                drawOffset = extension.drawOffset;
            }
        }

        public DisplayOffsetInfo(DisplayOffsetInfo copyFrom)
        {
            Assign(copyFrom);
        }

        public void Assign(Vector3 drawOffset, float drawSizeMult = 1, float drawRot = 0, bool shouldFlip = false, bool useSingleText = false, bool showInnerContainer = false, Rot4? rotOverride = null, bool hideTop = true)
        {
            this.drawSizeMult = drawSizeMult;
            this.drawRot = drawRot;
            this.drawOffset = drawOffset;
            this.shouldFlip = shouldFlip;
            this.useSingleText = useSingleText;
            this.showInnerContainer = showInnerContainer;
            this.rotOverride = rotOverride ?? Rot4.Invalid;
            this.hideTop = hideTop;
        }

        public void Assign(DisplayOffsetInfo source)
        {
            drawSizeMult = source.drawSizeMult;
            drawRot = source.drawRot;
            drawOffset = source.drawOffset;
            shouldFlip = source.shouldFlip;
            useSingleText = source.useSingleText;
            showInnerContainer = source.showInnerContainer;
            rotOverride = source.rotOverride;
            hideTop = source.hideTop;
        }

        public void AssignShelfVars(DisplayOffsetInfo source)
        {
            description = source.description;
            hideTop = source.hideTop;
        }

        public void CycleRotOverride()
        {
            if (rotOverride == null)
            {
                rotOverride = new Rot4(0);
                return;
            }
            if (rotOverride == Rot4.Invalid)
            {
                rotOverride = Rot4.North;
                return;
            }
            if (rotOverride == Rot4.West)
            {
                rotOverride = Rot4.Invalid;
                return;
            }
            rotOverride.Rotate(RotationDirection.Clockwise);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("BDshelvesDisplayOffsetInfo_Drawsize".Translate() + drawSizeMult.ToString());
            stringBuilder.AppendLine("BDshelvesDisplayOffsetInfo_Rotation".Translate() + drawRot.ToString());
            stringBuilder.AppendLine("BDshelvesDisplayOffsetInfo_DrawOffset".Translate() + drawOffset.ToString());
            stringBuilder.AppendLine("BDshelvesDisplayOffsetInfo_FlipTexture".Translate() + shouldFlip.ToString());
            stringBuilder.AppendLine("BDshelvesDisplayOffsetInfo_SingleTexture".Translate() + useSingleText.ToString());
            stringBuilder.AppendLine("BDshelvesDisplayOffsetInfo_ShowInnerContainer".Translate() + showInnerContainer.ToString());
            stringBuilder.AppendLine("BDshelvesDisplayOffsetInfo_RotOverride".Translate() + rotOverride.ToString());
            stringBuilder.AppendLine("BDshelvesDisplayOffsetInfo_HideTop".Translate() + hideTop.ToString());
            return stringBuilder.ToString();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref drawSizeMult, "drawSizeMult", 1);
            Scribe_Values.Look(ref drawRot, "drawRot", 0);
            Scribe_Values.Look(ref drawOffset, "drawOffset");
            Scribe_Values.Look(ref shouldFlip, "shouldFlip", false);
            Scribe_Values.Look(ref description, "description");
            Scribe_Values.Look(ref useSingleText, "useSingleText", false);
            Scribe_Values.Look(ref showInnerContainer, "showInnerContainer", false);
            Scribe_Values.Look(ref rotOverride, "rotOverride");
            Scribe_Values.Look(ref hideTop, "showTop", false);
        }
    }
}
