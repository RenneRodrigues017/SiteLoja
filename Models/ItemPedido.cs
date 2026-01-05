// Models/ItemPedido.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SiteLoja.Models
{
    public class ItemPedido
    {
        [Key]
        public int IdItemPedido { get; set; }

        // Chave estrangeira para Pedido
        public int PedidoId { get; set; }
        [ForeignKey("PedidoId")]
        public Pedido? Pedido { get; set; }

        // Detalhes do Produto
        public int ProdutoId { get; set; }
        public string NomeProduto { get; set; } = string.Empty;
        public string Tamanho { get; set; } = string.Empty;
        public decimal PrecoUnitario { get; set; }
        public int Quantidade { get; set; } = 1;
    }
}