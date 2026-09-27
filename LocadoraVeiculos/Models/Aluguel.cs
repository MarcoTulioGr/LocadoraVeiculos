using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocadoraVeiculos.Models
{
    public class Aluguel
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        public int VeiculoId { get; set; }
        public Veiculo Veiculo { get; set; } = null!;

        [Required]
        public DateTime DataRetirada { get; set; }

        [Required]
        public DateTime DataPrevistaDevolucao { get; set; }

        public DateTime? DataDevolucaoReal { get; set; }

        [Required]
        public int KmInicial { get; set; }

        public int? KmFinal { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal ValorDiaria { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? ValorTotal { get; set; }

        [MaxLength(300)]
        public string? Observacoes { get; set; }
    }
}
