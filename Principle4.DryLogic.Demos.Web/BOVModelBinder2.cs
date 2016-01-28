using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Principle4.DryLogic.Validation;
using System.Reflection;

namespace Principle4.DryLogic.MVC
{
  //Experimental - not complete - easier to do a custom HtmlHelper for now
  //http://www.codeproject.com/Articles/605595/ASP-NET-MVC-Custom-Model-Binder
  public class BOVModelBinder2 : DefaultModelBinder
  {

    protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, System.ComponentModel.PropertyDescriptor propertyDescriptor)
    {
      if (!Attribute.IsDefined(bindingContext.ModelType, typeof(DryLogicObjectAttribute)) || propertyDescriptor.PropertyType == typeof(ObjectInstance))
        base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
      else
      {
        var oi = ObjectInstance.GetObjectInstance(bindingContext.Model);
        //base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
        
        var request = controllerContext.HttpContext.Request;
        string prefix = bindingContext.ModelName;
        if (!String.IsNullOrEmpty(prefix))
          prefix += ".";

        //make sure this is actually a property that can be set (don't want to provide a back door to overposting)
        var prop = bindingContext.ModelType.GetProperty(propertyDescriptor.DisplayName, BindingFlags.Public | BindingFlags.Instance);
        if (prop == null || prop.CanWrite == false)
          throw new InvalidOperationException($"Property '{propertyDescriptor.DisplayName}' cannot be written to.");

        if (oi.PropertyValues[propertyDescriptor.DisplayName].ValueType == typeof(Boolean))
        {
          //mvc rendered checkboxes with an extra hidden tag so that an unchecked input still returns a value.
          //  unfortunately this also means that a checked value returns the value of both so it comes back as "true,false"
          oi.PropertyValues[propertyDescriptor.DisplayName].Value = !(request.Form[prefix + propertyDescriptor.DisplayName] == "false");
        }
        else
        {
          oi.PropertyValues[propertyDescriptor.DisplayName].StringValue = request.Form[prefix + propertyDescriptor.DisplayName];
        }
      }
    }


    public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
    {
      var obj = base.BindModel(controllerContext, bindingContext);
      //if this is a BOV backed object...
      if (Attribute.IsDefined(bindingContext.ModelType, typeof(DryLogicObjectAttribute)))
      {
        //...then get the violated rules add add them to the modelstate
        var oi = ObjectInstance.GetObjectInstance(obj);
        foreach(RuleViolation violation in oi.GetRuleViolations())
        {
          String modelStateKey = null;
          if (violation.AppliedRule is PropertyRule)
          {
            var propertyRule = (PropertyRule)violation.AppliedRule;
            //string prefix = bindingContext.ModelMetadata.DisplayName;
            string prefix = bindingContext.FallbackToEmptyPrefix? "" : bindingContext.ModelName;
            if (!String.IsNullOrEmpty(prefix))
              prefix += ".";
            modelStateKey = prefix + propertyRule.Property.SystemName;
          }
          else
          {
            modelStateKey = bindingContext.ModelName;
          }
          bindingContext.ModelState.AddModelError(modelStateKey, violation.ErrorMessage);
        }
      }
      return obj;

    }
  } 

}