using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Principle4.DryLogic.Validation
{
  public class RequiredRule : PropertyRule
  {
    public RequiredRule(PropertyDefinition propertyDefinition) : base(propertyDefinition)
    {
      base.Assertion = oi => !String.IsNullOrWhiteSpace(oi.GetUntypedValue(propertyDefinition).StringValue);

      base.ErrorMessageGenerator = () => String.Format("'{0}' is required.", propertyDefinition.CurrentName);
    }

  }
}
