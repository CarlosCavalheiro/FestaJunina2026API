using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiFestaJulina.DTO
{
    public class NewUsuario
    {
        public string Nome {get; set;}
        public string Email {get; set;}
        public string Senha {get; set;}
        public string Telefone {get; set;}
        public int IdPerfil {get; set;}
        public bool PossuiDeficiencia {get; set;}
        public string? TipoDeficiencia {get; set;}
        public bool Status {get; set;}
    }
}