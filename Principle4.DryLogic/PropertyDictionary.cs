using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Principle4.DryLogic
{
  public class PropertyDictionary : Dictionary<String, PropertyDefinition>
  {
    public ObjectDefinition Owner { get; private set; }

    public PropertyDictionary(ObjectDefinition owner)
    {
      Owner = owner;
    }

    public PropertyDefinition<T> Add<T>(
      String systemName, 
      String humanName, 
      Action<PropertyDefinition<T>> initialization)
    {

      PropertyDefinition<T> prop = new PropertyDefinition<T>(systemName, humanName, this);
      prop.Init(initialization);
      
      base.Add(systemName, prop);
      return prop;
    }
  }
}
