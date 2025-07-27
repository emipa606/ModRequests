using System;
using System.Collections.Generic;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Grammar;

namespace CM_Callouts.PendingCallouts
{
    public enum CalloutCategory
    {
        Undefined,
        Combat,
        Animal
    }

    public abstract class PendingCalloutEvent
    {
        public CalloutCategory category = CalloutCategory.Undefined;

        public BodyDef body = null;
        public List<BodyPartRecord> bodyPartsDamaged = new List<BodyPartRecord>();
        public List<bool> bodyPartsDestroyed = new List<bool>();

        public PendingCalloutEvent(CalloutCategory _category)
        {
            category = _category;
        }

        public void FillBodyPartInfo(BodyDef _body, List<BodyPartRecord> _bodyPartsDamaged, List<bool> _bodyPartsDestroyed)
        {
            body = _body;
            bodyPartsDamaged = _bodyPartsDamaged;
            bodyPartsDestroyed = _bodyPartsDestroyed;
        }

        public virtual void AttemptCallout()
        {
            Logger.MessageFormat(this, "Attempting callout.");
        }
    }
}
