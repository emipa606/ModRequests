using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TakeOffIndoor
{
    public class DefsCustomData
    {
        public string DefName;
        public LayerPriority Layer;
        public VisibleMode Visibly;
        public ThingDef Thing;
        public DefsCustomData(string str, LayerPriority lp,VisibleMode visibly=VisibleMode.None)
        {
            DefName = str;
            Layer = lp;
            Visibly = visibly;
        }
        public DefsCustomData(ThingDef thing, LayerPriority lp, VisibleMode visibly = VisibleMode.None)
        {
            Thing = thing;
            DefName = thing.defName;
            Layer = lp;
            Visibly = visibly;
        }
        public DefsCustomData(string str)
        {
            var tmp = str.Split(',');
            //debug.WriteLine(str);
            if (tmp.Length >= 2)
            {
                try
                {
                    DefName = tmp[0];
                    //debug.WriteLine("1");
                    if(Enum.TryParse<LayerPriority>(tmp[1],out LayerPriority lp))
                    {
                        //debug.WriteLine("2");
                        Layer = lp;
                        if (tmp.Length == 3 && Enum.TryParse<VisibleMode>(tmp[2], out VisibleMode vm))
                        {
                            Visibly = vm;
                        }
                        else
                        {
                            Visibly = VisibleMode.None;
                        }
                        return;
                    }
                    else
                    {
                        //debug.WriteLine("3");
                        Layer = LayerPriority.Null;
                        Visibly = VisibleMode.None;
                    }
                }
                catch //下でエラー表示するので潰す
                {
                }
            }
            DefName = str;
            Layer = LayerPriority.Null;
            Visibly = VisibleMode.None;
            ParseHelper.Parsers<string>.Register(new Func<string, string>(ParseHelper.ParseString));
                Log.Warning("TakeOffIndoor XML error:" + str);
        }

        public string ToSaveString
        {
            get
            {
                return DefName + "," + Layer.ToString() + "," + Visibly.ToString();
            }
        }
    }
    public class CustomLayerDictionary : Def , IExposable
    {
        public List<string> DefsList;
        public static Dictionary<string, LayerPriority> DefsDic=new Dictionary<string, LayerPriority>();

        public void MakeDictionary()
        {
            foreach(string tmp in DefsList)
            {
                //debug.WriteLine(tmp);
                DefsCustomData tmps = new DefsCustomData(tmp);
                //debug.WriteLine("  key:"+tmps.Key+" val:"+tmps.Value);
                if (DefsDic.ContainsKey(tmps.DefName))
                {
                    if (!TakeOffSettings.dontShowDuplicateWaring)
                    {
                        Log.Warning("Dupulicate setting of " + tmps);
                    }
                }
                else
                {
                    DefsDic.Add(tmps.DefName, tmps.Layer);
                }
            }
        }
        
        public void ExposeData()
        {
        }
    }

    public class CustomApparelLayer : Def
    {
        public static Dictionary<string, LayerPriority> defsDic = new Dictionary<string, LayerPriority>();
        public string DefName;
        public LayerPriority Layer;
    }

    //衣服個別用
    public class CustomApparelLayerDictionary : IExposable
    {
        public List<string> DefsList = new List<string>();

        public Dictionary<string, DefsCustomData> DefsDicString = new Dictionary<string, DefsCustomData>(); //保存用 存在しないものは表示はしないが保存はする
        public static Dictionary<ThingDef, LayerPriority> DefsLayerDic = new Dictionary<ThingDef, LayerPriority>();
        public static Dictionary<ThingDef, VisibleMode> DefsModeDic = new Dictionary<ThingDef, VisibleMode>();

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                foreach (KeyValuePair<ThingDef,LayerPriority> defLayer in DefsLayerDic)
                {
                    LayerPriority lpDefault = TakeOffComp.CalcLayerPriorityDefault(defLayer.Key.apparel.LastLayer);
                    if (defLayer.Value == lpDefault)
                    {
                        if (DefsDicString.TryGetValue(defLayer.Key.defName,out DefsCustomData defsCustom)) //ここで一旦削除
                        {
                            if (defsCustom.Visibly == VisibleMode.None)
                            {
                                DefsDicString.Remove(defLayer.Key.defName);
                            }
                        }
                    }
                    else
                    {
                        if (DefsDicString.ContainsKey(defLayer.Key.defName))
                        {
                            DefsDicString[defLayer.Key.defName].Layer = defLayer.Value;
                        }
                        else
                        {
                            DefsDicString.Add(defLayer.Key.defName, new DefsCustomData(defLayer.Key, defLayer.Value));
                        }
                    }
                }
                foreach (KeyValuePair<ThingDef, VisibleMode> defMode in DefsModeDic.Where(x=>x.Value != VisibleMode.None)) //noneは除外する
                {
                    if (DefsDicString.ContainsKey(defMode.Key.defName))
                    {
                        DefsDicString[defMode.Key.defName].Visibly=defMode.Value;
                    }
                    else
                    {
                        DefsDicString.Add(defMode.Key.defName, new DefsCustomData(defMode.Key, LayerPriority.Null , defMode.Value));
                    }
                }

                DefsList = new List<string>();

                foreach(DefsCustomData defCustom in DefsDicString.Values) //ここで削除しようとするとforループになって面倒なのでやらない
                {
                    DefsList.Add(defCustom.ToSaveString);
                }
            }

            Scribe_Collections.Look(ref DefsList, "DefsList", LookMode.Value,null);

            if (Scribe.mode == LoadSaveMode.PostLoadInit) //この段階で処理しようとするとDefが無くてエラーが出る
            {
                if (DefsList == null)
                {
                    DefsList = new List<string>();
                }
                else
                {
                    foreach (string str in DefsList)
                    {
                        DefsCustomData defCustom = new DefsCustomData(str);
                        DefsDicString[defCustom.DefName] = defCustom;
                    }
                }
            }
        }
        public void MakeDefsDic() //これをstatic constructorから呼び出し
        {
            foreach (CustomApparelLayer customLayer in DefDatabase<CustomApparelLayer>.AllDefsListForReading)
            {
                if (customLayer.Layer != LayerPriority.Null)
                {
                    CustomApparelLayer.defsDic[customLayer.defName] = customLayer.Layer;
                    ThingDef def = DefDatabase<ThingDef>.GetNamed(customLayer.DefName, false);
                    if (def != null)
                    {
                        DefsCustomData defCustom = new DefsCustomData(def, customLayer.Layer);
                        if (defCustom.Layer != LayerPriority.Null)
                        {
                            DefsLayerDic[def] = defCustom.Layer;
                        }
                    }

                }

            }

            foreach (DefsCustomData defCustom in DefsDicString.Values)
            {
                ThingDef def = DefDatabase<ThingDef>.GetNamed(defCustom.DefName, false);
                if (def != null)
                {
                    defCustom.Thing = def;
                    if (defCustom.Layer != LayerPriority.Null)
                    {
                        DefsLayerDic[def]= defCustom.Layer;
                    }
                    if (defCustom.Visibly != VisibleMode.None)
                    {
                        DefsModeDic[def] = defCustom.Visibly;
                    }
                }
            }
        }
        public static VisibleModeSingle GetMode(ThingDef apparel,bool drafted)
        {
            if(DefsModeDic.TryGetValue(apparel,out VisibleMode vm))
            {
                if (drafted)
                {
                    return vm.GetDrafted();
                }
                else
                {
                    return vm.GetNormal();
                }
            }
            else
            {
                return VisibleModeSingle.None;
            }
        }

        public static void SetVisible(ThingDef apparel,VisibleModeSingle vms,bool drafted)
        {
            if(DefsModeDic.TryGetValue(apparel,out VisibleMode vm))
            {
                DefsModeDic[apparel] = vm.Change(vms, drafted);
            }
            else
            {
                DefsModeDic[apparel] = VisibleMode.None.Change(vms, drafted);
            }
        }
        public static void SetVisible(ThingDef apparel, VisibleMode vm)
        {
            DefsModeDic[apparel] = vm;
        }
    }
}
