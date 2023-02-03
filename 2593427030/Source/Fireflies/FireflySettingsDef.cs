using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Fireflies
{
    public class FireflySettingsDef : Def
    {
        public List<AnimalBiomeRecord> maxGroupsByBiome;
        public IntRange spawnHours = new IntRange(18, 22);
        public IntRange despawnHours = new IntRange(6, 10);
        public int despawnInterval = 250;

        public static FireflySettingsDef Def => FFDefOf.FireflySettings;

        protected Dictionary<string, int> _maxGroupsByBiomeName = new Dictionary<string, int>();

        public IReadOnlyDictionary<string, int> MaxGroupsByBiomeName
        {
            get
            {
                if (_maxGroupsByBiomeName.NullOrEmpty())
                    _maxGroupsByBiomeName = maxGroupsByBiome.ToDictionary(abr => abr.biome.defName, abr => UnityEngine.Mathf.FloorToInt(abr.commonality));

                return _maxGroupsByBiomeName as IReadOnlyDictionary<string, int>;
            }
        }

        public int MaxGroupCount(BiomeDef forBiome)
        {
            if (maxGroupsByBiome.NullOrEmpty() || !MaxGroupsByBiomeName.ContainsKey(forBiome.defName)) return 0;
            return (int)maxGroupsByBiome.Find(b => b.biome == forBiome).commonality;
        }
    }
}
