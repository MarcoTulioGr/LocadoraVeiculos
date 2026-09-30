using LocadoraVeiculos.Data;
using LocadoraVeiculos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FabricantesController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public FabricantesController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista todos os fabricantes cadastrados, em ordem alfabética.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Fabricante>>> Listar()
        {
            var fabricantes = await _context.Fabricantes
                .AsNoTracking()
                .OrderBy(f => f.Nome)
                .ToListAsync();

            return Ok(fabricantes);
        }

        /// <summary>
        /// Obtém um fabricante específico pelo seu identificador.
        /// </summary>
        /// <param name="id">Identificador do fabricante.</param>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Fabricante>> ObterPorId(int id)
        {
            var fabricante = await _context.Fabricantes
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fabricante is null)
                return NotFound(new { mensagem = $"Fabricante {id} n\u00e3o encontrado." });

            return Ok(fabricante);
        }

        /// <summary>
        /// Cadastra um novo fabricante. O nome deve ser único no sistema.
        /// </summary>
        /// <param name="fabricante">Dados do fabricante. O campo Nome é obrigatório (máximo 60 caracteres).</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Fabricante>> Criar([FromBody] Fabricante fabricante)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var nomeEmUso = await _context.Fabricantes
                .AnyAsync(f => f.Nome == fabricante.Nome);

            if (nomeEmUso)
                return Conflict(new { mensagem = $"J\u00e1 existe um fabricante chamado '{fabricante.Nome}'." });

            fabricante.Id = 0;
            _context.Fabricantes.Add(fabricante);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObterPorId), new { id = fabricante.Id }, fabricante);
        }

        /// <summary>
        /// Atualiza os dados de um fabricante existente.
        /// </summary>
        /// <param name="id">Identificador do fabricante.</param>
        /// <param name="fabricante">Dados atualizados. O Id do corpo deve ser igual ao da URL.</param>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Atualizar(int id, [FromBody] Fabricante fabricante)
        {
            if (id != fabricante.Id)
                return BadRequest(new { mensagem = "O id da URL n\u00e3o corresponde ao id enviado no corpo." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existe = await _context.Fabricantes.AnyAsync(f => f.Id == id);
            if (!existe)
                return NotFound(new { mensagem = $"Fabricante {id} n\u00e3o encontrado." });

            var nomeEmUso = await _context.Fabricantes
                .AnyAsync(f => f.Nome == fabricante.Nome && f.Id != id);

            if (nomeEmUso)
                return Conflict(new { mensagem = $"J\u00e1 existe outro fabricante chamado '{fabricante.Nome}'." });

            _context.Entry(fabricante).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Exclui um fabricante. A operação é recusada com 409 se houver veículos vinculados a ele.
        /// </summary>
        /// <param name="id">Identificador do fabricante.</param>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Excluir(int id)
        {
            var fabricante = await _context.Fabricantes.FindAsync(id);
            if (fabricante is null)
                return NotFound(new { mensagem = $"Fabricante {id} n\u00e3o encontrado." });

            var possuiVeiculos = await _context.Veiculos.AnyAsync(v => v.FabricanteId == id);
            if (possuiVeiculos)
                return Conflict(new { mensagem = "N\u00e3o \u00e9 poss\u00edvel excluir: existem ve\u00edculos vinculados a este fabricante." });

            _context.Fabricantes.Remove(fabricante);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
