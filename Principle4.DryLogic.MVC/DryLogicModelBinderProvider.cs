using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Principle4.DryLogic.MVC
{
  public class DryLogicModelBinderProvider : IModelBinderProvider
  {
    public IModelBinder GetBinder(Type modelType)
    {
      if (ObjectInstance.IsDryObject(modelType))
        return new DryLogicModelBinder();


      return null;
    }
  }
}