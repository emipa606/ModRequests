using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Meeseeks_Box
{
    public class MeeseeksBillKnowledge : IExposable
    {
        public Bill originalBill = null;
        public BillStack billStack = null;
        public Thing billGiver = null;

        public Bill duplicateBill
        {
            get
            {
                if (billStack != null && billStack.Bills != null && billStack.Bills.Count > 0)
                    return billStack.Bills[0];

                else
                    return null;
            }
        }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                if (originalBill.DeletedOrDereferenced)
                    originalBill = null;
            }

            Scribe_References.Look(ref originalBill, "originalBill");

            Scribe_References.Look(ref billGiver, "billGiver");
            Scribe_Deep.Look(ref billStack, "billStack", billGiver as IBillGiver);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                // Hmm so the billstacks billgiver is going to be null at this point anyway, so lets set it directly
                billStack.billGiver = billGiver as IBillGiver;
            }
        }
    }
}
