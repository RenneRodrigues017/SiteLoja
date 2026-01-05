using System.ComponentModel.DataAnnotations;

namespace SiteLoja.Models
{
    // Modelo Endereco usado para receber os dados do checkout.
    public class Endereco
    {
        public int Id { get; set; }
        // Propriedades do Produto (Hidden fields no formulário)
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public string Tamanho { get; set; } = string.Empty;
        // PROPRIEDADE PARCELAMENTO ADICIONADA
        public string Parcelamento { get; set; } = string.Empty;
        public string Pagamento { get; set; } = string.Empty;
        public string ImagemUrl { get; set; } = string.Empty;
        public decimal PrecoPix { get; set; }

        // Propriedades de Preço e Frete (Atualizadas pelo JavaScript e submetidas)
        // Preço base do produto (sem frete)
        public decimal Preco { get; set; }
        // Valor do frete - NOVO
        public decimal Frete { get; set; } = 0.00m;
        // Valor total (Preco + Frete) - NOVO
        public decimal PrecoTotal { get; set; } = 0.00m;

        // Propriedades de Endereço

        [Required(ErrorMessage = "O campo CEP é obrigatório.")]
        [StringLength(9, MinimumLength = 8, ErrorMessage = "O CEP deve ter 8 dígitos.")]
        public string CEP { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Rua é obrigatório.")]
        public string Rua { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Número é obrigatório.")]
        public string Numero { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Cidade é obrigatório.")]
        public string Cidade { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Estado é obrigatório.")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "O Estado deve ser a sigla de 2 letras.")]
        public string Estado { get; set; } = string.Empty;

        public string? Complemento { get; set; } 
    }
}