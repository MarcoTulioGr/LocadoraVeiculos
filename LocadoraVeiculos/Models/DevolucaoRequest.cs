using System.ComponentModel.DataAnnotations;

namespace LocadoraVeiculos.Models
{
    public class DevolucaoRequest
    {
        [Required(ErrorMessage = "A data de devolu\u00e7\u00e3o \u00e9 obrigat\u00f3ria.")]
        public DateTime DataDevolucaoReal { get; set; }

        [Required(ErrorMessage = "A quilometragem final \u00e9 obrigat\u00f3ria.")]
        [Range(0, int.MaxValue, ErrorMessage = "A quilometragem final n\u00e3o pode ser negativa.")]
        public int KmFinal { get; set; }

        [MaxLength(300)]
        public string? Observacoes { get; set; }
    }
}
