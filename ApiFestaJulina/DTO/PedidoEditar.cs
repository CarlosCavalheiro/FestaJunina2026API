using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiFestaJulina.DTO
{
    public class PedidoEditar
    {
        public int Quantidade {get; set;}
        public int IdStatus {get; set;}
        public decimal Valor {get; set;}
        public int TipoPagamento {get; set;}
    }
}