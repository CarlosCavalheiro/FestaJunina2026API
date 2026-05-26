using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ApiFestaJulina.Models
{
    public class StatusValidacao
    {
        [Key]
        [Column("id_status_validacao")]
        public int IdStatusValidacao {get; set;}

        [Column("descricao")]
        public string DescricaoStatusValidacao {get; set;}
    }   
}