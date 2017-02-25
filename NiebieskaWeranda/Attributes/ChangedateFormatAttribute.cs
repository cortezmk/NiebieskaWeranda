using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace NiebieskaWeranda.Attributes
{
    public class ChangeDateFormatAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var tempCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            tempCulture.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy";
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("pl-PL");
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("pl-PL");
        }
    }
}