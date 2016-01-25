using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Principle4.DryLogic.MVC
{
  public class DryLogicModelValidatorProvider : ModelValidatorProvider
  {
    //it's somewhat counter intuitive, but remember that this is called with metadata for EACH property so the returned validator already has that properties context.
    public override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context)
    {
      //if this is a bov property...
      if(ObjectDefinition.GetObjectDefinition(metadata.ContainerType,false)?.Properties.ContainsKey(metadata.PropertyName) == true)
        return new[] { new DryLogicModelValidator(metadata, context) };

      return Enumerable.Empty<ModelValidator>();
    }
  }
}