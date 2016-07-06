using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Principle4.DryLogic.Validation
{
  public class InvalidStringRule : PropertyRule
  {
    public string[] InvalidStrings { get; private set; }
    public InvalidStringRule(PropertyDefinition propertyDefinition, params string[] invalidStrings) : base(propertyDefinition)
    {
      InvalidStrings = invalidStrings;

      base.Assertion = oi =>
      {
        var stringValue = oi.GetUntypedValue(propertyDefinition).StringValue;
        if (stringValue == null)
        {
          return true;
        }
        return !InvalidStrings.Any(ic => stringValue.Contains(ic));
      };

      base.ErrorMessageInstanceGenerator = (oi) =>
        $"'{propertyDefinition.CurrentName}' cannot contain any of the following values {string.Join(",", InvalidStrings)}.";
    }
	}
}
