namespace SiteLoja.ViewModels
{
    // Este ViewModel será usado apenas para exibir dados na tela do Catálogo.
    public class ProdutoCatalogoViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string ImagemBase64 { get; set; } = string.Empty; // Para exibir o byte[] ImagemData
        public decimal PrecoNormal { get; set; }
        public decimal? PrecoPromocional { get; set; } // O Preço com desconto, se existir
        public string Parcelamento { get; set; } = string.Empty;
        public string PrecoPix { get; set; } = string.Empty;
    }
}