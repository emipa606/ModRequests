using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FilterableDesignator
{
	public interface DesignatorInterface
	{
		void BuildFloatMenuOption(Designator __instance, Event ev);
		string MouseAttachmentText(Designator __instance, string text);
	}
}
