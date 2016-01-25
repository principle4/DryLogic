using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;

namespace Principle4.DryLogic
{
  //Note 2/27/2014 - changed this to be more of a string proxy instead.  Being able to access any property by string OR by value was overcomplicating things 
  /// <summary>
  /// 
  /// </summary>
  public class DryLogicProxy :DynamicObject, IDataErrorInfo, INotifyPropertyChanged
  {
    Object proxiedObject = null;
    ObjectInstance objectInstance = null;

    public DryLogicProxy(Object objectToProxy)
    {
      this.proxiedObject = objectToProxy;
      dynamic dynamicObject = objectToProxy;
      PropertyInfo prop = objectToProxy.GetType().GetProperty("OI", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

      this.objectInstance = ObjectInstance.GetObjectInstance(objectToProxy, true);
      //this.domainObject = dynamicObject.DomainContainer;

      //if the parent object implements INotifyPropertyChanged...
      if (this.proxiedObject is INotifyPropertyChanged)
      {
        //then wire it up
        ((INotifyPropertyChanged)proxiedObject).PropertyChanged += new PropertyChangedEventHandler(Object_PropertyChanged);
      }
      else //parent does not implement INotify...
      {
        //wire up the domain object instead
        this.objectInstance.PropertyChanged += new PropertyChangedEventHandler(Object_PropertyChanged);
      }
    }





    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
      //if this is not a DomainProperty...
      if (!objectInstance.ObjectDefinition.Properties.ContainsKey(binder.Name))
      {
        //...try to get it as a normal property
        PropertyInfo propInfo = this.proxiedObject.GetType().GetProperty(binder.Name);
        if (propInfo != null)
        {
          result = propInfo.GetValue(this.proxiedObject, null);
          return true;
        }
        else
        {
          result = null;
          return false;
        }
      }
      else  //is a DomainProperty, get it from the Properties collection
      {
        PropertyDefinition prop = objectInstance.ObjectDefinition.Properties[binder.Name];
        var propValue = this.objectInstance.GetUntypedValue(prop);

        result = propValue.StringValue;
        return true;
      }
    }


    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
      //if this is not a DomainProperty...
      if (!objectInstance.ObjectDefinition.Properties.ContainsKey(binder.Name))
      {
        //...try to set it as a normal property
        PropertyInfo propInfo = this.proxiedObject.GetType().GetProperty(binder.Name);
        if (propInfo != null)
        {
          propInfo.SetValue(this.proxiedObject, value, null);
          return true;
        }
        else //property not found.
        {
          if (Debugger.IsAttached)
          {
            Debug.WriteLine("BOVProxy TrySetMember failed: Property '{0}' could not be found on object '{1}'", binder.Name, objectInstance.ObjectDefinition.SystemName);
          }
          return false;
        }
      }
      else
      {
        var propertyDefinition = objectInstance.ObjectDefinition.Properties[binder.Name];
        var propertyValue = propertyDefinition.GetUntypedValue(objectInstance);
        propertyValue.StringValue = value.ToString();
        return true;
      }
    }

    //passthru property changed events from the proxied object
    void Object_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      OnPropertyChanged(e.PropertyName);
    }

    public Boolean Validate()
    {
      return objectInstance.Validate();
    }
    public Boolean Validate(out List<RuleViolation> ruleViolations)
    {
      return this.objectInstance.ObjectDefinition.Validate(objectInstance, out ruleViolations);
    }

    public void RaiseChangedForAllProperties()
    {
      objectInstance.RaiseChangedForAllProperties();
    }



    #region IDataErrorInfo Members

    public string Error
    {
      get {
        if (IsValidationSuppressed)
          return null;
        else
          return this.objectInstance.Error; 
      }
    }

    public string this[string columnName]
    {
      get {
        if(columnName == nameof(IsValidationSuppressed))
           return null;
        if (IsValidationSuppressed)
          return null;

        return this.objectInstance[columnName];   
      }
    }

    #endregion

    public Boolean IsValidationSuppressed { 
      get;
      set; 
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;
    void OnPropertyChanged(string propertyName)
    {
      if (this.PropertyChanged != null)
        this.PropertyChanged(
            this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
  }
}
