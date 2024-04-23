using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FatturaSubito.Models
{
    public class FatturaViewModel
    {
        public FatturaViewModel() {
            RigheDocumento = new List<RigaDocumento>();
        }
        public Documento Documento { get; set; }
        public Utente Utente { get; set; }
        public List<RigaDocumento> RigheDocumento { get; set; } = new List<RigaDocumento>();

        public StatoFattura StatoSelezionato { get; set; }
    }
}