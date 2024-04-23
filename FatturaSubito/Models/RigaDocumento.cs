using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FatturaSubito.Models
{
    public class RigaDocumento
    {
        public int RigaDocumentoId { get; set; }
        public string Descrizione { get; set; }
        public int Quantita { get; set; }
        public decimal PrezzoUnitario { get; set; }
        //  public decimal AliquotaIVA { get; set; }
        [Required]
        public int AliquotaIVAId { get; set; }
        [ForeignKey("AliquotaIVAId")]
        public virtual AliquotaIVA AliquotaIVA { get; set; }

        public int DocumentoId { get; set; }
        [ForeignKey("DocumentoId")]
        public virtual Documento Documento { get; set; }
        [NotMapped]
        public decimal Totale => Quantita * PrezzoUnitario * (1 + (AliquotaIVA?.ValorePercentuale ?? 0) / 100);

    }
}