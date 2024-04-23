using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FatturaSubito.Models
{
    public class DocumentoViewModel
    {
        public int Id { get; set; }
        public int Numero { get; set; }
        public string TipoDocumento { get; set; }
        public DateTime Data { get; set; }
        public string NomeAzienda { get; set; }
        public string Email { get; set; }
        public decimal TotaleDocumento { get; set; }
        public string Stato { get; set; }
    }
}