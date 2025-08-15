using System.Collections;
using System.Collections.Generic;
using Verse;

namespace StylishRim
{

    public class ApparelDict : IExposable, IEnumerable
    {
        List<string> keys;
        List<StylishSet> values;
        public ApparelDict()
        {
            keys = new List<string>();
            values = new List<StylishSet>();
        }
        public ApparelDict(string modName)
        {
            keys = new List<string> { StylishSet.BODY_COMMON };
            values = new List<StylishSet> { new StylishSet(modName, StylishSet.BODY_COMMON, StylishSet.BODY_COMMON) };
        }
        public ApparelDict(string key, StylishSet value)
        {
            keys = new List<string> { key };
            values = new List<StylishSet> { value };
        }

        public StylishSet this[string key]
        {
            get
            {
                return values[keys.IndexOf(key)];
            }
            set
            {
                values[keys.IndexOf(key)] = value;
            }
        }
        public StylishSet GetCommon
        {
            get
            {
                if (keys.Count == 0) return new StylishSet();
                if (!keys.Contains(StylishSet.BODY_COMMON))
                {
                    return new StylishSet(string.Empty, StylishSet.BODY_COMMON, StylishSet.BODY_COMMON);
                }
                return values[keys.IndexOf(StylishSet.BODY_COMMON)];
            }
        }
        public Dictionary<string, StylishSet> Dict
        {
            get
            {
                Dictionary<string, StylishSet> d = new Dictionary<string, StylishSet>();
                for (int i = 0; i < keys.Count; i++)
                {
                    d.Add(keys[i], values[i]);
                }
                return d;
            }
        }
        public void RemoveNull()
        {
            List<string> list = new List<string>();
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] == null) list.Add(keys[i]);
            }
            foreach (string key in list)
            {
                Remove(key);
            }
            list.Clear();
        }
        public List<string> Keys => keys;
        public List<StylishSet> Values => values;
        public int Count => keys.Count;

        public bool ContainsKey(string key) => keys.Contains(key);
        public bool ContainsValue(StylishSet value) => values.Contains(value);
        public void Add(string key, StylishSet value)
        {
            if (ContainsKey(key))
            {
                values[keys.IndexOf(key)] = value;
            }
            else
            {
                keys.Add(key);
                values.Add(value);
            }
        }
        public void Remove(string key)
        {
            if (keys.Contains(key))
            {
                int index = keys.IndexOf(key);
                keys.RemoveAt(index);
                values.RemoveAt(index);
            }
        }
        public KeyValuePair<string, StylishSet> First()
        {
            if (keys == null || keys.Count == 0) return new KeyValuePair<string, StylishSet>();
            return new KeyValuePair<string, StylishSet>(keys[0], values[0]);
        }
        public ApparelDict Clone
        {
            get
            {
                ApparelDict temp = new ApparelDict();
                for (int i = 0; i < keys.Count; i++)
                {
                    temp.keys.Add(keys[i]);
                    temp.values.Add(values[i].Clone);
                }
                return temp;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return Dict.GetEnumerator();
        }
        public void ExposeData()
        {
            Scribe_Collections.Look(ref keys, nameof(keys), LookMode.Value);
            Scribe_Collections.Look(ref values, nameof(values), LookMode.Deep);
        }

    }
}
