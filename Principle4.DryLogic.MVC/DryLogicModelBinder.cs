using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Principle4.DryLogic.Validation;
using System.Reflection;
using System.ComponentModel;

namespace Principle4.DryLogic.MVC
{
  //Experimental - not complete - easier to do a custom HtmlHelper for now
  //http://www.codeproject.com/Articles/605595/ASP-NET-MVC-Custom-Model-Binder
  public class DryLogicModelBinder : DefaultModelBinder
  {

    protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, System.ComponentModel.PropertyDescriptor propertyDescriptor)
    {
      //make sure this is actually a property that can be set (don't want to provide a back door to overposting)
      var prop = bindingContext.ModelType.GetProperty(propertyDescriptor.DisplayName, BindingFlags.Public | BindingFlags.Instance);
      if (prop == null || prop.CanWrite == false)
        throw new InvalidOperationException($"Property '{propertyDescriptor.DisplayName}' cannot be written to.");


      var oi = ObjectInstance.GetObjectInstance(bindingContext.Model, false);
      //if this isn't a BOV backed property...
      if(oi.ObjectDefinition.Properties.ContainsKey(propertyDescriptor.DisplayName) == false)
        //...then use the default binder
        base.BindProperty(controllerContext, bindingContext, propertyDescriptor);

      //BOV binder
      else
      {
        //base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
        
        var request = controllerContext.HttpContext.Request;
        string prefix = bindingContext.ModelName;
        if (!String.IsNullOrEmpty(prefix))
          prefix += ".";

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
        var oi = ObjectInstance.GetObjectInstance(obj, true);
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
    protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
    {
      //base.OnModelUpdated(controllerContext, bindingContext);

      //start reflected code
      Dictionary<string, bool> dictionary = new Dictionary<string, bool>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
      foreach (ModelValidationResult validationResult in ModelValidator.GetModelValidator(bindingContext.ModelMetadata, controllerContext).Validate((object)null))
      {
        string subPropertyName = DefaultModelBinder.CreateSubPropertyName(bindingContext.ModelName, validationResult.MemberName);
        if (!dictionary.ContainsKey(subPropertyName))
          dictionary[subPropertyName] = bindingContext.ModelState.IsValidField(subPropertyName);
        if (dictionary[subPropertyName])
          bindingContext.ModelState.AddModelError(subPropertyName, validationResult.Message);
      }
      //end reflected code

    }

    protected override object GetPropertyValue(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor, IModelBinder propertyBinder)
    {
      return base.GetPropertyValue(controllerContext, bindingContext, propertyDescriptor, propertyBinder);
    }

    protected override void SetProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor, object value)
    {
      base.SetProperty(controllerContext, bindingContext, propertyDescriptor, value);
    }

    protected override bool OnPropertyValidating(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor, object value)
    {
      return base.OnPropertyValidating(controllerContext, bindingContext, propertyDescriptor, value);
    }
  } 

}