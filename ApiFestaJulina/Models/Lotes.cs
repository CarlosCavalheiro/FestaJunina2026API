using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ApiFestaJulina.Models
{
    public class Lotes
    {
        [Key]
        [Column("id_lote")]
        public int IdLote {get; set;}

        [Column("id_evento")]
        public int IdEvento {get; set;}

        [Column("qtde_ingressos_lotes")]
        public int QtdeIngressosLotes {get; set;}

        [Column("valor_ing")]
        public decimal ValorIng {get; set;}

        [Column("dta_criacao")]
        public DateTime? DataCriacao {get; set;} = DateTime.Now;

        [Column("dta_fechamento")]
        public DateTime? DataFechamento {get; set;}

        [Column("descricao")]
        public string? Descricao {get; set;}

        [Column("saldo")]
        public int Saldo {get; set;}

        [Column("ativo")]
        public bool Ativo {get; set;} = true;

        [Column("tipo_lote")]
        public int TipoLote {get; set;}

        public virtual ICollection<Ingressos> Ingressos {get; set;}
    }
}