using System.ComponentModel.DataAnnotations;

namespace LocadoraVeiculos.Models
{
    public class Veiculo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A placa é obrigatória.")]
        [MaxLength(8)]
        public string Placa { get; set; } = string.Empty;

        [Required(ErrorMessage = "O modelo é obrigatório.")]
        [MaxLength(60)]
        public string Modelo { get; set; } = string.Empty;

        [Required]
        [Range(1950, 2100, ErrorMessage = "Ano de fabricação inválido.")]
        public int AnoFabricacao { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "A quilometragem não pode ser negativa.")]
        public int Quilometragem { get; set; }

        [MaxLength(30)]
        public string? Cor { get; set; }

        public bool Disponivel { get; set; } = true;

        public int FabricanteId { get; set; }
        public Fabricante Fabricante { get; set; } = null!;

        public int CategoriaVeiculoId { get; set; }
        public CategoriaVeiculo CategoriaVeiculo { get; set; } = null!;

        public ICollection<Aluguel> Alugueis { get; set; } = new List<Aluguel>();
    }
}
