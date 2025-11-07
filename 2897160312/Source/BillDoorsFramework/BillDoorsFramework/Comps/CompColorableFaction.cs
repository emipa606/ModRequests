using RimWorld;
using UnityEngine;
using Verse;

namespace BillDoorsFramework
{
    public class CompColorableFaction : ThingComp
    {
        public CompProperties_ColorableFaction Props
        {
            get
            {
                return (CompProperties_ColorableFaction)props;
            }
        }

        public Color colorCache;

        private bool colorGetted;

        protected virtual Faction GetFaction()
        {
            Faction faction = parent.Faction;
            if (parent is Projectile projectile)
            {
                faction = projectile.Launcher?.Faction;
            }
            return faction;
        }

        public Color FactionColor()
        {
            if (!BDPMod.enableProjectileColoring)
            {
                return parent.Graphic.Color;
            }
            if (Props.discoLightMode || BDPMod.discoLightMode)
            {
                if (!colorGetted)
                {
                    colorCache = Color.HSVToRGB(Rand.Value, Rand.Range(8, 10) / 10f, Rand.Range(8, 10) / 10f);
                    colorGetted = true;
                }
                return colorCache;
            }
            else
            {
                Faction faction = GetFaction();
                if (faction != null)
                {
                    if (BDPMod.useFactionColor)
                    {
                        return (faction.Color);
                    }
                    else
                    {
                        if (faction == Faction.OfPlayer)
                        {
                            return BDPMod.CustomProjectileColor ? BDPMod.CustomPlayerProjectileColor : (Props.colorPlayer);
                        }
                        if (faction == Faction.OfPirates)
                        {
                            return BDPMod.CustomProjectileColor ? BDPMod.CustomPirateProjectileColor : (Props.colorPirate);
                        }
                        if (faction == Faction.OfEmpire)
                        {
                            return BDPMod.CustomProjectileColor ? BDPMod.CustomEmpireProjectileColor : (Props.colorEmpire);
                        }
                        if (faction == Faction.OfMechanoids)
                        {
                            return BDPMod.CustomProjectileColor ? BDPMod.CustomMechanoidProjectileColor : (Props.colorMechanoid);
                        }
                        if (faction.HostileTo(Faction.OfPlayer))
                        {
                            return BDPMod.CustomProjectileColor ? BDPMod.CustomHostilesProjectileColor : (Props.colorHostile);
                        }
                        if (faction.AllyOrNeutralTo(Faction.OfPlayer))
                        {
                            return BDPMod.CustomProjectileColor ? BDPMod.CustomOthersProjectileColor : (Props.colorNeutualOrAlly);
                        }
                    }
                }
            }
            return Color.white;
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if ((parent.def.graphic.color == Color.white || Props.overrideExistingColoring) && parent is Projectile projectile)
            {
                Vector3 drawPos = projectile.DrawPos;
                Material material = projectile.def.DrawMatSingle;
                material.color = FactionColor();
                Graphics.DrawMesh(MeshPool.GridPlane(projectile.def.graphicData.drawSize), drawPos, projectile.ExactRotation, material, 0);
            }
        }
    }

    public class CompProperties_ColorableFaction : CompProperties
    {
        public Color colorPlayer = Color.cyan;

        public Color colorPirate = Color.yellow;

        public Color colorEmpire = Color.green;

        public Color colorHostile = Color.red;

        public Color colorNeutualOrAlly = Color.cyan;

        public Color colorMechanoid = Color.magenta;

        public bool overrideExistingColoring = false;

        public bool discoLightMode = false;

        public CompProperties_ColorableFaction()
        {
            compClass = typeof(CompColorableFaction);
        }
    }
}
