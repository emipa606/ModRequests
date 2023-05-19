using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace WorksController
{
    [StaticConstructorOnStartup]
    public class WC_DataObject
    {

        public class IntRangeWrapper
        {
            public IntRange Range;
            public int min => Range.min;
            public int max => Range.max;
            public IntRangeWrapper(ref IntRange range)
            {
                Range = range;
            }
            public void SetValue(int min, int max)
            {
                Range.min = min;
                Range.max = max;
            }
            public void Clear()
            {
                Range.min = DEFAULT_SKILL_MIN;
                Range.max = DEFAULT_SKILL_MAX;
            }
        }

        public void ClearInputableData()
        {
            this.m_Enable = false;
            foreach (IntRangeWrapper range in this.m_SkillRanges)
            {
                range.Clear();
            }
            NextStatusUpdateTick = 0;
        }

        public const int DEFAULT_SKILL_MIN = 0;
        public const int DEFAULT_SKILL_MAX = 20;

        private bool m_Enable;
        private WorkGiverDef m_WorkGiverDef;
        private WorkTypeDef m_WorkTypeDef;
        private List<SkillDef> m_SkillDefs = new List<SkillDef>();
        private List<IntRangeWrapper> m_SkillRanges = new List<IntRangeWrapper>();

        public bool Enable
        {
            get => m_Enable;
            set => m_Enable = value;
        }
        public WorkGiverDef WorkGiverDef => m_WorkGiverDef;
        public WorkTypeDef WorkTypeDef => m_WorkTypeDef;
        public List<SkillDef> SkillDefs => m_SkillDefs;
        public List<IntRangeWrapper> SkillRanges => m_SkillRanges;
        public string WorkGiverLabel
        {
            get
            {
                string retText = m_WorkGiverDef.LabelCap;
                if (retText == null)
                {
                    return WorkGiverDefName;
                }
                return retText;
            }
        }
        public string WorkGiverDefName => m_WorkGiverDef.defName;
        public string WorkTypeLabel => m_WorkTypeDef?.labelShort;
        public string PrimarySkillLabel => m_SkillDefs.FirstOrDefault()?.LabelCap;
        public string Name => m_WorkGiverDef.LabelCap;
        public string Label => String.Format("{0}-{1}", string.Join(",", SkillDefs), WorkGiverLabel);
        public string ModLabel => m_WorkGiverDef.modContentPack?.Name ?? "RimWorld";
        public int LabelInt => Math.Min((m_SkillDefs.Sum(x=>x.shortHash) * 10000) + m_WorkGiverDef.shortHash, int.MaxValue);
        public bool NotValue => m_Enable && m_SkillRanges.Where(x=>x.min != DEFAULT_SKILL_MIN).EnumerableNullOrEmpty() && m_SkillRanges.Where(x => x.max != DEFAULT_SKILL_MAX).EnumerableNullOrEmpty();
        public bool Active => m_Enable && (m_SkillRanges.Any(x=>x.min > DEFAULT_SKILL_MIN) || m_SkillRanges.Any(x => x.max < DEFAULT_SKILL_MAX));
        public bool SkillRangeNotChanged => m_SkillRanges.Where(x => x.min != DEFAULT_SKILL_MIN).EnumerableNullOrEmpty() && m_SkillRanges.Where(x => x.max != DEFAULT_SKILL_MAX).EnumerableNullOrEmpty();
        

        public int NextStatusUpdateTick { get; set; } = 0;

        public WC_DataObject(WorkGiverDef workGiverDef)
        {
            this.m_WorkGiverDef = workGiverDef;
            this.m_WorkTypeDef = workGiverDef.workType;
            this.m_SkillDefs = this.m_WorkGiverDef.workType.relevantSkills;
            for (int i = 0; i < this.m_SkillDefs.Count; i++)
            {
                IntRange range = new IntRange(DEFAULT_SKILL_MIN, DEFAULT_SKILL_MAX);
                this.m_SkillRanges.Add(new IntRangeWrapper(ref range));
            }
            m_Enable = false;
        }

        public void SetSkillRangeValue(int min, int max, int index)
        {
            if (m_SkillRanges.Count > index)
            {
                m_SkillRanges[index].SetValue(min, max);
            }
        }

        public override string ToString()
        {
            return Label;
        }

        public override int GetHashCode()
        {
            return m_WorkGiverDef.defName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            WC_DataObject data = obj as WC_DataObject;
            if (data == null)
            {
                return false;
            }
            return data.GetHashCode() == this.GetHashCode();
        }

    }
}
