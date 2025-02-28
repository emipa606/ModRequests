namespace EvolvedOrgansRedux {
    class PatchOperationCombatibilitySwitchEvorVersionMidSaveUnchecked : Verse.PatchOperationPathed {
		private enum Order {
			Append,
			Prepend
		}

		private Verse.XmlContainer value;

		private PatchOperationCombatibilitySwitchEvorVersionMidSaveUnchecked.Order order;

		protected override bool ApplyWorker(System.Xml.XmlDocument xml) {
				System.Xml.XmlNode node = this.value.node;
				bool result = false;

			if (xml.SelectNodes(this.xpath).Count > 0)
				result = true;

			if (!Verse.LoadedModManager.GetMod<EvolvedOrgansReduxSettings>().GetSettings<Settings>().CombatibilitySwitchEorVersionMidSave) {
				foreach (object current in xml.SelectNodes(this.xpath)) {
					System.Xml.XmlNode xmlNode = current as System.Xml.XmlNode;
					if (this.order == PatchOperationCombatibilitySwitchEvorVersionMidSaveUnchecked.Order.Append) {
						for (int i = 0; i < node.ChildNodes.Count; i++) {
							xmlNode.AppendChild(xmlNode.OwnerDocument.ImportNode(node.ChildNodes[i], true));
						}
					} else if (this.order == PatchOperationCombatibilitySwitchEvorVersionMidSaveUnchecked.Order.Prepend) {
						for (int j = node.ChildNodes.Count - 1; j >= 0; j--) {
							xmlNode.PrependChild(xmlNode.OwnerDocument.ImportNode(node.ChildNodes[j], true));
						}
					}
				}
			}
			return result;
		}
	}
	public class PatchOperationCombatibilitySwitchEvorVersionMidSaveChecked : Verse.PatchOperationPathed {
		private enum Order {
			Append,
			Prepend
		}

		private Verse.XmlContainer value;

		private PatchOperationCombatibilitySwitchEvorVersionMidSaveChecked.Order order = PatchOperationCombatibilitySwitchEvorVersionMidSaveChecked.Order.Prepend;

		protected override bool ApplyWorker(System.Xml.XmlDocument xml) {
			System.Xml.XmlNode node = this.value.node;
			bool result = false;
			if (xml.SelectNodes(this.xpath).Count > 0)
				result = true;
			if (Verse.LoadedModManager.GetMod<EvolvedOrgansReduxSettings>().GetSettings<Settings>().CombatibilitySwitchEorVersionMidSave) {

				foreach (object current in xml.SelectNodes(this.xpath)) {
					result = true;
					System.Xml.XmlNode xmlNode = current as System.Xml.XmlNode;
					System.Xml.XmlNode parentNode = xmlNode.ParentNode;
					if (this.order == PatchOperationCombatibilitySwitchEvorVersionMidSaveChecked.Order.Append) {
						for (int i = 0; i < node.ChildNodes.Count; i++) {
							parentNode.InsertAfter(parentNode.OwnerDocument.ImportNode(node.ChildNodes[i], true), xmlNode);
						}
					} else if (this.order == PatchOperationCombatibilitySwitchEvorVersionMidSaveChecked.Order.Prepend) {
						for (int j = node.ChildNodes.Count - 1; j >= 0; j--) {
							parentNode.InsertBefore(parentNode.OwnerDocument.ImportNode(node.ChildNodes[j], true), xmlNode);
						}
					}
				}
			}
			return result;
		}
	}
}