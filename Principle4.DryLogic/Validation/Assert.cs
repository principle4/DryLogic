using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Principle4.DryLogic.Validation
{
  public class Assert
  {

    //public static RuleBuilder<T> That<T>(T definition)  where T : IValidatable
    //{
    //  RuleBuilder<T> ruleBuilder = new RuleBuilder<T>(definition);
    //  return ruleBuilder;
    //}

    public static ObjectRuleBuilder That(ObjectDefinition o)
    {
      throw new NotImplementedException();
    }

    public static PropertyRuleBuilder That(PropertyDefinition p)
    {
      return new PropertyRuleBuilder(p);
    }
  }
}
