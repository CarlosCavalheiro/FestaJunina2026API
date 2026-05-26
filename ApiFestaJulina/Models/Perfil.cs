using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ApiFestaJulina.Models
{
    [Table("perfil")]
    public class Perfil
    {
        [Key]
        [Column("id_perfil")]
        public int IdPerfil {get; set;}

        [Column("descricao")]
        public string? Descricao {get; set;}
    }
}