using RimWorld;
using System.Text;
using Verse;

namespace BillDoorsFramework
{
    public class StatWorker_SecondaryAmmoMagazine : StatWorker
    {
        private ThingDef GunDef(StatRequest req)
        {
            var def = req.Def as ThingDef;

            if (def?.building?.IsTurret ?? false)
                def = def.building.turretGunDef;

            return def;
        }

        public override bool ShouldShowFor(StatRequest req)
        {
            CompProperties_SecondaryAmmo compAmmoProp = GunDef(req)?.GetCompProperties<CompProperties_SecondaryAmmo>();

            if (compAmmoProp != null && !compAmmoProp.showSecondaryAmmoStat) return false;

            return base.ShouldShowFor(req) && (compAmmoProp?.secondaryAmmoProps.magazineSize ?? 0) > 0;
        }

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            return GunDef(req)?.GetCompProperties<CompProperties_SecondaryAmmo>()?.secondaryAmmoProps.magazineSize ?? 0;
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            StringBuilder stringBuilder = new StringBuilder();
            var ammoProps = GunDef(req)?.GetCompProperties<CompProperties_SecondaryAmmo>();
            stringBuilder.AppendLine("CE_MagazineSize".Translate() + ": " + GenText.ToStringByStyle(ammoProps.secondaryAmmoProps.magazineSize, ToStringStyle.Integer));
            stringBuilder.AppendLine("CE_ReloadTime".Translate() + ": " + GenText.ToStringByStyle((ammoProps.secondaryAmmoProps.reloadTime), ToStringStyle.FloatTwo) + " " + "LetterSecond".Translate());
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized = true)
        {
            var ammoProps = GunDef(optionalReq)?.GetCompProperties<CompProperties_SecondaryAmmo>();
            return ammoProps.secondaryAmmoProps.magazineSize + " / " + GenText.ToStringByStyle((ammoProps.secondaryAmmoProps.reloadTime), ToStringStyle.FloatTwo) + " " + "LetterSecond".Translate();
        }
    }
}