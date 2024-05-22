using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using RimWorld;

namespace TakeOffIndoor
{
    public class UpdateNotice
    {
        public static void Notice(int nowVer,int lastVer)
        {
            string ttl = "Take off coat indoors" + "  " + "LatestUpdates".TranslateW("Latest updates.");
            if (lastVer == 0)
            {
                ttl = "Take off coat indoors" + "  " + "ChangeLog".TranslateW("Change log.");
            }
            if (nowVer > lastVer)
            {
                string dialogText = "";
                if (lastVer < 20221205) dialogText += Up20221205();
                if (lastVer < 20221129) dialogText += Up20221129();
                if (lastVer < 20221112) dialogText += Up20221112();
                if (lastVer < 20220420) dialogText += Up20220420();
                if (lastVer < 20211028) dialogText += Up20211028();
                if (lastVer < 20210803) dialogText += Up20210803();
                if (lastVer < 20210715) dialogText += Up20210715();
                if (lastVer < 20210517) dialogText += Up20210517();
                if (lastVer < 20210515) dialogText += Up20210515();
                if (lastVer < 20210508) dialogText += Up20210508();
                if (lastVer < 20210417) dialogText += Up20210417();
                if (lastVer < 20210415) dialogText += Upbef20210415();

                Find.WindowStack.Add(new Dialog_MessageBox(dialogText, title:ttl));
            }
        }

        private static string Up20221205()
        {
            string ret = "<b>ver20221205</b>\n";

            ret += "TOC.Up20221205".TranslateW("-Fixed a bug that the debug window cannot be opened when HAR is not installed.\n- Fixed indivisually visible settings window not working properly.\n\n");
            return ret;
        }
        private static string Up20221129()
        {
            string ret = "<b>ver20221129</b>\n";

            ret += "TOC.Up20221129".TranslateW("-Correspond to specification change of HAR. \n Re - implemented the function to patch the HAR and correct the display state of the additional body parts.\n Fixed wrong name of HAR support setting(only if Languages is not included)\n"
                +"-Added preset function.\n After creating a preset first, it is possible to automatically set it with a filter.\n Also added a function to manually select presets in the pawn tab, and made it possible to modify some of the common settings from the tab window.\n"
                +"- Fixed the apparel indivisually visible settings.You can now modify layers in-game without using Def.\n In addition, the forced visible function has also been improved.\n If other mods implement unexpected layers, please use it.\n"
                +"-Fixed an issue that was conflicting with Dubs Bad Hygiene's shower etc. when using portrait features.\n Since it only fix Dubs Bad Hygiene, conflicts with MOD that have the same function.\n If conflicts occur, turn off the portrait features.\n\n");
            return ret;
        }
        private static string Up20221112()
        {
            string ret = "<b>ver20221112</b>\n";

            ret += "TOC.Up20221112".TranslateW("Update for Rimworld 1.4.\nUpdate for HAR specification changes. Maybe fixes an issue where HAR races weren't displaying properly.\n\n");
            return ret;
        }

        private static string Up20220420()
        {
            string ret = "<b>ver20220420</b>\n";

            ret += "TOC.Up20220420".TranslateW("Fixed an issue that was conflicting with \"Vanilla Factions Expanded - Pirates\".\nI didn't supposed clothes it had no BodyPartGroups, so I decided to treat them as \"Torso\".\nIf you have any problems, please contact me.\n\n");
            return ret;
        }
        private static string Up20211028()
        {
            string ret = "<b>ver20211028</b>\n";

            ret += "TOC.Up20211028".TranslateW("-Added the ability to select the sleeping layer in bed. (For support of some mods)\n-Fixed an issue where the option layout was broken.\n-Added some indentation to the options.\n- Changed the position of the \"Check Private Room Interval\" option and changed it to select with a radio button.\n-Added an option to recognize the barracks as private room.\n\n");
            return ret;
        }
        private static string Up20210803()
        {
            string ret = "<b>ver20210803</b>\n";

            ret += "TOC.Up20210803".TranslateW("-Fixed the process in 1.3. Improved that changing the draft status while posing is not immediately reflected.\n"
                + "-Added the function to forcibly show or hide each apparels. If you want to use it, check the option \"Use apparel indivisually visible settings\" and show the setting window.\n"
                + "-Added the function to disable personal settings. \n\n");
            return ret;
        }

        private static string Up20210715()
        {
            string ret = "<b>ver20210715</b>\n";

            ret += "TOC.Up20210715".TranslateW("-Added support for Ver1.3.\n"
                +"-Since the drawing process has changed in Ver1.3, Legacy mode has been deleted. (Only when using Ver1.3)\n"
                +"-I will deal with conflicting mods after the update is stable to some extent. Also, if you know the competing mods, I would appreciate it if you could provide information.  \n\n");
            return ret;
        }
        private static string Up20210517()
        {
            string ret = "<b>ver20210517</b>\n";

            ret += "TOC.Up20210517".TranslateW("-Fixed so that the processing order is the same as before ver20210415.\n" +
                    "And moved the processing of ver20210515 to Legacy3. \n\n");
            return ret;
        }


        private static string Up20210515()
        {
            string ret = "<b>ver20210515</b>\n";

            ret += "TOC.Up20210515".TranslateW("-Fixed functions that may be causing conflicts.\n" +
                    "Removed the layer update interval setting.\n\n" +
                    "-Considering the possibility of conflict, added an option to enable / disable the function of patching HAR. (Reboot required, default off)\n" +
                    "If you are worried about the problem that HAR races ears are not displayed properly when wearing a helmet, turn it on.\n"+
                    "-Added the function to display the change log in the game. \n\n");
            return ret;
        }

        private static string Up20210508()
        {
            string ret = "<b>ver20210508</b>\n";
            ret += "TOC.Up20210508".TranslateW("-Fixed functions that may be causing conflicts. \n\n");

            return ret;
        }

        private static string Up20210417()
        {
            string ret = "<b>ver20210417</b>\n";
            ret += "TOC.Up20210417".TranslateW("-Added legacy mode.\n" +
                "If you run into problems, please try changing the ResolveGraphicsMode option.\n\n");

            return ret;
        }
        private static string Upbef20210415()
        {
            string ret = "<b>ver20210415</b>\n";
            ret += "TOC.Up20210415".TranslateW("-Fixed:Save data size becomes smaller.\n" +
                "I am trying to take over the settings in the old version, but there may be cases where the settings are initialized.\n"+
                "-Fixed:Exclude compClass and ITab from Animals and Mechanoids.\n"+
                "-When using the temperature setting, once the temperature setting is applied, it will not be canceled unless it exceeds ± 2 ° C from the set value.\n"+
                "This prevents flicker when the ambient temperature is not stable near the set temperature.\n"+
                "-Fixed:Portraits may not be displayed in the latest Garam Race Addon.\n"+
                "-Fixed:If HAR races with their own ears(such as Ratkin and Rabbie) would not see their ears when wearing a helmet.\n"+
                "*HAR does not support the \"Hats shown only on game map option\" in vanilla(ears are not displayed properly), "+
                "so if you do not want to display hats on portraits, please use the settings of this mod.\n"+
                "-Fixed the hot setting to apply only when the layer has a lower priority(apply light clothing) "+
                "and the cold setting to apply only when the layer has a higher priority(apply thick clothing)."+
                "ex.In the case of Indoors:Underwear Hot:Middle, if the temperature conditions are satisfied, "+
                "the Middle will be applied outdoors, but Underwear will be applied indoors. (Middle is applicable indoors with the old specifications)\n"+
                "-Added a setting to prevent getting naked. If it will be naked, show an equipment in the lowest priority layer. (priority to Torso)\n"+
                "-Fixed:Conflict with JetPack. \n\n");

            ret += "<b>ver20210319</b>\n";
            ret += "TOC.Up20210319".TranslateW("-Due to layer setting of Kurin HAR Edition was not supported by default, the layer setting was added.\n" +
                    "-Corrected a misspelling.(Midlle->Middle)\n\n");


            ret += "<b>ver20200220</b>\n";
            ret += "TOC.Up20210220".TranslateW("-Fixed an error that was occurring in Misc. Robots.\n\n");

            ret += "<b>ver20201123</b>\n";
            ret += "TOC.Up20201123".TranslateW("-Added the function to specify the layer for each type of clothes.\n" +
                "If you want to specify a layer, add it to CustomLayerDictionary.xml in Defs.\n"+
                "OnHead->Headgear, StrippedHead->Headgear, MiddleHead->Headgear are listed as examples.\n\n");

            ret += "<b>ver20201007</b>\n";
            ret += "Up20201007".TranslateW("-Fixed: Conflict with Faction Colors.\n\n");

            ret += "<b>ver20200909</b>\n";
            ret += "TOC.Up2020909".TranslateW("-Fixed: Hat might not be hidden.\n" +
                "-Changed the process that caused conflicts with some mods.\n\n");

            ret += "<b>ver20200906</b>\n";
            ret += "TOC.Up2020906".TranslateW("-Added the function to change the display layer to the ambient temperature.\n" +
                "-Added the function to deal hats in a specific layer.\n\n");

            ret += "<b>ver20200901</b>\n";
            ret+= "TOC.Up20200901".TranslateW("-The behavior when not introducing Appearance clothes, was corrected.\n" +
                "Speed and conflicts with some MOD are improved.\n\n");

            ret += "<b>ver20200828</b>\n";
            ret += "TOC.Up20200828".TranslateW("-Fixed: Error occurs when mech appearance.\n\n");

            ret += "<b>ver20200825</b>\n";
            ret += "TOC.Up20200825".TranslateW("-Added the function to don't show hats indoors.\n" +
                "-Appearance clothes was made unnecessary.\n\n");


            return ret;
        }

    }
}
