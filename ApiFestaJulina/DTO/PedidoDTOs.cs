namespace ApiFestaJulina.DTO
{
    public class PedidoDTO
    {
        public int IdUsuario { get; set; }
        public int Quantidade { get; set; }
        public int IdLote { get; set; }
        public int Valor { get; set; }
        public int TipoPagamento { get; set; }

        public List<IngressoDTO> ListaIngressos { get; set; }
    }

}
