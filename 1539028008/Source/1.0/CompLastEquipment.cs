using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhereIsMyWeapon {
    public class CompLastEquipment : ThingComp {
        public ThingList equipments = new ThingList();
        public ThingList inventory = new ThingList();

        public void Clear() {
            if (equipments != null) {
                equipments.Clear();
            } else {
                equipments = new ThingList();
            }

            if (inventory != null) {
                inventory.Clear();
            } else {
                inventory = new ThingList();
            }
        }

        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Deep.Look(ref this.equipments, "wimw_equipments");
            Scribe_Deep.Look(ref this.inventory, "wimw_inventory");
        }
    }
}
