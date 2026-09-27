using LocadoraVeiculos.Data;
using LocadoraVeiculos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class VeiculosController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public VeiculosController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Veiculo>>> Listar()
        {
            var veiculos = await _context.Veiculos
                .AsNoTracking()
                .Include(v => v.Fabricante)
                .Include(v => v.CategoriaVeiculo)
                .OrderBy(v => v.Modelo)
                .ToListAsync();

            return Ok(veiculos);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Veiculo>> ObterPorId(int id)
        {
            var veiculo = await _context.Veiculos
                .AsNoTracking()
                .Include(v => v.Fabricante)
                .Include(v => v.CategoriaVeiculo)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (veiculo is null)
                return NotFound(new { mensagem = $"Ve\u00edculo {id} n\u00e3o encontrado." });

            return Ok(veiculo);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Veiculo>> Criar([FromBody] Veiculo veiculo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var erro = await ValidarVinculos(veiculo);
            if (erro is not null)
                return BadRequest(new { mensagem = erro });

            var placaEmUso = await _context.Veiculos.AnyAsync(v => v.Placa == veiculo.Placa);
            if (placaEmUso)
                return Conflict(new { mensagem = $"J\u00e1 existe um ve\u00edculo com a placa '{veiculo.Placa}'." });

            veiculo.Id = 0;
            veiculo.Fabricante = null!;
            veiculo.CategoriaVeiculo = null!;

            _context.Veiculos.Add(veiculo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObterPorId), new { id = veiculo.Id }, veiculo);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Atualizar(int id, [FromBody] Veiculo veiculo)
        {
            if (id != veiculo.Id)
                return BadRequest(new { mensagem = "O id da URL n\u00e3o corresponde ao id enviado no corpo." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existe = await _context.Veiculos.AnyAsync(v => v.Id == id);
            if (!existe)
                return NotFound(new { mensagem = $"Ve\u00edculo {id} n\u00e3o encontrado." });

            var erro = await ValidarVinculos(veiculo);
            if (erro is not null)
                return BadRequest(new { mensagem = erro });

            var placaEmUso = await _context.Veiculos
                .AnyAsync(v => v.Placa == veiculo.Placa && v.Id != id);

            if (placaEmUso)
                return Conflict(new { mensagem = $"J\u00e1 existe outro ve\u00edculo com a placa '{veiculo.Placa}'." });

            veiculo.Fabricante = null!;
            veiculo.CategoriaVeiculo = null!;

            _context.Entry(veiculo).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Excluir(int id)
        {
            var veiculo = await _context.Veiculos.FindAsync(id);
            if (veiculo is null)
                return NotFound(new { mensagem = $"Ve\u00edculo {id} n\u00e3o encontrado." });

            var possuiAlugueis = await _context.Alugueis.AnyAsync(a => a.VeiculoId == id);
            if (possuiAlugueis)
                return Conflict(new { mensagem = "N\u00e3o \u00e9 poss\u00edvel excluir: existem alugu\u00e9is registrados para este ve\u00edculo." });

            _context.Veiculos.Remove(veiculo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<string?> ValidarVinculos(Veiculo veiculo)
        {
            var fabricanteExiste = await _context.Fabricantes.AnyAsync(f => f.Id == veiculo.FabricanteId);
            if (!fabricanteExiste)
                return $"Fabricante {veiculo.FabricanteId} n\u00e3o existe.";

            var categoriaExiste = await _context.CategoriasVeiculo.AnyAsync(c => c.Id == veiculo.CategoriaVeiculoId);
            if (!categoriaExiste)
                return $"Categoria {veiculo.CategoriaVeiculoId} n\u00e3o existe.";

            if (veiculo.AnoFabricacao > DateTime.Now.Year + 1)
                return "O ano de fabrica\u00e7\u00e3o n\u00e3o pode ser maior que o pr\u00f3ximo ano.";

            return null;
        }
    }
}
