using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BaseRobot
{
    // Token: 0x0200000A RID: 10
    public class ThingDef_BaseRobot : ThingDef
	{
		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000034 RID: 52 RVA: 0x000043C4 File Offset: 0x000025C4
		// (set) Token: 0x06000035 RID: 53 RVA: 0x000043F3 File Offset: 0x000025F3
		public WorkTags RobotWorkTags
        {
            get
            {
                var flag = robotWorkTypes.Count > 0;
                if (flag)
                {
                    InitWorkTagsFromWorkTypes();
                }
                return robotWorkTagsInt;
            }
            set => robotWorkTagsInt = value;
        }

        // Token: 0x06000036 RID: 54 RVA: 0x000043FC File Offset: 0x000025FC
        private WorkTags InitWorkTagsFromWorkTypes()
		{
			WorkTags workTags = WorkTags.None;
			foreach (RobotWorkTypes robotWorkTypes in robotWorkTypes)
			{
				workTags |= robotWorkTypes.workTypeDef.workTags;
			}
			return workTags;
		}

		// Token: 0x04000023 RID: 35
		public List<RobotSkills> robotSkills = new List<RobotSkills>();

		// Token: 0x04000024 RID: 36
		public ThingDef destroyedDef;

		// Token: 0x04000025 RID: 37
		public bool allowLearning;

		// Token: 0x04000026 RID: 38
		public WorkTags robotWorkTagsInt;

		// Token: 0x04000027 RID: 39
		public List<RobotWorkTypes> robotWorkTypes = new List<RobotWorkTypes>();

		// Token: 0x0200000B RID: 11
		public class RobotSkills
		{
			// Token: 0x04000028 RID: 40
			public SkillDef skillDef;

			// Token: 0x04000029 RID: 41
			public int level = 0;

			// Token: 0x0400002A RID: 42
			public Passion passion = Passion.None;
		}

		// Token: 0x0200000C RID: 12
		public class RobotWorkTypes
		{
			// Token: 0x0400002B RID: 43
			public WorkTypeDef workTypeDef;

			// Token: 0x0400002C RID: 44
			public int priority = 1;
		}
	}
}
