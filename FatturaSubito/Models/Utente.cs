using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace FatturaSubito.Models
{
    public class Utente
    {
        public int UtenteId { get; set; }
        
        [Required]
        public string Nome { get; set; }
        [Required]
        public string Cognome { get; set; }
        [Required]
        public string NomeAzienda { get; set; }
        [Required]
        public string PartitaIVA { get; set; }
        [Required]
        public string CodiceFiscale { get; set; }
        [Required(ErrorMessage = "L'email è obbligatoria.")]
        [EmailAddress(ErrorMessage = "Inserire un indirizzo email valido.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "La password è obbligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string Indirizzo { get; set; }
        public string Comune { get; set; }
        public string Provincia { get; set; }
        public string CAP { get; set; }
        public string Logo { get; set; }
        [Required]
        public bool Attivo { get; set; }

        [Required]
        public string Ruolo { get; set; }

        [Required]
        public DateTime DataIscrizione { get; set; }

        public virtual ICollection<Cliente> Clienti { get; set; }
        public virtual ICollection<Fornitore> Fornitori { get; set; }
        public virtual ICollection<Documento> Documenti { get; set; }
        
    }
}