using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Principle4.DryLogic
{
  public class ObjectDefinition<T> : ObjectDefinition
  {
    public ObjectDefinition() : base(typeof(T))
    {

    }

		public PropertyDefinition<U> AddProperty<U>(Expression<Func<T, U>> propertyAccessor, Action<PropertyDefinition<U>> initializer)
		{
			String propertyName = Utility.GetPropertyName(propertyAccessor);
			return Properties.Add(
				propertyName,
				Utility.AddSpacesToCapitalCase(propertyName),
				initializer
				);
		}
		public PropertyDefinition<U> AddProperty<U>(Expression<Func<T, U>> propertyAccessor, string humanName, Action<PropertyDefinition<U>> initializer)
		{
			String propertyName = Utility.GetPropertyName(propertyAccessor);
			return Properties.Add(
				propertyName,
				humanName,
				initializer
				);
		}

        //not sure if this should be here...oi already has validate methods that point back to the object definition...redundant
        public Boolean Validate(T dryObject)
        {
            var oi = ObjectInstance.GetObjectInstance(dryObject);
            return this.Validate(oi);
        }

        public Boolean Validate(T dryObject, out List<RuleViolation> ruleViolations)
        {
            var oi = ObjectInstance.GetObjectInstance(dryObject);
            return this.Validate(oi, out ruleViolations);

        }
    }

	public abstract class ObjectDefinition : IValidatable 
  {
    public static ObjectDefinition GetObjectDefinition(Type objectType, Boolean throwException)
    {
      if (ObjectInstance.CheckIsDryObject(objectType, throwException) == false)
        return null;
      
      var bovAttrib = (DryLogicObjectAttribute)objectType.GetCustomAttributes(typeof(DryLogicObjectAttribute), true)[0];


      var odFieldInfo = objectType.GetField(bovAttrib.DefinitionPropertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
      if (odFieldInfo == null)
      {
        if (throwException)
        {
          throw new DryLogicException($"Field '{bovAttrib.InstancePropertyName}' as specified by the BOVObjectAttribute could not be found on type '{objectType}'.");
        }
        return null;
      }
      return (ObjectDefinition)odFieldInfo.GetValue(null);
    }


    public string SystemName { get; private set; }
    public string FriendlyName { get; private set; }
    public Type ObjectType { get; private set; }

    public ObjectDefinition(Type objectType)
    {
      ObjectType = objectType;
      Properties = new PropertyDictionary(this);
      SystemName = ObjectType.Name;
      FriendlyName = Utility.AddSpacesToCapitalCase(SystemName);
    }

    public ObjectDefinition(Type objectType, String systemName, String friendlyName) : this(objectType)
    {
      SystemName = systemName;
      FriendlyName = friendlyName;
    }
    public Rule AddRule(Rule rule)
    {
      throw new NotImplementedException();
    }

    public ObjectInstance CreateInstance(Object parentObject)
    {
      return new ObjectInstance(this, parentObject);
    }

		public Boolean Validate(ObjectInstance oi)
		{
			return ValidateProperties(oi); //&& //TODO: check class level rules
		}
		public Boolean Validate(ObjectInstance oi, out List<RuleViolation> ruleViolations)
		{
			return ValidateProperties(oi, out ruleViolations);
			//TODO: Also include class level rules
		}
		public Boolean ValidateProperties(ObjectInstance oi, out List<RuleViolation> ruleViolations)
		{
			ruleViolations = GetPropertyRuleViolations(oi).ToList();
			return !ruleViolations.Any();
		}
		public Boolean ValidateProperties(ObjectInstance oi)
		{
			return !GetPropertyRuleViolations(oi).Any();
		}

		public IEnumerable<RuleViolation> GetRuleViolations(ObjectInstance oi)
		{
			return GetPropertyRuleViolations(oi);
			//TODO: also do class level rules
		}

		public IEnumerable<RuleViolation> GetPropertyRuleViolations(ObjectInstance oi)
    {
      RuleViolationFromException ruleViolationFromException=null;
      Boolean successfullyReturned = false;
      foreach (var propDef in this.Properties.Values)
      {
        RuleViolation ruleViolation;
        if (!propDef.Validate(oi, out ruleViolation))
        {
          //if the current violation was the result of an exception...
          if (ruleViolation is RuleViolationFromException)
          {
            //and if a clean rule violation has not yet been found, and we've not yet caught an exception...
            if (successfullyReturned == false && ruleViolationFromException == null)
            {
              ruleViolationFromException = (RuleViolationFromException)ruleViolation;
            }
          }
          else //found a rule that was able to evaluate.  From here we can assume the previous exception occured because some property was not valid
          {
            successfullyReturned = true;
            ruleViolationFromException = null;
            yield return ruleViolation;

          }
        }
      }
      //if we managed to exit the loop and all we found was a faulted evaluation...
      if (ruleViolationFromException != null)
      {
        //...then something must be wrong with either the ordering of the rule or the rule itself
        throw new DryLogicException(ruleViolationFromException.ErrorMessage, ruleViolationFromException.CaughtException);
      }
    }




    public PropertyDictionary Properties { get; private set; }

		//determines the default value for CreateImplicitRulesEnabled for properties added to this object
		//public Boolean CreateImplicitRulesPropertyDefault { get; set; }
  }
}
