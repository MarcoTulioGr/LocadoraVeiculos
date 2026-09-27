using LocadoraVeiculos.Data;
using LocadoraVeiculos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AlugueisController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public AlugueisController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Aluguel>>> Listar()
        {
            var alugueis = await _context.Alugueis
                .AsNoTracking()
                .Include(a => a.Cliente)
                .Include(a => a.Veiculo)
                .OrderByDescending(a => a.DataRetirada)
                .ToListAsync();

            return Ok(alugueis);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Aluguel>> ObterPorId(int id)
        {
            var aluguel = await _context.Alugueis
                .AsNoTracking()
                .Include(a => a.Cliente)
                .Include(a => a.Veiculo)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (aluguel is null)
                return NotFound(new { mensagem = $"Aluguel {id} n\u00e3o encontrado." });

            return Ok(aluguel);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Aluguel>> Criar([FromBody] Aluguel aluguel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var clienteExiste = await _context.Clientes.AnyAsync(c => c.Id == aluguel.ClienteId);
            if (!clienteExiste)
                return BadRequest(new { mensagem = $"Cliente {aluguel.ClienteId} n\u00e3o existe." });

            var veiculo = await _context.Veiculos
                .Include(v => v.CategoriaVeiculo)
                .FirstOrDefaultAsync(v => v.Id == aluguel.VeiculoId);

            if (veiculo is null)
                return BadRequest(new { mensagem = $"Ve\u00edculo {aluguel.VeiculoId} n\u00e3o existe." });

            if (aluguel.DataPrevistaDevolucao <= aluguel.DataRetirada)
                return BadRequest(new { mensagem = "A data prevista de devolu\u00e7\u00e3o deve ser posterior \u00e0 data de retirada." });

            var veiculoOcupado = await _context.Alugueis
                .AnyAsync(a => a.VeiculoId == aluguel.VeiculoId && a.DataDevolucaoReal == null);

            if (veiculoOcupado)
                return Conflict(new { mensagem = $"O ve\u00edculo {veiculo.Placa} j\u00e1 est\u00e1 alugado e ainda n\u00e3o foi devolvido." });

            if (aluguel.ValorDiaria <= 0)
                aluguel.ValorDiaria = veiculo.CategoriaVeiculo.ValorDiariaBase;

            aluguel.Id = 0;
            aluguel.KmInicial = veiculo.Quilometragem;
            aluguel.DataDevolucaoReal = null;
            aluguel.KmFinal = null;
            aluguel.ValorTotal = null;
            aluguel.Cliente = null!;
            aluguel.Veiculo = null!;

            veiculo.Disponivel = false;

            _context.Alugueis.Add(aluguel);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObterPorId), new { id = aluguel.Id }, aluguel);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Atualizar(int id, [FromBody] Aluguel aluguel)
        {
            if (id != aluguel.Id)
                return BadRequest(new { mensagem = "O id da URL n\u00e3o corresponde ao id enviado no corpo." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existe = await _context.Alugueis.AnyAsync(a => a.Id == id);
            if (!existe)
                return NotFound(new { mensagem = $"Aluguel {id} n\u00e3o encontrado." });

            if (aluguel.DataPrevistaDevolucao <= aluguel.DataRetirada)
                return BadRequest(new { mensagem = "A data prevista de devolu\u00e7\u00e3o deve ser posterior \u00e0 data de retirada." });

            if (aluguel.KmFinal.HasValue && aluguel.KmFinal.Value < aluguel.KmInicial)
                return BadRequest(new { mensagem = "A quilometragem final n\u00e3o pode ser menor que a inicial." });

            aluguel.Cliente = null!;
            aluguel.Veiculo = null!;

            _context.Entry(aluguel).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id:int}/devolucao")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Aluguel>> RegistrarDevolucao(int id, [FromBody] DevolucaoRequest devolucao)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var aluguel = await _context.Alugueis
                .Include(a => a.Veiculo)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (aluguel is null)
                return NotFound(new { mensagem = $"Aluguel {id} n\u00e3o encontrado." });

            if (aluguel.DataDevolucaoReal is not null)
                return Conflict(new { mensagem = "Este aluguel j\u00e1 foi encerrado." });

            if (devolucao.DataDevolucaoReal < aluguel.DataRetirada)
                return BadRequest(new { mensagem = "A data de devolu\u00e7\u00e3o n\u00e3o pode ser anterior \u00e0 data de retirada." });

            if (devolucao.KmFinal < aluguel.KmInicial)
                return BadRequest(new { mensagem = "A quilometragem final n\u00e3o pode ser menor que a inicial." });

            var dias = (devolucao.DataDevolucaoReal.Date - aluguel.DataRetirada.Date).Days;
            if (dias < 1) dias = 1;

            aluguel.DataDevolucaoReal = devolucao.DataDevolucaoReal;
            aluguel.KmFinal = devolucao.KmFinal;
            aluguel.ValorTotal = aluguel.ValorDiaria * dias;

            if (!string.IsNullOrWhiteSpace(devolucao.Observacoes))
                aluguel.Observacoes = devolucao.Observacoes;

            aluguel.Veiculo.Quilometragem = devolucao.KmFinal;
            aluguel.Veiculo.Disponivel = true;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                aluguel.Id,
                aluguel.DataRetirada,
                aluguel.DataDevolucaoReal,
                DiasCobrados = dias,
                aluguel.ValorDiaria,
                aluguel.ValorTotal,
                KmRodados = devolucao.KmFinal - aluguel.KmInicial
            });
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Excluir(int id)
        {
            var aluguel = await _context.Alugueis
                .Include(a => a.Veiculo)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (aluguel is null)
                return NotFound(new { mensagem = $"Aluguel {id} n\u00e3o encontrado." });

            if (aluguel.DataDevolucaoReal is null)
                aluguel.Veiculo.Disponivel = true;

            _context.Alugueis.Remove(aluguel);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
