namespace WebApiControleEstoque.Models
{
   public enum TipoMovimentacao
    {
        Entrada,
        Saida,
        Ajuste
    }
    
    public class Movimentacao
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public string Produto {  get; set; }
        public TipoMovimentacao Tipo {  get; set; }
        public int Quantidade { get; set; }
        public DateTime DataMovimentacao { get; set; }
        public string? OrigemDestino { get; set; }
        public string? Observacao { get; set; }
    }
}
