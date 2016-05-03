using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace Principle4.DryLogic.MVC
{
  public class DryLogicModelMetadataProvider : DataAnnotationsModelMetadataProvider
  {
    protected override ModelMetadata CreateMetadata(IEnumerable<Attribute> attributes, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName)
    {
      Object container = null;
      //containerTYpe is null for simple types
      if(
        containerType != null 
        && ObjectInstance.IsBOVObject(containerType)
        && ObjectDefinition.GetObjectDefinition(containerType, true).Properties.ContainsKey(propertyName)
        )
      {

        if (modelAccessor != null)
        {
          var rootModelType = modelAccessor.Target.GetType();
          var field = rootModelType.GetField("container");
          if (field != null)
          {
            container = field.GetValue(modelAccessor.Target);
            //if we don't have a reference to the container yet...
            if (container.GetType() != containerType)
            {
              //...then try to break down the expression to get it
              //get the expression as text, ie "model.EmployeeViewModel.MyEmployee" and split it
              var expressionParts = ((LambdaExpression)rootModelType.GetField("expression").GetValue(modelAccessor.Target)).Body.ToString().Split('.');
              //var expressionParts = new string[] { };

              //loop thru the parts in the middle
              for (int i = 1; i < expressionParts.Length - 1; i++)
              {
                container = container.GetType().GetProperty(expressionParts[i]).GetValue(container);
              }
            }
            var oi = ObjectInstance.GetObjectInstance(container, true);

            var modelMetadata = new ModelMetadata(this, containerType, modelAccessor, modelType, propertyName);
            modelMetadata.Container = container;
						//internally, setting model wipes out modelAccessor (caching of sorts)
						modelMetadata.Model = oi.PropertyValues[propertyName].FormattedValue;

            modelMetadata.DisplayName = oi.PropertyValues[propertyName].Definition.CurrentName;
            //we could make this a configurable option
            //modelMetadata.TemplateHint = "PropertyValue";
            return modelMetadata;
          }

        }
      }
      return base.CreateMetadata(attributes, containerType, modelAccessor, modelType, propertyName);
    }
  }

  public class BOVPropertyMetadata : ModelMetadata
  {
    public BOVPropertyMetadata(ModelMetadataProvider provider, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName) 
      : base(provider, containerType, modelAccessor, modelType, propertyName)
    {
    }
  }
}