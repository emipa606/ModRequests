﻿using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace DualWield.Harmony
{
    [HarmonyPatch(typeof(PawnWeaponGenerator), "TryGenerateWeaponFor")]
    class PawnWeaponGenerator_TryGenerateWeaponFor
    {
        static void Postfix(Pawn pawn)
        {
            if (pawn.RaceProps.Humanlike && Rand.Chance(((float)Base.NPCDualWieldChance)/100f))
            {
                float randomInRange = pawn.kindDef.weaponMoney.RandomInRange;
                List<ThingStuffPair> allWeaponPairs = Traverse.Create(typeof(PawnWeaponGenerator)).Field("allWeaponPairs").GetValue<List<ThingStuffPair>>();
                List<ThingStuffPair> workingWeapons = Traverse.Create(typeof(PawnWeaponGenerator)).Field("workingWeapons").GetValue<List<ThingStuffPair>>();
                if (pawn.equipment != null && pawn.equipment.Primary != null && pawn.equipment.Primary.def.IsTwoHand())
                {
                    return;
                }
                if(pawn.equipment == null || pawn.equipment.Primary == null)
                {
                    return;
                }
                for (int i = 0; i < allWeaponPairs.Count; i++)
                {
                    ThingStuffPair w = allWeaponPairs[i];
                    if (w.Price <= randomInRange)
                    {
                        if (pawn.kindDef.weaponTags == null || pawn.kindDef.weaponTags.Any((string tag) => w.thing.weaponTags.Contains(tag)))
                        {
                            if (w.thing.generateAllowChance >= 1f || Rand.ChanceSeeded(w.thing.generateAllowChance, pawn.thingIDNumber ^ (int)w.thing.shortHash ^ 28554824))
                            {
                                workingWeapons.Add(w);
                            }
                        }
                    }
                }
                if (workingWeapons.Count == 0)
                {
                    return;
                }
                ThingStuffPair thingStuffPair;
                IEnumerable<ThingStuffPair> matchingWeapons = workingWeapons.Where((ThingStuffPair tsp) =>
                tsp.thing.CanBeOffHand() &&
                !tsp.thing.IsTwoHand());
                if (matchingWeapons != null && matchingWeapons.TryRandomElementByWeight((ThingStuffPair w) => w.Commonality * w.Price, out thingStuffPair))
                {
                    ThingWithComps thingWithComps = (ThingWithComps)ThingMaker.MakeThing(thingStuffPair.thing, thingStuffPair.stuff);
                    PawnGenerator.PostProcessGeneratedGear(thingWithComps, pawn);
                    pawn.equipment.AddOffHandEquipment(thingWithComps);
                }

            }
        }
    }
}
