using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Principle4.DryLogic
{
	public class Utility
	{
		public static string AddSpacesToCapitalCase(string value)
		{
			if (string.IsNullOrWhiteSpace(value)) return string.Empty;

			string final = string.Empty;
			for (int i = 0; i < value.Length; i++)
			{
				final += (Char.IsUpper(value[i]) && ((i == 0 || !Char.IsUpper(value[i - 1])) ||
													 (i != (value.Length - 1) && !Char.IsUpper(value[i + 1]))) ?
						  " " : "") + value[i];
			}

			return final.TrimStart(' ');
		}

		public static string GetPropertyName<TModel, TProperty>(Expression<Func<TModel, TProperty>> property)
		{
			var lambda = (LambdaExpression)property;

			MemberExpression memberExpression;
			if (lambda.Body is UnaryExpression)
			{
				var unaryExpression = (UnaryExpression)lambda.Body;
				memberExpression = (MemberExpression)unaryExpression.Operand;
			}
			else
			{
				memberExpression = (MemberExpression)lambda.Body;
			}

			return memberExpression.Member.Name;
		}

		/// <summary>
		/// Attempts to return the value of the property, but returns default(T) if the property value is not available
		/// </summary>
		/// <typeparam name="dryObj">Dry Object Type</typeparam>
		/// <typeparam name="T">Parameter Type</typeparam>
		/// <param name="dryObject">Dry Object</param>
		/// <param name="propertyPath">expression to the property</param>
		/// <returns>Property value or default(T) if not available</returns>
		public static T TryGetValue<dryObj, T>(dryObj dryObject, Expression<Func<dryObj, T>> propertyPath)
		{
			return TryGetValue(dryObject, propertyPath, default(T));
		}

		/// <summary>
		/// Attempts to return the value of the property, but returns fallbackValue if the property value is not available
		/// </summary>
		/// <typeparam name="dryObj">Dry Object Type</typeparam>
		/// <typeparam name="T">Parameter Type</typeparam>
		/// <param name="dryObject">Dry Object</param>
		/// <param name="propertyPath">expression to the property</param>
		/// <param name="fallbackValue">value to return, if a typed value is not available</param>
		/// <returns>Property value or fallbackValue if not available</returns>
		public static T TryGetValue<dryObj, T>(dryObj dryObject, Expression<Func<dryObj, T>> propertyPath, T fallbackValue)
		{
			var propertyValue = ObjectInstance.GetObjectInstance(dryObject).PropertyValues[GetPropertyName(propertyPath)];
			if (propertyValue.TypedValueIsAvailable)
				return (T)propertyValue.Value;
			return fallbackValue;
		}

	}
}