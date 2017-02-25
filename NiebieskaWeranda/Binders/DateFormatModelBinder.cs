using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace NiebieskaWeranda.Binders
{
    public class DateFormatModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            //            var displayFormat = bindingContext.ModelMetadata.DisplayFormatString;
            //            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            //
            //            if (string.IsNullOrEmpty(displayFormat) || value == null)
            //            {
            //                return base.BindModel(controllerContext, bindingContext);
            //            }
            //            DateTime date;
            //            displayFormat = displayFormat.Replace("{0:", string.Empty).Replace("}", string.Empty);
            //            // use the format specified in the DisplayFormat attribute to parse the date
            //            if (DateTime.TryParseExact(value.AttemptedValue, displayFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            //            {
            //                return date;
            //            }
            //            bindingContext.ModelState.AddModelError(
            //                bindingContext.ModelName,
            //                $"{value.AttemptedValue} is an invalid date format"
            //                );
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("pl-PL");
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("pl-PL");
            return base.BindModel(controllerContext, bindingContext);
        }
    }

    public class DateFormatAttribute : CustomModelBinderAttribute
    {
        private readonly IModelBinder _binder;

        public DateFormatAttribute()
        {
            _binder = new DateFormatModelBinder();
        }

        public override IModelBinder GetBinder() { return _binder; }
    }
}