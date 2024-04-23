using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FatturaSubito.Models
{
    public class RegistrazioneViewModel
    {
        [Required(ErrorMessage = "Il nome è obbligatorio.")]
        [Display(Name = "Nome")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "Il cognome è obbligatorio.")]
        [Display(Name = "Cognome")]
        public string Cognome { get; set; }

        [Required(ErrorMessage = "Il nome dell'azienda è obbligatorio.")]
        [Display(Name = "Nome Azienda")]
        public string NomeAzienda { get; set; }

        [Required(ErrorMessage = "La partita IVA è obbligatoria.")]
        [Display(Name = "Partita IVA")]
        public string PartitaIVA { get; set; }

        [Required(ErrorMessage = "Il codice fiscale è obbligatorio.")]
        [Display(Name = "Codice Fiscale")]
        public string CodiceFiscale { get; set; }

        [Required(ErrorMessage = "L'email è obbligatoria.")]
        [EmailAddress(ErrorMessage = "Inserire un indirizzo email valido.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La password è obbligatoria.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z]).{8,}$", ErrorMessage = "La password deve contenere almeno 8 caratteri e includere almeno un carattere minuscolo e uno maiuscolo.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "La password e la conferma password non corrispondono.")]
        [Display(Name = "Conferma password")]
        public string ConfermaPassword { get; set; }

        [Required(ErrorMessage = "L'indirizzo è obbligatorio.")]
        [Display(Name = "Indirizzo")]
        public string Indirizzo { get; set; }

        [Required(ErrorMessage = "Il comune è obbligatorio.")]
        [Display(Name = "Comune")]
        public string Comune { get; set; }

        [Required(ErrorMessage = "La provincia è obbligatoria.")]
        [Display(Name = "Provincia")]
        public string Provincia { get; set; }

        [Required(ErrorMessage = "Il CAP è obbligatorio.")]
        [Display(Name = "CAP")]
        public string CAP { get; set; }

        [Display(Name = "Logo")]
        public string Logo { get; set; }

    }
}