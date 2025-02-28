namespace EvolvedOrgansRedux {
	public class PatchOperationAddTwoArms : Verse.PatchOperationPathed {
		protected override bool ApplyWorker(System.Xml.XmlDocument xml) {
			bool result = false;
				System.Xml.XmlNode node = this.value.node;
				foreach (object obj in xml.SelectNodes(this.xpath)) {
				result = true;
				if (Settings.AmountOfArms > 2) {
					System.Xml.XmlNode xmlNode = obj as System.Xml.XmlNode;
					if (this.order == Order.Append) {
						System.Collections.IEnumerator enumerator2 = node.ChildNodes.GetEnumerator();
						while (enumerator2.MoveNext()) {
							object obj2 = enumerator2.Current;
							System.Xml.XmlNode node2 = (System.Xml.XmlNode)obj2;
							xmlNode.AppendChild(xmlNode.OwnerDocument.ImportNode(node2, true));
						}
						continue;
					}
					if (this.order == Order.Prepend) {
						for (int i = node.ChildNodes.Count - 1; i >= 0; i--) {
							xmlNode.PrependChild(xmlNode.OwnerDocument.ImportNode(node.ChildNodes[i], true));
						}
					}
				}
			}
			return result;
		}

		private Verse.XmlContainer value;

		private Order order;

		private enum Order {
			Append,
			Prepend
		}
	}
	public class PatchOperationAddFourArms : Verse.PatchOperationPathed {
		protected override bool ApplyWorker(System.Xml.XmlDocument xml) {
			bool result = false;
			System.Xml.XmlNode node = this.value.node;
			foreach (object obj in xml.SelectNodes(this.xpath)) {
				result = true;
				if (Settings.AmountOfArms > 4) {
					System.Xml.XmlNode xmlNode = obj as System.Xml.XmlNode;
					if (this.order == Order.Append) {
						System.Collections.IEnumerator enumerator2 = node.ChildNodes.GetEnumerator();
						while (enumerator2.MoveNext()) {
							object obj2 = enumerator2.Current;
							System.Xml.XmlNode node2 = (System.Xml.XmlNode)obj2;
							xmlNode.AppendChild(xmlNode.OwnerDocument.ImportNode(node2, true));
						}
						continue;
					}
					if (this.order == Order.Prepend) {
						for (int i = node.ChildNodes.Count - 1; i >= 0; i--) {
							xmlNode.PrependChild(xmlNode.OwnerDocument.ImportNode(node.ChildNodes[i], true));
						}
					}
				}
			}
			return result;
		}

		private Verse.XmlContainer value;

		private Order order;

		private enum Order {
			Append,
			Prepend
		}
	}
	public class PatchOperationAddSixArms : Verse.PatchOperationPathed {
		protected override bool ApplyWorker(System.Xml.XmlDocument xml) {
			bool result = false;
			System.Xml.XmlNode node = this.value.node;
			foreach (object obj in xml.SelectNodes(this.xpath)) {
				result = true;
				if (Settings.AmountOfArms > 6) {
					System.Xml.XmlNode xmlNode = obj as System.Xml.XmlNode;
					if (this.order == Order.Append) {
						System.Collections.IEnumerator enumerator2 = node.ChildNodes.GetEnumerator();
						while (enumerator2.MoveNext()) {
							object obj2 = enumerator2.Current;
							System.Xml.XmlNode node2 = (System.Xml.XmlNode)obj2;
							xmlNode.AppendChild(xmlNode.OwnerDocument.ImportNode(node2, true));
						}
						continue;
					}
					if (this.order == Order.Prepend) {
						for (int i = node.ChildNodes.Count - 1; i >= 0; i--) {
							xmlNode.PrependChild(xmlNode.OwnerDocument.ImportNode(node.ChildNodes[i], true));
						}
					}
				}
			}
			return result;
		}

		private Verse.XmlContainer value;

		private Order order;

		private enum Order {
			Append,
			Prepend
		}
	}
	//public class PatchOperationAddTwoLegs : Verse.PatchOperationPathed {
	//	protected override bool ApplyWorker(System.Xml.XmlDocument xml) {
	//		bool result = false;
	//		System.Xml.XmlNode node = this.value.node;
	//		foreach (object obj in xml.SelectNodes(this.xpath)) {
	//			result = true;
	//			if (Settings.AmountOfLegs > 2) {
	//				System.Xml.XmlNode xmlNode = obj as System.Xml.XmlNode;
	//				if (this.order == Order.Append) {
	//					System.Collections.IEnumerator enumerator2 = node.ChildNodes.GetEnumerator();
	//					while (enumerator2.MoveNext()) {
	//						object obj2 = enumerator2.Current;
	//						System.Xml.XmlNode node2 = (System.Xml.XmlNode)obj2;
	//						xmlNode.AppendChild(xmlNode.OwnerDocument.ImportNode(node2, true));
	//					}
	//					continue;
	//				}
	//				if (this.order == Order.Prepend) {
	//					for (int i = node.ChildNodes.Count - 1; i >= 0; i--) {
	//						xmlNode.PrependChild(xmlNode.OwnerDocument.ImportNode(node.ChildNodes[i], true));
	//					}
	//				}
	//			}
	//		}
	//		return result;
	//	}
	//
	//	private Verse.XmlContainer value;
	//
	//	private Order order;
	//
	//	private enum Order {
	//		Append,
	//		Prepend
	//	}
	//}
	//public class PatchOperationAddFourLegs : Verse.PatchOperationPathed {
	//	protected override bool ApplyWorker(System.Xml.XmlDocument xml) {
	//		bool result = false;
	//		System.Xml.XmlNode node = this.value.node;
	//		foreach (object obj in xml.SelectNodes(this.xpath)) {
	//			result = true;
	//			if (Settings.AmountOfLegs > 4) {
	//				System.Xml.XmlNode xmlNode = obj as System.Xml.XmlNode;
	//				if (this.order == Order.Append) {
	//					System.Collections.IEnumerator enumerator2 = node.ChildNodes.GetEnumerator();
	//					while (enumerator2.MoveNext()) {
	//						object obj2 = enumerator2.Current;
	//						System.Xml.XmlNode node2 = (System.Xml.XmlNode)obj2;
	//						xmlNode.AppendChild(xmlNode.OwnerDocument.ImportNode(node2, true));
	//					}
	//					continue;
	//				}
	//				if (this.order == Order.Prepend) {
	//					for (int i = node.ChildNodes.Count - 1; i >= 0; i--) {
	//						xmlNode.PrependChild(xmlNode.OwnerDocument.ImportNode(node.ChildNodes[i], true));
	//					}
	//				}
	//			}
	//		}
	//		return result;
	//	}
	//
	//	private Verse.XmlContainer value;
	//
	//	private Order order;
	//
	//	private enum Order {
	//		Append,
	//		Prepend
	//	}
	//}
	//public class PatchOperationAddSixLegs : Verse.PatchOperationPathed {
	//	protected override bool ApplyWorker(System.Xml.XmlDocument xml) {
	//		bool result = false;
	//		System.Xml.XmlNode node = this.value.node;
	//		foreach (object obj in xml.SelectNodes(this.xpath)) {
	//			result = true;
	//			if (Settings.AmountOfLegs > 6) {
	//				System.Xml.XmlNode xmlNode = obj as System.Xml.XmlNode;
	//				if (this.order == Order.Append) {
	//					System.Collections.IEnumerator enumerator2 = node.ChildNodes.GetEnumerator();
	//					while (enumerator2.MoveNext()) {
	//						object obj2 = enumerator2.Current;
	//						System.Xml.XmlNode node2 = (System.Xml.XmlNode)obj2;
	//						xmlNode.AppendChild(xmlNode.OwnerDocument.ImportNode(node2, true));
	//					}
	//					continue;
	//				}
	//				if (this.order == Order.Prepend) {
	//					for (int i = node.ChildNodes.Count - 1; i >= 0; i--) {
	//						xmlNode.PrependChild(xmlNode.OwnerDocument.ImportNode(node.ChildNodes[i], true));
	//					}
	//				}
	//			}
	//		}
	//		return result;
	//	}
	//
	//	private Verse.XmlContainer value;
	//
	//	private Order order;
	//
	//	private enum Order {
	//		Append,
	//		Prepend
	//	}
	//}
	//public class PatchOperationAddTwoEyes : Verse.PatchOperationPathed {
	//	protected override bool ApplyWorker(System.Xml.XmlDocument xml) {
	//		bool result = false;
	//		System.Xml.XmlNode node = this.value.node;
	//		foreach (object obj in xml.SelectNodes(this.xpath)) {
	//			result = true;
	//			if (Settings.AmountOfEyes > 2) {
	//				System.Xml.XmlNode xmlNode = obj as System.Xml.XmlNode;
	//				if (this.order == Order.Append) {
	//					System.Collections.IEnumerator enumerator2 = node.ChildNodes.GetEnumerator();
	//					while (enumerator2.MoveNext()) {
	//						object obj2 = enumerator2.Current;
	//						System.Xml.XmlNode node2 = (System.Xml.XmlNode)obj2;
	//						xmlNode.AppendChild(xmlNode.OwnerDocument.ImportNode(node2, true));
	//					}
	//					continue;
	//				}
	//				if (this.order == Order.Prepend) {
	//					for (int i = node.ChildNodes.Count - 1; i >= 0; i--) {
	//						xmlNode.PrependChild(xmlNode.OwnerDocument.ImportNode(node.ChildNodes[i], true));
	//					}
	//				}
	//			}
	//		}
	//		return result;
	//	}
	//
	//	private Verse.XmlContainer value;
	//
	//	private Order order;
	//
	//	private enum Order {
	//		Append,
	//		Prepend
	//	}
	//}
	//public class PatchOperationAddFourEyes : Verse.PatchOperationPathed {
	//	protected override bool ApplyWorker(System.Xml.XmlDocument xml) {
	//		bool result = false;
	//		System.Xml.XmlNode node = this.value.node;
	//		foreach (object obj in xml.SelectNodes(this.xpath)) {
	//			result = true;
	//			if (Settings.AmountOfEyes > 4) {
	//				System.Xml.XmlNode xmlNode = obj as System.Xml.XmlNode;
	//				if (this.order == Order.Append) {
	//					System.Collections.IEnumerator enumerator2 = node.ChildNodes.GetEnumerator();
	//					while (enumerator2.MoveNext()) {
	//						object obj2 = enumerator2.Current;
	//						System.Xml.XmlNode node2 = (System.Xml.XmlNode)obj2;
	//						xmlNode.AppendChild(xmlNode.OwnerDocument.ImportNode(node2, true));
	//					}
	//					continue;
	//				}
	//				if (this.order == Order.Prepend) {
	//					for (int i = node.ChildNodes.Count - 1; i >= 0; i--) {
	//						xmlNode.PrependChild(xmlNode.OwnerDocument.ImportNode(node.ChildNodes[i], true));
	//					}
	//				}
	//			}
	//		}
	//		return result;
	//	}
	//
	//	private Verse.XmlContainer value;
	//
	//	private Order order;
	//
	//	private enum Order {
	//		Append,
	//		Prepend
	//	}
	//}
	//public class PatchOperationAddSixEyes : Verse.PatchOperationPathed {
	//	protected override bool ApplyWorker(System.Xml.XmlDocument xml) {
	//		bool result = false;
	//		System.Xml.XmlNode node = this.value.node;
	//		foreach (object obj in xml.SelectNodes(this.xpath)) {
	//			result = true;
	//			if (Settings.AmountOfEyes > 6) {
	//				System.Xml.XmlNode xmlNode = obj as System.Xml.XmlNode;
	//				if (this.order == Order.Append) {
	//					System.Collections.IEnumerator enumerator2 = node.ChildNodes.GetEnumerator();
	//					while (enumerator2.MoveNext()) {
	//						object obj2 = enumerator2.Current;
	//						System.Xml.XmlNode node2 = (System.Xml.XmlNode)obj2;
	//						xmlNode.AppendChild(xmlNode.OwnerDocument.ImportNode(node2, true));
	//					}
	//					continue;
	//				}
	//				if (this.order == Order.Prepend) {
	//					for (int i = node.ChildNodes.Count - 1; i >= 0; i--) {
	//						xmlNode.PrependChild(xmlNode.OwnerDocument.ImportNode(node.ChildNodes[i], true));
	//					}
	//				}
	//			}
	//		}
	//		return result;
	//	}
	//
	//	private Verse.XmlContainer value;
	//
	//	private Order order;
	//
	//	private enum Order {
	//		Append,
	//		Prepend
	//	}
	//}
	//public class PatchOperationAddTwoEars : Verse.PatchOperationPathed {
	//	protected override bool ApplyWorker(System.Xml.XmlDocument xml) {
	//		bool result = false;
	//		System.Xml.XmlNode node = this.value.node;
	//		foreach (object obj in xml.SelectNodes(this.xpath)) {
	//			result = true;
	//			if (Settings.AmountOfEars > 2) {
	//				System.Xml.XmlNode xmlNode = obj as System.Xml.XmlNode;
	//				if (this.order == Order.Append) {
	//					System.Collections.IEnumerator enumerator2 = node.ChildNodes.GetEnumerator();
	//					while (enumerator2.MoveNext()) {
	//						object obj2 = enumerator2.Current;
	//						System.Xml.XmlNode node2 = (System.Xml.XmlNode)obj2;
	//						xmlNode.AppendChild(xmlNode.OwnerDocument.ImportNode(node2, true));
	//					}
	//					continue;
	//				}
	//				if (this.order == Order.Prepend) {
	//					for (int i = node.ChildNodes.Count - 1; i >= 0; i--) {
	//						xmlNode.PrependChild(xmlNode.OwnerDocument.ImportNode(node.ChildNodes[i], true));
	//					}
	//				}
	//			}
	//		}
	//		return result;
	//	}
	//
	//	private Verse.XmlContainer value;
	//
	//	private Order order;
	//
	//	private enum Order {
	//		Append,
	//		Prepend
	//	}
	//}
	//public class PatchOperationAddFourEars : Verse.PatchOperationPathed {
	//	protected override bool ApplyWorker(System.Xml.XmlDocument xml) {
	//		bool result = false;
	//		System.Xml.XmlNode node = this.value.node;
	//		foreach (object obj in xml.SelectNodes(this.xpath)) {
	//			result = true;
	//			if (Settings.AmountOfEars > 4) {
	//				System.Xml.XmlNode xmlNode = obj as System.Xml.XmlNode;
	//				if (this.order == Order.Append) {
	//					System.Collections.IEnumerator enumerator2 = node.ChildNodes.GetEnumerator();
	//					while (enumerator2.MoveNext()) {
	//						object obj2 = enumerator2.Current;
	//						System.Xml.XmlNode node2 = (System.Xml.XmlNode)obj2;
	//						xmlNode.AppendChild(xmlNode.OwnerDocument.ImportNode(node2, true));
	//					}
	//					continue;
	//				}
	//				if (this.order == Order.Prepend) {
	//					for (int i = node.ChildNodes.Count - 1; i >= 0; i--) {
	//						xmlNode.PrependChild(xmlNode.OwnerDocument.ImportNode(node.ChildNodes[i], true));
	//					}
	//				}
	//			}
	//		}
	//		return result;
	//	}
	//
	//	private Verse.XmlContainer value;
	//
	//	private Order order;
	//
	//	private enum Order {
	//		Append,
	//		Prepend
	//	}
	//}
	//public class PatchOperationAddSixEars : Verse.PatchOperationPathed {
	//	protected override bool ApplyWorker(System.Xml.XmlDocument xml) {
	//		bool result = false;
	//		System.Xml.XmlNode node = this.value.node;
	//		foreach (object obj in xml.SelectNodes(this.xpath)) {
	//			result = true;
	//			if (Settings.AmountOfEars > 6) {
	//				System.Xml.XmlNode xmlNode = obj as System.Xml.XmlNode;
	//				if (this.order == Order.Append) {
	//					System.Collections.IEnumerator enumerator2 = node.ChildNodes.GetEnumerator();
	//					while (enumerator2.MoveNext()) {
	//						object obj2 = enumerator2.Current;
	//						System.Xml.XmlNode node2 = (System.Xml.XmlNode)obj2;
	//						xmlNode.AppendChild(xmlNode.OwnerDocument.ImportNode(node2, true));
	//					}
	//					continue;
	//				}
	//				if (this.order == Order.Prepend) {
	//					for (int i = node.ChildNodes.Count - 1; i >= 0; i--) {
	//						xmlNode.PrependChild(xmlNode.OwnerDocument.ImportNode(node.ChildNodes[i], true));
	//					}
	//				}
	//			}
	//		}
	//		return result;
	//	}
	//
	//	private Verse.XmlContainer value;
	//
	//	private Order order;
	//
	//	private enum Order {
	//		Append,
	//		Prepend
	//	}
	//}
}
