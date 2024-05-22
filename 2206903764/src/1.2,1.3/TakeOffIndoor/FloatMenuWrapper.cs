using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using System.Text.RegularExpressions;

namespace FloatMenuWrapper
{
    public class FloatMenuW //インスタンスは必ず静的に呼び出し
    {
        //static bool once = false;
        List<KeyValuePair<string, int>> tmplist=new List<KeyValuePair<string, int>>();
        string tmpstring ="";
        //static int tmpint = -1;
        Action<int> action;
        object tmpobject;

        public string LastString
        {
            get
            {
                return tmpstring;
            }
        }

        public void SetList(List<KeyValuePair<string,int>> list)
        {
            tmplist = list;
        }
        public void ResetList()
        {
            tmplist = new List<KeyValuePair<string, int>>();
        }
        public void AddMenu(string label,int num)
        {
            tmplist.Add(new KeyValuePair<string, int>(label, num));
        }
        public void SetObject(object obj)
        {
            tmpobject = obj;
        }
        public object GetObject
        {
            get
            {
                return tmpobject;
            }
        }

        public void SetAction(Action<int> _action)
        {
            action = _action;
        }

        public void Show()
        {
            Func<KeyValuePair<string, int>, string> labelFunc = (label) => { return label.Key; };

            FloatMenuUtility.MakeMenu<KeyValuePair<string, int>>(tmplist, labelFunc, (KeyValuePair<string, int> lb) => delegate { action(lb.Value); });

        }
        //(KeyValuePair<string, int> lb) => delegate { action(lb.Value); }

        //public static bool TryGetMenuReturn(out int num)
        //{
        //    if (once)
        //    {
        //        once = false;
        //        num = tmpint;
        //        return true;
        //    }
        //    num = -1;
        //    return false;
        //}



    }
}
