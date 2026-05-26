using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiFestaJulina.DTO
{
    public class NewPerguntas
    {
        public int IdTpUser {get; set;}
        public string DescricaoPergunta {get; set;}
        public int TipoPergunta {get; set;}
    }
}