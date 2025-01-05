using Verse;
using System;
using System.Collections.Generic;
using static PawnTrackerMain.PawnTrackerUtility;

namespace PawnTrackerMain
{
    public class DocumentedEventDef : ThingDef
    {
        new public string description;
        public string docEventSubType = "None";
        public string docModSpecific = "None";
        public List<string> centralForPawnStatusStrings = new List<string>() {};
        public List<PawnStatus> centralForPawnStatus = new List<PawnStatus>();
        public bool centralForAllPawns = false;
        public bool canJoinAfterRescue = false;
        public bool combineNextEvent = false;
        public List<string> requiredProperties = new List<string>();
        public EventSubType eventSubType => (EventSubType) Enum.Parse(typeof(EventSubType), docEventSubType);
        public ModSpecific modSpecific => (ModSpecific) Enum.Parse(typeof(ModSpecific), docModSpecific);
        public DocumentedEventDef() {}

        public override void PostLoad()
        {
            base.PostLoad();
            if (centralForAllPawns == true)
            {
                if (centralForPawnStatusStrings != null && centralForPawnStatusStrings.Any())
                    Log.Warning("centralForPawnStatusStrings is ignored when centralForAllPawns is true");
                centralForPawnStatus = new List<PawnStatus>() {PawnStatus.Colonist, PawnStatus.Prisoner, PawnStatus.Other};
            }    
            else
            {
                foreach (string cf in centralForPawnStatusStrings)
                    centralForPawnStatus.Add((PawnStatus) Enum.Parse(typeof(PawnStatus), cf));
            }

            if ((eventSubType == EventSubType.Relationship || this == DocumentedEventDefOf.PawnDied) && !requiredProperties.Contains("otherPawns"))
                requiredProperties.Add("otherPawns");
        }
        
    }

    public class JoinEventDef : DocumentedEventDef
    {
        public JoinEventDef() {}
        public override void PostLoad()
        {
            if (centralForPawnStatusStrings != null && centralForPawnStatusStrings.Any())
                Log.Error("Do not specify centralForPawnStatusStrings on Join events...");
            centralForAllPawns = true;
            base.PostLoad();            
        }
    }

    public class ArriveEventDef : DocumentedEventDef
    {
        
        public ArriveEventDef() {}
        
        public override void PostLoad()
        {
            if (centralForPawnStatusStrings != null && centralForPawnStatusStrings.Any())
                Log.Error("Do not specify centralForPawnStatusStrings on Arrive events...");
            centralForAllPawns = true;
            base.PostLoad();
        }
    }

    public class LeaveEventDef : DocumentedEventDef
    {
        public LeaveEventDef() {}
        public override void PostLoad()
        {
            if (centralForPawnStatusStrings != null && centralForPawnStatusStrings.Any())
                Log.Error("Do not specify centralForPawnStatusStrings on Leave events...");
            centralForAllPawns = true;
            base.PostLoad();
        }
    }

    public class DeathEventDef : DocumentedEventDef
    {
        public DeathEventDef() {}
        public override void PostLoad()
        {
            if (centralForPawnStatusStrings != null && centralForPawnStatusStrings.Any())
                Log.Error("Do not specify centralForPawnStatusStrings on Death events...");
            centralForAllPawns = true;
            docEventSubType = "Death";
            base.PostLoad();
        }
    }

    public class LifeEventDef : DocumentedEventDef
    {
        public string lifeEventCategory;
        public LifeEventCategory Category => lifeEventCategory == null ? LifeEventCategory.None : (LifeEventCategory) Enum.Parse(typeof(LifeEventCategory), lifeEventCategory);
        public LifeEventDef() {}
        public override void PostLoad()
        {
            base.PostLoad();
        }

        
    }
}