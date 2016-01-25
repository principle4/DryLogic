using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Principle4.DryLogic
{
	public static class UtilityExtensions
	{
		public static Boolean IsNullable(this Type type)
		{
			if (!type.IsValueType) return true; // ref-type
			if (Nullable.GetUnderlyingType(type) != null) return true; // Nullable<T>
			return false; // value-type
		}
	}
}
