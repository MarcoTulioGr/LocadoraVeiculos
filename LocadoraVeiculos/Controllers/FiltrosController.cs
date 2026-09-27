using LocadoraVeiculos.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FiltrosController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public FiltrosController(ApplicationContext context)
        {
            _context = context;
        }

        // FILTRO 1 - INNER JOIN entre Veiculo, Fabricante e CategoriaVeiculo
        [HttpGet("veiculos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> FiltrarVeiculos(
            [FromQuery] string? fabricante,
            [FromQuery] string? categoria,
            [FromQuery] bool? disponivel,
            [FromQuery] int? anoMinimo)
        {
            var consulta = from v in _context.Veiculos.AsNoTracking()
                           join f in _context.Fabricantes on v.FabricanteId equals f.Id
                           join c in _context.CategoriasVeiculo on v.CategoriaVeiculoId equals c.Id
                           select new { v, f, c };

            if (!string.IsNullOrWhiteSpace(fabricante))
                consulta = consulta.Where(x => x.f.Nome.Contains(fabricante));

            if (!string.IsNullOrWhiteSpace(categoria))
                consulta = consulta.Where(x => x.c.Nome.Contains(categoria));

            if (disponivel.HasValue)
                consulta = consulta.Where(x => x.v.Disponivel == disponivel.Value);

            if (anoMinimo.HasValue)
                consulta = consulta.Where(x => x.v.AnoFabricacao >= anoMinimo.Value);

            var resultado = await consulta
                .OrderBy(x => x.f.Nome).ThenBy(x => x.v.Modelo)
                .Select(x => new
                {
                    x.v.Id,
                    x.v.Placa,
                    x.v.Modelo,
                    Fabricante = x.f.Nome,
                    Categoria = x.c.Nome,
                    x.v.AnoFabricacao,
                    x.v.Quilometragem,
                    x.v.Cor,
                    x.c.ValorDiariaBase,
                    x.v.Disponivel
                })
                .ToListAsync();

            return Ok(new { total = resultado.Count, dados = resultado });
        }

        // FILTRO 2 - LEFT JOIN: todos os clientes, inclusive os que nunca alugaram
        [HttpGet("clientes-e-alugueis")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ClientesEAlugueis([FromQuery] bool? somenteSemAluguel)
        {
            var consulta = from cl in _context.Clientes.AsNoTracking()
                           join a in _context.Alugueis on cl.Id equals a.ClienteId into alugueisDoCliente
                           select new
                           {
                               cl.Id,
                               cl.Nome,
                               cl.Cpf,
                               cl.Email,
                               TotalAlugueis = alugueisDoCliente.Count(),
                               AlugueisEmAberto = alugueisDoCliente.Count(x => x.DataDevolucaoReal == null),
                               ValorGastoTotal = alugueisDoCliente.Sum(x => x.ValorTotal ?? 0m)
                           };

            if (somenteSemAluguel == true)
                consulta = consulta.Where(x => x.TotalAlugueis == 0);

            var resultado = await consulta
                .OrderByDescending(x => x.ValorGastoTotal)
                .ThenBy(x => x.Nome)
                .ToListAsync();

            return Ok(new { total = resultado.Count, dados = resultado });
        }

        // FILTRO 3 - INNER JOIN entre Aluguel, Cliente e Veiculo
        [HttpGet("alugueis-em-aberto")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AlugueisEmAberto()
        {
            var consulta = await (from a in _context.Alugueis.AsNoTracking()
                                  join cl in _context.Clientes on a.ClienteId equals cl.Id
                                  join v in _context.Veiculos on a.VeiculoId equals v.Id
                                  where a.DataDevolucaoReal == null
                                  orderby a.DataPrevistaDevolucao
                                  select new
                                  {
                                      AluguelId = a.Id,
                                      Cliente = cl.Nome,
                                      cl.Telefone,
                                      v.Placa,
                                      v.Modelo,
                                      a.DataRetirada,
                                      a.DataPrevistaDevolucao,
                                      a.ValorDiaria
                                  }).ToListAsync();

            var hoje = DateTime.Today;
            var resultado = consulta.Select(x => new
            {
                x.AluguelId,
                x.Cliente,
                x.Telefone,
                x.Placa,
                x.Modelo,
                x.DataRetirada,
                x.DataPrevistaDevolucao,
                x.ValorDiaria,
                DiasContratados = (x.DataPrevistaDevolucao.Date - x.DataRetirada.Date).Days,
                Atrasado = hoje > x.DataPrevistaDevolucao.Date,
                DiasDeAtraso = hoje > x.DataPrevistaDevolucao.Date
                    ? (hoje - x.DataPrevistaDevolucao.Date).Days
                    : 0
            }).ToList();

            return Ok(new { total = resultado.Count, dados = resultado });
        }

        // FILTRO 4 - INNER JOIN com agregacao por categoria
        [HttpGet("faturamento-por-categoria")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> FaturamentoPorCategoria()
        {
            var resultado = await (from a in _context.Alugueis.AsNoTracking()
                                   join v in _context.Veiculos on a.VeiculoId equals v.Id
                                   join c in _context.CategoriasVeiculo on v.CategoriaVeiculoId equals c.Id
                                   where a.DataDevolucaoReal != null
                                   group new { a, v } by c.Nome into g
                                   select new
                                   {
                                       Categoria = g.Key,
                                       QtdeLocacoes = g.Count(),
                                       Faturamento = g.Sum(x => x.a.ValorTotal ?? 0m),
                                       KmRodados = g.Sum(x => (x.a.KmFinal ?? x.a.KmInicial) - x.a.KmInicial)
                                   })
                                   .OrderByDescending(x => x.Faturamento)
                                   .ToListAsync();

            return Ok(new
            {
                total = resultado.Count,
                faturamentoGeral = resultado.Sum(x => x.Faturamento),
                dados = resultado
            });
        }

        // FILTRO 5 - LEFT JOIN com DefaultIfEmpty: veiculos que nunca foram alugados
        [HttpGet("veiculos-nunca-alugados")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> VeiculosNuncaAlugados()
        {
            var resultado = await (from v in _context.Veiculos.AsNoTracking()
                                   join f in _context.Fabricantes on v.FabricanteId equals f.Id
                                   join c in _context.CategoriasVeiculo on v.CategoriaVeiculoId equals c.Id
                                   join a in _context.Alugueis on v.Id equals a.VeiculoId into alugueis
                                   from a in alugueis.DefaultIfEmpty()
                                   where a == null
                                   orderby v.Modelo
                                   select new
                                   {
                                       v.Id,
                                       v.Placa,
                                       v.Modelo,
                                       Fabricante = f.Nome,
                                       Categoria = c.Nome,
                                       v.AnoFabricacao,
                                       v.Quilometragem,
                                       c.ValorDiariaBase
                                   }).ToListAsync();

            return Ok(new { total = resultado.Count, dados = resultado });
        }

        // FILTRO 6 - INNER JOIN: devolucoes entregues depois do prazo
        [HttpGet("devolucoes-em-atraso")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DevolucoesEmAtraso()
        {
            var consulta = await (from a in _context.Alugueis.AsNoTracking()
                                  join cl in _context.Clientes on a.ClienteId equals cl.Id
                                  join v in _context.Veiculos on a.VeiculoId equals v.Id
                                  where a.DataDevolucaoReal != null
                                        && a.DataDevolucaoReal > a.DataPrevistaDevolucao
                                  select new
                                  {
                                      AluguelId = a.Id,
                                      Cliente = cl.Nome,
                                      cl.Email,
                                      v.Placa,
                                      v.Modelo,
                                      a.DataPrevistaDevolucao,
                                      a.DataDevolucaoReal,
                                      a.ValorDiaria,
                                      a.ValorTotal
                                  }).ToListAsync();

            var resultado = consulta
                .Select(x => new
                {
                    x.AluguelId,
                    x.Cliente,
                    x.Email,
                    x.Placa,
                    x.Modelo,
                    x.DataPrevistaDevolucao,
                    x.DataDevolucaoReal,
                    DiasAtraso = (x.DataDevolucaoReal!.Value.Date - x.DataPrevistaDevolucao.Date).Days,
                    x.ValorDiaria,
                    x.ValorTotal
                })
                .OrderByDescending(x => x.DiasAtraso)
                .ToList();

            return Ok(new { total = resultado.Count, dados = resultado });
        }
    }
}
