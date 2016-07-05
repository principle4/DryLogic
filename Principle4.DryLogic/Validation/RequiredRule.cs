using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Principle4.DryLogic.Validation
{
  public class RequiredRule : PropertyRule, IStaticErrorMessage
  {
    public RequiredRule(PropertyDefinition propertyDefinition) : base(propertyDefinition)
    {
      base.Assertion = oi => !String.IsNullOrWhiteSpace(oi.GetUntypedValue(propertyDefinition).StringValue);

      base.ErrorMessageGenerator = (oi) => String.Format("'{0}' is required.", propertyDefinition.CurrentName);
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
