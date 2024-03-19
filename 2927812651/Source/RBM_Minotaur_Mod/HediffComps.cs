using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;


namespace RBM_Minotaur
{
    // Midas Touch Hediff Comp (on Death replace Corpse with Gold)
    public class HediffComp_MidasTouch : HediffComp
    {
        public HediffCompProperties_MidasTouch Props
        {
            get
            {
                return (HediffCompProperties_MidasTouch)this.props;
            }
        }

        // Overrides Notify_PawnKilled to create an effect when a humanlike pawn is killed under the effects of midas touch.
        public override void Notify_PawnKilled()
        {
            Log.Message("public override void Notify_PawnKilled()");

            base.Notify_PawnKilled();

            if (!base.Pawn.RaceProps.Humanlike)
            {
                return;
            }

            if (base.Pawn.health.hediffSet.GetFirstHediffOfDef(RBM_DefOf.MidasTouch).Severity < 0.5)
            {
                return;
            }

            if (this.Props.mote == null && this.Props.fleck == null)
            {
                return;
            }

            Vector3 drawPos = base.Pawn.DrawPos;
            Map map = base.Pawn.Map;

            for (int i = 0; i < this.Props.moteCount; i++)
            {
                Vector2 vector = Rand.InsideUnitCircle * this.Props.moteOffsetRange.RandomInRange * (float)Rand.Sign;
                Vector3 loc = new Vector3(drawPos.x + vector.x, drawPos.y, drawPos.z + vector.y);
                if (this.Props.mote != null)
                {
                    MoteMaker.MakeStaticMote(loc, map, this.Props.mote, 1f, false);
                }
                else
                {
                    FleckMaker.Static(loc, map, this.Props.fleck, 1f);
                }
            }

            if (this.Props.filth != null)
            {
                FilthMaker.TryMakeFilth(base.Pawn.Position, map, this.Props.filth, this.Props.filthCount, FilthSourceFlags.None, true);
            }
            this.Props.sound?.PlayOneShot(SoundInfo.InMap(base.Pawn, MaintenanceType.None));
        }

        // Overrides Notify_PawnDied to spawn gold when a humanlike pawn is killed under the effects of midas touch.

        public override void Notify_PawnDied()
        {
            Log.Message("public override void Notify_PawnDied()");
            base.Notify_PawnDied();

            if (!base.Pawn.RaceProps.Humanlike)
            {
                return;
            }

            if (base.Pawn.health.hediffSet.GetFirstHediffOfDef(RBM_DefOf.MidasTouch).Severity < 0.5)
            {
                return;
            }

            IntVec3 pawnPos = base.Pawn.Corpse.Position;
            Map map = this.parent.pawn.Corpse.Map;

            if (this.Props.destroyBody && MinotaurSettings.midasRemovesCorpse)
            {
                if (MinotaurSettings.midasDespawnDontDestroy)
                {
                    base.Pawn.Corpse.DeSpawn();
                }
                else
                {
                    base.Pawn.Corpse.Destroy();
                }
            }

            //thing.stackCount = Props.goldAmount;
            if (MinotaurSettings.midasGoldAmount != 0)
            {
                Thing thing = ThingMaker.MakeThing(ThingDefOf.Gold, null);
                thing.stackCount = MinotaurSettings.midasGoldAmount;
                GenSpawn.Spawn(thing, pawnPos, map, WipeMode.VanishOrMoveAside);
            }

        }
    }

    //Midas Touch CompProperties
    public class HediffCompProperties_MidasTouch : HediffCompProperties
    {
        public HediffCompProperties_MidasTouch()
        {
            this.compClass = typeof(HediffComp_MidasTouch);
        }

        public FleckDef fleck;
        public ThingDef mote;
        public int moteCount = 3;
        public FloatRange moteOffsetRange = new FloatRange(0.2f, 0.4f);
        public ThingDef filth;
        public int filthCount = 10;
        public SoundDef sound;
        public int goldAmount = 20;
        public bool destroyBody = true;
        public bool destroyItems = true;
    }
}

