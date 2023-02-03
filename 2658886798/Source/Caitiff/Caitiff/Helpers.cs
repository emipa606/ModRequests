using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Verse;

namespace Vampire
{
	// Token: 0x0200003C RID: 60
	internal static class Helpers
	{
		/// <summary>
		/// Calculates the prefix of the given length.
		/// </summary>
		/// <param name="path">The xpath you want to get the prefix of.</param>
		/// <param name="length">The number of nodes that appear in the xpath.</param>
		/// <returns>The prefix.</returns>
		// Token: 0x060000B4 RID: 180 RVA: 0x0000C298 File Offset: 0x0000A498
		public static string GetPrefix(string path, int length = 2)
		{
			bool flag = path[0] == '/';
			if (flag)
			{
				path = path.Substring(1);
			}
			string[] array = path.Split(new char[]
			{
				'/'
			});
			string text = "";
			for (int i = 0; i < length; i++)
			{
				text += "/";
				text += array[i];
			}
			return text.Substring(1);
		}

		/// <summary>
		/// Substitutes a variable with its value in a given a string.
		/// </summary>
		/// <param name="str">The string that you want to edit.</param>
		/// <param name="var">The name of the variable.</param>
		/// <param name="val">The value of the variable.</param>
		/// <param name="brackets">The left and right brackets that surround the variable.</param>
		/// <returns>The new string after the substitution.</returns>
		// Token: 0x060000B5 RID: 181 RVA: 0x0000C314 File Offset: 0x0000A514
		public static string SubstituteVariable(string str, string var, string val, string brackets = "{}")
		{
			string oldValue = brackets[0].ToString() + var + brackets[1].ToString();
			return str.Replace(oldValue, val);
		}

		/// <summary>
		/// Substitutes a list of variables with their corresponding values in a given string.
		/// </summary>
		/// <param name="str">The string that you want to edit.</param>
		/// <param name="vars">The list of variable names.</param>
		/// <param name="vals">The list of values for the variables.</param>
		/// <param name="brackets">The left and right brackets that surround the variables.</param>
		/// <returns>The new string after the substitution.</returns>
		// Token: 0x060000B6 RID: 182 RVA: 0x0000C354 File Offset: 0x0000A554
		public static string SubstituteVariables(string str, List<string> vars, List<string> vals, string brackets)
		{
			int num = 0;
			StringBuilder stringBuilder = new StringBuilder(str);
			foreach (string str2 in vars)
			{
				stringBuilder.Replace(brackets[0].ToString() + str2 + brackets[1].ToString(), vals[num]);
				num++;
			}
			return stringBuilder.ToString();
		}

		/// <summary>
		/// Substitutes a variable with its value in a given XmlContainer.
		/// </summary>
		/// <param name="container">The XmlContainer that you want to edit.</param>
		/// <param name="var">The name of the variable.</param>
		/// <param name="val">The value of the variable.</param>
		/// <param name="brackets">The left and right brackets that surround the variable.</param>
		/// <returns>The new XmlContainer after the substitution.</returns>
		// Token: 0x060000B7 RID: 183 RVA: 0x0000C3F0 File Offset: 0x0000A5F0
		public static XmlContainer SubstituteVariableXmlContainer(XmlContainer container, string var, string val, string brackets)
		{
			string outerXml = container.node.OuterXml;
			string str = Helpers.SubstituteVariable(outerXml, var, val, brackets);
			return new XmlContainer
			{
				node = Helpers.GetNodeFromString(str)
			};
		}

		/// <summary>
		/// Substitutes a list of variables with their corresponding values in a given string.
		/// </summary>
		/// <param name="container">The string that you want to edit.</param>
		/// <param name="var">The list of variable names.</param>
		/// <param name="val">The list of values for the variables.</param>
		/// <param name="brackets">The left and right brackets that surround the variables.</param>
		/// <returns>The new string after the substitution.</returns>
		// Token: 0x060000B8 RID: 184 RVA: 0x0000C42C File Offset: 0x0000A62C
		public static XmlContainer SubstituteVariablesXmlContainer(XmlContainer container, List<string> var, List<string> val, string brackets)
		{
			string outerXml = container.node.OuterXml;
			string str = Helpers.SubstituteVariables(outerXml, var, val, brackets);
			return new XmlContainer
			{
				node = Helpers.GetNodeFromString(str)
			};
		}

		/// <summary>
		/// Creates a PatchOperation from its OuterXml.
		/// </summary>
		/// <param name="OuterXml">The OuterXml of the PatchOperation.</param>
		/// <returns>A PatchOperation from the given OuterXml.</returns>
		// Token: 0x060000B9 RID: 185 RVA: 0x0000C468 File Offset: 0x0000A668
		public static PatchOperation GetPatchFromString(string OuterXml)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(OuterXml);
			XmlNode documentElement = xmlDocument.DocumentElement;
			return CustomXmlLoader.ObjectFromXml<PatchOperation>(documentElement, false);
		}

		// Token: 0x060000BA RID: 186 RVA: 0x0000C498 File Offset: 0x0000A698
		public static XmlNode GetNodeFromString(string str)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(str);
			return xmlDocument.DocumentElement;
		}

		/// <summary>
		/// Performs a mathematical on two strings.
		/// </summary>
		/// <param name="str1">The first string.</param>
		/// <param name="str2">The second string.</param>
		/// <param name="operation">The operation to be performed.</param>
		/// <returns>The resulting string.</returns>
		// Token: 0x060000BB RID: 187 RVA: 0x0000C4C0 File Offset: 0x0000A6C0
		public static string OperationOnString(string str1, string str2, string operation)
		{
			string result = "";
			bool flag = operation == "negate";
			if (flag)
			{
				bool flag2 = str1 == "true";
				if (flag2)
				{
					result = "false";
				}
				else
				{
					result = "true";
				}
			}
			else
			{
				bool flag3 = operation == "+";
				if (flag3)
				{
					float num = float.Parse(str1);
					float num2 = float.Parse(str2);
					result = (num + num2).ToString();
				}
				else
				{
					bool flag4 = operation == "*";
					if (flag4)
					{
						float num3 = float.Parse(str1);
						float num4 = float.Parse(str2);
						result = (num3 * num4).ToString();
					}
					else
					{
						bool flag5 = operation == "/";
						if (flag5)
						{
							float num5 = float.Parse(str1);
							float num6 = float.Parse(str2);
							result = (num5 / num6).ToString();
						}
						else
						{
							bool flag6 = operation == "/2";
							if (flag6)
							{
								float num7 = float.Parse(str1);
								float num8 = float.Parse(str2);
								result = (num8 / num7).ToString();
							}
							else
							{
								bool flag7 = operation == "-";
								if (flag7)
								{
									float num9 = float.Parse(str1);
									float num10 = float.Parse(str2);
									result = (num9 - num10).ToString();
								}
								else
								{
									bool flag8 = operation == "-2";
									if (flag8)
									{
										float num11 = float.Parse(str1);
										float num12 = float.Parse(str2);
										result = (num12 - num11).ToString();
									}
									else
									{
										bool flag9 = operation == "%";
										if (flag9)
										{
											float num13 = float.Parse(str1);
											float num14 = float.Parse(str2);
											result = (num13 % num14).ToString();
										}
										else
										{
											bool flag10 = operation == "%2";
											if (flag10)
											{
												float num15 = float.Parse(str1);
												float num16 = float.Parse(str2);
												result = (num16 % num15).ToString();
											}
											else
											{
												bool flag11 = operation == "^";
												if (flag11)
												{
													float num17 = float.Parse(str1);
													float num18 = float.Parse(str2);
													result = Math.Pow((double)num17, (double)num18).ToString();
												}
												else
												{
													bool flag12 = operation == "^2";
													if (flag12)
													{
														float num19 = float.Parse(str1);
														float num20 = float.Parse(str2);
														result = Math.Pow((double)num20, (double)num19).ToString();
													}
													else
													{
														bool flag13 = operation == "log";
														if (flag13)
														{
															float num21 = float.Parse(str1);
															float num22 = float.Parse(str2);
															result = Math.Log((double)num21, (double)num22).ToString();
														}
														else
														{
															bool flag14 = operation == "log2";
															if (flag14)
															{
																float num23 = float.Parse(str1);
																float num24 = float.Parse(str2);
																result = Math.Log((double)num24, (double)num23).ToString();
															}
															else
															{
																bool flag15 = operation == "min";
																if (flag15)
																{
																	float val = float.Parse(str1);
																	float val2 = float.Parse(str2);
																	result = Math.Min(val, val2).ToString();
																}
																else
																{
																	bool flag16 = operation == "max";
																	if (flag16)
																	{
																		float val3 = float.Parse(str1);
																		float val4 = float.Parse(str2);
																		result = Math.Max(val3, val4).ToString();
																	}
																	else
																	{
																		bool flag17 = operation == "average";
																		if (flag17)
																		{
																			float num25 = float.Parse(str1);
																			float num26 = float.Parse(str2);
																			result = ((num25 + num26) / 2f).ToString();
																		}
																		else
																		{
																			bool flag18 = operation == "concat";
																			if (flag18)
																			{
																				result = str1 + str2;
																			}
																			else
																			{
																				bool flag19 = operation == "concat2";
																				if (flag19)
																				{
																					result = str2 + str1;
																				}
																			}
																		}
																	}
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Computes a binary relation between two strings.
		/// </summary>
		/// <param name="str1">The first string.</param>
		/// <param name="str2">The second string.</param>
		/// <param name="relation">The binary relation.</param>
		/// <param name="nonNumeric">If true, then the inputs will be interpreted as strings and dictionary order is used instead.</param>
		/// <returns>Whether or not the relations holds.</returns>
		// Token: 0x060000BC RID: 188 RVA: 0x0000C8A4 File Offset: 0x0000AAA4
		public static bool RelationOnString(string str1, string str2, string relation, bool nonNumeric)
		{
			bool result;
			if (nonNumeric)
			{
				int num = str1.CompareTo(str2);
				bool flag = relation == "eq";
				if (flag)
				{
					bool flag2 = num == 0;
					result = flag2;
				}
				else
				{
					bool flag3 = relation == "sl";
					if (flag3)
					{
						bool flag4 = num < 0;
						result = flag4;
					}
					else
					{
						bool flag5 = relation == "leq";
						if (flag5)
						{
							bool flag6 = num <= 0;
							result = flag6;
						}
						else
						{
							bool flag7 = relation == "sg";
							if (flag7)
							{
								bool flag8 = num > 0;
								result = flag8;
							}
							else
							{
								bool flag9 = relation == "geq";
								if (flag9)
								{
									bool flag10 = num >= 0;
									result = flag10;
								}
								else
								{
									bool flag11 = relation == "neq";
									if (flag11)
									{
										bool flag12 = num != 0;
										result = flag12;
									}
									else
									{
										result = (num == 0);
									}
								}
							}
						}
					}
				}
			}
			else
			{
				float num2 = float.Parse(str1);
				float num3 = float.Parse(str2);
				bool flag13 = relation == "eq";
				if (flag13)
				{
					result = (num2 == num3);
				}
				else
				{
					bool flag14 = relation == "sl";
					if (flag14)
					{
						result = (num2 < num3);
					}
					else
					{
						bool flag15 = relation == "leq";
						if (flag15)
						{
							result = (num2 <= num3);
						}
						else
						{
							bool flag16 = relation == "sg";
							if (flag16)
							{
								result = (num2 > num3);
							}
							else
							{
								bool flag17 = relation == "geq";
								if (flag17)
								{
									result = (num2 >= num3);
								}
								else
								{
									result = (num2 == num3);
								}
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x060000BD RID: 189 RVA: 0x0000CA84 File Offset: 0x0000AC84
		public static bool RunPatchesInPatchContainer(PatchContainer container, XmlDocument xml, ref int errNum)
		{
			bool result;
			try
			{
				for (int i = 0; i < container.patches.Count; i++)
				{
					bool flag = !container.patches[i].Apply(xml);
					if (flag)
					{
						errNum = i + 1;
						return false;
					}
				}
				result = true;
			}
			catch (Exception ex)
			{
				ErrorManager.AddError(ex.Message);
				result = false;
			}
			return result;
		}

		// Token: 0x060000BE RID: 190 RVA: 0x0000CAFC File Offset: 0x0000ACFC
		public static string TryTranslate(string str, string tKey)
		{
			bool flag = tKey != null;
			string result;
			if (flag)
			{
				TaggedString taggedString = default(TaggedString);
				bool flag2 = Translator.TryTranslate(tKey, ref taggedString);
				if (flag2)
				{
					result = taggedString.RawText;
				}
				else
				{
					result = str;
				}
			}
			else
			{
				result = str;
			}
			return result;
		}

		// Token: 0x060000BF RID: 191 RVA: 0x0000CB3C File Offset: 0x0000AD3C
		public static List<string> GetDefsFromPath(string path, XmlDocument xml)
		{
			XmlNodeList xmlNodeList = xml.SelectNodes(path);
			List<string> list = new List<string>();
			foreach (object obj in xmlNodeList)
			{
				XmlNode node = (XmlNode)obj;
				string defNameFromNode = Helpers.GetDefNameFromNode(Helpers.GetDefNode(node));
				bool flag = defNameFromNode != null;
				if (flag)
				{
					list.Add(defNameFromNode);
				}
			}
			return list;
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x0000CBCC File Offset: 0x0000ADCC
		public static XmlNode GetDefNode(XmlNode node)
		{
			XmlNode xmlNode = node;
			while (xmlNode.ParentNode != null && xmlNode.ParentNode.Name != "Defs")
			{
				xmlNode = xmlNode.ParentNode;
			}
			return xmlNode;
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x0000CC10 File Offset: 0x0000AE10
		public static string GetDefNameFromNode(XmlNode root)
		{
			bool flag = root == null;
			string result;
			if (flag)
			{
				result = null;
			}
			else
			{
				XmlNode xmlNode = root.SelectSingleNode("defName");
				string text = null;
				bool flag2 = xmlNode != null;
				if (flag2)
				{
					text = xmlNode.InnerText;
				}
				else
				{
					XmlAttributeCollection attributes = root.Attributes;
					XmlAttribute xmlAttribute = (attributes != null) ? attributes["Name"] : null;
					bool flag3 = xmlAttribute != null;
					if (flag3)
					{
						text = "@" + xmlAttribute.Value;
					}
				}
				bool flag4 = text != null;
				if (flag4)
				{
					text = root.Name + ";" + text;
				}
				result = text;
			}
			return result;
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x0000CCAC File Offset: 0x0000AEAC
		public static XmlNodeList SelectNodes(string path, XmlDocument xml, PatchOperation operation)
		{
			XmlNodeList result = xml.SelectNodes(path);
			bool flag = XmlMod.allSettings.advancedDebugging && PatchManager.applyingPatches;
			if (flag)
			{
				foreach (string name in Helpers.GetDefsFromPath(path, xml))
				{
					PatchManager.ModPatchedDef(name, null, operation.GetType());
				}
			}
			return result;
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x0000CD38 File Offset: 0x0000AF38
		public static XmlNode SelectSingleNode(string path, XmlDocument xml, PatchOperation operation)
		{
			XmlNode xmlNode = xml.SelectSingleNode(path);
			bool flag = XmlMod.allSettings.advancedDebugging && PatchManager.applyingPatches;
			if (flag)
			{
				string defNameFromNode = Helpers.GetDefNameFromNode(xmlNode);
				PatchManager.ModPatchedDef(defNameFromNode, null, operation.GetType());
			}
			return xmlNode;
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x0000CD84 File Offset: 0x0000AF84
		public static string GetNameFromName(string name)
		{
			bool flag = name == null;
			string result;
			if (flag)
			{
				result = null;
			}
			else
			{
				List<string> list = new List<string>(name.Split(new char[]
				{
					';'
				}));
				bool flag2 = list.Count != 2;
				if (flag2)
				{
					result = null;
				}
				else
				{
					result = "(" + ((list[0][0] == '@') ? "@Name=\"" : "defName=\"") + list[1] + "\")";
				}
			}
			return result;
		}
	}
}
