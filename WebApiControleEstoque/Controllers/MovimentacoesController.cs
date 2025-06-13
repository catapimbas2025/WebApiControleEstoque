using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiControleEstoque.Data;
using WebApiControleEstoque.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApiControleEstoque.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovimentacoesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MovimentacoesController(AppDbContext context)
        {
            _context = context;
        }

        //POST
        [HttpPost]
        public async Task<ActionResult<Movimentacao>> PostMovimentacao(Movimentacao movimentacao)
        {
            //VALIDAR PRODUTO
            var produto = await _context.Produtos.FindAsync(movimentacao.ProdutoId);
            if (produto == null)
            {
                return BadRequest("Produto não encontrado");
            }

            //VALIDAR QUANTIDADE PARA SAIDA
            if (movimentacao.Tipo == TipoMovimentacao.Saida)
            {
                var saldo = await CalcularSaldoProduto(movimentacao.ProdutoId);
                if (saldo < movimentacao.Quantidade)
                {
                    return BadRequest("Quantidade em estoque insuficiente");
                }
            }

            movimentacao.DataMovimentacao = DateTime.UtcNow;
            _context.Movimentacoes.Add(movimentacao);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMovimentacao), new { id = movimentacao.Id }, movimentacao);
        }

        //GET
        [HttpGet("{id}")]
        public async Task<ActionResult<Movimentacao>>GetMovimentacao(int id)
        {
            var movimentacao = await _context.Movimentacoes.FindAsync(id);
            return movimentacao == null ? NotFound() : movimentacao;
        }

        //GET 
        [HttpGet("Produto/{produtoId}")]
        public async Task<ActionResult<IEnumerable<Movimentacao>>>
        GetMovimentacoesPorProduto(int produtoId)
        {
            return await _context.Movimentacoes
            .Where(m =>  m.ProdutoId == produtoId)
            .OrderByDescending(m => m.DataMovimentacao)
            .ToListAsync();
        }

        //GET 
        [HttpGet("Estoque")]
        public async Task<ActionResult<IEnumerable<Estoque>>>GetEstoque()
        {
            var produtos = await _context.Produtos.ToListAsync();
            var resultado = new List<Estoque>();

            foreach (var produto in produtos)
            {
                var saldo = await CalcularSaldoProduto(produto.Id);
                var ultimaMovimentacao = await _context.Movimentacoes
                .Where(m => m.ProdutoId==produto.Id)
                .OrderByDescending(m =>m.DataMovimentacao)
                .FirstOrDefaultAsync();

                resultado.Add(new Estoque
                {
                    ProdutoId = produto.Id,
                    ProdutoNome = produto.Nome,
                    SaldoAtual = saldo,
                    UltimaAtualizacao = ultimaMovimentacao?.DataMovimentacao??produto.DataCadastro
                });

            }
            return resultado;            
           
        }

        //GET
        [HttpGet("Estoque/{produtoId}")]
        public async Task<ActionResult<Estoque>>GetEstoqueProduto(int produtoId)
        {
            var produto = await _context.Produtos.FindAsync(produtoId);
            if (produto == null) return NotFound();

            var entradas = await _context.Movimentacoes
             .Where(m => m.ProdutoId == produto.Id && m.Tipo == TipoMovimentacao.Entrada)
             .SumAsync(m => m.Quantidade);

            var saidas = await _context.Movimentacoes
                .Where(m => m.ProdutoId == produtoId && (m.Tipo == TipoMovimentacao.Saida || m.Tipo == TipoMovimentacao.Ajuste))
                .SumAsync(m => m.Quantidade);
                 
             var ultimaMovimentacao = await _context.Movimentacoes
             .Where(m => m.ProdutoId == produtoId)
             .OrderByDescending(m => m.DataMovimentacao)
             .FirstOrDefaultAsync();

            return new Estoque
            {
                ProdutoId = produtoId,
                ProdutoNome = produto.Nome,
                Entradas = entradas,
                Saidas = saidas,
                SaldoAtual = entradas - saidas,
                UltimaAtualizacao = ultimaMovimentacao?.DataMovimentacao ?? produto.DataCadastro
            };
            
        }
        private async Task<int>CalcularSaldoProduto(int produtoId)
        {
            var entradas = await _context.Movimentacoes
                .Where(m => m.ProdutoId == produtoId && m.Tipo == TipoMovimentacao.Entrada)
                .SumAsync(m => m.Quantidade);

            var saidas = await _context.Movimentacoes
                .Where(m => m.ProdutoId == produtoId && (m.Tipo == TipoMovimentacao.Saida || m.Tipo == TipoMovimentacao.Ajuste))
                .SumAsync(m => m.Quantidade);

                return entradas - saidas;
        }
    }
}






