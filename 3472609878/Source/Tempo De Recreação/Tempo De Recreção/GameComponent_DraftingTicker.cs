using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace TempoDeRecreacao
{
    public class GameComponent_DraftingTicker : GameComponent
    {
        private Dictionary<Pawn, int> draftedPawns = new Dictionary<Pawn, int>();
        private Dictionary<Pawn, int> lastDraftTickPerPawn = new Dictionary<Pawn, int>();

        public GameComponent_DraftingTicker(Game game) : base() { }

        public override void GameComponentTick()
        {
            if (!RecreationDraftSettingsHelper.Enabled) return;

            int currentTick = Find.TickManager.TicksGame;

            foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonistsSpawned)
            {
                if (ShouldSkipPawn(pawn)) continue;

                // Verifica se já foi alistado hoje (60000 ticks = 1 dia no jogo)
                if (!lastDraftTickPerPawn.TryGetValue(pawn, out int lastTick) || currentTick - lastTick >= 60000)
                {
                    DraftPawn(pawn);
                    lastDraftTickPerPawn[pawn] = currentTick;
                }
            }

            // Desalistar após o tempo definido
            List<Pawn> pawnsToUndraft = new List<Pawn>();
            foreach (var kvp in draftedPawns)
            {
                if (currentTick > kvp.Value)
                {
                    pawnsToUndraft.Add(kvp.Key);
                }
            }

            foreach (Pawn pawn in pawnsToUndraft)
            {
                UndraftPawn(pawn);
            }
        }

        private bool ShouldSkipPawn(Pawn pawn)
        {
            if (!pawn.Spawned || pawn.drafter == null || pawn.Drafted) return true;
            if (pawn.InMentalState || pawn.Downed) return true;
            if (pawn.CurJobDef == JobDefOf.LayDown) return true; // Dormindo
            if (pawn.CurJobDef == JobDefOf.TendPatient) return true; // Cuidando de outro
            if (pawn.CurJobDef == JobDefOf.Rescue) return true; // Resgatando
            if (pawn.CurJobDef == JobDefOf.FleeAndCower) return true; // Fugindo
            if (pawn.health.HasHediffsNeedingTend()) return true; // Precisa de tratamento
            if (pawn.CurJobDef == JobDefOf.Capture) return true; // Capturando prisioneiro
            // Verifica se o colono está fugindo devido a uma ameaça (FleeAndCower)
            if (pawn.CurJobDef == JobDefOf.FleeAndCower || pawn.CurJobDef == JobDefOf.Flee) return true;
            if (pawn.CurJobDef == JobDefOf.DoBill) return true; //Operando Alguém
            // Não está no horário de recreação
            if (pawn.timetable?.CurrentAssignment != TimeAssignmentDefOf.Joy) return true;
            if (pawn.CurJob?.targetA.Thing is Pawn carried && carried.DevelopmentalStage.Baby())
            {
                return true; // Carregando um bebê
            }

            return false;
        }

        private void DraftPawn(Pawn pawn)
        {
            if (!RecreationDraftSettingsHelper.DraftingEnabled) return;

            pawn.drafter.Drafted = true;
            draftedPawns[pawn] = Find.TickManager.TicksGame + RecreationDraftSettingsHelper.GetDraftDurationTicks();

            if (RecreationDraftSettingsHelper.NotificationsEnabled)
            {
                Messages.Message($"[Recreation Time] {pawn.NameShortColored} Recalculating Tasks.",
                    pawn, MessageTypeDefOf.NeutralEvent);
            }
        }

        private void UndraftPawn(Pawn pawn)
        {
            if (pawn?.drafter != null && pawn.Drafted)
            {
                pawn.drafter.Drafted = false;
                draftedPawns.Remove(pawn);

                if (RecreationDraftSettingsHelper.NotificationsEnabled)
                {
                    Messages.Message($"[Recreation Time] {pawn.NameShortColored} Delisted.",
                        pawn, MessageTypeDefOf.NeutralEvent);
                }
            }
        }
    }
}
