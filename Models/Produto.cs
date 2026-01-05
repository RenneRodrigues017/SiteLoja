using System.ComponentModel.DataAnnotations;

namespace SiteLoja.Models
{
    public class Produto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O Nome é obrigatório.")]
        public string Nome { get; set; } = string.Empty;

        public byte[]? ImagemData { get; set; }

        [Required(ErrorMessage = "O Preço é obrigatório.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O Preço deve ser positivo.")]
        public decimal Preco { get; set; }

        public decimal PrecoOriginal { get; set; }
        public string Desconto { get; set; } = string.Empty;
        public string Parcelamento { get; set; } = string.Empty;
        public string PrecoPix { get; set; } = string.Empty;

        [Required(ErrorMessage = "A Categoria é obrigatória.")]
        public string Categoria { get; set; } = string.Empty;
        
    }
}