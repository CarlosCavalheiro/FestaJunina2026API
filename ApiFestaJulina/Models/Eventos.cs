using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ApiFestaJulina.Models
{

    [Table("evento")]
    public class Eventos
    {
         [Key]
         [Column("id_evento")]
        public int IdEvento {get; set;}

        [Column("nome_evento")]
        public string NomeEvento {get; set;}

        [Column("data")]
        public DateTime? Data {get; set;}

        [Column("ativo")]
        public bool?  Ativo {get; set;}

        [Column("local")]
        public string Local {get; set;}

        [Column("descricao")]
        public string? Descricao {get; set;}

        [Column("qtde_ingressos")]
        public int Qtde_Ingressos {get; set;}

        [Column("qtde_lotes")]
        public int Qtde_Lotes {get; set;}


    }
}