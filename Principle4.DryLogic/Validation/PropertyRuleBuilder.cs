using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Principle4.DryLogic.Validation
{
  public class PropertyRuleBuilder
  {
    public PropertyDefinition Property { get; private set; } //ObjectDefinition, PropertyDefinition
    //"current" wasn't the original route I was planning on going and did it by accident,
		//  orginally I planned on having an "And" operator that created a new rule builder but now I'm not sure how this helps.
		public PropertyRule CurrentRule { get; private set; }

    public PropertyRuleBuilder(PropertyDefinition property)
    {
      this.Property = property;
    }

    internal PropertyRule AddRule(PropertyRule rule)
    {
      CurrentRule = rule;
      this.Property.AddRule(rule);
      return rule;
    }

		public PropertyRuleBuilder IsAdhearingTo(Func<ObjectInstance, Boolean> assertion)
		{
			var rule = new PropertyRule(this.Property);
			rule.Assertion = assertion;
			this.AddRule(rule);
			return this;
		}

		public PropertyRuleBuilder When(Func<ObjectInstance, Boolean> condition)
		{
      CurrentRule.Condition = condition;
			return this;
		}


    public PropertyRuleBuilder IdentifiedBy(String ruleId)
    {
      CurrentRule.Id = ruleId;
      return this;
    }
    public PropertyRuleBuilder WithMessage(Func<String> errorMessageGenerator)
    {
      CurrentRule.ErrorMessageGenerator = new Func<ObjectInstance, String>( (oi) => errorMessageGenerator());
      return this;
    }

    //assumes a single placeholder {0} to be replaced by current name
    public PropertyRuleBuilder WithMessage(String errorMessageFormatter)
    {
      CurrentRule.ErrorMessageGenerator
        = new Func<ObjectInstance,string>(
          (oi) => String.Format(errorMessageFormatter, CurrentRule.Property.CurrentName));
      return this;
    }
		public PropertyRuleBuilder WithMessage(Func<ObjectInstance, String> errorMessageGenerator)
		{
			CurrentRule.ErrorMessageGenerator = errorMessageGenerator;
			return this;
		}
  }
  public static class RuleBuilderExtensions
  {
    public static PropertyRuleBuilder IsRequired(this PropertyRuleBuilder rb)
    {
      rb.AddRule(new RequiredRule(rb.Property));
      return rb;
    }
    public static PropertyRuleBuilder IsConvertable(this PropertyRuleBuilder rb)
    {
      rb.AddRule(new TypeConvertableRule(rb.Property));
      return rb;
    }

    public static PropertyRuleBuilder IsStringLengthBetween(this PropertyRuleBuilder rb, int minLength, int maxLength)
    {
      rb.AddRule(new StringLengthRule(rb.Property, minLength, maxLength));
      return rb;
    }
    public static PropertyRuleBuilder IsValidString(this PropertyRuleBuilder rb, params string[] invalidStrings)
    {
      rb.AddRule(new InvalidStringRule(rb.Property, invalidStrings));
      return rb;
    }
    public static PropertyRuleBuilder IsMatchingPattern(this PropertyRuleBuilder rb, string pattern)
    {
      rb.AddRule(new RegexRule(rb.Property, pattern));
      return rb;
    }
    public static PropertyRuleBuilder IsBetween(this PropertyRuleBuilder rb, object minimum, object maximum)
    {
      rb.AddRule(new RangeRule(rb.Property, minimum, maximum));
      return rb;
    }
    //public static RuleBuilder<PropertyDefinition<String>> IsRequired(this RuleBuilder<PropertyDefinition<String>> rb)
    //{
    //  rb.AddRule(new RequiredRule());
    //  return rb;
    //}
  }

}
