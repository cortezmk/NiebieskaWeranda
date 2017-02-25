using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Foolproof;
using NiebieskaWeranda.Attributes;
using Compare = System.ComponentModel.DataAnnotations.CompareAttribute;

namespace NiebieskaWeranda.Models
{
    public class ReservationRequestModel
    {
        [Display(Name = "Imię")]
        [Required(ErrorMessage = "Pole wymagane.")]
        [UmbracoOptions(Alias = "imie")]
        public string FirstName { get; set; }

        [Display(Name = "Nazwisko")]
        [Required(ErrorMessage = "Pole wymagane.")]
        [UmbracoOptions(Alias = "nazwisko")]
        public string LastName { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Pole wymagane.")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy adres email.")]
        [UmbracoOptions(Alias = "email")]
        public string Email { get; set; }

        [Display(Name = "Powtórz email")]
        [Required(ErrorMessage = "Pole wymagane.")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy adres email.")]
        [Compare("Email", ErrorMessage = "Podane adresy email muszą być takie same.")]
        [SkipInEmail]
        public string RepeatEmail { get; set; }

        [Display(Name = "Ulica")]
        [Required(ErrorMessage = "Pole wymagane.")]
        [UmbracoOptions(Alias = "ulica")]
        public string Street { get; set; }

        [Display(Name = "Kod pocztowy")]
        [Required(ErrorMessage = "Pole wymagane.")]
        [UmbracoOptions(Alias = "kodPocztowy")]
        public string Zip { get; set; }

        [Display(Name = "Miasto")]
        [Required(ErrorMessage = "Pole wymagane.")]
        [UmbracoOptions(Alias = "miasto")]
        public string City { get; set; }

        [Display(Name = "Kraj")]
        [Required(ErrorMessage = "Pole wymagane.")]
        [UmbracoOptions(Alias = "kraj")]
        public string Country { get; set; }

        [Display(Name = "Nazwa apartamentu", AutoGenerateField = false)]
        [UmbracoOptions(Alias = "apartament")]
        [HiddenInput(DisplayValue = false)]
        public string ApartmentName { get; set; }

        [Display(Name = "Data przyjazdu")]
        [Required(ErrorMessage = "Pole wymagane.")]
        [UmbracoOptions(Alias = "dataPrzyjazdu")]
//        [CheckDateRange(ErrorMessage = "Data przyjazdu musi być późniejsza niż dzisiejsza.")]
//        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public string ArrivalDate { get; set; }

        [Display(Name = "Data wyjazdu")]
        [Required(ErrorMessage = "Pole wymagane.")]
        [UmbracoOptions(Alias = "dataWyjazdu")]
//        [GreaterThan("ArrivalDate", ErrorMessage = "Data wyjazdu musi być późniejsza od daty przyjazdu.")]
//        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public string DepartureDate { get; set; }

        [Display(Name = "Ilość osób")]
        [Range(1, 10, ErrorMessage = "Ilość osób nie może przekraczać 10.")]
        [Required(ErrorMessage = "Pole wymagane.")]
        [UmbracoOptions(Alias = "iloscOsob")]
        public int NumberOfPersons { get; set; }

        [Display(Name = "Dodatkowe informacje")]
        [DataType(DataType.MultilineText)]
        [UmbracoOptions(Alias = "dodatkoweInformacje")]
        public string Comments { get; set; }

        [UmbracoOptions(Ignore = true)]
        [Required(ErrorMessage = "Warunki muszą być zaakceptowane.")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Warunki muszą być zaakceptowane.")]
        [SkipInEmail]
        public bool Agreed { get; set; }

        [Display(Name = "Telefon")]
        [Required(ErrorMessage = "Pole wymagane.")]
        [UmbracoOptions(Alias = "telefon")]
        public string Phone { get; set; }

        public int MaxNumPersons { get; set; }
    }
}
