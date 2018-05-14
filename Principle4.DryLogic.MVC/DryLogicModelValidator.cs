using Principle4.DryLogic.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Principle4.DryLogic.MVC
{
  public class DryLogicModelValidator : ModelValidator
  {
    public DryLogicModelValidator(ModelMetadata metadata, ControllerContext controllerContext) : base(metadata, controllerContext)
    {

    }
    public override IEnumerable<ModelValidationResult> Validate(object container)
    {
      return Enumerable.Empty<ModelValidationResult>();
      //throw new NotImplementedException();
    }

    public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
    {
      var od = ObjectDefinition.GetObjectDefinition(Metadata.ContainerType, true);
      //validation should call this and the context is 'this'!!!
      var propDef = od.Properties[Metadata.PropertyName];
      foreach (Rule rule in propDef.Rules)
      {
				if (rule.ErrorMessageStaticGenerator != null)
				{
					if (rule is RequiredRule)
						yield return new ModelClientValidationRequiredRule(rule.ErrorMessageStaticGenerator());
					else if (rule is StringLengthRule)
						yield return new ModelClientValidationStringLengthRule(rule.ErrorMessageStaticGenerator(), ((StringLengthRule)rule).MinimumLength, ((StringLengthRule)rule).MaximumLength);
					else if (rule is RegexRule)
						yield return new ModelClientValidationRegexRule(rule.ErrorMessageStaticGenerator(), ((RegexRule)rule).Pattern);
					else if (rule is RangeRule)
						yield return new ModelClientValidationRangeRule(rule.ErrorMessageStaticGenerator(), ((RangeRule)rule).MinimumValue, ((RangeRule)rule).MaximumValue);
					//very helpful ideas:
					//http://stackoverflow.com/questions/4828297/how-to-change-data-val-number-message-validation-in-mvc-while-it-is-generated
					else if (rule is TypeConvertableRule && propDef.ValueType == typeof(DateTime))
						yield return new ModelClientValidationRule()
						{
							//data-val-number
							ValidationType = "date",
							ErrorMessage = rule.ErrorMessageStaticGenerator()
						};
				}
			}
    }
  }
}