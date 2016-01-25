using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Principle4.DryLogic.Validation
{
  public class ObjectRuleBuilder
  {
    private ObjectDefinition parentObject; //ObjectDefinition, PropertyDefinition
    public Rule Rule { get; private set; }

    public ObjectRuleBuilder(ObjectDefinition parent)
    {
      this.parentObject = parent;
    }

    internal Rule AddRule(Rule rule)
    {
      Rule = rule;
      this.parentObject.AddRule(rule);
      return rule;
    }

    public ObjectRuleBuilder AdhearsTo(Func<ObjectInstance, Boolean> ruleTest)
    {
      var rule = AddRule(new Rule());
      return this;
    }

    public ObjectRuleBuilder When(Func<ObjectInstance, Boolean> condition)
    {
      Rule.Assertion = condition;
      return this;
    }


    internal void Notifying(params PropertyDefinition[] properties)
    {
      throw new NotImplementedException();
    }
  }
}
