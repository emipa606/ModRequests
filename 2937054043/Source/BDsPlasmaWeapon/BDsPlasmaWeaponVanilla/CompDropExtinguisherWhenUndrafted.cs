using System.Collections.Generic;
using PipeSystem;
using RimWorld;
using UnityEngine;
using Verse;
using System;
using Verse.AI;

namespace BDsPlasmaWeaponVanilla
{
    public class CompDropExtinguisherWhenUndrafted : ThingComp
    {
        public DropLogic dropLogic;

        CompReloadableFromFiller compTank;

        CompEquippable compEquippable;

        public CompProperties_DropExtinguisherWhenUndrafted Props
        {
            get
            {
                return (CompProperties_DropExtinguisherWhenUndrafted)props;
            }
        }

        public bool isEquipment => compEquippable != null;

        public bool isApparel => parent is Apparel;

        public Pawn pawn
        {
            get
            {
                if (isEquipment)
                {
                    return compEquippable.PrimaryVerb.CasterPawn;
                }
                if (isApparel)
                {
                    return (parent as Apparel).Wearer;
                }
                return null;
            }
        }
        public void switchMode(DropLogic DropLogic)
        {
            dropLogic = DropLogic;
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            compTank = parent.TryGetComp<CompReloadableFromFiller>();
            compEquippable = parent.TryGetComp<CompEquippable>();
            dropLogic = Props.defaultDropLogic;
            if (compTank == null)
            {
                Log.Error("CompDropExtinguisherWhenUndrafted is meant to work with CompReloadableFromFiller!");
            }
        }


        public bool shouldDrop
        {
            get
            {
                if (pawn != null || !pawn.Drafted)
                {
                    switch (dropLogic)
                    {
                        case DropLogic.DontDrop:
                            return false;
                        case DropLogic.AlwaysDrop:
                            return true;
                        case DropLogic.DropWhenEmpty:
                            if (compTank.RemainingCharges == 0)
                            {
                                return true;
                            }
                            return false;
                        case DropLogic.DropWhenFull:
                            if (compTank.emptySpace == 0)
                            {
                                return true;
                            }
                            return false;
                        case DropLogic.DropIfNotFull:
                            if (compTank.emptySpace > 0)
                            {
                                return true;
                            }
                            return false;
                        default:
                            return false;
                    }
                }
                return false;
            }
        }

        private string getIconTex(DropLogic dropLogic)
        {
            switch (dropLogic)
            {
                case DropLogic.DontDrop:
                    return Props.dontDropIcon;
                case DropLogic.AlwaysDrop:
                    return Props.alwaysDropIcon;
                case DropLogic.DropWhenEmpty:
                    return Props.dropWhenEmptyIcon;
                case DropLogic.DropWhenFull:
                    return Props.dropWhenFullIcon;
                case DropLogic.DropIfNotFull:
                    return Props.dropIfNotFullIcon;
                default:
                    return "UI/Commands/DesirePower";
            }
        }

        private string getLabel(DropLogic dropLogic)
        {
            switch (dropLogic)
            {
                case DropLogic.DontDrop:
                    return "BDP_dontDrop".Translate();
                case DropLogic.AlwaysDrop:
                    return "BDP_alwaysDrop".Translate();
                case DropLogic.DropWhenEmpty:
                    return "BDP_dropWhenEmpty".Translate();
                case DropLogic.DropWhenFull:
                    return "BDP_dropWhenFull".Translate();
                case DropLogic.DropIfNotFull:
                    return "BDP_dropIfNotFull".Translate();
                default:
                    return "None";
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent == null)
            {
                yield break;
            }
            yield return new Command_SwitchDropLogic
            {
                compExtinguisher = this,
                defaultLabel = Props.label.Translate() + ": " + getLabel(dropLogic),
                defaultDesc = Props.description.Translate(),
                icon = ContentFinder<Texture2D>.Get(getIconTex(dropLogic)),
            };
            yield break;
        }
    }

    public class CompProperties_DropExtinguisherWhenUndrafted : CompProperties
    {
        public DropLogic defaultDropLogic = DropLogic.DontDrop;
        public string dontDropIcon = "UI/Icons/DropLogicIcons_dontDrop";
        public string dropWhenEmptyIcon = "UI/Icons/DropLogicIcons_dropWhenEmpty";
        public string dropIfNotFullIcon = "UI/Icons/DropLogicIcons_dropIfNotFull";
        public string dropWhenFullIcon = "UI/Icons/DropLogicIcons_dropWhenFull";
        public string alwaysDropIcon = "UI/Icons/DropLogicIcons_alwaysDrop";
        public string description = "BDP_DropLogicDesc";
        public string label = "BDP_Droplogic";
        public CompProperties_DropExtinguisherWhenUndrafted()
        {
            compClass = typeof(CompDropExtinguisherWhenUndrafted);
        }
    }

    public enum DropLogic : byte
    {
        DontDrop,
        DropWhenEmpty,
        DropIfNotFull,
        DropWhenFull,
        AlwaysDrop,
    }

    public class Command_SwitchDropLogic : Command_Action
    {
        private List<Command_SwitchDropLogic> others;

        public CompDropExtinguisherWhenUndrafted compExtinguisher;

        public override bool GroupsWith(Gizmo other)
        {
            Command_SwitchDropLogic command_Reload = other as Command_SwitchDropLogic;
            return command_Reload != null;
        }

        public override void MergeWith(Gizmo other)
        {
            Command_SwitchDropLogic item = other as Command_SwitchDropLogic;
            if (others == null)
            {
                others = new List<Command_SwitchDropLogic>();
                others.Add(this);
            }
            others.Add(item);
        }

        public override void ProcessInput(Event ev)
        {
            Find.WindowStack.Add(MakeAmmoMenu());
        }

        private FloatMenu MakeAmmoMenu()
        {
            return new FloatMenu(BuildAmmoOptions());
        }

        private List<FloatMenuOption> BuildAmmoOptions()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            Action action1 = delegate
            {
                compExtinguisher.switchMode(DropLogic.DontDrop);
            };
            Action action2 = delegate
            {
                compExtinguisher.switchMode(DropLogic.AlwaysDrop);
            };
            Action action3 = delegate
            {
                compExtinguisher.switchMode(DropLogic.DropIfNotFull);
            };
            Action action4 = delegate
            {
                compExtinguisher.switchMode(DropLogic.DropWhenFull);
            };
            Action action5 = delegate
            {
                compExtinguisher.switchMode(DropLogic.DropWhenEmpty);
            };

            list.Add(new FloatMenuOption("BDP_dontDrop".Translate(), action1));
            list.Add(new FloatMenuOption("BDP_alwaysDrop".Translate(), action2));
            list.Add(new FloatMenuOption("BDP_dropIfNotFull".Translate(), action3));
            list.Add(new FloatMenuOption("BDP_dropWhenFull".Translate(), action4));
            list.Add(new FloatMenuOption("BDP_dropWhenEmpty".Translate(), action5));
            return list;
        }
    }

    public class JobGiver_DropExtinguisher : ThinkNode_JobGiver
    {
        private const bool forceReloadWhenLookingForWork = true;
        public override float GetPriority(Pawn pawn)
        {
            return 10f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            ThingWithComps Thing = FindExtinguisher(pawn);
            if (!pawn.CanReserve(Thing, 1, -1, null))
            {
                return null;
            }
            if (Thing == null || Thing.TryGetComp<CompDropExtinguisherWhenUndrafted>() == null)
            {
                return null;
            }
            if (!StoreUtility.TryFindBestBetterStorageFor(Thing, pawn, pawn.Map, StoragePriority.Unstored, pawn.Faction, out var _, out var _, needAccurateResult: false))
            {
                return null;
            }
            if (pawn.equipment.TryDropEquipment(Thing, out _, pawn.Position, false)) ;
            {
                return HaulAIUtility.HaulToStorageJob(pawn, Thing);
            }
        }

        public static ThingWithComps FindExtinguisher(Pawn pawn)
        {
            List<ThingWithComps> weapons = pawn?.equipment.AllEquipmentListForReading;
            for (int i = 0; i < weapons.Count; i++)
            {
                CompDropExtinguisherWhenUndrafted comp = weapons[i].TryGetComp<CompDropExtinguisherWhenUndrafted>();
                if (comp != null && comp.shouldDrop)
                {
                    return weapons[i];
                }
            }
            return null;
        }
    }
}
