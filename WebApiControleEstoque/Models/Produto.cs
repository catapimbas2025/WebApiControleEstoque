namespace WebApiControleEstoque.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public string? CodigoBarras { get; set; }
        public string? Descricao { get; set; }
        public decimal PrecoCusto { get; set; }
        public decimal PrecoVenda { get; set; }
        public DateTime DataCadastro { get; set; }= DateTime.UtcNow;
    }
}
