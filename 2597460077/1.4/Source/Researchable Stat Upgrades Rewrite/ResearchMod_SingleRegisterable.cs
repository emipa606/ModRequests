using Verse;

namespace ResearchableStatUpgrades
{
	public abstract class ResearchMod_SingleRegisterable : ResearchMod, IRegisterable
	{
		public abstract void Register(WorldComponent_DefEditingResearchManager comp);
		public WorldComponent_DefEditingResearchManager WorldComp { get; protected set; }
		public LogicFieldEditor Editor { get; protected set; }

        public override void Apply()
		{
			bool flag = this.WorldComp != null && this.Editor != null;
			if (flag)
			{
				this.WorldComp.SetEditorValue(this.Editor, true);
			}
		}
	}
}