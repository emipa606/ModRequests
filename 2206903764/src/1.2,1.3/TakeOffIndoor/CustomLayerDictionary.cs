using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TakeOffIndoor
{
    public struct DefsLayerPair
    {
        public string Key;
        public LayerPriority Value;
        public DefsLayerPair(string str, LayerPriority lp)
        {
            Key = str;
            Value = lp;
        }
        public DefsLayerPair(string str)
        {
            var tmp = str.Split(',');
            if (tmp.Length == 2)
            {
                try
                {
                    Key = tmp[0];
                    //debug.WriteLine("1");
                    if(Enum.TryParse<LayerPriority>(tmp[1],out LayerPriority lp))
                    {
                        //debug.WriteLine("2");
                        Value = lp;
                        return;
                    }
                    else
                    {
                        //debug.WriteLine("3");
                        Value = LayerPriority.Null;
                    }
                }
                catch //下でエラー表示するので潰す
                {
                }
            }
            Key = str;
            Value = LayerPriority.Null;
            ParseHelper.Parsers<string>.Register(new Func<string, string>(ParseHelper.ParseString));
                Log.Warning("TakeOffIndoor XML error:" + str);
        }
    }
    public class CustomLayerDictionary : Def
    {
        public List<string> DefsList;
        public static Dictionary<string, LayerPriority> DefsDic=new Dictionary<string, LayerPriority>();

        public void MakeDictionary()
        {
            foreach(string tmp in DefsList)
            {
                //debug.WriteLine(tmp);
                DefsLayerPair tmps = new DefsLayerPair(tmp);
                //debug.WriteLine("  key:"+tmps.Key+" val:"+tmps.Value);
                if (DefsDic.ContainsKey(tmps.Key))
                {
                    if (!TakeOffSettings.dontShowDuplicateWaring)
                    {
                        Log.Warning("Dupulicate setting of " + tmps);
                    }
                }
                else
                {
                    DefsDic.Add(tmps.Key, tmps.Value);
                }
            }
        }
    }
}
