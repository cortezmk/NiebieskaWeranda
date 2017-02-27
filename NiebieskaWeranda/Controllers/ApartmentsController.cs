using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using NiebieskaWeranda.Attributes;
using NiebieskaWeranda.Binders;
using NiebieskaWeranda.HelperClasses;
using NiebieskaWeranda.Models;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace NiebieskaWeranda.Controllers
{
    public class GetTotalPriceResult
    {
        public decimal TotalPrice { get; set; }
        public int TotalNights { get; set; }
    }

    public class ApartmentsController : SurfaceController
    {
        private DateTime ParseDate(string date)
        {
            return DateTime.ParseExact(date, "dd.MM.yyyy", CultureInfo.InvariantCulture);
        }

        private void CheckDates(string dateFrom, string dateTo)
        {
            DateTime from = DateTime.Now, to = DateTime.Now.AddDays(1);
            try
            {
                from = ParseDate(dateFrom);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ArrivalDate", "Nieprawidłowy format daty.");
            }
            try
            {
                to = ParseDate(dateTo);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("DepartureDate", "Nieprawidłowy format daty.");
            }
            if (from > to)
            {
                ModelState.AddModelError("DepartureDate", "Data przyjazdu nie może być póżniejsza od daty wyjazdu.");
            }
            if (from < DateTime.Now.AddDays(-1))
            {
                ModelState.AddModelError("ArrivalDate", "Data przyjazdu musi być późniejsza niż wczorajsza.");
            }
        }

        // GET: Apartments
        [HttpPost]
        [ChangeDateFormat]
        public ActionResult SubmitRequest(ReservationRequestModel model)
        {
            ViewBag.IsReturning = true;

            CheckDates(model.ArrivalDate, model.DepartureDate);
//            if(model.NumberOfPersons > )

            if (!ModelState.IsValid)
            {
                //Perhaps you might want to add a custom message to the ViewBag
                //which will be available on the View when it renders (since we're not 
                //redirecting)
                TempData["ErrorMessage"] = "Popraw błędy i spróbuj ponownie przesłać formularz.";
                return CurrentUmbracoPage();
            }
            if (CheckReservationTermAlreadyBooked(model.ApartmentName, ParseDate(model.ArrivalDate), ParseDate(model.DepartureDate)))
            {
                TempData["ErrorMessage"] =
                    "Wybrane mieszkanie jest niestety zajęte w podanym czasie. Proszę wybrać inny termin.";
                return CurrentUmbracoPage();
            }

            var reservationResult = AddReservation(model);
            SendEmail(model, reservationResult ? string.Empty : "Otrzymano nowy wniosek o rezerwację jednak z powodu błędu nie został on zapisany:");

            //Add a message in TempData which will be available 
            //in the View after the redirect 
            TempData["SuccessMessage"] = "Twój wniosek o rezerwację został poprawnie zarejestrowany. Prosimy o oczekiwanie na kontakt.";

            //redirect to current page to clear the form
            var url = Umbraco.Url(CurrentPage.Id);
            return Redirect($"{url}#reservation");
        }

        private bool CheckReservationTermAlreadyBooked(string apartmentName, DateTime arrival, DateTime departure)
        {
            var reservations =
                Umbraco.TypedContentAtRoot()
                    .First(c => c.DocumentTypeAlias == "Rezerwacje")
                    ?.Children.Where(c => c.DocumentTypeAlias == "Rezerwacja")
                    ?.Where(c => c.GetPropertyValue<string>("apartament") == apartmentName)
                    ?.Where(c => c.GetPropertyValue<string>("status") == "zaakceptowana")
                    ?.Select(
                        c =>
                            new
                            {
                                Arrival = c.GetPropertyValue<DateTime>("dataPrzyjazdu"),
                                Departure = c.GetPropertyValue<DateTime>("dataWyjazdu")
                            });
            if (reservations == null)
            {
                return false;
            }
            return reservations.Any(
                r => ((r.Arrival.Date != departure && (r.Departure.Date != arrival)) && 
                    (r.Arrival <= arrival && r.Departure > arrival) ||
                    (r.Arrival < departure && r.Departure >= departure)));
        }

        private bool CompareDates(DateTime date1, DateTime date2)
        {
            return date1.Month == date2.Month && date1.Year == date2.Year;
        }

        public GetTotalPriceResult GetTotalPriceInner(string apartmentName, string sFrom, string sTo, int numberOfPersons)
        {
            var sum = 0m;
            var from = ParseDate(sFrom);
            var to = ParseDate(sTo);

            if (from >= to)
            {
                return null;
            }
            var curDate = from;
            while(true)
            {
                var priceStrings = GetPriceStrings(apartmentName, curDate.Year, curDate.Month);
                var numFrom = 0;
                var numTo = DateTime.DaysInMonth(curDate.Year, curDate.Month);
                if (CompareDates(curDate, from))
                {
                    numFrom = from.Day;
                }
                if (CompareDates(curDate, to))
                {
                    numTo = to.Day;
                }
                sum += priceStrings.Skip(numFrom)
                    .Take(numTo - numFrom)
                    .Select(c => decimal.Parse(c.Replace(",", "."), CultureInfo.InvariantCulture))
                    .Sum();
                if (CompareDates(curDate, to))
                {
                    break;
                }
                curDate = curDate.AddMonths(1);
            }
            var totalNights = (to - @from).Days;
            sum += Math.Max(numberOfPersons - 2, 0) * GetAdditionalPrice(apartmentName) * totalNights;
            return new GetTotalPriceResult {TotalPrice = sum, TotalNights = totalNights};
        }

        public ActionResult GetTotalPrice(string apartmentName, string dateFrom, string dateTo, int numberOfPersons)
        {
            var result = GetTotalPriceInner(apartmentName, dateFrom, dateTo, numberOfPersons);
            return result == null ? Json(new {success = false}) : Json(new { totalPrice = result.TotalPrice, totalNights = result.TotalNights, success = true });
        }

        private static void SetReservationStatus(IContentBase apartment, string valueName)
        {
            var dts = new DataTypeService();
            var statusEditor = dts.GetAllDataTypeDefinitions().First(x => x.Name == "Status Rezerwacji");
            var preValueId = dts.GetPreValuesCollectionByDataTypeId(statusEditor.Id).PreValuesAsDictionary.Where(d => d.Value.Value == valueName).Select(f => f.Value.Id).First();
            apartment.SetValue("status", preValueId);
        }

        private bool AddReservation(ReservationRequestModel model)
        {
            var reservationsNode = Umbraco.TypedContentAtRoot().First(c => c.DocumentTypeAlias == "Rezerwacje");
            var dt = Services.ContentTypeService.GetContentType("Rezerwacja");
            var doc = new Content($"{model.FirstName} {model.LastName}", reservationsNode.Id, dt);
            doc.SetPropertyValue("imie", model.FirstName);
            doc.SetPropertyValue("nazwisko", model.LastName);
            doc.SetPropertyValue("email", model.Email);
            doc.SetPropertyValue("apartament", model.ApartmentName);
            doc.SetPropertyValue("dataPrzyjazdu", ParseDate(model.ArrivalDate));
            doc.SetPropertyValue("dataWyjazdu", ParseDate(model.DepartureDate));
            doc.SetPropertyValue("iloscOsob", model.NumberOfPersons);
            doc.SetPropertyValue("dodatkoweInformacje", model.Comments);
            doc.SetPropertyValue("ulica", model.Street);
            doc.SetPropertyValue("kodPocztowy", model.Zip);
            doc.SetPropertyValue("miasto", model.City);
            doc.SetPropertyValue("kraj", model.Country);
            doc.SetPropertyValue("telefon", model.Phone);
            SetReservationStatus(doc, "oczekująca");
            var status = Services.ContentService.SaveAndPublishWithStatus(doc);
            return status.Success;
        }

        private string CreateMailBody(ReservationRequestModel model)
        {
            var price = GetTotalPriceInner(model.ApartmentName, model.ArrivalDate, model.DepartureDate, model.NumberOfPersons);
            return $@"Nazwa apartamentu: <b>{model.ApartmentName}</b><br>
Cena całkowita: <b>{price.TotalPrice} PLN</b><br>
Data przyjazdu: <b>{model.ArrivalDate} godz. 15:00</b><br>
Data wyjazdu: <b>{model.DepartureDate} godz. 11:00</b><br>
Liczba nocy: <b>{price.TotalNights}</b><br>
Ilość osób: <b>{model.NumberOfPersons}</b><br>
Dodatkowe informacje: <b>{model.Comments}</b><br>
<br>
Imię: <b>{model.FirstName}</b><br>
Nazwisko: <b>{model.LastName}</b><br>
Ulica: <b>{model.Street}</b><br>
Kod pocztowy: <b>{model.Zip}</b><br>
Miasto: <b>{model.City}</b><br>
Kraj: <b>{model.Country}</b><br>
Telefon: <b>{model.Phone}</b><br>
Email: <b>{model.Email}</b>";
//            return string.Join(Environment.NewLine,
//                model.GetType()
//                    ?.GetProperties()
//                    ?.Where(p => p.GetCustomAttribute<SkipInEmailAttribute>() == null)
//                    ?.Select(
//                        p =>
//                            $"{p.GetCustomAttribute<DisplayAttribute>()?.Name ?? string.Empty}: <b>{p.GetValue(model)?.ToString() ?? string.Empty}</b>") ?? new string[0]);
        }

        public ActionResult GetReservationInfo(string apartmentName, int year, int month)
        {
            var reservationStrings = GetReservationStrings(apartmentName, year, month);
            var priceStrings = GetPriceStrings(apartmentName, year, month);
            return Content($"{{ \"reservedDays\":{{{string.Join($",{Environment.NewLine}", reservationStrings)}}}, \"prices\": [{string.Join(",", priceStrings)}]}}", "application/json");
        }

        private decimal GetAdditionalPrice(string apartmentName)
        {
            var apartment = Umbraco.TypedContentAtRoot()
                .First(c => c.DocumentTypeAlias == "StronaGlowna").Children
                .First(c => c.DocumentTypeAlias == "Apartamenty")
                .Children.First(a => a.Name == apartmentName);
            var price = apartment.GetPropertyValue<string>("dodatkowaCena");
            decimal result;
            return decimal.TryParse(price, out result) ? result : 0m;
        }

        private int GetMaxPersons(string apartmentName)
        {
            var apartment = Umbraco.TypedContentAtRoot()
                .First(c => c.DocumentTypeAlias == "StronaGlowna").Children
                .First(c => c.DocumentTypeAlias == "Apartamenty")
                .Children.First(a => a.Name == apartmentName);
            return apartment.GetPropertyValue<int>("maxIloscOsob");
        }

        private IEnumerable<string> GetPriceStrings(string apartmentName, int year, int month)
        {
            var dateLower = new DateTime(year, month, 1);
            var dateUpper = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            var apartment = Umbraco.TypedContentAtRoot()
                .First(c => c.DocumentTypeAlias == "StronaGlowna").Children
                .First(c => c.DocumentTypeAlias == "Apartamenty")
                .Children.First(a => a.Name == apartmentName);
            var defaultPrice = apartment.GetPropertyValue<string>("domyslnaCena");
            var priceNodes =
                apartment
                    .Children.Where(c => c.DocumentTypeAlias == "Cena")
                    .Where(c => c.GetPropertyValue<DateTime>("dataDo") >= dateLower && c.GetPropertyValue<DateTime>("dataOd") <= dateUpper)
                    .OrderBy(c => c.GetPropertyValue<DateTime>("dataDo") - c.GetPropertyValue<DateTime>("dataOd")).ToList();
            var priceStrings = new List<string>();
            for (var i = 1; i <= DateTime.DaysInMonth(year, month); i++)
            {
                var foundNode = priceNodes.FirstOrDefault(n => n.GetPropertyValue<DateTime>("dataDo").Day >= i &&
                                               n.GetPropertyValue<DateTime>("dataOd").Day <= i);
                priceStrings.Add(foundNode == null ? defaultPrice : foundNode.GetPropertyValue<string>("cena"));
            }
            return priceStrings;
        }

        private IEnumerable<string> GetReservationStrings(string apartmentName, int year, int month)
        {
            var reservationStrings = new List<string>();
            var reservations =
                Umbraco.TypedContentAtRoot()
                    .First(c => c.DocumentTypeAlias == "Rezerwacje")
                    ?.Children.Where(c => c.DocumentTypeAlias == "Rezerwacja")
                    ?.Where(c => c.GetPropertyValue<string>("apartament") == apartmentName)
                    ?.Where(c => c.GetPropertyValue<string>("status") == "zaakceptowana")
                    ?.Select(
                        c =>
                            new ReservationPeriod
                            {
                                Arrival = c.GetPropertyValue<DateTime>("dataPrzyjazdu"),
                                Departure = c.GetPropertyValue<DateTime>("dataWyjazdu")
                            })
                    ?
                    .Where(
                        d =>
                            (d.Arrival.Year == year && d.Arrival.Month == month) ||
                            (d.Departure.Year == year && d.Departure.Month == month))
                    ?.OrderBy(d => d.Arrival).ToList();
            if (reservations == null)
            {
                return reservationStrings;
            }

            // Unite reservations which have overlapping days.
            var i = 0;
            while (i < reservations.Count - 1)
            {
                if (reservations[i].Departure.Date == reservations[i + 1].Arrival.Date)
                {
                    reservations[i].Departure = reservations[i + 1].Departure;
                    reservations.RemoveAt(i + 1);
                }
                else
                {
                    i++;
                }
            }

            foreach (var reservation in reservations)
            {
                var firstDay = reservation.Arrival.Month == month ? reservation.Arrival.Day : 1;
                var lastDay = reservation.Departure.Month == month
                    ? reservation.Departure.Day
                    : DateTime.DaysInMonth(year, month);
                var isBorderedLeft = reservation.Arrival.Month != month;
                var isBorderedRight = reservation.Departure.Month != month;
                if (firstDay > lastDay)
                {
                    continue;
                }
                if (firstDay == lastDay)
                {
                    if (reservation.Arrival.Hour > reservation.Departure.Hour)
                    {
                        continue;
                    }
                    reservationStrings.Add($"\"{firstDay}\": [12, 12]");
                }
                else
                {
                    if (isBorderedLeft)
                    {
                        reservationStrings.Add($"\"{firstDay}\": [0, 24]");
                    }
                    else
                    {
                        reservationStrings.Add($"\"{firstDay}\": [12, 24]");
                    }
                    for (i = firstDay + 1; i < lastDay; i++)
                    {
                        reservationStrings.Add($"\"{i}\": [0, 24]");
                    }
                    if (isBorderedRight)
                    {
                        reservationStrings.Add($"\"{lastDay}\": [0, 24]");
                    }
                    else
                    {
                        reservationStrings.Add($"\"{lastDay}\": [0, 12]");
                    }
                }
            }
            return reservationStrings;
        }

        private void SendEmail(ReservationRequestModel model, string additionalMessage = "")
        {
            var smtpClient = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("niebieskaweranda@gmail.com", "kingsofmetal8008")
            };
            var adminEmail =
                Umbraco.TypedContentAtRoot()
                    .First(c => c.DocumentTypeAlias == "Rezerwacje")
                    .GetPropertyValue<string>("email");
            var message = new MailMessage("niebieskaweranda@gmail.com", adminEmail)
            {
                Body = $"{additionalMessage}{Environment.NewLine}{CreateMailBody(model)}",
                Subject = $"Rezerwacja - {model.FirstName} {model.LastName}, {model.ArrivalDate}-{model.DepartureDate}, {model.ApartmentName}",
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8,
                IsBodyHtml = true
            };
            smtpClient.Send(message);
        }
    }
}