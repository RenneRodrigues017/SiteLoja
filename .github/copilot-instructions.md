# SiteLoja - Instruções para Agentes de IA

## Visão Geral do Projeto
**SiteLoja** é uma aplicação ASP.NET Core 9 MVC para e-commerce de calçados esportivos. A arquitetura é simples e direta: Controller → ViewModel → View (Razor).

## Arquitetura Principal

### Controllers (`Controllers/LojaController.cs`)
- **Action `Lancamentos()`**: Retorna a página inicial com 5 produtos em destaque
- **Action `Categoria(string nome)`**: Filtra produtos por categoria (Nike, Adidas, Mizuno, etc.)
- **Dados**: Hardcoded em memória usando `List<ProdutoViewModel>` (sem banco de dados ainda)

### Models (`Models/ProdutoViewModel.cs`)
Propriedades essenciais:
- `Id`, `Nome`, `ImagemUrl`: Informações básicas
- `PrecoOriginal`, `PrecoPromocional`: Suporta descontos
- `Parcelamento`, `PrecoPix`: Opções de pagamento
- `Desconto`: String com % OFF (ex: "20% OFF")
- `Categoria`: Classificação do produto

### Views (`Views/Loja/`)
- **`Lancamentos.cshtml`**: Página com grid de categorias + JavaScript para filtrar produtos
  - Cada categoria tem um objeto com `nome`, `preco`, `imagem`
  - Usa `forEach` em JavaScript para renderizar dinamicamente
  - Grid responsivo: `grid-template-columns: repeat(auto-fill, minmax(220px, 1fr))`

## Convenções de Código

### Padrão de Dados de Produtos
```javascript
produtos = [
    { 
        nome: categoria + " Variação", 
        preco: 429.90, 
        imagem: "/images/[Marca]/[Modelo]-[Cor].jpg" 
    }
]
```
- **Preço**: Sempre em decimal com 2 casas (ex: `429.90`)
- **Nome**: Concatena categoria + descrição da cor/variação
- **Imagem**: Caminho relativo em `wwwroot/images/[Marca]/`

### Estrutura de Pastas de Imagens
```
wwwroot/images/
├── Mizuno/    (Pro-6, Pro-7, Pro-8, Pro-13, Pro-14)
├── Nike/      (Exx, nike_tn.jpg, etc.)
├── NB/        (NB-Preto.jpeg, NB-Azul.jpeg, etc.)
├── Armani/    (Branco.jpg, Preto.jpg, etc.)
└── Oakley/
```

## Padrões JavaScript

### Adicionar Nova Categoria
1. Adicione o objeto na array `categorias` (linhas ~17-29)
2. Adicione um `else if` no evento click dos botões (linhas ~68+)
3. Defina a array `produtos` com os items seguindo o padrão acima

### Exemplo (já implementado):
```javascript
else if(categoria === "Mizuno Pro 14") {
    produtos = [
        { nome: categoria + " Azul", preco: 449.90, imagem: "/images/Mizuno/Pro-14-Azul.jpg" },
        // ... mais items
    ];
}
```

## Estilo CSS

### Variáveis Globais (`site.css` linhas 1-10)
```css
--bg: #f6f6f6         /* Fundo dos contêineres */
--text: #111          /* Cor de texto padrão */
--accent: #000        /* Cor de destaque (preto) */
--success: #28a745    /* Verde para sucessos */
```

### Grids Responsivos
- Categorias: `repeat(auto-fill, minmax(220px, 1fr))`
- Produtos: `repeat(auto-fill, minmax(220px, 1fr))`
- Breakpoint implícito: 220px = largura mínima de card

### Botões
- `.btn-view`: Preto com hover para cinza (#444)
- `.btn-buy`: Vermelho (#ff4d4d) com hover para vermelho escuro (#d63b3b)
- `.btn-back`: Cinza (#555) com hover para cinza escuro (#333)

## Fluxo de Desenvolvimento

### Adicionar Novo Produto a Categoria Existente
1. Encontre a categoria em `Lancamentos.cshtml` (ex: "Mizuno Pro 14" linha ~125)
2. Adicione novo objeto à array `produtos` seguindo o padrão
3. Teste no navegador (não requer rebuild)

### Adicionar Nova Categoria Completa
1. Adicione objeto com `Nome` e `Imagem` à array `categorias` (linha ~18)
2. Adicione o `else if` no handler de clique (após linha ~166)
3. Verifique se as imagens existem em `wwwroot/images/[Marca]/`

### Modificar Preços
- **Frontend (Lancamentos.cshtml)**: Edite a propriedade `preco` dos produtos
- **Backend (LojaController.cs)**: Edite `PrecoPromocional` nos `ProdutoViewModel`

## Build & Run

```powershell
# Restaurar dependências
dotnet restore

# Build
dotnet build

# Run (localhost:5000 e 5001 com HTTPS)
dotnet run

# Watch mode (auto-reload)
dotnet watch run
```

## Estrutura de Rota Padrão
Definida em `Program.cs` linha 25:
```csharp
pattern: "{controller=Loja}/{action=Lancamentos}/{id?}"
```
- Rota padrão: `/Loja/Lancamentos`
- Alternativas: `/Loja/Categoria?nome=Nike`

## Próximas Melhorias (Planejadas)
- [ ] Banco de dados (SQL Server ou SQLite) para produtos
- [ ] Carrinho de compras com localStorage/sessão
- [ ] Autenticação de usuário
- [ ] Página de detalhes do produto
- [ ] Filtros avançados (preço, marca, cor)

---

**Última atualização**: 10 de novembro de 2025

Se encontrar padrões não documentados ou precisar adicionar funcionalidades, mantenha este arquivo atualizado!
