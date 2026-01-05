namespace SiteLoja.ViewModels
{
    public class CategoriaViewModel
    {
        public string Nome { get; set; } = string.Empty;
        public string ImagemUrl { get; set; } = string.Empty;
        public int ProdutoIdRepresentante { get; set; } // Adicionei o ID para facilitar a URL da imagem
    }
}