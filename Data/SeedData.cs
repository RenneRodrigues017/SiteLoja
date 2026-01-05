/*using SiteLoja.Models;

namespace SiteLoja.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            // Verifica se já existem produtos no banco. Se houver, não faz nada.
            if (context.ProdutosDb.Any())
            {
                return;
            }

            // 1. Defina a estrutura de dados (usando uma lista de objetos Produto)
            var produtosSeed = new List<Produto>();

            // 2. Converta sua estrutura de dados (aqui usamos a string categoria para facilitar)
            produtosSeed.AddRange(CriarProdutos("Mizuno Pro 8", 429.90m));
            produtosSeed.AddRange(CriarProdutos("Mizuno Pro 6", 429.90m));
            produtosSeed.AddRange(CriarProdutos("Mizuno Pro 7", 429.90m));
            produtosSeed.AddRange(CriarProdutos("Mizuno Pro 13", 449.90m));
            produtosSeed.AddRange(CriarProdutos("Mizuno Pro 14", 449.90m));
            produtosSeed.AddRange(CriarProdutos("NB 2000", 479.90m));
            produtosSeed.AddRange(CriarProdutos("Air Max Exx", 429.90m));
            produtosSeed.AddRange(CriarProdutos("Armani", 379.90m));
            produtosSeed.AddRange(CriarProdutos("Gucci", 399.90m)); // Nota: preços variam no seu original, usei 399.90m como base
            produtosSeed.AddRange(CriarProdutos("Nocta", 479.90m));
            produtosSeed.AddRange(CriarProdutos("Dunk Twist", 379.90m));
            produtosSeed.AddRange(CriarProdutos("Air Force", 279.90m));
            produtosSeed.AddRange(CriarProdutos("Baileli", 479.90m));

            // 3. Adicione todos os produtos e salve as mudanças
            context.ProdutosDb.AddRange(produtosSeed);
            context.SaveChanges();
        }

        // Método auxiliar para criar produtos por categoria
        private static IEnumerable<Produto> CriarProdutos(string categoria, decimal precoBase)
        {
            // Lista de variações que você forneceu (cores/nomes adicionais)
            List<string> variacoes = new List<string>();

            if (categoria == "Mizuno Pro 8")
                variacoes = new List<string> { "Preto", "Branco", "Azul", "Vermelho", "Verde", "Amarelo", "Cinza", "Rosa", "Laranja" };
            else if (categoria == "Mizuno Pro 6")
                variacoes = new List<string> { "Branco", "Azul Rosa", "Camaleão", "Cinza Azul", "Cinza Dourado", "Cinza Rosa" };
            // ... (Você deve preencher todas as variações aqui, o código fica longo)
            else if (categoria == "NB 2000")
                variacoes = new List<string> { "Preto", "Azul", "Azul Branco", "Azul Preto", "Cinza Vermelho", "Preto Cinza" };
            else if (categoria == "Air Force")
                variacoes = new List<string> { "Azul Lilais", "Branco", "Branco Azul", "Branco Lakers", "Branco Vermelho", "Camuflado", "Cinza Azul", "Cinza Branco", "Cinza Escuro", "Marrom Claro", "Preto", "Preto Marrom" };
            // ... etc

            // Exemplo de como gerar os objetos Produto (AJUSTE OS CAMINHOS DE IMAGEM!)
            return variacoes.Select(variacao => new Produto
            {
                Nome = $"{categoria} {variacao}",
                Preco = precoBase,
                // O ImagemUrl deve ser ajustado manualmente, pois o padrão que você usou não é uniforme
                ImagemUrl = $"/images/{categoria.Replace(" ", "")}/{variacao.Replace(" ", "")}.jpg", // Isso é um PALPITE baseado na sua estrutura, deve ser ajustado
                Categoria = categoria
            });
        }
    }
}*/