using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FatturaSubito.Models
{
    public enum TipoDocumento
    {
        FatturaVendita,
        FatturaAcquisto,
        Proforma,
        Preventivo,
        BollaDiTrasporto
    }

    public enum StatoFattura
    {
        NonPagato,
        Pagato,
        PagatoParzialmente,
        InRitardo
    }

    public enum StatoProformaPreventivo
    {
        InAttesaDiApprovazione,
        Approvato,
        Rifiutato
    }

    public enum StatoBollaDiTrasporto
    {
        Spedito,
        Ricevuto
    }

    public class Documento
    {
        public int DocumentoId { get; set; }
        public TipoDocumento Tipo { get; set; }
        [Required]
        public int Numero { get; set; }
        public DateTime Data { get; set; }
        public int UtenteId { get; set; }
        public virtual Utente Utenti { get; set; }

        public int? ClienteId { get; set; }
        public virtual Cliente Cliente { get; set; }
        public int? FornitoreId { get; set; }
        public virtual Fornitore Fornitore { get; set; }
        public bool Cestinato { get; set; } = false;
        public virtual ICollection<RigaDocumento> RigheDocumento {get; set;}

        public int StatoValore { get; set; }

        [NotMapped]
        public decimal Totale => RigheDocumento?.Sum(rd => rd.Totale) ?? 0;

        [NotMapped]
        public object Stato
        {
            get
            {
                switch(Tipo)
                {
                    case TipoDocumento.FatturaVendita:
                    case TipoDocumento.FatturaAcquisto:
                        return (StatoFattura)StatoValore;
                    case TipoDocumento.Proforma:
                    case TipoDocumento.Preventivo:
                        return (StatoProformaPreventivo)StatoValore;
                    case TipoDocumento.BollaDiTrasporto:
                        return (StatoBollaDiTrasporto)StatoValore;
                    default:
                        throw new InvalidOperationException("Tipo di documento non valido per lo stato.");
                }
            }
            set
            {
                if (value.GetType().BaseType != typeof(Enum))
                    throw new InvalidOperationException("Il valore deve essere un enum.");

                StatoValore = (int)value;
            }
        }

        [DataType(DataType.MultilineText)]
        public string TerminiCondizioni { get; set; }
    }
}