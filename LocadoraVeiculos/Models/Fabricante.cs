using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

        [JsonIgnore]
        public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    }
}