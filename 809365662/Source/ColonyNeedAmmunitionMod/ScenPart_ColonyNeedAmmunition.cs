using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;

namespace ColonyNeedAmmunitionMod
{
    public class ScenPart_ColonyNeedAmmunition : ScenPart
    {
        public int ClipSize;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.ClipSize, "ClipSize", 30, false);

            Scribe_Collections.LookDictionary<Pawn, int>(ref this.warmlist, "warmlist", LookMode.Deep, LookMode.Value);
            Scribe_Collections.LookDictionary<IntVec3,bool>(ref this.warmlistturret, "warmlistturret", LookMode.Deep, LookMode.Value);
            Scribe_Collections.LookList<Pawn>(ref this.chklist, "chklist", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                AdditionalTradeItem();
            }
        }

        Dictionary<Pawn, int> warmlist = new Dictionary<Pawn, int>();
        Dictionary<IntVec3,bool> warmlistturret = new Dictionary<IntVec3, bool>();
        List<Pawn> chklist = new List<Pawn>();
        List<Thing> turrets = new List<Thing>();
        public void FindTurrets() {

        }

        public override void Tick()
        {
            AmmoConsumeRoutine();
        }

        private AmmunitionType getRandomAmmoType() {
            int val = Rand.Int % 6;
            switch (val) {
                case 0:
                    return AmmunitionType.Arrow;
                case 1:
                    return AmmunitionType.LowCalibar;
                case 2:
                    return AmmunitionType.HighCalibar;
                case 3:
                    return AmmunitionType.ShotgunShell;
                case 4:
                    return AmmunitionType.SniperRifle;
                case 5:
                    return AmmunitionType.Energy;
            }
            return AmmunitionType.Arrow;
        }

        private void AmmoConsumeRoutine()
        {
            List<Pawn> dellist = new List<Pawn>();
            Dictionary<Pawn, int> addlist = new Dictionary<Pawn, int>();
            foreach (Pawn pawn in warmlist.Keys)
            {
                //Log.Message(pawn + " is on warmlist!");
                if (pawn.Dead || pawn.Downed || !pawn.Spawned || pawn.equipment == null || pawn.equipment.Primary == null || !pawn.equipment.Primary.def.IsRangedWeapon)
                {
                    dellist.Add(pawn);
                    continue;
                }
                if (warmlist[pawn] == 1)
                {
                    Stance_Warmup warm = pawn.stances.curStance as Stance_Warmup;
                    // 同期ずれ対策（ワームアップ）
                    if (warm != null && warm.ticksLeft != 1)
                    {
                        addlist.Add(pawn, warm.ticksLeft - 1);
                        continue;
                    }
                    bool bBursting = pawn.equipment.PrimaryEq.PrimaryVerb.Bursting;
                    if ((warm == null || warm.ticksLeft != 1) && !bBursting)
                    {
                        dellist.Add(pawn);
                        continue;
                    }
                    // 弾切れチェック+装備ドロップ
                    AmmunitionType type = AmmoBoxManager.getInstance().getAmmunitionType(pawn.equipment.Primary.def);
                    if (type == AmmunitionType.None)
                    {
                        dellist.Add(pawn);
                        continue;
                    }
                    int count = AmmoBoxManager.getInstance().CountAmmunition(type);
                    if (count == 0)
                    {
                        AmmoOut(pawn, type);
                        dellist.Add(pawn);
                        continue;
                    }
                }
                if (warmlist[pawn] == 0)
                {
                    // 弾消費
                    AmmunitionType type = AmmoBoxManager.getInstance().getAmmunitionType(pawn.equipment.Primary.def);
                    if (pawn.stances.curStance as Stance_Cooldown == null)
                    {
                        //Log.Message("STRANGE:射撃後にスタンスがクールダウン以外でした。");
                        dellist.Add(pawn);
                        continue;
                    }
                    if (!AmmoBoxManager.getInstance().Consume(type))
                    {
                        //Log.Message("STRANGE:弾数チェック後に射撃が行われましたが弾がありませんでした。");
                        AmmoOut(pawn, type);
                    }

                    // ここでバーストに対応
                    if (pawn.equipment.PrimaryEq.PrimaryVerb.Bursting)
                    {
                        addlist.Add(pawn, pawn.equipment.PrimaryEq.PrimaryVerb.verbProps.ticksBetweenBurstShots);
                    }
                    else
                    {
                        dellist.Add(pawn);
                    }

                    continue;
                }
            }
            // タレット対応
            List<IntVec3> delloclist = new List<IntVec3>();
            List<IntVec3> flgloclist = new List<IntVec3>();
            foreach (IntVec3 loc in warmlistturret.Keys) {
                Building_Turret mono = loc.GetEdifice() as Building_Turret;
                if (mono == null) {
                    delloclist.Add(loc);
                    continue;
                }
                Verb_Shoot verb = mono.AttackVerb as Verb_Shoot;
                if (verb.Bursting)
                {
                    flgloclist.Add(loc);
                }
                else {
                    // バースト完了
                    if (warmlistturret[loc])
                    {
                        ThingWithComps wep = verb.ownerEquipment;
                        AmmunitionType type = AmmoBoxManager.getInstance().getAmmunitionType(wep.def);

                        for (int i = 0; i < verb.verbProps.burstShotCount; i++) {
                            AmmoBoxManager.getInstance().Consume(type);
                        }
                        delloclist.Add(loc);
                    }
                }
            }
            
            for (int i = 0; i < flgloclist.Count; i++) {
                warmlistturret[flgloclist[i]] = true;
            }

            foreach (IntVec3 loc in delloclist) {
                warmlistturret.Remove(loc);
            }

            foreach (Pawn pawn in dellist)
            {
                warmlist.Remove(pawn);
            }
            List<Pawn> keylist = warmlist.Keys.ToList();
            for (int i = 0; i < keylist.Count; i++)
            {
                warmlist[keylist[i]]--;
            }
            foreach (Pawn pawn in addlist.Keys)
            {
                warmlist.Remove(pawn);
                warmlist.Add(pawn, addlist[pawn]);
            }

            if (Find.TickManager.TicksGame % 22 == 0)
            {
                // コロニストの弾丸発射判定
                foreach (Pawn colonist in Find.ColonistBar.ColonistsInOrder)
                {
                    if (!colonist.Spawned || colonist.Downed || colonist.Dead) continue;
                    if (colonist.equipment.Primary != null && colonist.equipment.Primary.def.IsRangedWeapon)
                    {
                        Stance_Warmup warm = colonist.stances.curStance as Stance_Warmup;
                        int reserveWarming = -1;
                        if (warmlist.Keys.Contains(colonist))
                        {
                            reserveWarming = warmlist[colonist];
                        }
                        if (warm != null)
                        {
                            if ((5 < warm.ticksLeft && warm.ticksLeft <= 22 + 5) && (warm.ticksLeft < reserveWarming || reserveWarming == -1))
                            {
                                warmlist.Remove(colonist);
                                warmlist.Add(colonist, warm.ticksLeft - 1);
                                //Log.Message(colonist + " reserve set :" + warmlist[colonist]);
                            }
                        }
                    }
                }

                // タレットの弾丸発射判定
                foreach (Building_Turret mono in Find.ListerBuildings.allBuildingsColonistCombatTargets)
                {
                    if ( mono == null || !mono.Spawned || mono.Destroyed || mono.ThreatDisabled() || mono.CurrentTarget==null) {
                        continue;
                    }
                    Verb_Shoot verb = mono.AttackVerb as Verb_Shoot;
                    if (verb == null) continue;
                    ThingWithComps wep = verb.ownerEquipment;
                    if (wep == null || !wep.def.IsRangedWeapon) continue;
                    AmmunitionType type = AmmoBoxManager.getInstance().getAmmunitionType(wep.def);
                    //Log.Message("Turret found " + mono + " type:" + type);
                    if (type != AmmunitionType.None) {
                        int count = AmmoBoxManager.getInstance().CountAmmunition(type);
                        if (count < verb.verbProps.burstShotCount) {
                            // 弾切れ
                            Find.LetterStack.ReceiveLetter("AmmoOut".Translate(), "AmmoOutDesc".Translate(), LetterType.BadNonUrgent, mono);
                            mono.GetComp<CompFlickable>().DoFlick();
                            continue;
                        }
                        if (!warmlistturret.Keys.Contains(mono.Position))
                        {
                            //Log.Message("turret set to list");
                            warmlistturret.Add(mono.Position,false);
                        }
                    }
                }
            }
        }

        private void AmmoOut(Pawn pawn , AmmunitionType type)
        {
            // ドロップ処理
            ThingWithComps thing;
            pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out thing, pawn.Position, true);
            pawn.jobs.EndCurrentJob(Verse.AI.JobCondition.Incompletable);

            // 通知
            Find.LetterStack.ReceiveLetter("AmmoOut".Translate(), "AmmoOutDesc".Translate(), LetterType.BadNonUrgent, pawn);
        }

        public override void Notify_PawnGenerated(Pawn pawn, PawnGenerationContext context)
        {
            base.Notify_PawnGenerated(pawn, context);

            // アモドロップ
            if (pawn.Faction != null && !pawn.Faction.IsPlayer && pawn.Faction.def != FactionDefOf.Mechanoid) {
                if (pawn.equipment != null && pawn.equipment.Primary != null) {
                    AmmunitionType type = AmmunitionType.None;
                    
                    type = AmmoBoxManager.getInstance().getAmmunitionType(pawn.equipment.Primary.def);

                    if (type != AmmunitionType.None)
                    {
                        int dropAmount;
                        ThingDef ammodef = ThingDef.Named(AmmoBoxManager.getAmmunitionDefNameByType(type));
                        if (type == AmmunitionType.SniperRifle)
                        {
                            dropAmount = ClipSize / 4 + new Random().Next() % (int)(0.5f * (float)ClipSize / 4 );
                        }
                        else
                        {
                            dropAmount = ClipSize + (new Random().Next() % (int)(0.5f * (float)ClipSize));
                        }
                        Thing mono = ThingMaker.MakeThing(ammodef);
                        mono.stackCount = dropAmount;
                        pawn.inventory.container.TryAdd(mono);
                    }
                }
            }
        }


        public override void PostGameStart()
        {
            AdditionalTradeItem();
        }

        private void AdditionalTradeItem()
        {
            foreach (TraderKindDef def in DefDatabase<TraderKindDef>.AllDefs) {
                //Log.Message("name:" + def.defName);
                bool bSulfur = def.WillTrade(ThingDefOf.Steel);
                bool bArrow = def.WillTrade(ThingDef.Named("Bow_Short"));
                bool bAmmo = def.WillTrade(ThingDef.Named("Gun_Pistol"));
                bool bBulk = false;
                int min, max;

                foreach (StockGenerator stock in def.stockGenerators) {
                    if (stock.HandlesThingDef(ThingDefOf.Silver)) {
                        bBulk = 1000 < stock.countRange.max;
                        break;
                    }
                }
                //Log.Message("FLGs bulk:" + bBulk + " sulfur:" + bSulfur + " arrow:" + bArrow + " ammo:" + bAmmo);

                if (bSulfur) {
                    if (bBulk) { min = 300; max = 1000; } else { min = 100; max = 300; }
                    def.stockGenerators.Add(new StockGenerator_SingleDefCrack(ThingDef.Named("Sulfur"),min,max));
                //    Log.Message("Add Sulufer!");
                }
                if (bArrow)
                {
                    if (bBulk) { min = 100; max = 400; } else { min = 30; max = 100; }
                    def.stockGenerators.Add(new StockGenerator_SingleDefCrack(ThingDef.Named("ArrowAmmo"), min, max));
                //    Log.Message("Add arrow!");
                }
                if (bAmmo)
                {
                    if (bBulk) { min = 0; max = 400; } else { min = 0; max = 80; }

                    def.stockGenerators.Add(new StockGenerator_SingleDefCrack(ThingDef.Named(AmmoBoxManager.getAmmunitionDefNameByType(AmmunitionType.Arrow)), min, max));
                    def.stockGenerators.Add(new StockGenerator_SingleDefCrack(ThingDef.Named(AmmoBoxManager.getAmmunitionDefNameByType(AmmunitionType.LowCalibar)), min, max));
                    def.stockGenerators.Add(new StockGenerator_SingleDefCrack(ThingDef.Named(AmmoBoxManager.getAmmunitionDefNameByType(AmmunitionType.ShotgunShell)), min, max));
                    def.stockGenerators.Add(new StockGenerator_SingleDefCrack(ThingDef.Named(AmmoBoxManager.getAmmunitionDefNameByType(AmmunitionType.HighCalibar)), min, max));
                    def.stockGenerators.Add(new StockGenerator_SingleDefCrack(ThingDef.Named(AmmoBoxManager.getAmmunitionDefNameByType(AmmunitionType.SniperRifle)), min/10, max/10));
                    def.stockGenerators.Add(new StockGenerator_SingleDefCrack(ThingDef.Named(AmmoBoxManager.getAmmunitionDefNameByType(AmmunitionType.Energy)), min, max));
                //    Log.Message("Add ammos!");
                }
            }
        }
    }
}
