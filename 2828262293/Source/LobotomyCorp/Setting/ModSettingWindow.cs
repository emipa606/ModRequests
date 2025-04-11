using System;

using UnityEngine;
using Verse;
using LobotomyCorp.Tool;

namespace LobotomyCorp.Setting
{
    public class ModSettingWindow : Mod
    {
        public ModSettingWindow(ModContentPack content) : base(content)
        {
            LCModSetting.Instance = base.GetSettings<LCModSetting>();
        }

        public override string SettingsCategory()
        {
            return "LobotomyCorp.";
        }


        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);

            Rect leftRect = inRect;
            leftRect.width = inRect.width / 3 - 5f;
            leftRect.height = Text.LineHeight;
            LeftContent(leftRect);
            Widgets.DrawLineVertical(leftRect.width + 5f, 40f, inRect.height - 50f);

            Rect middleRect = leftRect;
            middleRect.x += leftRect.width + 10f;
            middleRect.width = inRect.width * 4 / 9 - 10f;
            //MiddleContent(middleRect);
            Widgets.DrawLineVertical(middleRect.x + middleRect.width - 10f, 40f, inRect.height - 50f);

            Rect rightRect = middleRect;
            rightRect.x += middleRect.width + 5f;
            rightRect.width = inRect.width - leftRect.width - middleRect.width - 15f;
            //RightContent(rightRect);
        }

        private void LeftContent(Rect rect)
        {
            Rect cRect = rect;
            cRect.height = 25f;

            CheckWorker(ref LCModSetting.isLCDamageAvailable,
                () => WindowTool.StringCheckBox(cRect, Cache.isLCDamageAvailable,
                        ref LCModSetting.isLCDamageAvailable),
                Cache.setLCDamage
            );

            cRect.y += cRect.height;
            WindowTool.StringCheckBox(cRect, Cache.canDamageColonist,
                ref LCModSetting.canDamageColonist);

            cRect.y += cRect.height;
            WindowTool.StringCheckBox(cRect, Cache.canDamageSameThingsInDifferentCell
                , ref LCModSetting.canDamageSameThingsInDifferentCell);

            /*
            if (Widgets.ButtonText(cRect, "list"))
            {
                StringBuilder sb = new StringBuilder("weapon : ");
                foreach(ThingDef td in Tool.Cache.LCWeaponList)
                {
                    sb.Append(td.label + ",");
                }
                sb.Append("\n\nbullet : ");
                foreach (ThingDef td in Tool.Cache.LCBulletList)
                {
                    sb.Append(td.label + ",");
                }
                Log.Message(sb.ToString());
            }
            */

        }



        private void CheckWorker<T>(ref T check, Action act, Action worker)
        {
            T before = check;
            act?.Invoke();
            if (!before.Equals(check)) worker?.Invoke();
        }

    }
}
