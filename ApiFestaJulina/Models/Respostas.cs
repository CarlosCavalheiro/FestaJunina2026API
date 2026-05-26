using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ApiFestaJulina.Models
{
    public class Resposta
    {
        [Key]
        [Column("id_resposta")]
        public int IdResposta {get; set;}

        [Column("id_pergunta")]
        public int IdPergunta {get; set;}

        [Column("resposta")]
        public string resposta {get; set;}

        [Column("data_resposta")]
        public DateTime? DtResposta {get; set;} = DateTime.Now;
    }
}