using Principle4.DryLogic.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Principle4.DryLogic.MVC
{
  public class DryLogicClientDataTypeModelValidatorProvider : ClientDataTypeModelValidatorProvider
  {
    //needed to disable to regular ClientDataTypeModelValidatorProvider since it causes the -number and -date unobtrusive attributes to be emited,
    //but I didn't want to disable that for all properties, just bov properties
    public override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context)
    {
      var od = ObjectDefinition.GetObjectDefinition(metadata.ContainerType, false);
      //if this is a BOV property...
      if (od != null && od.Properties.ContainsKey(metadata.PropertyName))
      {
        //...then do nothing, the BovModelValidatoProvider will already handle it...
        return Enumerable.Empty<ModelValidator>();
      }

      //fallback to the default functionality
      return base.GetValidators(metadata, context);

    }

  }
}