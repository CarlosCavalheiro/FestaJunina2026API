using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ApiFestaJulina.Models
{

    [Table("sessao")]
    public class Sessao
    {  
        [Key]
        [Column("id_sessao")]
        public int IdSessao { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("token")]
        public string? Token { get; set; }

        [Column("data_inicio")]
        public DateTime? DataInicio { get; set; }

        [Column("ativo")]
        public bool? Ativo { get; set; }

        public virtual Usuarios? Usuario { get; set; }
    }


}