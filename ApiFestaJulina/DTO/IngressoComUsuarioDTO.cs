using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiFestaJulina.DTO
{
    public class IngressoComUsuarioDTO
    {
        // Ingresso
        public int IdIngresso { get; set; }
        public int IdPedido { get; set; }
        public int IdLote { get; set; }
        public int IdTipo { get; set; }

        public string? NomeTipo { get; set; }

        public decimal Valor { get; set; }

        public string? QrCode { get; set; }

        public int IdUsuario { get; set; }

        public int PedidoIdStatus { get; set; }

        public int IdStatusValidacao { get; set; }

        // NOVO
        public int TipoPagamento { get; set; }
        public string? PedidoFtComprovante { get; set; }
        public int PedidoTipoPagamento { get; set; }

        // Usuario
        public string Nome { get; set; }

        public string Email { get; set; }

        public string Telefone { get; set; }
    }
}