using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Principle4.DryLogic.Validation
{
  public class RangeRule : PropertyRule, IStaticErrorMessage
  {
    public object MinimumValue { get; private set; }
    public object MaximumValue { get; private set; }
    private Func<object, object> Conversion
    {
      get;
      set;
    }

    public RangeRule(PropertyDefinition propertyDefinition, object minValue, object maxValue) : base(propertyDefinition)
    {
      MinimumValue = minValue;
      MaximumValue = maxValue;

      base.Assertion = oi =>
      {
        var stringValue = oi.GetUntypedValue(propertyDefinition).StringValue;
        if (stringValue == null)
        {
          return true;
        }

        Type propertyType = propertyDefinition.ValueType;
        object min;
        object max;

        try
        {
          min = Convert.ChangeType(MinimumValue, propertyType);
          max = Convert.ChangeType(MaximumValue, propertyType);
        }
        catch (InvalidCastException cx)
        {
          return false;
        }

        var objValue = oi.GetUntypedValue(propertyDefinition).Value;
        if (propertyType == typeof(int))
        {
          return (int)objValue >= (int)minValue && (int)objValue <= (int)maxValue;
        }
        if (propertyType == typeof(double) || propertyType == typeof(float))
        {
          return (double)objValue >= (double)minValue && (double)objValue <= (double)maxValue;
        }
        if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime))
        {
          return (DateTime)objValue >= (DateTime)minValue && (DateTime)objValue <= (DateTime)maxValue;
        }
        throw new InvalidOperationException("Range type must be one of the following types: int, float, double, DateTime");
      };

      base.ErrorMessageGenerator = (oi) =>
        $"'{propertyDefinition.CurrentName}' must be between {MinimumValue} and {MaximumValue}.";
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
