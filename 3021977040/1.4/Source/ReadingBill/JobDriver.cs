using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using RimWorld;
using Verse;

namespace ReadingBill
{
    public class JobDriver_ReadBook: VanillaBooksExpanded.JobDriver_ReadBook
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            List<Toil> toils = base.MakeNewToils().ToList();
            toils[3] = FindSeatsForReading(pawn); // override, only find the billgiver
            toils.Insert(toils.Count - 1, new Toil // decrease remaining repeats by 1 before end the job
            {
                initAction = delegate
                {
                    Bill_Production bill = (Bill_Production)pawn.CurJob.bill;
                    Notify_IterationCompleted(pawn, bill);
                }
            });
            return toils.AsEnumerable();
        }

        private Toil FindSeatsForReading(Pawn p)
        {
            return new Toil
            {
                initAction = delegate
                {
                    Building seat = (p.CurJob.targetA.Thing as Building).InteractionCell.GetEdifice(p.Map);
                    p.CurJob.targetC = seat;
                }
            };
        }

        public void Notify_IterationCompleted(Pawn billDoer, Bill_Production bill)
        {
            if (bill.repeatMode == BillRepeatModeDefOf.RepeatCount)
            {
                if (bill.repeatCount > 0)
                {
                    bill.repeatCount--;
                }
                if (bill.repeatCount == 0)
                {
                    Messages.Message("MessageBillComplete".Translate(bill.LabelCap), (Thing)bill.billStack.billGiver, MessageTypeDefOf.TaskCompletion);
                }
            }
        }
    }
}
