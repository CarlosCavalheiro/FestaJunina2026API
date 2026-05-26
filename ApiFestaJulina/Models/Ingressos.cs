using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ApiFestaJulina.Models
{
    public class Ingressos
    {
        [Key]
        [Column("id_ingresso")]
        public int IdIngresso {get; set;}

        [Column("id_pedido")]
        public int IdPedido {get; set;} 

        [Column("id_lote")]
        public int IdLote {get; set;}

        [Column("id_tipo")]
        public int IdTipo {get; set;}

        [Column("valor")]
        public decimal Valor {get; set;}

        [Column("qr_code")]
        public string? QrCode {get; set;}

        [Column("id_usuario")]
        public int IdUsuario {get; set;}

        [Column("id_status_validacao")]
        public int IdStatusValidacao {get; set;}

        [Column("dt_entrada")]
        public DateTime? DtEntrada {get; set;}

        [Column("usuario_que_leu")]
        public int? UsuarioQueLeu {get; set;}

        public virtual Usuarios Usuario {get; set;}
        public virtual Pedidos Pedido {get; set;}
        public virtual Lotes Lote {get; set;}
    }
}