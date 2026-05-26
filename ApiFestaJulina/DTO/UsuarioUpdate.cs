using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiFestaJulina.DTO
{
    public class UsuarioUpdate
    {
        public string Nome {get; set;}
        public string Email {get; set;}
        public string Senha {get; set;}
        public string Telefone {get; set;}
        public int IdPerfil {get; set;}
    }
}