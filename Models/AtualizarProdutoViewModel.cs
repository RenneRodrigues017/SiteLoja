using SiteLoja.Models;
using System.ComponentModel.DataAnnotations;

namespace SiteLoja.ViewModels;

public class EditarProdutoCompletoViewModel
{
    // Usamos o ProdutoUpdateViewModel que já existia para os dados do formulário
    public ProdutoUpdateViewModel ProdutoParaEdicao { get; set; } = new ProdutoUpdateViewModel();

    // Este campo é CRÍTICO. Armazena a busca original para o retorno.
    public string? TermoBuscaOriginal { get; set; }
}

// Mantenha seu ProdutoUpdateViewModel como estava
public class ProdutoUpdateViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    public string? Nome { get; set; }

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    public string? Categoria { get; set; }
    // Adicione outros campos de edição aqui, se necessário
}

// E mantenha seu AtualizarProdutoViewModel para a tela de busca
public class AtualizarProdutoViewModel
{
    public string? TermoBusca { get; set; }
    public List<Produto>? ResultadosBusca { get; set; }

    // Removido o ProdutoParaEdicao daqui, pois ele está na nova View
}