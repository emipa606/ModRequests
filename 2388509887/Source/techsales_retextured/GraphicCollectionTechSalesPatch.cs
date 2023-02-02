using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TechSales;
using UnityEngine;
using Verse;

namespace techsales_retextured
{
    public class DoHarmony: Mod
    {
        public static ModContentPack CONTENT;

        public DoHarmony(ModContentPack content): base(content)
        {
            var harmony = new Harmony("im.skye.rimworld.techSalesRe");
            harmony.PatchAll();
            CONTENT = content;
        }
    }
    
    [HarmonyPatch(typeof(Graphic_Collection_TechSales), "Init")]
    public static class GraphicCollectionTechSales_Init_Patch
    {
        /// Hijack the graphics collection for techsales to inject our own graphics
        /// aka "haha postfix go brrrr"
        static void Postfix(Graphic_Collection_TechSales __instance, GraphicRequest req)
        {
            // Getting all our textures
            var list = (from x in DoHarmony.CONTENT.GetContentHolder<Texture2D>()
                        .GetAllUnderPath("Things/Books")
                    where !x.name.EndsWith(Graphic_Single.MaskSuffix)
                    orderby x.name
                    select x).ToList();
            // Using Traverse & reflection to get the protected fields
            var traverse = new Traverse(__instance);
            var subGraphics = traverse.Field<Graphic[]>("subGraphics").Value;

            for (int i = 0; i < list.Count; i++)
            {
                string subpath = Path.Combine("Things/Books/", list[i].name);
                subGraphics[i] = GraphicDatabase.Get(
                    typeof(Graphic_Single),
                    subpath,
                    req.shader,
                    __instance.drawSize,
                    __instance.color,
                    __instance.colorTwo,
                    null,
                    req.shaderParameters
                );
            }

            var mats = traverse.Field<List<Material>>("mats").Value;
            
            mats.Clear();
            
            MaterialRequest req2 = default;
            req2.shader = req.shader;
            req2.color = __instance.color;
            req2.colorTwo = __instance.colorTwo;
            req2.renderQueue = req.renderQueue;
            req2.shaderParameters = req.shaderParameters;

            req2.mainTex = list[0];
            mats.Add(MaterialPool.MatFrom(req2));
            req2.mainTex = list[1];
            mats.Add(MaterialPool.MatFrom(req2));
            req2.mainTex = list[2];
            mats.Add(MaterialPool.MatFrom(req2));
            req2.mainTex = list[3];
            mats.Add(MaterialPool.MatFrom(req2));
            req2.mainTex = list[4];
            mats.Add(MaterialPool.MatFrom(req2));
            req2.mainTex = list[5];
            mats.Add(MaterialPool.MatFrom(req2));
            req2.mainTex = list[6];
            mats.Add(MaterialPool.MatFrom(req2));
            req2.mainTex = list[7];
            mats.Add(MaterialPool.MatFrom(req2));

            req2.mainTex = list[2];

            traverse.Field<Material>("mat").Value = MaterialPool.MatFrom(req2);
            traverse.Field<Graphic>("graphic").Value = subGraphics[2];
        }
    }
}