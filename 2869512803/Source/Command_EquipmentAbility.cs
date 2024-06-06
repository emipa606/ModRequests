using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VFECore.Abilities
{
    public class Command_EquipmentAbility : Command_Ability
    {
        public Command_EquipmentAbility(EquipmentAbility ability) : base(ability)
        {
        }

        public int curTicks = -1;
        public new EquipmentAbility ability => (EquipmentAbility)base.ability;
    }
}