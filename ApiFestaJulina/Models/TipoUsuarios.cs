using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ApiFestaJulina.Models
{
    public class TipoUsuario
    {
        [Key]
        [Column("id_tp_user")]
        public int IdTpUsuario {get; set;}

        [Column("descricao")]
        public string? DescricaoTpUser {get; set;}
    }
}