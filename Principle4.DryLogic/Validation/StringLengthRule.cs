using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Principle4.DryLogic.Validation
{
	public class StringLengthRule : PropertyRule
	{
		public int MinimumLength { get; private set; }
		public int MaximumLength { get; private set; }
		public StringLengthRule(PropertyDefinition propertyDefinition, int minLength, int maxLength) : base(propertyDefinition)
		{
			if (minLength < 1)
				throw new ArgumentOutOfRangeException(nameof(minLength), $"{nameof(minLength)} on {nameof(StringLengthRule)} for property {propertyDefinition.Owner.Owner.SystemName}.{propertyDefinition.SystemName} must be greater than 0.  To check for an empty field, use a RequiredRule.");
			MinimumLength = minLength;
			MaximumLength = maxLength;

			base.Assertion = oi =>
			{
				var stringValue = oi.GetUntypedValue(propertyDefinition).StringValue;
				var propVal = oi.GetUntypedValue(propertyDefinition);


				if (!propVal.HasAnyValue)
				{
					return true;
				}

				return stringValue.Length >= MinimumLength && stringValue.Length <= MaximumLength;
			};

			if (MaximumLength == MinimumLength)
			{
				base.ErrorMessageStaticGenerator = () =>
				  $"'{propertyDefinition.CurrentName}' must be {MinimumLength} characters in length.";
			}
			else
			{
				base.ErrorMessageStaticGenerator = () =>
				  $"'{propertyDefinition.CurrentName}' must be between {MinimumLength} and {MaximumLength} characters in length.";
			}
		}
	}
}
