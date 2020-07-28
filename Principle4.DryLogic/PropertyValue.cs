using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Principle4.DryLogic
{
  public abstract class PropertyValue //needed for PropertyValues dictionary in object instance - will be mixed types
  {

    //I can't think of a reason I need to do this.  We need the abstract class NOT for centralizing shared
    //functionality - we need it to provide access without a concern for the Type T in PropertyValue(T)
    //protected object baseValue;
    //protected object BaseValue
    //{
    //  get { return this.baseValue; }
    //  set
    //  {
    //    this.baseValue = value;
    //    this.stringValue = value.ToString();
    //    Initialized = true;
    //  }
    //}
    public abstract object Value { get; set;  }
    public Type ValueType { get; protected set; }

    private string stringValue;
    public string StringValue
    {
      get { return this.stringValue; }
      set
      {
        if (this.stringValue != value)
        {
          this.stringValue = value;
          TrySetValueFromString();
          
          //stuff like this makes me second guess creating property values on the fly,  would make more sense if it
          //was it's own event and event instance would listen. I suppose we could register the instant the value is created?
          this.ParentInstance.OnPropertyChanged(this.Definition.SystemName);

        }
      }
    }
	public Boolean HasAnyValue
	{
		get => !String.IsNullOrEmpty(StringValue);
	}
    public abstract string FormattedValue{get;}


    internal abstract void TrySetValueFromString();

    public ObjectInstance ParentInstance { get; private set; }
    public PropertyDefinition Definition { get; private set; }
    
    public PropertyValue(ObjectInstance parentInstance, PropertyDefinition definition)
    {
      ParentInstance = parentInstance;
      Definition = definition;
    }


    public abstract bool IsValid
    {
      get;
    }

    public bool TypedValueIsAvailable { get; protected set; }

    public override string ToString()
    {
      return StringValue;
    }

    //not sure yet if something like this is needed.  My thinking was that it might make sense to try to return the typed value to a binder
    // but still be able to return null for non-nullable types.  The original inspiration was formatters...  I can't bind 
    //to Value since it might not have a value yet and would throw an excetion but if I always bind to StringValue, 
    //does that put BOV on the hook for formatting or a separate display field?  The old bFrame proxy would try to 
    // return the strongly typed value if it could, so consequently formatters still worked on things like the 
    //text display on the WPFToolkit.Extended.DateTimePicker.  A formatter property in BOV does have it's advantages though
    // - it would be better aware of how to parse the value coming back in the case of the web.  I'll need a specific case before I do it I guess
    // ModelMetadata.GetSimpleDisplayText uses CultureInfo.CurrentCulture
    //Update: Gave it a shot - see FormattedValue
    internal Object BestValue
    {
      get
      {
        throw new NotImplementedException("Not ready to consider if this is usefull yet.");
        if (TypedValueIsAvailable)
          return Value;
        else
          return StringValue;
      }
    }

  }


  public class PropertyValue<T> : PropertyValue
  {
    public PropertyValue(ObjectInstance parentInstance, PropertyDefinition<T> parentProperty)
      : base(parentInstance, parentProperty)
    {
      ValueType = typeof(T);
      if (ValueType.IsNullable())
        TypedValueIsAvailable = true;
    }

    T typedValue;
    public T TypedValue { 
      get
      {
        if (TypedValueIsAvailable)
          return typedValue;
        else
        {
          throw new DryLogicException(
            String.Format("Property '{0}' is in an invalid state.  It has either not been set or it's backing string value of '{1}' is not convertable to it's type.",
            this.Definition.SystemName,
            this.StringValue
            )
          );
        }
      }
      internal set
      {
        SetTypedValue(value, true);
      }
    }

    private void SetTypedValue(T value, Boolean setString)
    {
      this.typedValue = value;
      TypedValueIsAvailable = true;
      if (setString)
        StringValue = value?.ToString(); //could be null in the case of a string
    }

    private void UnsetTypedValue()
    {
      this.typedValue = default(T);
      TypedValueIsAvailable = false;
    }

    //needed a way to get and accept type object without worring about directly setting base.Value to something that wasn't supported
    //by type T (nulls in particular).  
    //Value -> TypedValue -> BaseValue
    public override object Value
    {
      get
      {
        return (Object)TypedValue;
      }
      set
      {
        TypedValue = (T)value;
      }
    }

    public override string FormattedValue
    {
      get { return this.Definition.FormatValue(this); } 
    } 

    public override bool IsValid
    {
      get 
      {
			RuleViolation violation;
			return this.Definition.Validate(this.ParentInstance, out violation);
      }
    }



    internal override void TrySetValueFromString()
    {
      //ref: http://www.dogaoztuzun.com/post/C-Generic-Type-Conversion.aspx
      TypeConverter tc = TypeDescriptor.GetConverter(ValueType);
      try
      {
        SetTypedValue(
          (T)tc.ConvertFromString(StringValue),
          false);
      }
      catch (System.Exception ex)
      {
        UnsetTypedValue();
        
      }
    }

  }

}
