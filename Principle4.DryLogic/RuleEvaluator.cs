using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Principle4.DryLogic
{
  //would it be better to pass in the rule and value and let this evaluate in real time?  RuleEvaluator instead of RuleEvaluation.
  // not really

  public class RuleEvaluator
  {
    public static Boolean CheckRuleIsValid(Rule ruleToEval, ObjectInstance oi, out RuleViolation violatedRule)
    {
      try
      {
        if (CheckCondition(ruleToEval.Condition, oi))
        {
          if (ruleToEval.Assertion(oi) != true)
          {
            violatedRule = new RuleViolation(ruleToEval, oi);
            return false;
          }
          else
          {
            violatedRule = null;
            return true;
          }
        }
        violatedRule = null;
        return true;
      }
      catch(Exception ex)
      {
        violatedRule = new RuleViolationFromException(ruleToEval, oi, ex);
        return false;
      }
    }
    
    private static Boolean CheckCondition(Func<ObjectInstance, Boolean> condition, ObjectInstance oi)
    {
      if(condition == null)
        return true;
      else
      {
        try
        {
          return condition(oi);
        }
        //if the condition is dependent on another field which is also invalid, it will fail.
        catch(Exception ex)
        {
          if(Debugger.IsAttached)
          {
            Debug.WriteLine("Rule '{0}' not applied - condition invalid with exception:");
            Debug.WriteLine(ex);
          }
          throw;
        }
      }
    }
    
    public static RuleViolation GetFirstRuleViolation(IEnumerable<Rule> rules, ObjectInstance oi)
    {
      foreach (var rule in rules)
      {
        RuleViolation ruleViolation;
        //property rules should be evalated up until an invalid rule is found or a conditional rule is found 
        // (we'd have to make sure everything else is valid and then go back and do conditional rules 
        // (does a rule need some sort of flag indicating that it can be evaluated immediately or only after all immediate rules?)
        //update: it's not just conditions as rules that check other property values would also be affected
        //for now just catch the exceptions and or maybe return a special rule violation
        if (!CheckRuleIsValid(rule, oi, out ruleViolation))
        {
          return ruleViolation;
          //break;//found an invalid rule, no point in continuing
        }
      }

      return null;
    }
  }
  public class RuleViolation
  {
    internal RuleViolation(Rule appliedRule, ObjectInstance oi)
    {
      AppliedRule = appliedRule;
      InvalidObject = oi;
    }
    public Rule AppliedRule { get; private set; }
    public ObjectInstance InvalidObject { get; private set; }
    public virtual String ErrorMessage
    {
      get
      {
        return AppliedRule.ErrorMessageGenerator(InvalidObject);
      }
    }
 
  }
  public class RuleViolationFromException : RuleViolation
  {
    public override string ErrorMessage
    {
      get
      {
        return String.Format("Rule '{0}' evaluation aborted due to an exception.", AppliedRule.Id);
      }
    }
    public Exception CaughtException { get; private set; }


    public RuleViolationFromException(Rule appliedRule, ObjectInstance oi, Exception ex) : base(appliedRule, oi)
    {
      CaughtException = ex;
    }
  }
}
