using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Principle4.DryLogic.Validation
{
  public class StringLengthRule : PropertyRule, IStaticErrorMessage
  {
    public int MinimumLength { get; private set; }
    public int MaximumLength { get; private set; }
    public StringLengthRule(PropertyDefinition propertyDefinition, int minLength, int maxLength) : base(propertyDefinition)
    {
      MinimumLength = minLength;
      MaximumLength = maxLength;

      base.Assertion = oi =>
      {
        var stringValue = oi.GetUntypedValue(propertyDefinition).StringValue;
        if (stringValue == null)
        {
          return true;
        }
        return stringValue.Length >= MinimumLength && stringValue.Length <= MaximumLength;
      };

      base.ErrorMessageGenerator = (oi) => String.Format("'{0}' must be between {1} and {2}.", propertyDefinition.CurrentName, MinimumLength, MaximumLength);
    }

		public string ErrorMessage
		{
			get
			{
				return ErrorMessageGenerator(null);
			}
		}
	}
}
