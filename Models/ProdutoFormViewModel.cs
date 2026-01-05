// Em 'ViewModels/ProdutoFormViewModel.cs' (ou onde você preferir)
using SiteLoja.Models; // Ajuste o namespace
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class ProdutoFormViewModel
{
    // Campos do Produto que serão preenchidos
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public string Desconto { get; set; } = string.Empty;
    public string PrecoPix { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;

    // Propriedade para receber o arquivo de imagem do formulário
    [Required(ErrorMessage = "A Imagem é obrigatória.")]
    [Display(Name = "Imagem do Produto")]
    public IFormFile? ImagemUpload { get; set; }
}