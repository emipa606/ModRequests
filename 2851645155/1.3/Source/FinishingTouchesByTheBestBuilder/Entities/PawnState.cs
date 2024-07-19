using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FinishingTouchesByTheBestBuilder
{
    public class PawnState : IEquatable<PawnState>, IEqualityComparer<PawnState>
    {
        public Pawn pawn;
        public int skill;

        public int RefreshSkill()
        {
            if (!CommonHelper.TryGetSkillWhenWorkSettingEnabled(pawn, out int skill))
            {
                return -1;
            }
            return this.skill = skill;
        }
        public PawnState(Pawn pawn)
        {
            this.pawn = pawn;
            RefreshSkill();
        }

        public override int GetHashCode() => pawn.thingIDNumber;
        public override bool Equals(object obj) => obj is PawnState && this.GetHashCode() == obj.GetHashCode();
        public bool Equals(PawnState other) => this.GetHashCode() == other.GetHashCode();
        public bool Equals(PawnState x, PawnState y) => x.Equals(y);
        public int GetHashCode(PawnState obj) => obj.GetHashCode();
    }
}
