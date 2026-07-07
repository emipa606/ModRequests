using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VFE.Mechanoids;
using VFE.Mechanoids.Needs;
using VFEMech;

namespace FalloutCore
{
    public class CaravaneerMachine : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            var myPawn = this.parent as Pawn;
            if (myPawn.TryGetComp<StandaloneMachine>() != null)
                return;
            var mcs = myPawn.TryGetComp<CompMachine>().myBuilding.TryGetComp<CompMachineChargingStation>();
            StandaloneMachine.Setup(mcs.Props.allowedWorkTypes, mcs.Props.skillLevel, myPawn);
        }
    }
    public class CompProperties_CaravaneerMachine : CompProperties
    {
        public CompProperties_CaravaneerMachine()
        {
            this.compClass = typeof(CaravaneerMachine);
        }
    }
    public class StandaloneMachine : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {

            var props = this.props as CompProperties_StandaloneMachine;
            var myPawn = this.parent as Pawn;
            Setup(props.allowedWorkTypes, props.skillLevel, myPawn);
            if (myPawn.TryGetComp<CompMachine>().Props.violent)
            {
                if (myPawn.drafter == null)
                    myPawn.drafter = new Pawn_DraftController(myPawn);
                if (props.spawnWithWeapon != null)
                {
                    if (!myPawn.equipment.HasAnything())
                    {
                        ThingWithComps thing = (ThingWithComps)ThingMaker.MakeThing(props.spawnWithWeapon);
                        myPawn.equipment.AddEquipment(thing);
                    }
                }
            }
            if (myPawn.needs.TryGetNeed<Need_Power>() == null)
                typeof(Pawn_NeedsTracker).GetMethod("AddNeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(myPawn.needs, new object[] { DefDatabase<NeedDef>.GetNamed("VFE_Mechanoids_Power") });

            if (myPawn.Faction == null)
                myPawn.SetFactionDirect(Find.World.factionManager.OfPlayer);
        }

        public static void Setup(List<WorkTypeDef> allowedWorkTypes, int skillLevel, Pawn myPawn)
        {
            if (myPawn.psychicEntropy == null)
                myPawn.psychicEntropy = new Pawn_PsychicEntropyTracker(myPawn);
            if (myPawn.story == null)
                myPawn.story = new Pawn_StoryTracker(myPawn);
            if (myPawn.skills == null)
                myPawn.skills = new Pawn_SkillTracker(myPawn);
            if (myPawn.workSettings == null)
                myPawn.workSettings = new Pawn_WorkSettings(myPawn);
            if (myPawn.relations == null)
                myPawn.relations = new Pawn_RelationsTracker(myPawn);
            DefMap<WorkTypeDef, int> priorities = new DefMap<WorkTypeDef, int>();
            priorities.SetAll(0);
            typeof(Pawn_WorkSettings).GetField("priorities", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(myPawn.workSettings, priorities);
            foreach (WorkTypeDef workType in allowedWorkTypes)
            {
                foreach (SkillDef skill in workType.relevantSkills)
                {
                    SkillRecord record = myPawn.skills.skills.Find(rec => rec.def == skill);
                    record.levelInt = skillLevel;
                }
                myPawn.workSettings.SetPriority(workType, 1);
            }

            if (myPawn.TryGetComp<CompMachine>().Props.violent)
            {
                if (myPawn.drafter == null)
                    myPawn.drafter = new Pawn_DraftController(myPawn);
            }
        }
    }
    public class CompProperties_StandaloneMachine : CompProperties
    {
        public List<WorkTypeDef> allowedWorkTypes;
        public int skillLevel = 5;
        public bool draftable = false;
        public float extraChargingPower;
        public ThingDef spawnWithWeapon = null;
        public bool turret = false;
        public float hoursToRecharge = 24;
        public CompProperties_StandaloneMachine()
        {
            this.compClass = typeof(StandaloneMachine);
        }
    }
}
