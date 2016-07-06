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
		Func<ObjectInstance, String> errorMessageInstanceGenerator = null;
		public Func<ObjectInstance, String> ErrorMessageInstanceGenerator {
			get
			{
				if(errorMessageInstanceGenerator == null)
				{
					return new Func<ObjectInstance, String>(oi => ErrorMessageStaticGenerator());
				}
				return errorMessageInstanceGenerator;
			}
			set
			{
				errorMessageInstanceGenerator = value;
			}


		}

		public Func<String> ErrorMessageStaticGenerator { get; set; }



		// 7/2106 - Needed a way to sometimes display the value in the message and this only works when this isn't the case
		//public String ErrorMessage {
		//    get
		//    {
		//      return ErrorMessageStaticGenerator();
		//    }
		//  }

		public Func<ObjectInstance, bool> Condition { get; set; }

    public String Id { get; set; }

    public Rule()
    {
      //add the default error message formatter
      ErrorMessageStaticGenerator = () => String.Format("Rule {0} is invalid.", Id);
    }
	}
}
