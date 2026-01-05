// Interface/IPedidoService.cs
using SiteLoja.Models;
using SiteLoja.ViewModels;
using System.Threading.Tasks;

namespace SiteLoja.Interface
{
    public interface IPedidoService
    {
        // Salva o pedido e retorna o Pedido Model completo, incluindo o ID gerado.
        Task<Pedido> CriarNovoPedido(
            Endereco endereco,
            int produtoId,
            string produtoNome,
            string tamanho,
            decimal precoProduto,
            decimal frete,
            string pagamento,
            int parcelaSelecionada,
            int userId);
    }
}