
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace DTechprinting
{
    class UnfinishedTechshards : UnfinishedThing
    {

        public override string GetInspectString()
        {
            if (this.ingredients.Count < 1)
                return base.GetInspectString();
            string ing = "Item: " + ingredients[0].LabelCapNoCount;
            string rpd = "Tech: " + Base.thingDic[ingredients[0].def].LabelCap;
            return ing + "\n" + rpd + "\n" + base.GetInspectString();
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            
            if (mode == DestroyMode.Cancel)
            {
                foreach (Thing ingredient in this.ingredients)
                {
                    ingredient.HitPoints = Mathf.RoundToInt(ingredient.HitPoints*(0.5f + 0.5f*(this.workLeft / this.Recipe.workAmount)));
                    if (ingredient.HitPoints == 0) continue;
                    GenPlace.TryPlaceThing(ingredient, base.Position, base.Map, ThingPlaceMode.Near, (t, x) => t.SetForbidden(true), null, default(Rot4));
                }
                this.ingredients.Clear();
                base.Destroy(DestroyMode.Vanish);
            }
            else
            {
                base.Destroy(mode);
            }
            this.BoundBill = null;
        }
    }
}
