using RimWorld;
using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

namespace HDream
{
    public class Pawn_WishTracker : IExposable
    {

        public Pawn pawn;

        public List<Wish> wishes = new List<Wish>();

        public List<Wish_WantIngestible> ingestibleWishes = new List<Wish_WantIngestible>();

        private DefMap<WishDef, float> wishChances = new DefMap<WishDef, float>();

        public int tickWithoutWish = 0;
        public int withoutWishCache = 0;

        private int depressionTick = 0;
        private int bufferTick = (int)(WishUtility.Def.newWishBufferDepressionStartInDay * GenDate.TicksPerDay);

        private static int minTimeDreamMote = GenDate.TicksPerHour / 5;
        private static int maxTimeDreamMote = (GenDate.TicksPerHour * 2) / 3;
        private int nextDreamMote = 0;

        public Pawn_WishTracker(Pawn pawn)
        {
            this.pawn = pawn;
        }
        public void ExposeData()
        {

            Scribe_Collections.Look(ref wishes, "wishes", LookMode.Deep);
            Scribe_Values.Look(ref tickWithoutWish, "tickWithoutWish", 0);
            Scribe_Values.Look(ref withoutWishCache, "withoutWishCache", 0);
            Scribe_Values.Look(ref depressionTick, "depressionTick", 0);
            Scribe_Values.Look(ref bufferTick, "bufferTick");
            Scribe_Values.Look(ref nextDreamMote, "nextDreamMote", 0);
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                for (int i = 0; i < wishes.Count; i++)
                {
                    wishes[i].pawn = pawn;
                }
            }
        }
        public void Clear()
        {
            wishes.Clear();
        }

        public void SoftRefresh()
        {
            if (!wishes.NullOrEmpty())
            {
                for (int i = wishes.Count - 1; i >= 0; i--) wishes[i].RefreshPending();
            }
        }

        public void WishesTick()
        {
            if(!WishUtility.CanHaveWish(pawn)) return;
            if (!wishes.NullOrEmpty())
            {
                for (int i = wishes.Count - 1; i >= 0; i--)
                {
                    Wish wish = wishes[i];
                    wish.Tick();
                    wish.PostTick();
                }
                DreamFeedback();
            }
            if (pawn.IsHashIntervalTick(GenDate.TicksPerHour))
            {
                if (Rand.Value <= WishUtility.ChancePerHourToGetNewWish(pawn))
                {
                    Wish wish = TryGiveWish();
                    if (wish != null)
                    {
                        wishes.Add(wish);
                        wish.PostAdd();
                        Messages.Message(TranslationKey.MESSAGE_NEW_WISH.Translate(WishUtility.Def.tierSingular[wish.TierIndex], pawn.LabelShort, wish.LabelCap), new LookTargets(pawn), MessageTypeDefOf.CautionInput);
                        if (depressionTick > 0)
                        {
                            pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(HDThoughtDefOf.NoWishDepresion);
                            depressionTick = 0;
                        }
                        withoutWishCache = tickWithoutWish;
                        tickWithoutWish = 0;
                        bufferTick = (int)(WishUtility.Def.newWishBufferDepressionStartInDay * GenDate.TicksPerDay);
                    }
                }
            }
            if (bufferTick > 0) bufferTick--;
            else NoWishTime();
        }


        protected virtual void DreamFeedback()
        {
            if (pawn.Awake()) return;
            if (Find.TickManager.TicksGame < nextDreamMote) return;
            nextDreamMote = Find.TickManager.TicksGame + Rand.Range(minTimeDreamMote, maxTimeDreamMote);
            List<Texture2D> uiPath = new List<Texture2D>();
            List<Texture2D> wishIcons;
            for (int i = 0; i < wishes.Count; i++)
            {
                wishIcons = wishes[i].DreamIcon();
                if (!wishIcons.NullOrEmpty()) uiPath.Add(wishIcons[Rand.Range(0, wishIcons.Count - 1)]);
            }
            MoteBubble obj = MoteMaker.MakeThoughtBubble(pawn, "Things/Mote/SleepZ");
            if (uiPath.Count > 0)
            {
                obj.SetupMoteBubble(uiPath[Rand.Range(0, uiPath.Count - 1)], null);
                obj.def = HDThingDefOf.Mote_Dream;
            }
        }


        public void NoWishTime()
        {
            if (wishes.Count > 0) return;

            if ((++tickWithoutWish) * SettingMenu.settings.wishFrequencyFactor 
                >= (WishUtility.Def.dayToGetNoWishDepression + depressionTick * WishUtility.Def.dayToUpDepression) * GenDate.TicksPerDay)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(HDThoughtDefOf.NoWishDepresion, 0));
                depressionTick++;
            }
        }

        public void TryResolveIngestible(Thing thing, int amount, float nutriment)
        {
            if (ingestibleWishes.NullOrEmpty()) return;
            for (int i = 0; i < ingestibleWishes.Count; i++)
            {
                ingestibleWishes[i].CkeckResolve(thing, amount, nutriment);
            }
        }
        public Wish TryGiveWish()
        {
            if (!WishUtility.CanHaveWish(pawn)) return null;
            List<WishDef> timeWishes = DefDatabase<WishDef>.AllDefsListForReading.FindAll(wish => wish.category == WishCategory.Time);
            for (int i = 0; i < timeWishes.Count; i++)
            {
                wishChances[timeWishes[i]] = timeWishes[i].GetChance(pawn);

            }

            for (int j = 0; j < wishChances.Count; j++)
            {
                if (!timeWishes.TryRandomElementByWeight((WishDef d) => wishChances[d], out WishDef result))
                {
                    break;
                }
                Wish wish = TryGiveWishFromWishDefDirect(result, pawn);
                if (wish != null) return wish;
                wishChances[result] = 0f;
            }
            return null;
        }
        private Wish TryGiveWishFromWishDefDirect(WishDef def, Pawn pawn)
        {
            Wish wish = (Wish)Activator.CreateInstance(def.wishClass);
            wish.def = def;
            wish.pawn = pawn;
            wish.PostMake();
            return !wish.makeFailed ? wish : null;
        }

        public void RemoveWish(Wish wish)
        {
            if (wishes.Contains(wish))
            {
                wishes.Remove(wish);
                wish.PostRemoved();
            }
        }
        public void DismissWish(Wish wish)
        {
            if (wishes.Count == 1)
            {
                tickWithoutWish = withoutWishCache + wish.ageTicks;
                bufferTick = 0;
            }
            RemoveWish(wish);
            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(HDThoughtDefOf.WishDenial, 0));
        }
    }
}
