using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Quiver
{
    public class Projectile_ArrowBlunt: Bullet
    {
        #region Properties
        public ThingDef_ArrowBlunt Def
        {
            get
            {
                return this.def as ThingDef_ArrowBlunt;
            }
        }
        #endregion Properties
    }

    public class Projectile_ArrowFire : Bullet
    {
        #region Properties
        public ThingDef_ArrowFire Def
        {
            get
            {
                return this.def as ThingDef_ArrowFire;
            }
        }
        #endregion Properties

        #region Overrides
        // TODO: figure out how to start a small fire on miss (empty ground). currently, Impact only registers when an object is hit
        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);

            if (Def != null && hitThing != null)
            {
                // spawn a pool of chemfuel to guarantee an initial flame
                // GenSpawn.Spawn(ThingDefOf.Filth_Fuel, base.Position, Current.Game.CurrentMap);

                // set fire to struck unit (pawn, tree, etc.) only
                // hitThing.TryAttachFire(1);

                // set fire on map position, including ground or unit occupying that ground
                // chance depends on terrain type as seen in ChanceToStartFireIn()
                bool fireStarted = FireUtility.TryStartFireIn(base.Position, base.Map, 1);
                if (fireStarted)
                {
                    Messages.Message(TranslatorFormattedStringExtensions.Translate("Quiver_FireArrow_HitMessage", hitThing.Label, this.launcher.Label), MessageTypeDefOf.NeutralEvent);
                }
            }
        }
        #endregion Overrides
    }

    public class Projectile_ArrowFireExp : Projectile_Explosive
    {
        #region Properties
        public ThingDef_ArrowFireExp Def
        {
            get
            {
                return this.def as ThingDef_ArrowFireExp;
            }
        }
        #endregion Properties
    }

    public class Projectile_ArrowAdvanced : Bullet
    {
        #region Properties
        public ThingDef_ArrowAdvanced Def
        {
            get
            {
                return this.def as ThingDef_ArrowAdvanced;
            }
        }
        #endregion Properties
    }

    public class Projectile_ArrowBeamGrav : Bullet
    {
        #region Properties
        public ThingDef_ArrowBeamGrav Def
        {
            get
            {
                return this.def as ThingDef_ArrowBeamGrav;
            }
        }
        #endregion Properties

        #region Overrides
        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);

            if (Def != null && hitThing != null && hitThing is Pawn hitPawn)
            {
                // TODO: figure out how to determine which body part was hit, and insta-kill on headshot

                // small chance to apply catatonic hediff on hit
                var random = Rand.Range(0.0f, 1.0f);
                var successMin = 0.8f;
                if (random > successMin)
                {
                    Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.CatatonicBreakdown, hitPawn, null);
                    hediff.Severity = Rand.Range(0.2f, 0.4f);
                    hitPawn.health.AddHediff(hediff, null, null);
                    Messages.Message(TranslatorFormattedStringExtensions.Translate("Quiver_GravBeam_HitMessage", hitThing.Label, this.launcher.Label), MessageTypeDefOf.NeutralEvent);
                }
            }
        }
        #endregion Overrides
    }

    public class Projectile_ArrowBeamProton : Bullet
    {
        #region Properties
        public ThingDef_ArrowBeamProton Def
        {
            get
            {
                return this.def as ThingDef_ArrowBeamProton;
            }
        }
        #endregion Properties

        #region Overrides
        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);

            if (Def != null && hitThing != null && hitThing is Pawn hitPawn)
            {
                // TODO: figure out how to determine which body part was hit, and apply carcinoma to only that part
                

                // strong chance to apply carcinoma hediff on hit
                var random = Rand.Range(0.0f, 1.0f);
                var successMin = 0.7f;
                if (random > successMin)
                {
                    Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.Carcinoma, hitPawn, null);
                    hediff.Severity = Rand.Range(0.1f, 0.2f);
                    hitPawn.health.AddHediff(hediff, null, null);
                    Messages.Message(TranslatorFormattedStringExtensions.Translate("Quiver_ProtonBeam_HitMessage", hitThing.Label, this.launcher.Label), MessageTypeDefOf.NeutralEvent);
                }
            }
        }
        #endregion Overrides
    }

    public class Projectile_ArrowBeamElectron : Bullet
    {
        #region Properties
        public ThingDef_ArrowBeamElectron Def
        {
            get
            {
                return this.def as ThingDef_ArrowBeamElectron;
            }
        }
        #endregion Properties

        #region Overrides
        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);

            if (Def != null && hitThing != null && hitThing is Pawn hitPawn)
            {
                // very small chance to apply psychic shock hediff on hit
                var random = Rand.Range(0.0f, 1.0f);
                var successMin = 0.9f;
                if (random > successMin)
                {
                    Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.PsychicShock, hitPawn, null);
                    hitPawn.health.AddHediff(hediff, null, null);
                    Messages.Message(TranslatorFormattedStringExtensions.Translate("Quiver_ElectronBeam_HitMessage", hitThing.Label, this.launcher.Label), MessageTypeDefOf.NeutralEvent);
                }
            }
        }
        #endregion Overrides
    }
}
