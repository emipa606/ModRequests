using System.Collections.Generic;
using System.Linq;
using System.Xml;

using Verse;

namespace SurvivalistsAdditions {

	public class PatchOperationModDependent : PatchOperation {

		private List<string> requiredModsList;
		private List<string> optionalModsList;
		private string modName;


		protected override bool ApplyWorker(XmlDocument xml) {
			if (!requiredModsList.NullOrEmpty()) {
				return ApplyWorker_Multiple();
			}
			if (!optionalModsList.NullOrEmpty()) {
				return ApplyWorker_GetAny();
			}
			if (modName.NullOrEmpty()) {
				return false;
			}

			return ApplyWorker_Single();
		}


		private bool ApplyWorker_Multiple() {
			for (int m = 0; m < requiredModsList.Count; m++) {
				if (!ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name == requiredModsList[m])) {
					return false;
				}
			}
			return true;
		}


		private bool ApplyWorker_GetAny() {
			for (int m = 0; m < optionalModsList.Count; m++) {
				if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name == optionalModsList[m])) {
					return true;
				}
			}
			return false;
		}


		private bool ApplyWorker_Single() {
			return ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name == modName);
		}
	}
}
