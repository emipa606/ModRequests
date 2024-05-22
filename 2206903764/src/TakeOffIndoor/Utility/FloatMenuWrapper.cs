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
        object tmpparam;

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

        public void SetParam(object value)
        {
            tmpparam = value;
        }
        public T GetParam<T>()
        {
            return (T)tmpparam;
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

        public List<string> Keys
        {
            get
            {
                List<string> ret = new List<string>();
                foreach(KeyValuePair<string,int> tmp in tmplist)
                {
                    ret.Add(tmp.Key);
                }
                return ret;
            }
        }

        public List<int> Items
        {
            get
            {
                List<int> ret = new List<int>();
                foreach (KeyValuePair<string, int> tmp in tmplist)
                {
                    ret.Add(tmp.Value);
                }
                return ret;
            }
        }


    }
}
