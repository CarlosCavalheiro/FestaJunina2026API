namespace ApiFestaJulina.DTO
{
     public class PagamentoDTO
    {
        public int IdPedido { get; set; }
        public int Valor { get; set; }
        public int TipoPagamento { get; set; }
        public int Status { get; set; }
    }
}