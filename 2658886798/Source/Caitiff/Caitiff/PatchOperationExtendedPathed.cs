using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Vampire
{
	// Token: 0x0200004E RID: 78
	internal abstract class PatchOperationExtendedPathed : PatchOperationExtended
	{
		// Token: 0x06000106 RID: 262 RVA: 0x0000E678 File Offset: 0x0000C878
		protected override bool PreCheck(XmlDocument xml)
		{
			bool flag = this.xpath == null;
			bool result;
			if (flag)
			{
				base.NullError("xpath");
				result = false;
			}
			else
			{
				bool flag2 = this.selectSingleNode;
				if (flag2)
				{
					this.nodes = new List<XmlNode>
					{
						Helpers.SelectSingleNode(this.xpath, xml, this)
					};
				}
				else
				{
					this.nodes = Helpers.SelectNodes(this.xpath, xml, this).Cast<XmlNode>().ToList<XmlNode>();
				}
				bool flag3 = this.nodes == null || this.nodes.Count == 0;
				if (flag3)
				{
					base.XPathError("xpath");
					result = false;
				}
				else
				{
					result = true;
				}
			}
			return result;
		}

		// Token: 0x06000107 RID: 263 RVA: 0x0000E71F File Offset: 0x0000C91F
		protected override void SetException()
		{
			base.CreateExceptions(new string[]
			{
				this.xpath,
				"xpath"
			});
		}

		// Token: 0x040000BF RID: 191
		public string xpath;

		// Token: 0x040000C0 RID: 192
		public bool selectSingleNode = false;

		// Token: 0x040000C1 RID: 193
		protected List<XmlNode> nodes;
	}
}
