using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace SimpleSlaveryCollars
{
    public class CompRemoteSlaveCollar : ThingComp
    {
        public bool remotearmedExplosive = false;
        public bool remotearmedElectric = false;
        public bool remotearmedCrypto = false;
        private bool PowerOn
        {
            get
            {
                CompPowerTrader comp = this.parent.GetComp<CompPowerTrader>();
                return comp != null && comp.PowerOn;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!this.PowerOn)
                yield break;
            // 1. Arm the collar (Explosive)
            var armCollarExplosive = new Command_Toggle();
            Func<bool> isArmedExplosive = () => remotearmedExplosive;
            armCollarExplosive.isActive = isArmedExplosive;
            armCollarExplosive.defaultLabel = "Label_CollarExplosive_Arm_Remote".Translate();
            armCollarExplosive.defaultDesc = "Desc_CollarExplosive_Arm_Remote".Translate();
            armCollarExplosive.toggleAction = delegate
            {
                remotearmedExplosive = !remotearmedExplosive;
                DoRemoteCollarExplosive();
            };
            armCollarExplosive.activateSound = SoundDefOf.Click;
            armCollarExplosive.icon = ContentFinder<Texture2D>.Get("UI/Commands/ArmCollar", true);
            yield return armCollarExplosive;

            if (remotearmedExplosive)
            {
                // 2. Detonate the collar (Explosive)
                var detonate = new Command_Action();
                detonate.defaultLabel = "Label_CollarExplosive_Detonate_Remote".Translate();
                detonate.defaultDesc = "Desc_CollarExplosive_Detonate_Remote".Translate();
                detonate.action = delegate
                {
                    DoRemoteCollarGoBoom();
                };
                detonate.activateSound = SoundDefOf.Click;
                detonate.icon = ContentFinder<Texture2D>.Get("UI/Commands/DetonateCollar", true);
                yield return detonate;
            }

            // 3. Arm the collar (Electric)
            var armCollarElectric = new Command_Toggle();
            Func<bool> isArmedElectric = () => remotearmedElectric;
            armCollarElectric.isActive = isArmedElectric;
            armCollarElectric.defaultLabel = "Label_CollarElectric_Arm_Remote".Translate();
            armCollarElectric.defaultDesc = "Desc_CollarElectric_Arm_Remote".Translate();
            armCollarElectric.toggleAction = delegate
            {
                remotearmedElectric = !remotearmedElectric;
                DoRemoteCollarElectric();
            };
            armCollarElectric.activateSound = SoundDefOf.Click;
            armCollarElectric.icon = ContentFinder<Texture2D>.Get("UI/Commands/DetonateCollar", true);
            yield return armCollarElectric;

            // 4. Arm the collar (Crypto)
            var armCollarCrypto = new Command_Toggle();
            Func<bool> isArmedCrypto = () => remotearmedCrypto;
            armCollarCrypto.isActive = isArmedCrypto;
            armCollarCrypto.defaultLabel = "Label_CollarCrypto_Arm_Remote".Translate();
            armCollarCrypto.defaultDesc = "Desc_CollarCrypto_Arm_Remote".Translate();
            armCollarCrypto.toggleAction = delegate
            {
                remotearmedCrypto = !remotearmedCrypto;
                DoRemoteCollarCrypto();
            };
            armCollarCrypto.activateSound = SoundDefOf.Click;
            armCollarCrypto.icon = ContentFinder<Texture2D>.Get("UI/Commands/DetonateCollar", true);
            yield return armCollarCrypto;
        }
        private void DoRemoteCollarExplosive()
        {
            IEnumerable<Pawn> pawns = this.parent.Map.mapPawns.AllPawnsSpawned.Where<Pawn>((Func<Pawn, bool>)(p => SlaveUtility.IsColonyMember(p) && SlaveUtility.HasSlaveCollar(p) && SlaveUtility.GetSlaveCollar(p).def.thingClass == typeof(SlaveCollar_Explosive) && (SlaveUtility.GetSlaveCollar(p) as SlaveCollar_Explosive).armed != remotearmedExplosive));
            foreach (Pawn pawn in pawns)
            {
                (SlaveUtility.GetSlaveCollar(pawn) as SlaveCollar_Explosive).armed = remotearmedExplosive;
                if ((SlaveUtility.GetSlaveCollar(pawn) as SlaveCollar_Explosive).armed == true)
                {
                    if ((SlaveUtility.GetSlaveCollar(pawn) as SlaveCollar_Explosive).arm_cooldown == 0)
                    {
                        SlaveUtility.TryInstantBreak(pawn, Rand.Range(0.25f, 0.33f));
                        (SlaveUtility.GetSlaveCollar(pawn) as SlaveCollar_Explosive).arm_cooldown = 2500;
                    }
                }
            }
        }
        private void DoRemoteCollarElectric()
        {
            IEnumerable<Pawn> pawns = this.parent.Map.mapPawns.AllPawnsSpawned.Where<Pawn>((Func<Pawn, bool>)(p => SlaveUtility.IsColonyMember(p) && SlaveUtility.HasSlaveCollar(p) && SlaveUtility.GetSlaveCollar(p).def.thingClass == typeof(SlaveCollar_Electric) && (SlaveUtility.GetSlaveCollar(p) as SlaveCollar_Electric).armed != remotearmedElectric));
            foreach (Pawn pawn in pawns)
            {
                (SlaveUtility.GetSlaveCollar(pawn) as SlaveCollar_Electric).armed = remotearmedElectric;
            }
        }
        private void DoRemoteCollarCrypto()
        {
            IEnumerable<Pawn> pawns = this.parent.Map.mapPawns.AllPawnsSpawned.Where<Pawn>((Func<Pawn, bool>)(p => SlaveUtility.IsColonyMember(p) && SlaveUtility.HasSlaveCollar(p) && SlaveUtility.GetSlaveCollar(p).def.thingClass == typeof(SlaveCollar_Crypto) && (SlaveUtility.GetSlaveCollar(p) as SlaveCollar_Crypto).armed != remotearmedCrypto));
            foreach (Pawn pawn in pawns)
            {
                (SlaveUtility.GetSlaveCollar(pawn) as SlaveCollar_Crypto).armed = remotearmedCrypto;
                if ((SlaveUtility.GetSlaveCollar(pawn) as SlaveCollar_Crypto).armed == false)
                {
                    (SlaveUtility.GetSlaveCollar(pawn) as SlaveCollar_Crypto).RevertMentalState();
                }
            }
        }
        private void DoRemoteCollarGoBoom()
        {
            IEnumerable<Pawn> pawns = this.parent.Map.mapPawns.AllPawns.Where<Pawn>((Func<Pawn, bool>)(p => SlaveUtility.IsColonyMember(p) && SlaveUtility.HasSlaveCollar(p) && SlaveUtility.GetSlaveCollar(p).def.thingClass == typeof(SlaveCollar_Explosive) && (SlaveUtility.GetSlaveCollar(p) as SlaveCollar_Explosive).armed));
            foreach (Pawn pawn in pawns)
            {
                (SlaveUtility.GetSlaveCollar(pawn) as SlaveCollar_Explosive).GoBoom();
            }
        }
    }
}
