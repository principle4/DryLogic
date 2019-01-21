using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;

namespace Principle4.DryLogic
{
  public class ObjectInstance : INotifyPropertyChanged, IDataErrorInfo
  {
    public static ObjectInstance GetObjectInstance(Object obj)
    {
      return GetObjectInstance(obj, false);
    }
    internal static ObjectInstance GetObjectInstance(Object obj, Boolean throwException)
    {
        if (obj == null)
            throw new ArgumentNullException("obj");

      var objectType = obj.GetType();
      CheckIsDryObject(objectType, throwException);
      var bovAttrib = (DryLogicObjectAttribute)objectType.GetCustomAttributes(typeof(DryLogicObjectAttribute),true)[0];


      PropertyInfo prop = objectType.GetProperty(bovAttrib.InstancePropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      if (prop == null)
      {
        if (throwException)
        {
          throw new DryLogicException($"Property '{bovAttrib.InstancePropertyName}' as specified by the DryLogicObject attribute could not be found on type '{objectType}'.");
        }
        return null;
      }
      return (ObjectInstance)prop.GetValue(obj, null);
    }
    public static Boolean IsDryObject(Object obj)
    {
      return IsDryObject(obj.GetType());
    }
    public static Boolean IsDryObject(Type type)
    {
      return CheckIsDryObject(type, false);
    }

    //It might make more sense to move this to object definition.  Not sure.
    public static Boolean CheckIsDryObject(Type type, Boolean throwException)
    {
      if (type == null)
      {
        return false;
      }
      else
      {
        var defined = Attribute.IsDefined(type, typeof(DryLogicObjectAttribute));
        if (throwException && !defined)
        {
          throw new DryLogicException("The given object/type is not marked as a DryLogicObject.  Did you forget to mark the class with a [DryLogicObject] attribute?");
        }
        return defined;
      }
    }

    public ObjectDefinition ObjectDefinition { get; private set; }

    public Object ParentObject { get; private set; }

    public PropertyValueDictionary PropertyValues { get; private set; }
      

    internal ObjectInstance(ObjectDefinition objectDefinition, Object parentObject)
    {
      this.ObjectDefinition = objectDefinition;
      this.ParentObject = parentObject;
      PropertyValues = new PropertyValueDictionary(this);

    }

    //operations must be done relative to the propertyDefinition sinced that is where the type is known.
    //However, symantically these methods make more sense since people are used to the container.Set(key) pattern
    //more than they are to key.set(container) pattern
    public PropertyValue GetUntypedValue(PropertyDefinition propertyDefinition)
    {
      return propertyDefinition.GetUntypedValue(this);
    }
    ///decided to make the above public and create this to avoid getting the property value via the definition when it's right here
    public PropertyValue GetUntypedValue(String propertyName)
    {
      return ObjectDefinition.Properties[propertyName].GetUntypedValue(this);
    }

    public T GetValue<T>(PropertyDefinition<T> propertyDefinition)
    {
      //return propertyDefinition.GetPropertyValue(this).TypedValue;
      return propertyDefinition[this];
    }

    public void SetValue<T>(PropertyDefinition<T> propertyDefinition, T value)
    {
      propertyDefinition.GetPropertyValue(this).TypedValue = value;
    }

		public Boolean Validate()
		{
			return ObjectDefinition.Validate(this);
		}
		public Boolean ValidateProperties()
		{
			return ObjectDefinition.ValidateProperties(this);
		}
		public Boolean Validate(out List<RuleViolation> ruleViolations)
    {
      return ObjectDefinition.Validate(this, out ruleViolations);
    }
    public IEnumerable<RuleViolation> GetRuleViolations()
    {
      return ObjectDefinition.GetRuleViolations(this);
    }

    public void RaiseChangedForAllProperties()
    {
      foreach (String key in ObjectDefinition.Properties.Keys)
      {
        this.OnPropertyChanged(key);
      }
    }



    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;
    internal void OnPropertyChanged(string propertyName)
    {
      if (this.PropertyChanged != null)
        this.PropertyChanged(
            this, new PropertyChangedEventArgs(propertyName));
    }
 
    #endregion

    #region IDataErrorInfo Members

    public string Error
    {
      get {
        //might be worth adding a short circuit into isvalid (although I don't think that "Error" is commonly used over the indexer)
        List<RuleViolation> ruleViolations;
        if (!Validate(out ruleViolations) && ruleViolations.Any())
        {
          return ruleViolations.First().ErrorMessage;
        }
        else
          return "";
      }
    }

    public string this[string columnName]
    {
      get {
        if (this.ObjectDefinition.Properties.ContainsKey(columnName) == false)
          return "";

        var propDef = this.ObjectDefinition.Properties[columnName];
        RuleViolation ruleViolation;
        if (!propDef.Validate(this, out ruleViolation))
        {
          return ruleViolation.ErrorMessage;
        }
        else
        {
          return "";
        }
      }
    }

    #endregion
  }
}
