using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ApiFestaJulina.Models
{
    public class Tipo
    {
        [Key]
        [Column("id_tipo")]
        public int IdTipo { get; set; }

        [Column("descricao")]
        public string? Descricao { get; set; }
    }
}