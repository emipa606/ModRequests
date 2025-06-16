﻿using RimWorld;
using Verse;
using Verse.Sound;

namespace ProjectJedi
{
    public class FlyingObject_Equipable : FlyingObject
    {
        protected override void Impact(Thing hitThing)
        {
            if (flyingThing != null)
            {
                GenSpawn.Spawn(flyingThing, Position, Map);
                if (launcher is Pawn {equipment: { }} equipper)
                {
                    if (flyingThing is ThingWithComps flyingThingWithComps)
                    {
                        Equip(equipper, flyingThingWithComps);
                    }
                }
            }

            Destroy();
        }

        public void Equip(Pawn equipper, ThingWithComps thingWithComps)
        {
            if (thingWithComps.def.IsApparel)
            {
                var apparel = (Apparel) thingWithComps;
                equipper.apparel.Wear(apparel);
                if (equipper.outfits != null)
                {
                    equipper.outfits.forcedHandler.SetForced(apparel, true);
                }
            }
            else
            {
                ThingWithComps thingWithComps2;
                if (thingWithComps.def.stackLimit > 1 && thingWithComps.stackCount > 1)
                {
                    thingWithComps2 = (ThingWithComps) thingWithComps.SplitOff(1);
                }
                else
                {
                    thingWithComps2 = thingWithComps;
                    thingWithComps2.DeSpawn();
                }

                equipper.equipment.MakeRoomFor(thingWithComps2);
                equipper.equipment.AddEquipment(thingWithComps2);
                if (thingWithComps.def.soundInteract != null)
                {
                    thingWithComps.def.soundInteract.PlayOneShot(new TargetInfo(equipper.Position, equipper.Map));
                }
            }
        }
    }
}