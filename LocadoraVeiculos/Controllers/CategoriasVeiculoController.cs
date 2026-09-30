using LocadoraVeiculos.Data;
using LocadoraVeiculos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CategoriasVeiculoController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public CategoriasVeiculoController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista todas as categorias de veículo com seus valores de diária base.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoriaVeiculo>>> Listar()
        {
            var categorias = await _context.CategoriasVeiculo
                .AsNoTracking()
                .OrderBy(c => c.Nome)
                .ToListAsync();

            return Ok(categorias);
        }

        /// <summary>
        /// Obtém uma categoria específica pelo seu identificador.
        /// </summary>
        /// <param name="id">Identificador da categoria.</param>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoriaVeiculo>> ObterPorId(int id)
        {
            var categoria = await _context.CategoriasVeiculo
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria is null)
                return NotFound(new { mensagem = $"Categoria {id} n\u00e3o encontrada." });

            return Ok(categoria);
        }

        /// <summary>
        /// Cadastra uma nova categoria. O nome deve ser único e a diária base maior que zero.
        /// </summary>
        /// <param name="categoria">Dados da categoria a cadastrar.</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CategoriaVeiculo>> Criar([FromBody] CategoriaVeiculo categoria)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (categoria.ValorDiariaBase <= 0)
                return BadRequest(new { mensagem = "O valor da di\u00e1ria base deve ser maior que zero." });

            var nomeEmUso = await _context.CategoriasVeiculo.AnyAsync(c => c.Nome == categoria.Nome);
            if (nomeEmUso)
                return Conflict(new { mensagem = $"J\u00e1 existe uma categoria chamada '{categoria.Nome}'." });

            categoria.Id = 0;
            _context.CategoriasVeiculo.Add(categoria);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObterPorId), new { id = categoria.Id }, categoria);
        }

        /// <summary>
        /// Atualiza os dados de uma categoria existente.
        /// </summary>
        /// <param name="id">Identificador da categoria.</param>
        /// <param name="categoria">Dados atualizados. O Id do corpo deve ser igual ao da URL.</param>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Atualizar(int id, [FromBody] CategoriaVeiculo categoria)
        {
            if (id != categoria.Id)
                return BadRequest(new { mensagem = "O id da URL n\u00e3o corresponde ao id enviado no corpo." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (categoria.ValorDiariaBase <= 0)
                return BadRequest(new { mensagem = "O valor da di\u00e1ria base deve ser maior que zero." });

            var existe = await _context.CategoriasVeiculo.AnyAsync(c => c.Id == id);
            if (!existe)
                return NotFound(new { mensagem = $"Categoria {id} n\u00e3o encontrada." });

            var nomeEmUso = await _context.CategoriasVeiculo
                .AnyAsync(c => c.Nome == categoria.Nome && c.Id != id);

            if (nomeEmUso)
                return Conflict(new { mensagem = $"J\u00e1 existe outra categoria chamada '{categoria.Nome}'." });

            _context.Entry(categoria).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Exclui uma categoria. Recusada com 409 se houver veículos classificados nela.
        /// </summary>
        /// <param name="id">Identificador da categoria.</param>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Excluir(int id)
        {
            var categoria = await _context.CategoriasVeiculo.FindAsync(id);
            if (categoria is null)
                return NotFound(new { mensagem = $"Categoria {id} n\u00e3o encontrada." });

            var possuiVeiculos = await _context.Veiculos.AnyAsync(v => v.CategoriaVeiculoId == id);
            if (possuiVeiculos)
                return Conflict(new { mensagem = "N\u00e3o \u00e9 poss\u00edvel excluir: existem ve\u00edculos nesta categoria." });

            _context.CategoriasVeiculo.Remove(categoria);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
