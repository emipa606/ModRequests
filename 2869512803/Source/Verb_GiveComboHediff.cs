using AbilitiesExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VFECore.Abilities
{
    // AbilitiesExtended.Verb_ShootEquipment
    public class Verb_GiveComboHediff : Verb_EquipmentLaunchProjectile
    {
        // Token: 0x0600219D RID: 8605 RVA: 0x000CC0DB File Offset: 0x000CA2DB
        protected override bool TryCastShot()
        {
			HealthUtility.AdjustSeverity(this.CasterPawn, WP_DefOf.WP_VoltaicComboHD, 5f);
			base.TryCastShot();
            return false;
        } 
    }
}