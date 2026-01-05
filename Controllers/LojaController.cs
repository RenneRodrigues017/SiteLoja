using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteLoja.Interface;
using SiteLoja.Models;
using SiteLoja.Services;
using SiteLoja.ViewModels;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;

namespace SiteLoja.Controllers
{
    public class LojaController : Controller
    {
        // --- CONSTANTES DE PARCELAMENTO (Lógica de Negócio) ---
        private const int MAX_PARCELAS = 12;
        private const int CEP_PREFIXO_INICIO_SJRP = 15000;
        private const int CEP_PREFIXO_FIM_SJRP = 15150;
        private const decimal FRETE_PADRAO = 35.00m;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            // Garante que o formato numérico seja neutro (ponto como separador decimal)
            // Isso é crucial para desserializar os dados do TempData corretamente após o POST
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString |
                    System.Text.Json.Serialization.JsonNumberHandling.WriteAsString
        };
        private readonly ProdutoService _produtoservice;
        private readonly IFreteService _freteService;
        private readonly IUsuario _usuarioService;
        private readonly IPedidoService _pedidoService;

        public LojaController(ProdutoService produtoservice, IFreteService freteService, IUsuario usuarioService, IPedidoService pedidoService) // ⬅️ Alterado
        {
            _produtoservice = produtoservice;
            _freteService = freteService;
            _usuarioService = usuarioService; 
            _pedidoService = pedidoService;
        }

        public IActionResult Index()
        {

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Lancamentos()
        {
            // 1. BUSCA OS PRODUTOS
            // NOTA: É necessário implementar o método BuscarLancamentos() no ProdutoService.
            var produtos = await _produtoservice.BuscarTodos();

            // 2. DEFINE O TÍTULO PARA "CATÁLOGO"
            ViewData["Title"] = "Catálogo"; // Corrigido conforme sua solicitação

            // 3. PASSA A LISTA DE PRODUTOS PARA A VIEW
            // Se a lista estiver vazia (null ou Count=0), a View exibe a mensagem de "nenhum produto".
            return View(produtos);
        }

        [HttpGet]
        public async Task<IActionResult> Detalhes(int id)
        {
            // 1. Busca o produto completo no banco
            var produto = await _produtoservice.BuscarPorId(id);

            if (produto == null)
            {
                return NotFound(); // Retorna 404 se o produto não for encontrado
            }

            var model = new DetalheProdutoViewModel
            {
                Id = produto.Id,
                Nome = produto.Nome,
                PrecoVenda = produto.Preco,
                PrecoOriginal = produto.PrecoOriginal,
                Parcelamento = produto.Parcelamento?.Replace(" sem juros", string.Empty), // Parcelamento está aqui
                PrecoPix = produto.PrecoPix,

                // Converte ImagemData para Base64 (necessário para o HTML)
                ImagemBase64 = (produto.ImagemData != null && produto.ImagemData.Length > 0)
                             ? $"data:image/jpeg;base64,{Convert.ToBase64String(produto.ImagemData)}"
                             : "/images/placeholder.jpg"
            };

            // 3. Retorna a View
            return View("Detalhes", model);
        }

        [HttpGet]
        public async Task<IActionResult> ImagemProduto(int id)
        {
            var produto = await _produtoservice.BuscarPorId(id);

            // Verifica se o produto e os dados da imagem existem
            if (produto?.ImagemData != null && produto.ImagemData.Length > 0)
            {
                // Serve a imagem diretamente. Assumimos image/jpeg, mas ajuste se usar outro formato (png, etc.)
                return File(produto.ImagemData, "image/jpeg");
            }

            // Se não houver imagem, retorna 404
            return NotFound("Imagem não encontrada.");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Endereco(
            string ProdutoNome, string Tamanho,
            int ProdutoId)
        {
            
            string? imagemUrl = string.Empty;

            var produtoOriginal = await _produtoservice.BuscarPorId(ProdutoId);

            if (produtoOriginal == null)
            {
                return NotFound("Produto não encontrado no banco de dados.");
            }

            // --- INÍCIO: Lógica de Preços ---

            decimal precoNormal = produtoOriginal.Preco;
            decimal precoPix = precoNormal;

            // Lógica de conversão do PrecoPix
            if (!string.IsNullOrEmpty(produtoOriginal.PrecoPix) &&
                !decimal.TryParse(produtoOriginal.PrecoPix.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out precoPix))
            {
                precoPix = precoNormal;
            }

            // 1. NÃO CALCULA O FRETE INICIAL. ELE SERÁ 0.00m POR PADRÃO.

            // 2. Passar os preços e constantes de parcelamento para a View via ViewBag para o JavaScript
            ViewBag.PrecoNormal = precoNormal;
            ViewBag.PrecoPix = precoPix; // Valor correto de Preço Pix

            // NOVOS ViewBags: Passando as constantes de parcelamento para o JavaScript
            ViewBag.MaxParcelas = MAX_PARCELAS;
            // A constante PARCELAS_SEM_JUROS não é mais necessária, pois a lógica de juros está na tabela JS.
            // ViewBag.ParcelasSemJuros = PARCELAS_SEM_JUROS; 


            // 3. Gerar URL da Imagem (usando a nova Action ImagemProduto)
            imagemUrl = Url.Action("ImagemProduto", "Loja", new { id = ProdutoId }, HttpContext.Request.Scheme);

            // --- FIM: Lógica de Preços ---
            string cepPadraoInicial = string.Empty;
            string numeroPadraoInicial = string.Empty;

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                // 1. Chamar o Service usando o método BuscarPorIdComEndereco
                var usuario = await _usuarioService.BuscarPorIdComEndereco(userId);

                if (usuario?.EnderecoCliente != null && !string.IsNullOrEmpty(usuario.EnderecoCliente.CEP))
                {
                   cepPadraoInicial = usuario.EnderecoCliente.CEP;
                }
                if (usuario?.EnderecoCliente != null && !string.IsNullOrEmpty(usuario.EnderecoCliente.Numero))
                {
                    numeroPadraoInicial = usuario.EnderecoCliente.Numero;
                }
            }



            var model = new Endereco
            {
                ProdutoId = ProdutoId, // Adicionado para garantir que o ID é passado
                ProdutoNome = ProdutoNome,
                Tamanho = Tamanho,
                Preco = precoNormal,
                Frete = 0.00m, // Frete é 0.00m
                // PrecoTotal é apenas o preço do produto, já que Frete é 0.
                PrecoTotal = precoNormal,
                ImagemUrl = imagemUrl!,
                CEP = cepPadraoInicial, // CEP é string.Empty
                Numero = numeroPadraoInicial
            };

            return View(model);
        }

        // ACTION PARA CÁLCULO DE FRETE VIA AJAX COM LÓGICA DE SJ RIO PRETO
        [HttpGet]
        public IActionResult CalcularFrete(string cep)
        {
            try
            {
                // Limpa o CEP (mantém apenas dígitos)
                string cepLimpo = new string(cep.Where(char.IsDigit).ToArray());

                // VALOR PADRÃO DE FRETE
                decimal frete = FRETE_PADRAO; // 35.00m

                // --- LÓGICA PERSONALIZADA DE FRETE PARA SÃO JOSÉ DO RIO PRETO ---
                // Verifica se o CEP está na faixa de São José do Rio Preto (15000-000 até 15150-000)
                if (cepLimpo.Length == 8)
                {
                    if (int.TryParse(cepLimpo.Substring(0, 5), out int prefixoCep))
                    {
                        if (prefixoCep >= CEP_PREFIXO_INICIO_SJRP && prefixoCep <= CEP_PREFIXO_FIM_SJRP)
                        {
                            frete = 0.00m; // FRETE GRÁTIS
                        }
                    }
                }

                // Retorna o frete como um objeto JSON
                // Usamos InvariantCulture para o JavaScript poder parsear corretamente o valor '0.00' ou '35.00'
                return Json(new { frete = frete.ToString("F2", CultureInfo.InvariantCulture) });

            }
            catch (Exception ex)
            {
                // Em caso de erro, retorna um frete padrão e uma mensagem de erro
                return StatusCode(500, new { frete = FRETE_PADRAO.ToString("F2", CultureInfo.InvariantCulture), erro = "Falha ao calcular frete: " + ex.Message });
            }
        }

        // --- ACTION DE PROCESSAMENTO (Exemplo Simples) ---
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ProcessarPedido(Endereco model, string Pagamento, int ParcelaSelecionada)
        {
            if (!ModelState.IsValid)
            {
                // Este código irá imprimir no console/log do servidor o motivo da falha
                var erros = ModelState.Values.SelectMany(v => v.Errors)
                                             .Select(e => e.ErrorMessage);

                // Log do erro (Ajuste para seu sistema de log, ex: ILogger, Console.WriteLine)
                System.Diagnostics.Debug.WriteLine($"Falha na validação: {string.Join(", ", erros)}");
                // Se o ModelState não for válido, é preciso recarregar os ViewBags para que o Razor/JS funcione
                var produtoOriginal = _produtoservice.BuscarPorId(model.ProdutoId).Result; // Ou buscar de forma síncrona/cache, ou de forma assíncrona com .Result

                if (produtoOriginal == null)
                {
                    return NotFound("Produto não encontrado ao processar pedido.");
                }

                decimal precoNormal = produtoOriginal.Preco;
                decimal precoPix = precoNormal;

                if (!string.IsNullOrEmpty(produtoOriginal.PrecoPix) &&
                   !decimal.TryParse(produtoOriginal.PrecoPix.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out precoPix))
                {
                    precoPix = precoNormal;
                }

                ViewBag.PrecoNormal = precoNormal;
                ViewBag.PrecoPix = precoPix;
                ViewBag.MaxParcelas = MAX_PARCELAS;

                // Revalidação do pagamento (se a validação falhar por aqui)
                if (string.IsNullOrEmpty(Pagamento))
                {
                    ModelState.AddModelError("Pagamento", "A forma de pagamento deve ser selecionada.");
                }

                return View("Endereco", model);
            }
            try
            {
                var pedidoSalvo = await _pedidoService.CriarNovoPedido(
                    endereco: model,
                    produtoId: model.ProdutoId,
                    produtoNome: model.ProdutoNome,
                    tamanho: model.Tamanho,
                    precoProduto: model.Preco,
                    frete: model.Frete,
                    pagamento: Pagamento,
                    parcelaSelecionada: ParcelaSelecionada,
                    userId: model.Id
                );

                // Agora você tem o IdPedido gerado em pedidoSalvo.IdPedido
            }
            catch (Exception ex)
            {
                // Lidar com falhas de banco de dados
                System.Diagnostics.Debug.WriteLine($"ERRO AO SALVAR PEDIDO: {ex.Message}");
                // Você pode adicionar um erro ao ModelState ou retornar uma View de erro
                TempData["ErroPedido"] = "Não foi possível finalizar o pedido. Tente novamente.";
                return RedirectToAction("Index");
            }


            // Exemplo de como usar os dados:
            string mensagemWhatsApp = $@"
                    *Novo Pedido - Loja Online*
                    *----------------------------------*
*Produto:* {model.ProdutoNome} *ID*:{model.ProdutoId}
*Tamanho:* {model.Tamanho}
*Pagamento:* {Pagamento} 
*Parcelas:* {ParcelaSelecionada} ({model.Parcelamento})
*Endereço de Entrega:*
{model.Rua}, {model.Numero} {model.Complemento}
*CEP*: {model.CEP} | {model.Cidade}-{model.Estado}
                    *----------------------------------*
                    ";

            string numeroWhatsApp = "17992664042";
            string mensagemCodificada = System.Net.WebUtility.UrlEncode(mensagemWhatsApp);
            string urlWhatsApp = $"https://api.whatsapp.com/send?phone={numeroWhatsApp}&text={mensagemCodificada}";
            TempData["MensagemPedido"] = mensagemWhatsApp;
            TempData["LinkWhatsApp"] = urlWhatsApp;

            // Envia para a Action de sucesso ou para o WhatsApp.
            return RedirectToAction("Confirmacao");
        }
        [HttpGet]
        public IActionResult Confirmacao()
        {
            string mensagemPedido = TempData["MensagemPedido"] as string ?? "Detalhes do pedido não encontrados.";
            string? urlWhatsApp = TempData["LinkWhatsApp"] as string;

            if (string.IsNullOrEmpty(urlWhatsApp))
            {
                // Se a primeira vez falhou e o TempData já foi consumido (ou nunca existiu)
                return RedirectToAction("Index");
            }

            TempData.Keep("LinkWhatsApp");
            TempData.Keep("MensagemPedido");

            ViewBag.DetalhesPedido = mensagemPedido;
            ViewBag.LinkWhatsApp = urlWhatsApp;

            // Retorna a View (GET) para o navegador
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Categoria(string q)
        {
            // 1. Definição do Termo de Busca e Título da Página
            string termoBusca = string.IsNullOrEmpty(q) ? "Todos os Produtos" : q;
            ViewData["TermoBusca"] = termoBusca;
            ViewData["TituloPagina"] = string.IsNullOrEmpty(q) ? "Todos os Produtos" : $"Resultados para: \"{q}\"";

            // 2. Lógica para buscar os produtos.
            // Assumindo que você tem um método no ProdutoService para buscar por termo:

            // ATENÇÃO: Se não tiver um filtro, ele deve retornar todos os produtos.
            var produtos = await _produtoservice.BuscarProdutosPorTermo(termoBusca);
            // ^ Ajuste o nome do método de busca, se necessário. Se for uma busca por nome/categoria, implemente-a no Service.

            // 3. Conversão para o ViewModel (ProdutoCatalogoViewModel)
            var produtosViewModel = produtos.Select(p => new ProdutoCatalogoViewModel
            {
                Id = p.Id,
                Nome = p.Nome,
                PrecoNormal = p.PrecoOriginal > p.Preco ? p.PrecoOriginal : p.Preco, // Usar PrecoOriginal se existir, senão Preco
                PrecoPromocional = p.PrecoOriginal > p.Preco ? p.Preco : (decimal?)null, // Se tem Original, Preco é promocional
                PrecoPix = p.PrecoPix,
                Parcelamento = p.Parcelamento?.Replace(" sem juros", string.Empty),

                // Conversão de Imagem Data para Base64
                ImagemBase64 = (p.ImagemData != null && p.ImagemData.Length > 0)
                               ? $"data:image/jpeg;base64,{Convert.ToBase64String(p.ImagemData)}"
                               : "/images/placeholder.jpg"
            }).ToList();

            // 4. Retorna a View Categoria.cshtml com a lista de produtos
            // A View Categoria.cshtml espera um IEnumerable<SiteLoja.ViewModels.ProdutoCatalogoViewModel>
            return View(produtosViewModel);
        }
        [HttpGet]
        public async Task<IActionResult> Colecao()
        {
            // 1. Configuração do título (opcional)
            ViewData["TituloPagina"] = "Coleção Completa";
            ViewData["TermoBusca"] = "Tênis Brancos"; // Para exibir na mensagem de "sem produtos", se necessário.

            // 2. Busca os dados no Service
            var produtos = await _produtoservice.BuscarColecao();

            // 3. Conversão para o ViewModel (ProdutoCatalogoViewModel)
            var produtosViewModel = produtos.Select(p => new ProdutoCatalogoViewModel
            {
                Id = p.Id,
                Nome = p.Nome,
                PrecoNormal = p.PrecoOriginal > p.Preco ? p.PrecoOriginal : p.Preco,
                PrecoPromocional = p.PrecoOriginal > p.Preco ? p.Preco : (decimal?)null,
                PrecoPix = p.PrecoPix,
                Parcelamento = p.Parcelamento?.Replace(" sem juros", string.Empty),
                ImagemBase64 = (p.ImagemData != null && p.ImagemData.Length > 0)
                                    ? $"data:image/jpeg;base64,{Convert.ToBase64String(p.ImagemData)}"
                                    : "/images/placeholder.jpg"
            }).ToList();

            // 4. Retorna a View 'Categoria.cshtml' usando o Model (Lista de Produtos)
            // Se você quer que 'Colecao' use a View 'Categoria', você deve especificar isso.
            return View("Categoria", produtosViewModel);
        }

        public async Task<IActionResult> Catalogo()
        {
            // Ação que usa o novo método no Service
            var categoriasViewModel = await _produtoservice.BuscarTodasCategoriasComDestaque();

            // Passa a lista de CategoriaViewModel diretamente para a View, 
            // que espera @model IEnumerable<SiteLoja.ViewModels.CategoriaViewModel>
            return View(categoriasViewModel);
        }

        //
        // AÇÃO 2: RETORNAR PRODUTOS POR CATEGORIA (VIA AJAX)
        //
        [HttpGet]
        public async Task<IActionResult> GetProdutosPorCategoria(string categoria)
        {
            if (string.IsNullOrEmpty(categoria))
            {
                return BadRequest(new { error = "A categoria é obrigatória." });
            }

            // Chama o método do Service para buscar todos os produtos daquela categoria
            var produtosDaCategoria = await _produtoservice.BuscarProdutosPorCategoria(categoria);

            // Mapeia o resultado para um objeto anônimo (DTO) para enviar ao JavaScript
            var resultadoParaJson = produtosDaCategoria
                .Select(p => new
                {
                    p.Id, // Inclui o ID caso seja necessário na View de Detalhes
                    p.Nome,
                    p.Preco,
                    // Constrói a URL da imagem para cada produto
                    ImagemUrl = $"/Loja/GetImagemProduto?produtoId={p.Id}"
                })
                .ToList();

            return Json(resultadoParaJson);
        }

        //
        // AÇÃO 3: AUXILIAR PARA SERVIR A IMAGEM (se você usa ImagemData byte[])
        //
        public async Task<IActionResult> GetImagemProduto(int produtoId)
        {
            var produto = await _produtoservice.BuscarPorId(produtoId);

            if (produto?.ImagemData != null)
            {
                // O tipo MIME pode ser "image/png", "image/gif", etc.
                return File(produto.ImagemData, "image/jpeg");
            }

            // Retorna um status Not Found ou uma imagem placeholder
            return NotFound();
        }
    }
}