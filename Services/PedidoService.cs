// Services/PedidoService.cs
using SiteLoja.Data;
using SiteLoja.Interface;
using SiteLoja.Models;
using System.Threading.Tasks;

namespace SiteLoja.Services
{
    public class PedidoService : IPedidoService
    {
        // Substitua pelo seu contexto de banco de dados real (ex: ApplicationDbContext)
        private readonly AppDbContext _dbContext;
        private readonly ProdutoService _produtoService;

        // Injeção de dependência
        public PedidoService(AppDbContext dbContext, ProdutoService produtoService)
        {
            _dbContext = dbContext;
            _produtoService = produtoService;
        }

        public async Task<Pedido> CriarNovoPedido(
            Endereco endereco,
            int produtoId,
            string produtoNome,
            string tamanho,
            decimal precoProduto,
            decimal frete,
            string pagamento,
            int parcelaSelecionada,
            int userId)
        {
            // 1. Cria o Endereço (se for persistido separadamente)
            // No seu modelo, o Endereco é parte do Pedido, mas é comum persistir o
            // objeto Endereco antes se ele for reusável ou complexo.
            // Para simplicidade, vamos apenas garantir que os campos necessários estão preenchidos no Model.

            // 2. Cria o objeto Pedido
            var novoPedido = new Pedido
            {
                IdUsuario = userId,
                DataPedido = DateTime.UtcNow, // Novo campo para rastrear a data
                Status = "PENDENTE_WHATSAPP",
                // Associa o Endereço. No Entity Framework, ao adicionar o Pedido,
                // o Endereco também será adicionado.
                Endereco = endereco,

                // Propriedades Adicionais (para o seu modelo, vamos usar uma Lista de Itens)
                ItensPedido = new List<ItemPedido> // Usaremos um Model ItemPedido para detalhes do produto
                {
                    new ItemPedido
                    {
                        ProdutoId = produtoId,
                        NomeProduto = produtoNome,
                        Tamanho = tamanho,
                        PrecoUnitario = precoProduto,
                        Quantidade = 1 // Assumindo quantidade 1
                    }
                },
                Frete = frete,
                FormaPagamento = pagamento,
                DetalhesParcelamento = $"{parcelaSelecionada}x ({endereco.Parcelamento})" // Captura os detalhes do parcelamento formatado
            };

            // 3. Adiciona e Salva no Banco de Dados
            _dbContext.Pedidos.Add(novoPedido);
            await _dbContext.SaveChangesAsync();

            return novoPedido;
        }
    }
}