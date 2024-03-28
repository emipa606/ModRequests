using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColonyNeedAmmunitionMod
{
    public class Building_AmmoBox : Building_Storage
    {
        public override void SpawnSetup()
        {
            base.SpawnSetup();
            AmmoBoxManager.getInstance().boxes.Add(this);
        }
        public override void Notify_ReceivedThing(Thing newItem)
        {
            base.Notify_ReceivedThing(newItem);
            if (AmmoBoxManager.isAmmo(newItem.def)) {
                int count = newItem.stackCount;
                AmmoBoxManager.getInstance().NotifyAmmoChangeInBox(this, AmmoBoxManager.ReferAmmunitionDef(newItem.def), count);
            }
        }
        public override void Notify_LostThing(Thing newItem)
        {
            base.Notify_LostThing(newItem);
            if (AmmoBoxManager.isAmmo(newItem.def))
            {
                int count = - newItem.stackCount;
                AmmoBoxManager.getInstance().NotifyAmmoChangeInBox(this, AmmoBoxManager.ReferAmmunitionDef(newItem.def), count);
            }
        }
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            AmmoBoxManager.getInstance().boxes.Remove(this);
            base.Destroy(mode);
        }
        public bool ConsumeAmmo(AmmunitionType type) {
            foreach (IntVec3 pos in this.AllSlotCellsList())
            {
                foreach (Thing mono in pos.GetThingList())
                {
                    if (this.GetStoreSettings().AllowedToAccept(mono)) {
                        if (AmmoBoxManager.ReferAmmunitionDef(mono.def) == type) {
                            int aftersize = mono.stackCount - 1;
                            if (aftersize == 0)
                            {
                                mono.Destroy();
                                return true;
                            }
                            else {
                                mono.stackCount--;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

    }
}
