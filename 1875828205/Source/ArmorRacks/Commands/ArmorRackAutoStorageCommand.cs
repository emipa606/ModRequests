using System;
using Verse;
using ArmorRacks.Things;
using UnityEngine;

namespace ArmorRacks.Commands
{
    public class ArmorRackAutoStorageCommand: Command_Toggle
    {
        public ArmorRack ArmorRack;
        
        public ArmorRackAutoStorageCommand(ArmorRack armorRack)
        {
            ArmorRack = armorRack;
            icon = ContentFinder<Texture2D>.Get(armorRack.def.graphicData.texPath + "_south", false);
            defaultIconColor = armorRack.Stuff.stuffProps.color;
            defaultLabel = "ArmorRacks_AutoStorageCommand_Label".Translate();
            defaultDesc = "ArmorRacks_AutoStorageCommand_Desc".Translate();
            toggleAction = delegate { ArmorRack.AutoSetStorageOnTransfer = !ArmorRack.AutoSetStorageOnTransfer; };
            isActive = () => ArmorRack.AutoSetStorageOnTransfer;
        }
    }
}