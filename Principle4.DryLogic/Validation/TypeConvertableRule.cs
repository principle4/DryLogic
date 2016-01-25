using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Principle4.DryLogic.Validation
{
  public class TypeConvertableRule : PropertyRule
  {
    public TypeConvertableRule(PropertyDefinition propertyDefinition)
      : base(propertyDefinition)
    {
      base.Assertion = oi =>
      {
        var propertyValue = oi.GetUntypedValue(propertyDefinition);
        //a type is converted at the time it's string value is set, so all we need to do is check to see if that was successful.
        return propertyValue.TypedValueIsAvailable;
      };	

      base.ErrorMessageGenerator = () => String.Format(
        "{0} must be a valid {1}.", 
        propertyDefinition.CurrentName, 
        App.CurrentContext.IsHumanInterface ? GetHumanNameForType(propertyDefinition.ValueType) : propertyDefinition.ValueType.ToString());
    }

    //at some point we may need a provider/dependency injection so that this can be controlled by the developer.
    private String GetHumanNameForType(Type type)
    {
      String typeName;
      if (type == typeof(Int64)
        || type == typeof(UInt64)
        || type == typeof(Int32)
        || type == typeof(UInt32)
        || type == typeof(Int16)
        || type == typeof(UInt16)
        || type == typeof(SByte)
        || type == typeof(Byte)
        || type == typeof(Single)
        || type == typeof(Decimal)
        || type == typeof(Double))
      {
        typeName = "number";
      }
      else if (type == typeof(DateTime))
      {
        typeName = "date";
      }
      else if (type == typeof(Char))
      {
        typeName = "single character";
      }
      else if (type == typeof(Boolean))
      {
        typeName = "true/false value";
      }
      else
      {
        typeName = type.ToString();
      }
      return typeName;
    }


  }
}
