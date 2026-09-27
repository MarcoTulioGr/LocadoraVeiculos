using System.ComponentModel.DataAnnotations;

namespace LocadoraVeiculos.Models
{
    public class Fabricante
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do fabricante é obrigatório.")]
        [MaxLength(60)]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? PaisOrigem { get; set; }

        public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    }
}