using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Principle4.DryLogic.Validation
{
  public class PropertyRule : Rule
  {
    public PropertyRule(PropertyDefinition propertyDefinition)
    {
      Property = propertyDefinition;
    }


    public PropertyDefinition Property { get; private set; }
  }
}
