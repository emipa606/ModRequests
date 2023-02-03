using System;
using System.Collections.Generic;
using System.Xml;
using Verse;

namespace Vampire
{
	// Token: 0x0200004C RID: 76
	internal abstract class PatchOperationExtended : PatchOperation
	{
		// Token: 0x060000F4 RID: 244 RVA: 0x0000E098 File Offset: 0x0000C298
		protected sealed override bool ApplyWorker(XmlDocument xml)
		{
			bool result;
			try
			{
				this.Initialize();
				bool flag = PatchManager.applyingPatches && this.requiresDelay;
				if (flag)
				{
					bool flag2 = !PatchManager.PatchModDict.ContainsKey(this) && PatchManager.ActiveMod != null;
					if (flag2)
					{
						PatchManager.PatchModDict.Add(this, PatchManager.ActiveMod);
					}
					PatchManager.delayedPatches.Add(this);
					result = true;
				}
				else
				{
					XmlDocument xml2 = xml;
					bool flag3 = this.xmlDoc != null;
					if (flag3)
					{
						bool flag4 = !PatchManager.XmlDocs.ContainsKey(this.xmlDoc);
						if (flag4)
						{
							this.Error("No XML document exists with docName=\"" + this.xmlDoc + "\"");
							return false;
						}
						xml2 = PatchManager.XmlDocs[this.xmlDoc];
					}
					bool flag5 = !this.PreCheck(xml2);
					if (flag5)
					{
						result = false;
					}
					else
					{
						result = this.Patch(xml2);
					}
				}
			}
			catch (Exception ex)
			{
				this.Error(ex.Message);
				result = false;
			}
			return result;
		}

		// Token: 0x060000F5 RID: 245 RVA: 0x0000E1A8 File Offset: 0x0000C3A8
		protected virtual void Initialize()
		{
		}

		// Token: 0x060000F6 RID: 246 RVA: 0x0000E1AC File Offset: 0x0000C3AC
		protected virtual bool PreCheck(XmlDocument xml)
		{
			return true;
		}

		// Token: 0x060000F7 RID: 247 RVA: 0x0000E1C0 File Offset: 0x0000C3C0
		protected virtual bool Patch(XmlDocument xml)
		{
			return false;
		}

		// Token: 0x060000F8 RID: 248 RVA: 0x0000E1D3 File Offset: 0x0000C3D3
		protected virtual void SetException()
		{
		}

		// Token: 0x060000F9 RID: 249 RVA: 0x0000E1D8 File Offset: 0x0000C3D8
		protected void CreateExceptions(params string[] values)
		{
			this.exceptionFields = new List<string>();
			this.exceptionVals = new List<string>();
			for (int i = 0; i < values.Length; i++)
			{
				bool flag = i % 2 == 0;
				if (flag)
				{
					this.exceptionVals.Add(values[i]);
				}
				else
				{
					this.exceptionFields.Add(values[i]);
				}
			}
		}

		/// <summary>
		/// Throws an error for the patch operation
		/// </summary>
		/// <param name="msg">The message to be displayed</param>
		// Token: 0x060000FA RID: 250 RVA: 0x0000E240 File Offset: 0x0000C440
		protected void Error(string msg)
		{
			this.SetException();
			string text = base.GetType().ToString();
			bool flag = this.exceptionVals != null;
			if (flag)
			{
				text = string.Concat(new string[]
				{
					text,
					"(",
					this.exceptionFields[0],
					"='",
					this.exceptionVals[0],
					"'"
				});
				for (int i = 1; i < this.exceptionVals.Count; i++)
				{
					text = string.Concat(new string[]
					{
						text,
						", ",
						this.exceptionFields[i],
						"='",
						this.exceptionVals[i],
						"'"
					});
				}
				text += ")";
			}
			ErrorManager.AddError(text + ": " + msg);
		}

		// Token: 0x060000FB RID: 251 RVA: 0x0000E33C File Offset: 0x0000C53C
		protected void Error(string[] vals, string[] fields, string msg)
		{
			string text = base.GetType().ToString();
			bool flag = vals != null;
			if (flag)
			{
				text = string.Concat(new string[]
				{
					text,
					"(",
					fields[0],
					"='",
					vals[0],
					"'"
				});
				for (int i = 1; i < vals.Length; i++)
				{
					text = string.Concat(new string[]
					{
						text,
						", ",
						fields[i],
						"='",
						vals[i],
						"'"
					});
				}
				text += ")";
			}
			ErrorManager.AddError(text + ": " + msg);
		}

		// Token: 0x060000FC RID: 252 RVA: 0x0000E400 File Offset: 0x0000C600
		protected bool ErrorIfFalse(bool condition, string message)
		{
			bool flag = !condition;
			bool result;
			if (flag)
			{
				this.Error(message);
				result = false;
			}
			else
			{
				result = true;
			}
			return result;
		}

		// Token: 0x060000FD RID: 253 RVA: 0x0000E428 File Offset: 0x0000C628
		protected void XPathError(string node = "xpath")
		{
			this.Error("Failed to find a node referenced by <" + node + ">");
		}

		// Token: 0x060000FE RID: 254 RVA: 0x0000E442 File Offset: 0x0000C642
		protected void NullError(string node)
		{
			this.Error("<" + node + "> is null");
		}

		// Token: 0x060000FF RID: 255 RVA: 0x0000E45C File Offset: 0x0000C65C
		protected bool RunPatchesConditional(bool b, XmlContainer caseTrue, XmlContainer caseFalse, XmlDocument xml)
		{
			if (b)
			{
				bool flag = caseTrue != null;
				if (flag)
				{
					return this.RunPatches(caseTrue, "caseTrue", xml);
				}
			}
			else
			{
				bool flag2 = caseFalse != null;
				if (flag2)
				{
					return this.RunPatches(caseFalse, "caseFalse", xml);
				}
			}
			return true;
		}

		// Token: 0x06000100 RID: 256 RVA: 0x0000E4B0 File Offset: 0x0000C6B0
		protected bool RunPatches(XmlContainer container, XmlDocument xml)
		{
			return this.RunPatches(container, "apply", xml);
		}

		// Token: 0x06000101 RID: 257 RVA: 0x0000E4D0 File Offset: 0x0000C6D0
		protected bool RunPatches(XmlContainer container, string name, XmlDocument xml)
		{
			bool result;
			try
			{
				for (int i = 0; i < container.node.ChildNodes.Count; i++)
				{
					PatchOperation patchFromString;
					try
					{
						patchFromString = Helpers.GetPatchFromString(container.node.ChildNodes[i].OuterXml);
					}
					catch (Exception ex)
					{
						this.Error(string.Concat(new string[]
						{
							"Failed to create patch in <",
							name,
							"> from the operation at position=",
							(i + 1).ToString(),
							":\n",
							ex.Message
						}));
						return false;
					}
					bool flag = !patchFromString.Apply(xml);
					if (flag)
					{
						this.Error("Error in <" + name + "> in the operation at position=" + (i + 1).ToString());
						return false;
					}
				}
				result = true;
			}
			catch (Exception ex2)
			{
				this.Error(ex2.Message);
				result = false;
			}
			return result;
		}

		// Token: 0x040000BA RID: 186
		public string xmlDoc;

		// Token: 0x040000BB RID: 187
		public bool requiresDelay = false;

		// Token: 0x040000BC RID: 188
		protected List<string> exceptionVals;

		// Token: 0x040000BD RID: 189
		protected List<string> exceptionFields;
	}
}
