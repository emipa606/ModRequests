using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using RimWorld.Planet;
using System.IO;

namespace WorksController
{

    [StaticConstructorOnStartup]
    public class WC_DataContext : WorldComponent
    {

        public const int VERSION1 = 121;
        public const int VERSION_LATEST = 121;

        private const int TICK_STATUS_UPDATE_INTERVAL = 15;

        private static readonly Dictionary<string, WC_DataObject> m_DataObjectHash = new Dictionary<string, WC_DataObject>();
        private static readonly Dictionary<string, SaveParameter> m_SaveParamterHash = new Dictionary<string, SaveParameter>();
        private static string m_SavedataString = String.Format("{0}|", VERSION_LATEST);
        public static bool m_ModEnable = true;
        public static bool m_WCOpend = false;

        private static int m_StatusUpdatedTick = 0;

        private static Dictionary<string, string> m_StatusLabelTextHash = new Dictionary<string, string>();

        private static Dictionary<string, string> m_StatusLabelTipHash = new Dictionary<string, string>();

        public static Dictionary<string, string> GetStatusLabelTextHash() => m_StatusLabelTextHash;

        public static Dictionary<string, string> GetStatusTipTextHash() => m_StatusLabelTipHash;

        public static void SetStatusLabelText(string workGiverDefName, string text) => m_StatusLabelTextHash[workGiverDefName] = text;

        public static void SetStatusTipText(string workGiverDefName, string text) => m_StatusLabelTipHash[workGiverDefName] = text;

        public static string GetStatusLabelText(string workGiverDefName) => m_StatusLabelTextHash.ContainsKey(workGiverDefName) ? m_StatusLabelTextHash[workGiverDefName] : null;

        public static string GetStatusTipText(string workGiverDefName) => m_StatusLabelTipHash.ContainsKey(workGiverDefName) ? m_StatusLabelTipHash[workGiverDefName] : null;

        public static IEnumerable<WC_DataObject> GetDataObjects() => m_DataObjectHash.Values;

        public static bool IsUniqueName(WC_DataObject data) => m_DataObjectHash.Values.Where(x => x.Name == data.Name).Count() <= 1;

        public static WC_DataObject GetData(string defName) => m_DataObjectHash.ContainsKey(defName) ? m_DataObjectHash[defName] : null;

        public static string GetSavedataString() => m_SavedataString;

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            int ticksGame = Find.TickManager.TicksGame;
            bool initialize = m_StatusUpdatedTick == 0;
            if (!initialize && ticksGame % TICK_STATUS_UPDATE_INTERVAL != 0)
            {
                return;
            }

            UpdateStatusDisplayInfo(initialize, ticksGame);

            if (initialize)
            {
                m_StatusUpdatedTick++;
            }
        }

        public static void UpdateStatusDisplayInfo(bool forceUpdate, int tick = 0)
        {
            if (forceUpdate)
            {
                foreach (WC_DataObject data in m_DataObjectHash.Values)
                {
                    if (CreateStatusLabelText(data) & CreateStatusTipText(data))
                    {
                        data.NextStatusUpdateTick = WC_DataContext.GetNextStatusUpdateTick();
                    }
                }
            }
            else if (m_WCOpend)
            {
                foreach (WC_DataObject data in m_DataObjectHash.Values.Where(x => tick >= x.NextStatusUpdateTick))
                {
                    if (CreateStatusLabelText(data) & CreateStatusTipText(data))
                    {
                        data.NextStatusUpdateTick = WC_DataContext.GetNextStatusUpdateTick();
                    }
                }
            }
        }

        public static int GetNextStatusUpdateTick()
        {
            int ticksGame = Find.TickManager.TicksGame;
            if (m_StatusUpdatedTick < ticksGame)
            {
                m_StatusUpdatedTick = ticksGame;
            }
            return m_StatusUpdatedTick += TICK_STATUS_UPDATE_INTERVAL;
        }

        static WC_DataContext()
        {
            WorksControllerUtil.CheckAndCreateConfigDirectory();
        }

        public WC_DataContext(World world) : base(world)
        {
        }


        public static bool CreateStatusLabelText(WC_DataObject data)
        {
            if (!WC_DataContext.m_ModEnable || !data.Active)
            {
                SetStatusLabelText(data.WorkGiverDefName, "WC_StatusNotActiveLabel".Translate());
                return false;
            }
            int allowPawnCount = 0;
            foreach (Map map in Find.Maps.Where(x => x.mapPawns.AllPawns.Any(y => y.Faction == Faction.OfPlayer)))
            {
                allowPawnCount += map.mapPawns.AllPawns.Where(x => WorksControllerUtil.WorkGiver_AllowPawn(data.WorkGiverDef, x)).Count();
            }

            string statusLabelText;
            if (allowPawnCount > 0)
            {
                statusLabelText = "WC_ExistAllowPerson".Translate(allowPawnCount); //Person allowed is {0}. / {0}人が許可
            }
            else
            {
                statusLabelText = "WC_NotExistAllowPerson".Translate(); //Person allowed is none. / 全員無許可
            }
            SetStatusLabelText(data.WorkGiverDefName, statusLabelText);
            return true;
        }

        public static bool CreateStatusTipText(WC_DataObject data)
        {
            if (!WC_DataContext.m_ModEnable || !data.Active)
            {
                SetStatusTipText(data.WorkGiverDefName, "WC_StatusNotActiveTip".Translate());
                return false;
            }
            bool existAllowedPawn = false;
            bool existNotAsignedPawn = false;
            StringBuilder sb = new StringBuilder();
            StringBuilder sbNA = new StringBuilder();
            foreach (Map map in Find.Maps.Where(x => x.mapPawns.AllPawns.Any(y => y.Faction == Faction.OfPlayer)))
            {
                foreach (Pawn pawn in map.mapPawns.AllPawns.Where(x => WorksControllerUtil.WorkGiver_AllowPawn(data.WorkGiverDef, x)).OrderByDescending(x => x.workSettings?.WorkIsActive(data.WorkTypeDef) ?? false).ThenBy(x=> { 
                    if (x.IsPrisonerOfColony)
                    {
                        return 2;
                    }
                    else if (x.Faction != Faction.OfPlayer || x.IsQuestLodger())
                    {
                        return 1;
                    }
                    return 0;
                }).ThenByDescending(x=> data.SkillDefs.FirstOrDefault() != null ? x.skills?.GetSkill(data.SkillDefs.First())?.Level : 0))
                {
                    bool notAsigned = !pawn.workSettings?.WorkIsActive(data.WorkTypeDef) ?? true;
                    existNotAsignedPawn |= notAsigned;
                    existAllowedPawn = true;
                    List<string> skillAndValues = new List<string>();
                    foreach (SkillDef skillDef in data.SkillDefs)
                    {
                        SkillRecord skill = pawn.skills.GetSkill(skillDef);
                        skillAndValues.Add($"{skillDef.LabelCap}({skill.Level})");
                    }
                    //sb.AppendLine(String.Format("{0}->{1}{2}", pawn.LabelCap, string.Join(",", skillAndValues), notAsigned? String.Format("({0}:{1})", "NotAssigned".Translate(), data.WorkTypeLabel) : ""));
                    string labelGuest = null;
                    if (pawn.IsPrisonerOfColony)
                    {
                        labelGuest = $"({"TabPrisoner".Translate()})";
                    }
                    else if (pawn.Faction != Faction.OfPlayer || pawn.IsQuestLodger())
                    {
                        labelGuest = $"({"TabGuest".Translate()})";
                    }
                    if (notAsigned)
                    {
                        sbNA.AppendLine($"{labelGuest}{pawn.LabelCap}->{string.Join(",", skillAndValues)}");
                    }
                    else
                    {
                        sb.AppendLine($"{labelGuest}{pawn.LabelCap}->{string.Join(",", skillAndValues)}");
                    }
                }
            }
            if (existAllowedPawn)
            {
                sb.Insert(0, String.Format("{0}{1}", "WC_ExistAllowPersonTip".Translate(), Environment.NewLine));
            }
            else
            {
                sb.Append("WC_NotExistAllowPersonTip".Translate());
            }
            if (existNotAsignedPawn)
            {
                sbNA.Insert(0, String.Format("{0}---{1}---{2}", Environment.NewLine, String.Format("{0}:{1}", "NotAssigned".Translate(), data.WorkTypeLabel), Environment.NewLine));
            }
            //SetStatusTipText(data.WorkGiverDefName, sb.ToString());
            SetStatusTipText(data.WorkGiverDefName, String.Format("{0}{1}", sb, sbNA));
            return true;
        }

        public static void AddNewData(WorkGiverDef workGiverDef)
        {
            string defName = workGiverDef.defName;
            if (!m_DataObjectHash.ContainsKey(defName))
            {
                m_DataObjectHash[defName] = new WC_DataObject(workGiverDef);
            }
        }

        public static void NotifyHeaderValueUpdated()
        {
            UpdateStatusDisplayInfo(true);
        }

        public static void NotifyValueUpdated(WC_DataObject data)
        {

            if (CreateStatusLabelText(data) & CreateStatusTipText(data))
            {
                data.NextStatusUpdateTick = WC_DataContext.GetNextStatusUpdateTick();
            }

            string defName = data.WorkGiverDefName;
            if (data.NotValue)
            {
                if (m_SaveParamterHash.ContainsKey(defName))
                {
                    m_SaveParamterHash.Remove(defName);
                }
            }
            else
            {
                SaveParameter param;
                if (m_SaveParamterHash.ContainsKey(defName))
                {
                    param = m_SaveParamterHash[defName];
                    param.Enable = data.Enable;
                    param.SkillDefs = data.SkillDefs;
                    param.SkillRanges = data.SkillRanges;
                }
                else
                {
                    param = new SaveParameter(defName, data.Enable, data.SkillDefs, data.SkillRanges);
                    m_SaveParamterHash[defName] = param;
                }
            }
            CreateSaveData();
        }

        private static void CreateSaveData()
        {
            m_SavedataString = String.Format("{0}|{1}", VERSION_LATEST, string.Join("|", m_SaveParamterHash.Values));
        }

        private static void ClearSaveParameter()
        {
            m_SaveParamterHash.Clear();
        }

        public static void ClearSaveData()
        {
            m_ModEnable = true;
            m_SavedataString = String.Format("{0}|", VERSION_LATEST);
            ClearInputableData();
            ClearSaveParameter();
            m_StatusUpdatedTick = 0;
        }

        public static void RestoreBySaveData(string data)
        {
            ClearSaveData();
            string[] splitData = data.Split('^');
            m_SavedataString = splitData[2];
            RestoreBySaveData();
            NotifyHeaderValueUpdated();
        }
        public static void RestoreBySaveData()
        {
            ClearSaveParameter();

            if (m_SavedataString?.EndsWith("|") ?? true)
            {
                ClearInputableData();
                return;
            }

            string[] data1 = m_SavedataString.Split('|');
            bool versionVerfied = int.TryParse(data1[0], out int version);
            if (!versionVerfied)
            {
                ClearInputableData();
                return;
            }

            string[] data2 = new string[data1.Count() - 1];
            Array.Copy(data1, 1, data2, 0, data2.Count());

            switch (version)
            {
                case VERSION_LATEST:
                    RestoreBySaveDataVersion1(data2);
                    break;
            }


        }

        private static void RestoreBySaveDataVersion1(string[] data)
        {

            foreach (string record in data)
            {
                string[] column = record.Split(',');
                string workGiverDefName = column[0];
                if (!m_DataObjectHash.ContainsKey(workGiverDefName))
                {
                    continue;
                }
                WC_DataObject dataObject = m_DataObjectHash[workGiverDefName];
                string enable = column[1];
                if (!bool.TryParse(enable, out bool enableFlg))
                {
                    continue;
                }
                string[] skills = new string[column.Count() - 2];
                Array.Copy(column, 2, skills, 0, skills.Count());
                for (int i = 0; i < skills.Count(); i++)
                {
                    string skill = skills[i];
                    string[] skillWork = skill.Split('@');
                    string skillDefName = skillWork[0];
                    string[] rangeWork = skillWork[1].Split('~');
                    bool verified = int.TryParse(rangeWork[0], out int min) & int.TryParse(rangeWork[1], out int max);
                    if (verified)
                    {
                        dataObject.Enable = enableFlg;
                        if (dataObject.SkillDefs.Count() > i && dataObject.SkillDefs[i].defName == skillDefName)
                        {
                            dataObject.SetSkillRangeValue(min, max, i);
                        }
                    }
                }
                m_SaveParamterHash[dataObject.WorkGiverDefName] = new SaveParameter(dataObject.WorkGiverDefName, dataObject.Enable, dataObject.SkillDefs, dataObject.SkillRanges);
            }
        }

        private static void ClearInputableData()
        {
            foreach (WC_DataObject data in m_DataObjectHash.Values)
            {
                data.ClearInputableData();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<string>(ref m_SavedataString, "WC_SavedataString", String.Format("{0}|", VERSION_LATEST), false);
            Scribe_Values.Look<bool>(ref m_ModEnable, "WC_ModEnable", true, false);
        }

        internal class SaveParameter
        {
            public string DefName { get; set; }
            public bool Enable { get; set; }
            public List<SkillDef> SkillDefs { get; set; }
            public List<WC_DataObject.IntRangeWrapper> SkillRanges { get; set; }

            public SaveParameter(string defName, bool enable, List<SkillDef> skillDefs, List<WC_DataObject.IntRangeWrapper> skillRanges)
            {
                this.DefName = defName;
                this.Enable = enable;
                this.SkillDefs = skillDefs;
                this.SkillRanges = skillRanges;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                object[] args = new object[2 + (SkillDefs.Count * 3)];
                sb.Append("{0},{1}");
                args[0] = DefName;
                args[1] = Enable;
                for (int i = 0, paramIndex = 2; i < SkillDefs.Count; i++, paramIndex += 3)
                {
                    SkillDef skill = SkillDefs[i];
                    WC_DataObject.IntRangeWrapper range = SkillRanges[i];
                    sb.Append(String.Format(",{{{0}}}@{{{1}}}~{{{2}}}", paramIndex, paramIndex + 1, paramIndex + 2));
                    args[paramIndex] = skill.defName;
                    args[paramIndex + 1] = range.min;
                    args[paramIndex + 2] = range.max;
                }
                return String.Format(sb.ToString(), args);
            }
        }
    }
}
