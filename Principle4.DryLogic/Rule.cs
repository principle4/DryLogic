using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Principle4.DryLogic
{
  public class Rule
  {
 
    public Func<ObjectInstance, bool> Assertion { get; set; }

    //public Func<ObjectInstance, String> ErrorMessageFormatter { get; set; }
    public Func<String> ErrorMessageGenerator { get; set; }

    public String ErrorMessage {
      get
      {
        return ErrorMessageGenerator();
      }
    }

    public Func<ObjectInstance, bool> Condition { get; set; }

    public String Id { get; set; }

    public Rule()
    {
      //add the default error message formatter
      ErrorMessageGenerator = () => String.Format("Rule {0} is invalid.", Id);
    }
	}
}
