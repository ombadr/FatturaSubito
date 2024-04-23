using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Policy;
using System.Web;

namespace FatturaSubito.Models
{
    public class AdminDashboardViewModel
    {
        public int NumeroFattureEmesse { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotaleValoreFatture { get; set; }
        public int NumeroFattureAcquisto { get; set; }
        public int NumeroPreventivi { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotaleValorePreventivi { get; set; }
        public int NumeroBolleDiTrasporto { get; set; }
        public int TotaleNumeroUtenti { get; set; }
        public int UtentiIscrittiOggi { get; set; }
    }
}