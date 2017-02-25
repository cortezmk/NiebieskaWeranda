using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NiebieskaWeranda.Binders;
using NiebieskaWeranda.Models;
using NiebieskaWeranda.Providers;

namespace NiebieskaWeranda
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            ModelMetadataProviders.Current = new RadioButtonEnumsModelMetadataProvider();
            ModelBinders.Binders.Add(typeof(DateTime), new DateFormatModelBinder());
            ModelBinders.Binders.Add(typeof(ReservationRequestModel), new DateFormatModelBinder());
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            var tempCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            tempCulture.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy";
            Thread.CurrentThread.CurrentCulture = tempCulture;
        }
    }
}
