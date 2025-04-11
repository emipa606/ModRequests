using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace LobotomyCorp.Weapon
{
    public class Verb_LCSPecialAttack : Verb_MeleeAttack
    {
        
        public VerbProperties_LCSpecialAttack verbProperties 
            => verbProps as VerbProperties_LCSpecialAttack;
        
        public override bool IsMeleeAttack => true;
        

        protected override bool TryCastShot()
        {
            Stance s = CasterPawn.stances.curStance;
            CasterPawn.stances.SetStance(new Stance_Mobile());
            bool f = base.TryCastShot();
            CasterPawn.stances.SetStance(s);

            return f;
        }

        protected override DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target)
        {
            DamageWorker.DamageResult res = new DamageWorker.DamageResult();
            foreach(DamageInfo di in DamageApplyer.DamageInfoIEnumerable(this, target.Thing))
            {
                if (target.ThingDestroyed) break;
                res = target.Thing.TakeDamage(di);
            }

            if (verbProperties.aoeType != null) DamageApplyer.TakeDamage(this);

            return res;
        }

        public override void DrawHighlight(LocalTargetInfo target)
        {
            List<IntVec3> cell;
            if (target == null)
            {
                return;
            }
            else if (Caster.GetRoom().IsRoomNothing() ||
                target.Thing.GetRoom() != Caster.GetRoom())
                cell = (verbProps as VerbProperties_LCSpecialAttack).aoeType.AffectCellsOf(target.Thing.Position, caster.RotationOf(target.Thing));
            else
                cell = Caster.GetRoom().Cells.ToList();

            GenDraw.DrawFieldEdges(cell, Color.white);
        }

    }
}
