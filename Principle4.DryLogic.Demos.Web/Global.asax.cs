using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace Principle4.DryLogic.Demos.Web
{
  public class Global : HttpApplication
  {
    void Application_Start(object sender, EventArgs e)
    {
      // Code that runs on application startup
      AreaRegistration.RegisterAllAreas();
      RouteConfig.RegisterRoutes(RouteTable.Routes);


      ModelMetadataProviders.Current = new Principle4.DryLogic.MVC.DryLogicModelMetadataProvider();
      //ModelBinders.Binders.DefaultBinder = new Principle4.DryLogic.MVC.BOVModelBinder2();

      ModelBinderProviders.BinderProviders.Add(new Principle4.DryLogic.MVC.DryLogicModelBinderProvider());


      DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = false;

      App.CurrentContext.IsHumanInterface = true;

      ModelValidatorProviders.Providers.Add(new Principle4.DryLogic.MVC.DryLogicModelValidatorProvider());


      var existingProvider = ModelValidatorProviders.Providers
          .Single(x => x is ClientDataTypeModelValidatorProvider);
      ModelValidatorProviders.Providers.Remove(existingProvider);
      ModelValidatorProviders.Providers.Add(new Principle4.DryLogic.MVC.DryLogicClientDataTypeModelValidatorProvider());

    }
  }
}