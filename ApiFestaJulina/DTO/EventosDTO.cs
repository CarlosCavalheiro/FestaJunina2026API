using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiFestaJulina.DTO
{
    public class EventosDTO
    {
        public string NomeEvento {get; set;}
        public DateTime? Data {get; set;}
        public bool?  Ativo {get; set;}
        public string Local {get; set;}
        public string? Descricao {get; set;}
        public int Qtde_Ingressos {get; set;}
        public int Qtde_Lotes {get; set;}
    }
}