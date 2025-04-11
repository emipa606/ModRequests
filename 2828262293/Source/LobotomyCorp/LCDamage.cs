using System.Collections.Generic;

using Verse;


namespace LobotomyCorp
{

    public class LCDamage : DefModExtension
    {
        public List<Damage> convert;
        public LCDamage()
        {
            convert = new List<Damage>();
        }

        public void Change(ref ThingDef td)
        {
            foreach(Damage d in convert)
            {
                Change(ref td, d);
            }
        }

        private void Change(ref ThingDef td, Damage d)
        {
            bool isUse = Setting.LCModSetting.isLCDamageAvailable;

            if (d.type == WeaponType.Tool)
            {
                if(d.id == -1 || td.tools.Count <= d.id)
                {
                    Log.Error("Miss: " + td.defName);
                    return;
                }
                td.tools[d.id].capacities = isUse ? d.LCCapacities : d.noLCCapacities;
                //Log.Message(td.defName + " -> " + td.tools[d.id].capacities.FirstOrFallback().defName);
            }
            else if(d.type == WeaponType.Projectile)
            {
                if(d.noLCDamage == null || d.LCDamage == null)
                {
                    Log.Error("there is no projectile:" + td.defName);
                    return;
                }
                td.projectile.damageDef = isUse ? d.LCDamage : d.noLCDamage;
                //Log.Message(td.defName + " -> " + td.projectile.damageDef.defName);
            }


        }


    }

    public class Damage
    {
        public WeaponType type = WeaponType.Tool;
        public int id = -1;
        public List<ToolCapacityDef> LCCapacities = null;
        public List<ToolCapacityDef> noLCCapacities = null;

        public DamageDef LCDamage = null;
        public DamageDef noLCDamage = null;
    }

    public enum WeaponType
    {
        Projectile,
        Tool,
    }

}
