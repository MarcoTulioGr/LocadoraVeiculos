using LocadoraVeiculos.Data;
using LocadoraVeiculos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public ClientesController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista todos os clientes cadastrados, em ordem alfabética.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Cliente>>> Listar()
        {
            var clientes = await _context.Clientes
                .AsNoTracking()
                .OrderBy(c => c.Nome)
                .ToListAsync();

            return Ok(clientes);
        }

        /// <summary>
        /// Obtém um cliente pelo seu identificador.
        /// </summary>
        /// <param name="id">Identificador do cliente.</param>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Cliente>> ObterPorId(int id)
        {
            var cliente = await _context.Clientes
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente is null)
                return NotFound(new { mensagem = $"Cliente {id} n\u00e3o encontrado." });

            return Ok(cliente);
        }

        /// <summary>
        /// Cadastra um cliente. O CPF deve ser único e o e-mail precisa ter formato válido.
        /// </summary>
        /// <param name="cliente">Dados do cliente. Nome, CPF e e-mail são obrigatórios.</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Cliente>> Criar([FromBody] Cliente cliente)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (cliente.DataNascimento.HasValue && cliente.DataNascimento.Value > DateTime.Today)
                return BadRequest(new { mensagem = "A data de nascimento n\u00e3o pode estar no futuro." });

            var cpfEmUso = await _context.Clientes.AnyAsync(c => c.Cpf == cliente.Cpf);
            if (cpfEmUso)
                return Conflict(new { mensagem = $"J\u00e1 existe um cliente com o CPF '{cliente.Cpf}'." });

            cliente.Id = 0;
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObterPorId), new { id = cliente.Id }, cliente);
        }

        /// <summary>
        /// Atualiza os dados de um cliente existente.
        /// </summary>
        /// <param name="id">Identificador do cliente.</param>
        /// <param name="cliente">Dados atualizados. O Id do corpo deve ser igual ao da URL.</param>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Atualizar(int id, [FromBody] Cliente cliente)
        {
            if (id != cliente.Id)
                return BadRequest(new { mensagem = "O id da URL n\u00e3o corresponde ao id enviado no corpo." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existe = await _context.Clientes.AnyAsync(c => c.Id == id);
            if (!existe)
                return NotFound(new { mensagem = $"Cliente {id} n\u00e3o encontrado." });

            var cpfEmUso = await _context.Clientes.AnyAsync(c => c.Cpf == cliente.Cpf && c.Id != id);
            if (cpfEmUso)
                return Conflict(new { mensagem = $"J\u00e1 existe outro cliente com o CPF '{cliente.Cpf}'." });

            _context.Entry(cliente).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Exclui um cliente. Recusada com 409 se houver aluguéis registrados em seu nome.
        /// </summary>
        /// <param name="id">Identificador do cliente.</param>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Excluir(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente is null)
                return NotFound(new { mensagem = $"Cliente {id} n\u00e3o encontrado." });

            var possuiAlugueis = await _context.Alugueis.AnyAsync(a => a.ClienteId == id);
            if (possuiAlugueis)
                return Conflict(new { mensagem = "N\u00e3o \u00e9 poss\u00edvel excluir: existem alugu\u00e9is registrados para este cliente." });

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
