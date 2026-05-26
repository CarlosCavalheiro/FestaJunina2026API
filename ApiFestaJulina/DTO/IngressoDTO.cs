using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiFestaJulina.DTO
{
    public class IngressoDTO
    {
        //public int IdPedido {get; set;}
        public decimal Valor {get; set;}
        public string? QrCode {get; set;}
        public int IdUsuario {get; set;}
        public int IdTipo {get; set;}
        public int IdLote {get; set;}
        public int IdStatusValidacao {get; set;}
        public int UsuarioQueLeu {get;set;}
    }
}