
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using SiteLoja.Data;
using SiteLoja.Interface;
using SiteLoja.Models;
using SiteLoja.ViewModels;

namespace SiteLoja.Services
{
    public class ProdutoService : IProduto
    {
        private readonly AppDbContext _context;

        public ProdutoService(AppDbContext context)
        {
            _context = context;
        }
        public async Task AdicionarProduto(Produto _produto)
        { 
            _produto.PrecoOriginal = _produto.Preco;
            _produto.Parcelamento = "Em até 12x ";

            await _context.ProdutosDb.AddAsync(_produto);
            await _context.SaveChangesAsync();
            
        }
        public bool Excluir(int id)
        {
            var produto = _context.ProdutosDb.FirstOrDefault(c => c.Id == id);

            if (produto == null)
            {
                return false;
            }

            _context.Remove(produto);
            _context.SaveChanges();
            return true;
        }
        public async Task<List<Produto>> BuscarTodos()
        {
            return await _context.ProdutosDb.ToListAsync();
        }
        public async Task<List<Produto>> BuscarProdutosPorTermo(string termo)
        {
            if (string.IsNullOrWhiteSpace(termo))
            {
                // Se o termo for vazio, retorna todos os produtos
                return await BuscarTodos();
            }

            var termoFormatado = termo.Trim().ToLower();

            // Busca produtos cujo Nome OU Categoria contenham o termo
            return await _context.ProdutosDb
                .Where(p =>
                    p.Nome.ToLower().Contains(termoFormatado) ||
                    p.Categoria.ToLower().Contains(termoFormatado))
                .ToListAsync();
        }
        public async Task<List<Produto>> BuscarProdutosPorCategoria(string categoria)
        {
            var categoriaFormatada = categoria.Trim().ToLower();

            return await _context.ProdutosDb
                .Where(p => p.Categoria.ToLower() == categoriaFormatada)
                .ToListAsync();
        }
        public async Task<Produto?> BuscarPorId(int id)
        {
            // Usa SingleOrDefaultAsync para buscar o produto pelo Id
            return await _context.ProdutosDb.SingleOrDefaultAsync(p => p.Id == id);
        }
        public async Task<IEnumerable<Produto>> BuscarDestaquesPorCategoria()
        {
            try
            {
                var destaques = await _context.ProdutosDb
                    // 1. Ordena os produtos de forma decrescente pelo Id.
                    //    Assumimos que Id maior significa produto mais novo.
                    .OrderByDescending(p => p.Id)

                    // 2. Agrupa os produtos pela Categoria.
                    .GroupBy(p => p.Categoria)

                    // 3. Seleciona o primeiro produto de cada grupo.
                    //    Devido à ordenação anterior, este será o produto com o Id mais alto (o "mais recente").
                    .Select(g => g.First()).Take(4)

                    // 4. Executa a consulta no banco de dados.
                    .ToListAsync();

                return destaques;
            }
            catch (Exception)
            {
                // Tratamento de erro
                return Enumerable.Empty<Produto>();
            }
        }
        public async Task<IEnumerable<CategoriaViewModel>> BuscarTodasCategoriasComDestaque()
        {
            try
            {
                // 1. Agrupar pela Categoria e selecionar o produto mais recente (maior Id) em cada grupo.
                var produtosRepresentantes = await _context.ProdutosDb
                    .GroupBy(p => p.Categoria)
                    // Seleciona o primeiro elemento de cada grupo, após ordenar internamente
                    .Select(g => g.OrderByDescending(p => p.Id).First())
                    .ToListAsync();

                // 2. Mapear os Produtos Representantes para CategoriaViewModel.
                // O mapeamento constrói a ImagemUrl, que o Controller usará.
                var categoriasViewModel = produtosRepresentantes
                    .Select(p => new CategoriaViewModel
                    {
                        Nome = p.Categoria, // Usamos o campo Categoria do Model Produto para o Nome da Categoria
                        ProdutoIdRepresentante = p.Id, // Usamos o Id do Produto Representante para a URL da imagem
                                                       // O Controller deve ser responsável por construir a URL final, mas
                                                       // para fins de mapeamento, você pode passar o ID ou a URL esperada.
                                                       // Vou manter o mapeamento mais limpo e deixar a construção da URL
                                                       // para o Controller, como você fez na Action Catalogo().
                                                       // Mas, se quiser mapear a URL de imagem aqui:
                        ImagemUrl = $"/Loja/GetImagemProduto?produtoId={p.Id}"
                    })
                    .OrderBy(c => c.Nome)
                    .ToList();

                // 3. Retorna a lista de ViewModels.
                return categoriasViewModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                // Em caso de erro, retorna uma lista vazia do tipo esperado.
                return Enumerable.Empty<CategoriaViewModel>();
            }
        }
        public async Task<IEnumerable<Produto>> BuscarColecao()
        {
            try
            {
                // Define os termos de busca (para evitar mágica na string)
                string termo1 = "Branco";
                string termo2 = "White";

                var colecaoFiltrada = await _context.ProdutosDb
                    // 1. FILTRA os produtos que contêm "Branco" OU "White" no Nome.
                    .Where(p => p.Nome.Contains(termo1) || p.Nome.Contains(termo2))

                    // 2. Ordena os produtos de forma decrescente pelo Id (mais novo primeiro).
                    .OrderByDescending(p => p.Id)

                    // 3. (Opcional) Limita o número de resultados, se desejar.
                    // .Take(4)

                    // 4. Executa a consulta no banco de dados.
                    .ToListAsync();

                return colecaoFiltrada;
            }
            catch (Exception)
            {
                // Logar a exceção 'ex' aqui
                return Enumerable.Empty<Produto>();
            }
        }
        // ProdutoService.cs

        public async Task AtualizarProduto(ProdutoFormViewModel produtoViewModel, string nomeOriginal)
        {
            // 1. Buscar o produto existente pelo Nome.
            // O ViewModel deve conter o NOME ATUAL do produto para que a busca funcione.
            var produtoAtualizar = await _context.ProdutosDb
                .FirstOrDefaultAsync(p => p.Nome == produtoViewModel.Nome); // Usamos o Nome no ViewModel para buscar.

            if (produtoAtualizar == null)
            {
                // O produto não foi encontrado.
                throw new InvalidOperationException($"Produto '{nomeOriginal}' não encontrado para atualização.");
            }

            // 2. Processar e atualizar a imagem se um novo arquivo foi enviado
            if (produtoViewModel.ImagemUpload != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await produtoViewModel.ImagemUpload.CopyToAsync(memoryStream);
                    produtoAtualizar.ImagemData = memoryStream.ToArray(); // Atualiza a ImagemData
                }
            }

            // 3. Atualizar as outras propriedades (permitindo a alteração do Nome)
            // O Nome no ViewModel agora contém o NOVO NOME que o usuário digitou.
            produtoAtualizar.Nome = produtoViewModel.Nome;
            produtoAtualizar.Categoria = produtoViewModel.Categoria;
            produtoAtualizar.Desconto = produtoViewModel.Desconto;
            produtoAtualizar.Preco = produtoViewModel.Preco;
            produtoAtualizar.PrecoPix = produtoViewModel.PrecoPix;

            // 4. Salvar as alterações no banco de dados
            await _context.SaveChangesAsync();
        }
        public async Task AtualizarProdutoSimplificado(ProdutoUpdateViewModel produtoViewModel)
        {
            // 1. Buscar o produto existente pelo ID (Chave segura)
            var produtoAtualizar = await _context.ProdutosDb
                .FirstOrDefaultAsync(p => p.Id == produtoViewModel.Id);

            if (produtoAtualizar == null)
            {
                throw new KeyNotFoundException($"Produto com ID '{produtoViewModel.Id}' não encontrado para atualização.");
            }

            // 2. Atualizar apenas Nome e Categoria
            // O Entity Framework rastreia 'produtoAtualizar' e detecta a alteração.
            produtoAtualizar.Nome = produtoViewModel!.Nome!;
            produtoAtualizar.Categoria = produtoViewModel!.Categoria!;

            // 3. Salvar as alterações no banco de dados
            await _context.SaveChangesAsync();
        }
        public async Task<List<Produto>> BuscarProdutosPorNome(string nomeTermo)
        {
            if (string.IsNullOrWhiteSpace(nomeTermo))
            {
                return new List<Produto>();
            }

            var termoFormatado = nomeTermo.Trim().ToLower();

            // Busca produtos cujo Nome contenha o termo
            return await _context.ProdutosDb
                .Where(p => p.Nome.ToLower().Contains(termoFormatado))
                .ToListAsync();
        }
        public async Task<List<Produto>> BuscarPorIds(List<int> ids)
        {
            return await _context.ProdutosDb
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();
        }
    }
}