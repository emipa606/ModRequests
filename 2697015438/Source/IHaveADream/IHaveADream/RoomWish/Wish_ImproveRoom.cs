using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace HDream
{
    public class Wish_ImproveRoom : WishWithComp
    {
        public RoomWishDef Def => (RoomWishDef)def;

        protected List<Room> AllRooms => pawn.Map.regionGrid.allRooms;

        protected int progress = 0;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref doAtTick, "doAtTick", 0);
            Scribe_Values.Look(ref progress, "progress", 0);
            Scribe_Values.Look(ref actualCount, "roomCount", 0);
        }

        // return the progress amount and set the count
        protected override int CountMatch()
        {
            int foundCount = 0;
            actualCount = 0;
            List<Room> rooms;
            for (int i = 0; i < Def.roomInfo.Count; i++)
            {
                rooms = AllRooms.FindAll((Room room) => (Def.roomInfo[i].role == null || Def.roomInfo[i].role == room.Role)
                                                        && (!Def.roomInfo[i].shoulBeOwner || room.Owners.Contains(pawn)));
                if (!rooms.NullOrEmpty())
                {
                    foundCount++;
                    for (int j = 0; j < rooms.Count; j++)
                    {
                        if (Def.roomInfo[i].ScoreStage.minScore <= rooms[j].GetStat(Def.roomInfo[i].relatedStats))
                        {
                            actualCount++;
                            if (actualCount >= def.amountNeeded) return actualCount * 2;
                            break;
                        }
                    }
                }
            }
            if (foundCount > def.amountNeeded) foundCount = (int)def.amountNeeded;
            return foundCount + actualCount;
        }
        public override void PostMake()
        {
            base.PostMake();
            if (def.countPreWishProgress) progress = CountMatch();
        }
        public override void Tick()
        {
            base.Tick();
            TickToResolve();
        }
        protected override void CheckResolve()
        {
            base.CheckResolve();
            int match = CountMatch();
            CountProgressStep(ref progress, match);
            if (actualCount >= def.amountNeeded) OnFulfill();
        }
        public override string FormateText(string text)
        {
            text = text.Replace(Def.amount_Key, ((int)Def.amountNeeded).ToString());
            string listing = "";
            for (int i = 0; i < Def.roomInfo.Count; i++)
            {
                if (Def.roomInfo[i].shoulBeOwner) listing += TranslationKey.ROOM_OWN.Translate();
                if(Def.roomInfo[i].ScoreStage.minScore > float.MinValue) listing += "" + Def.roomInfo[i].ScoreStage.label + " ";
                listing += Def.roomInfo[i].role != null ? Def.roomInfo[i].role.label : RoomRoleDefOf.None.label;
                if(i < Def.roomInfo.Count - 1) listing += def.listing_Separator;
            }
            text = text.Replace(Def.covetedObjects_Key, listing);

            return base.FormateText(text);
        }

        public override string DescriptionToFulfill
        {
            get
            {
                if(Def.amountNeeded > 1) return base.DescriptionToFulfill + " (" + actualCount.ToString() + "/" + Def.amountNeeded.ToString() + ")";
                return base.DescriptionToFulfill;
            }
        }

    }
}
