using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ApiFestaJulina.Models
{
    public class Perguntas
    {
        [Key]
        [Column("id_pergunta")]
        public int IdPergunta {get; set;}

        [Column("id_tp_user")]
        public int IdTpUser {get; set;}

        [Column("descricao_pergunta")]
        public string DescricaoPergunta {get; set;}

        [Column("tp_pergunta")]
        public int TipoPergunta {get; set;}

        // public virtual Resposta IdResposta { get; set; }
    }
}