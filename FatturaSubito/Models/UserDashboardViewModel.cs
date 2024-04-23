using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace FatturaSubito.Models
{
    public class UserDashboardViewModel
    {
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotaleFatturato { get; set; }
        public int NumeroFattureVendita { get; set; }
        public int FattureInRitardo { get; set; }
        public int FatturePagatoParzialmente { get; set; }
        public int FattureNonPagato { get; set; }
        public int FatturePagato { get; set; }
        public int NumeroFattureAcquisto { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal ValoreTotaleFattureAcquisto { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotaleValorePreventivi { get; set; }
        public int NumeroTotalePreventivi { get; set; }
        public int NumeroBolleDiTrasporto { get; set; }
    }
}