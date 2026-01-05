// Models/Pedido.cs
using System.ComponentModel.DataAnnotations;

namespace SiteLoja.Models
{
    public class Pedido
    {
        [Key]
        public int IdPedido { get; set; }

        // Substituindo List<Produto> por uma lista de Itens do Pedido
        public List<ItemPedido> ItensPedido { get; set; } = new List<ItemPedido>();

        // Chave estrangeira para Endereco (pode ser o ID ou o objeto)
        [Required(ErrorMessage = "Endereço obrigatorio")]
        public Endereco Endereco { get; set; }

        public int IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }

        // Novos campos para rastreamento
        public DateTime DataPedido { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "CRIADO";

        // Detalhes da Compra
        public decimal Frete { get; set; }
        public string FormaPagamento { get; set; } = string.Empty;
        public string DetalhesParcelamento { get; set; } = string.Empty;
    }
}