using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using HarmonyLib;
using RimWorld.Planet;

namespace YayoNature
{
    public class worldData : IExposable
    {
        private World w;

        public List<string> biomeDefForCheckChange = new List<string>();
        public bool worldRandomSetuped = false;
        List<string> s_ar_b = null;
        List<BiomeDef> _ar_b = null;
        public List<BiomeDef> ar_b
        {
            get
            {
                if (_ar_b == null)
                {
                    if(s_ar_b != null)
                    {
                        _ar_b = (from s in s_ar_b where BiomeDef.Named(s) != null select BiomeDef.Named(s)).ToList();
                    }
                    else
                    {
                        _ar_b = makeBiomeList();
                        s_ar_b = (from b in _ar_b select b.defName).ToList();
                    }
                }
                return _ar_b;
            }
            set
            {
                _ar_b = value;
                s_ar_b = _ar_b == null ? null : (from b in _ar_b select b.defName).ToList();
            }
        }

        List<string> s_ar_b_temp = null;
        List<float> _ar_b_temp = null;
        public List<float> ar_b_temp
        {
            get
            {
                if (_ar_b_temp == null) _ar_b_temp = makeBiomeTempList();
                return _ar_b_temp;
            }
            set
            {
                _ar_b_temp = value;
            }
        }

        int i;
        

        // Data
        public void ExposeData()
        {
            _ar_b = ar_b;
            Scribe_Collections.Look(ref s_ar_b, $"s_ar_b", LookMode.Value);
            _ar_b_temp = ar_b_temp;
            Scribe_Collections.Look(ref s_ar_b_temp, $"s_ar_b_temp", LookMode.Value);
            Scribe_Values.Look<bool>(ref worldRandomSetuped, $"worldRandomSetuped", false);
            Scribe_Collections.Look(ref biomeDefForCheckChange, $"biomeDefForCheckChange", LookMode.Value);

        }
        public void setParent(World _w)
        {
            w = _w;
        }


        public List<BiomeDef> makeBiomeList()
        {
            // 실제 존재하는 바이옴 리스트 생성
            List<BiomeDef> ar_tmp = new List<BiomeDef>();
            List<BiomeDef> ar_tmp2 = (from b in DefDatabase<BiomeDef>.AllDefs where b.canBuildBase select b).ToList();
            foreach (BiomeDef b in ar_tmp2)
            {
                foreach (Tile t in Find.WorldGrid.tiles)
                {
                    if (t.biome == b)
                    {
                        ar_tmp.Add(b);
                        break;
                    }
                }
            }
            return ar_tmp;
        }
        List<float> makeBiomeTempList()
        {
            List<float> ar_tmp = new List<float>();
            // 바이옴별 평균 온도 리스트 생성
            ar_tmp = new List<float>();
            foreach (BiomeDef b in ar_b) ar_tmp.Add(0f);

            for (i = 0; i < ar_b.Count; i++)
            {
                int count = 0;
                foreach (Tile t in Find.WorldGrid.tiles)
                {
                    if (t.biome == ar_b[i])
                    {
                        ar_tmp[i] += t.temperature;
                        count++;
                    }
                }
                ar_tmp[i] /= (float)count;
            }

            Log.Message($"--- yayo nature init");
            foreach (BiomeDef b in ar_b)
            {
                Log.Message($"({ar_b.IndexOf(b)}){b.label} - def : {b.defName}, temp : {ar_tmp[ar_b.IndexOf(b)]}");
            }
            Log.Message($"--- yayo nature init complete");
            return ar_tmp;
        }


    }
}
