using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiFestaJulina.DTO
{
    public class PedidosPost
    {
            
        public int IdUsuario { get; set; }
        public int Quantidade { get; set; }
        public string Status { get; set; }
        public decimal Valor { get; set; }
        public DateTime? DtaReserva { get; set; } = DateTime.Now;
        public string TipoPagamento { get; set; }
        public DateTime? DtaFechamento { get; set; }

    }
}