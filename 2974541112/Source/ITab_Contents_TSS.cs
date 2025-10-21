using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;
using UnityEngine;

namespace zed_0xff.CPS;

public class ITab_Contents_TSS : ITab_ContentsBase
{
    private List<Thing> listInt = new List<Thing>();

    public override bool UseDiscardMessage => false;

    public override IList<Thing> container {
        get {
            Building_TSS b = base.SelThing as Building_TSS;
            listInt.Clear();
            if (b != null && b.innerContainer != null)
            {
                listInt.AddRange(b.innerContainer);
            }
            return listInt;
        }
    }

    public ITab_Contents_TSS() {
        labelKey = "Contents";
        containedItemsKey = "Contents";
        canRemoveThings = true;
    }

    protected override void OnDropThing(Thing t, int count) {
        Building_TSS b = base.SelThing as Building_TSS;
        b.Eject(t);
    }
}
