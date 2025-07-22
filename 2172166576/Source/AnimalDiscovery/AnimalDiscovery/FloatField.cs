using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AnimalDiscovery
{
    public class FloatField
    {
		public float Value
		{
			get
			{
				return this.value;
			}
		}

		public FloatField(float value)
		{
			this.value = value;
			this.buffer = value.ToString();
		}

		public bool Update(Rect rect)
		{
			this.buffer = Widgets.TextField(rect, this.buffer);
			float num;
			if (FloatField.IsFullyTypedFloat(this.buffer) && float.TryParse(this.buffer, out num))
			{
				this.value = num;
				return true;
			}
			return false;
		}

		private static bool IsFullyTypedFloat(string str)
		{
			if (str == string.Empty)
			{
				return false;
			}
			string[] array = str.Split(new char[]
			{
				'.'
			});
			return array.Length <= 2 && array.Length >= 1 && FloatField.ContainsOnlyCharacters(array[0], "-0123456789") && (array.Length != 2 || FloatField.ContainsOnlyCharacters(array[1], "0123456789"));
		}

		private static bool ContainsOnlyCharacters(string str, string allowedChars)
		{
			for (int i = 0; i < str.Length; i++)
			{
				if (!allowedChars.Contains(str[i]))
				{
					return false;
				}
			}
			return true;
		}

		private string buffer = "";

		private float value;
	}
}
