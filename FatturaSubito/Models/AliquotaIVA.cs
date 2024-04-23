using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FatturaSubito.Models
{
    public class AliquotaIVA
    {
        [Key]
        public int AliquotaIVAId { get; set; }
        [Required]
        public string Descrizione { get; set; }
        [Required]
        public decimal ValorePercentuale { get; set; }
    }
}