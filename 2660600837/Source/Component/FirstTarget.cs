using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FilterableDesignator
{
	public class FirstTarget
	{
		public Designation designation;
		public Thing thing;

		private DesignationManager Manager => Find.CurrentMap.designationManager;

		public FirstTarget()
		{
			Clear();
		}

		public void Clear()
		{
			designation = null;
			thing = null;
		}

		public bool HasTarget
		{
			get
			{
				return (designation != null || thing != null);
			}
		}

		public void SetTarget(Thing newThing)
		{
			thing = newThing;
			designation = Manager.DesignationOn(thing);
		}

		public void SetTarget(Designation newDesignation)
		{
			designation = newDesignation;
			thing = designation.target.Thing;
		}

		public TargetType? CellOrThing
		{
			get
			{
				if (designation != null)
				{
					return designation.def.targetType;
				}
				else if (thing != null)
				{
					return TargetType.Thing;
				}
				return null;
			}
		}
	}
}
