using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Principle4.DryLogic.Validation;

namespace Principle4.DryLogic
{
  public abstract class PropertyDefinition : IValidatable
  {

    public PropertyDefinition(String systemName, String humanName, Type type, PropertyDictionary owner)
    {
      SystemName = systemName;
      HumanName = humanName;
      ValueType = type;
      Owner = owner;

      Rules = new List<Rule>();

    }


    public string SystemName { get; private set; }

    public string HumanName { get; private set; }

    public string CurrentName { 
      get
      {
        return App.CurrentContext.IsHumanInterface ? HumanName : SystemName;
      }
    }

    public PropertyDictionary Owner { get; private set; }

    public Type ValueType { get; private set; }

    internal abstract String FormatValue(PropertyValue v);

    public abstract PropertyValue GetUntypedValue(ObjectInstance oi);
    //this was redundant
    //public abstract PropertyValue SetUntypedValue(ObjectInstance oi, Object value);
    //public PropertyValue SetStringValue(ObjectInstance oi, String stringValue)
    //{
    //  var propertyValue = GetUntypedValue(oi);
    //  propertyValue.StringValue = stringValue;
    //  return propertyValue;
    //}


    public List<Rule> Rules { get; private set; }
    public Rule AddRule(Rule rule)
    {
      if (String.IsNullOrWhiteSpace(rule.Id))
        rule.Id = this.SystemName + "_" + Rules.Count.ToString();
      Rules.Add(rule);
      return rule;
    }

    public RuleViolation GetFirstRuleViolation(ObjectInstance oi)
    {
      //not wild about some of these one line wrapper functions, but it makes calling code neater.
      return RuleEvaluator.GetFirstRuleViolation(Rules, oi);
    }

    internal Boolean Validate(ObjectInstance oi, out RuleViolation ruleViolation)
    {

      ruleViolation = GetFirstRuleViolation(oi);
      return ruleViolation == null;
    }
    internal abstract PropertyValue CreatePropertyValue(ObjectInstance oi);
  }




  public class PropertyDefinition<T> : PropertyDefinition
  {

    public Func<T, String> Formatter { get; set; }

    internal override string FormatValue(PropertyValue v)
    {
      if (Formatter == null || v.TypedValueIsAvailable == false)
      {
        return v.StringValue;
      }
      else
      {
        return Formatter(((PropertyValue<T>)v).TypedValue);
      }
    }




    public PropertyDefinition(String systemName, String humanName, PropertyDictionary owner) 
      : base(systemName, humanName, typeof(T), owner)
    {
    }

    internal PropertyValue<T> GetPropertyValue(ObjectInstance oi)
    {
			try
			{
				return (PropertyValue<T>)oi.PropertyValues[this.SystemName];
			}
			catch(Exception ex)
			{
				throw new InvalidOperationException($"An error occured retrieving value for property {this.SystemName}",ex);
			}

	}

    //created this so the GetValue wasn't so verbose in the rules
    public T this[ObjectInstance oi]
    {
      get { return GetPropertyValue(oi).TypedValue; }
    }

    internal override PropertyValue CreatePropertyValue(ObjectInstance oi)
    {
        return new PropertyValue<T>(oi, this);
    }

    public override PropertyValue GetUntypedValue(ObjectInstance oi)
    {
      return (PropertyValue)GetPropertyValue(oi);
    }

    //redundant - GetPropertyValue could be called and then .Value can be called.
    //public PropertyValue<T> SetValue(ObjectInstance oi, T value)
    //{
    //  var propertyValue = GetPropertyValue(oi);
    //  propertyValue.Value = value;
    //  return propertyValue;
    //}


    //public override PropertyValue SetUntypedValue(ObjectInstance oi, object value)
    //{
    //  return (PropertyValue)SetValue(oi, (T)value);
    //}

    //public bool CreateImplicitRulesEnabled { get; set; }

    internal void Init(Action<PropertyDefinition<T>> initialization)
    {
      //this.CreateImplicitRulesEnabled = Owner.Owner.CreateImplicitRulesPropertyDefault;
      if (initialization != null)
        initialization(this);
      //This is a bad idea since #1 - A regular expression validator could replace a type validator,
      //  #2 - It adds a lot of questions to simply replace a single, simple call to .IsRequired
      //if (CreateImplicitRulesEnabled)
      //{
      //  Type type = typeof(T);
      //  //if the type isn't nullable...
      //  if (type.IsValueType)
      //  {
      //    //...then add a required rule
      //    //rule and it's prop definition are double linked - does it make sense here to pass "this"
      //    AddRule(new RequiredRule(this));
          
      //    var nullableType = Nullable.GetUnderlyingType(type);
      //    if (nullableType != null)
      //      type = nullableType;
      //  }

      //  //
      //  if(type != null && App.KnownValueTypes.Contains(type))
      //  {
      //    AddRule(new TypeValidationRule(this));
      //  }
      //}
    }
  }
}
