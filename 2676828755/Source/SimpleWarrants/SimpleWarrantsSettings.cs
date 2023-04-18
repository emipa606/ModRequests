using UnityEngine;
using Verse;

namespace SimpleWarrants
{
    public class SimpleWarrantsSettings : ModSettings
    {
        [Header("SW.General")]
        [Label("SW.EnableWarrantsOnAnimals")]
        public bool enableWarrantsOnAnimals = true;

        [Label("SW.EnableTamingWarrants")]
        public bool enableTamingWarrants = true;

        [Label("SW.EnableWarrantsOnArtifact")]
        public bool enableWarrantsOnArtifact = true;

        [Label("SW.EnableWarrantsOnColonists")]
        public bool enableWarrantsOnColonists = true;

        [Label("SW.ChanceOfWarrantsMadeOnColonist")]
        [DrawIf(nameof(enableWarrantsOnColonists))]
        [Percentage]
        public float chanceOfWarrantsMadeOnColonist = 0.05f;

        [Label("SW.FailedPlayerWarrantRelationshipDamage")]
        [Range(0, 100)]
        public int failedPlayerWarrantRelationshipDamage = 30;

        [Label("SW.FailedAIWarrantRelationshipDamage")]
        [Range(0, 100)]
        public int failedAIWarrantRelationshipDamage = 20;

        [Label("SW.WarrantGenMTB")]
        [Range(0.5f, 30f)]
        public float warrantGenMTB = 7f;

        [Header("SW.Rewards")]
        [Label("SW.WarrantRewardMax")]
        [Percentage]
        public float warrantRewardMax = 0.08f;

        [Label("SW.warrantRewardScaling")]
        public bool warrantRewardScaling = true;

        [Header("SW.Raids")]

        [Label("SW.BountyHunterRaidScale")]
        [Range(0.25f, 2f)]
        [Percentage]
        public float bountyHunterRaidScale = 1f;

        [Label("SW.BountyRaidGenMTB")]
        [Range(1f, 30f)]
        public float bountyHunterMTB = 5f;

        public override void ExposeData()
        {
            base.ExposeData();
            SimpleSettings.AutoExpose(this);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            SimpleSettings.DrawWindow(this, inRect);
        }
    }
}
