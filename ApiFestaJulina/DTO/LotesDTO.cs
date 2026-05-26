using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiFestaJulina.DTO
{
    public class LotesDTO
    {
        public int IdEvento {get ; set;}
        public int QtdeIngressosLotes {get; set;}
        public decimal ValorIng {get; set;}
        public DateTime? DataFechamento {get; set;}
        public string? Descricao {get; set;}
        public int Saldo {get; set;}

        public int TipoLote {get; set;}
    }
}