using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FatturaSubito.Models
{
    public class Cliente
    {
        public int ClienteId { get; set; }
        [Required]
        public int UtenteId { get; set; }
        [Required(ErrorMessage = "Il nome è obbligatorio.")]
        public string Nome { get; set; }
        [Required(ErrorMessage = "Il cognome è obbligatorio.")]
        public string Cognome { get; set; }
        [Required(ErrorMessage = "Il nome dell'azienda è obbligatorio.")]
        public string NomeAzienda { get; set; }
        [Required(ErrorMessage = "La partita IVA è obbligatoria.")]
        public string PartitaIVA { get; set; }
        [Required(ErrorMessage = "Il codice fiscale è obbligatorio.")]
        public string CodiceFiscale { get; set; }
        [Required(ErrorMessage = "L'email è obbligatoria.")]
        [EmailAddress(ErrorMessage = "Inserire un'email valida.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "L'indirizzo è obbligatorio.")]
        public string Indirizzo { get; set; }
        [Required(ErrorMessage = "Il comune è obbligatorio.")]
        public string Comune { get; set; }
        [Required(ErrorMessage = "La provincia è obbligatoria.")]
        public string Provincia { get; set; }
        [Required(ErrorMessage = "Il CAP è obbligatorio.")]
        public string CAP { get; set; }
        public virtual ICollection<Documento> Documenti { get; set; }
    }
}