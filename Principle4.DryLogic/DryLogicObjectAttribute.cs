using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Principle4.DryLogic
{
  [System.AttributeUsage(AttributeTargets.Class)]
  public class DryLogicObjectAttribute : Attribute
  {
    public String InstancePropertyName { get; set; }
    public String DefinitionPropertyName { get; set; }

    public DryLogicObjectAttribute()
    {
      InstancePropertyName = "OI";
      DefinitionPropertyName = "OD";
    }
  }
}
