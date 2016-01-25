using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Principle4.DryLogic.Validation;

namespace Principle4.DryLogic.Web.MVC
{
  public class MVCUtil
  {
    public static Dictionary<String, Object> GetValidationAttributes(PropertyValue pv)
    {
      //http://stackoverflow.com/questions/11124880/jquery-unobtrusive-validation-attributes-reference
      var valAttribs = new Dictionary<String, Object>();

      foreach (Rule rule in pv.Definition.Rules)
      {
        if (rule is RequiredRule)
        {
          valAttribs.Add("data-val-required", rule.ErrorMessageGenerator());
        }
        //else if (rule is TypeConvertableRule)
        //{
        //  valAttribs.Add("data-val-required", rule.ErrorMessageFormatter(pv.ParentInstance));
        //}
      }
      if (valAttribs.Count > 0)
      {
        valAttribs.Add("data-val", "true");
      }
      return valAttribs;

    }

  }
}