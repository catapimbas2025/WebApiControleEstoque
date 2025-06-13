using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiControleEstoque.Data;
using WebApiControleEstoque.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApiControleEstoque.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ProdutosController(AppDbContext context)
        {
            _context = context;
        }

        //GET:api/Produtos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
        {
            return await _context.Produtos.ToListAsync();
        }

        //Get:api/Produtos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> GetProduto(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            return produto == null ? NotFound() : produto;
        }

        //POST
        [HttpPost]
        public async Task<ActionResult<Produto>> PostProduto(Produto produto)
        {
            produto.DataCadastro = DateTime.UtcNow;
            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProduto), new { id = produto.Id }, produto);
        }

        //PUT
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduto(int id, Produto produto)
        {
            if (id != produto.Id) return BadRequest();
            var existing = await _context.Produtos.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Nome = produto.Nome;
            existing.CodigoBarras = produto.CodigoBarras;
            existing.Descricao = produto.Descricao;
            existing.PrecoVenda = produto.PrecoVenda;
            existing.PrecoCusto = produto.PrecoCusto;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {

                if (!ProdutoExists(id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        //DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult>DeleteProduto(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null) return NotFound();

            //VERIFICAR SE HÁ MOVIMENTAÇÕES ASSOCIADAS
            var hasMovimentacoes = await _context.Movimentacoes.AnyAsync(m => m.ProdutoId == id);
            if (hasMovimentacoes)
            {
                return BadRequest("Não é possível excluir produto com movimentações registradas");
            }

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        private bool ProdutoExists(int id) => _context.Produtos.Any(e => e.Id == id);
    }

}
