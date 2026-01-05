namespace SiteLoja.Models
{
    // NOVO MODELO (ViewModel Detalhes)
    public class DetalheProdutoViewModel // Renomeei para evitar conflito com a Entidade
    {
        public int Id { get; set; } // Adicionamos o ID para passar para o Endereco
        public string Nome { get; set; } = string.Empty;

        // Preço de venda/promocional
        public decimal PrecoVenda { get; set; }

        // Preço original (se for diferente do venda, indica desconto)
        public decimal PrecoOriginal { get; set; }

        public string ImagemBase64 { get; set; } = string.Empty; // Dados da imagem

        // Dados adicionais do seu modelo Produto
        public string Parcelamento { get; set; } = string.Empty;
        public string PrecoPix { get; set; } = string.Empty;
    }
}