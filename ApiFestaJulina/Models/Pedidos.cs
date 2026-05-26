using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ApiFestaJulina.Models
{
    public class Pedidos
    {
        [Key]
        [Column("id_pedido")]
        public int IdPedido {get; set;}

        [Column("id_usuario")]
        public int IdUsuario {get; set;}

        [Column("quantidade")]
        public int Quantidade {get; set;}

        [Column("id_status")]
        public int IdStatus {get; set;}

        [Column("valor")]
        public decimal Valor {get; set;}

        [Column("dta_reserva")]
        public DateTime? DtaReserva {get; set;} = DateTime.Now;

        [Column("id_tipo_pagamento")]
        public int TipoPagamento {get; set;}

        [Column("dta_fechamento")]
        public DateTime? DtaFechamento {get; set;}

        [Column("ft_comprovante")]
        public string? FtComprovante {get; set;}

        [Column("ultima_acao_por")]
        public int? UltimaAcaoPor { get; set; }
        public virtual ICollection<Ingressos> Ingressos {get; set;}
    }
}