using System.Collections.Generic;
using System.Xml;
using Verse;

namespace EvolvedOrgansRedux.PatchOperations {
	public class PatchOperationSequenceRequireResearch : PatchOperation {
		// Token: 0x0600130B RID: 4875 RVA: 0x0006EE58 File Offset: 0x0006D058
		protected override bool ApplyWorker(XmlDocument xml) {
			if (Settings.RequireResearchProject)
				foreach (PatchOperation patchOperation in this.operations) {
					if (!patchOperation.Apply(xml)) {
						this.lastFailedOperation = patchOperation;
						return false;
					}
				}
				return true;
		}

		// Token: 0x0600130C RID: 4876 RVA: 0x0006EEBC File Offset: 0x0006D0BC
		public override void Complete(string modIdentifier) {
			base.Complete(modIdentifier);
			this.lastFailedOperation = null;
		}

		// Token: 0x0600130D RID: 4877 RVA: 0x0006EECC File Offset: 0x0006D0CC
		public override string ToString() {
			int num = (this.operations != null) ? this.operations.Count : 0;
			string text = string.Format("{0}(count={1}", base.ToString(), num);
			if (this.lastFailedOperation != null) {
				text = text + ", lastFailedOperation=" + this.lastFailedOperation;
			}
			return text + ")";
		}

		// Token: 0x04000FB0 RID: 4016
		private List<PatchOperation> operations;

		// Token: 0x04000FB1 RID: 4017
		private PatchOperation lastFailedOperation;
	}
}
