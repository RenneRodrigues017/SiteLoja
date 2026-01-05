using Microsoft.AspNetCore.Mvc;
using SiteLoja.Services;
using SiteLoja.ViewModels;

namespace SiteLoja.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProdutoService _produtoService;
        public HomeController (ProdutoService produtoService)
        {
            _produtoService = produtoService;
        }
        public async Task<IActionResult> Index()
        {
            // IDs que devem aparecer nos Destaques
            var idsDestaque = new List<int> { 34, 1081, 1209, 1127 };

            // Busca somente esses produtos do banco
            var produtos = await _produtoService.BuscarPorIds(idsDestaque);

            // Mapeamento para ViewModel
            var destaquesViewModel = produtos.Select(p => new ProdutoCatalogoViewModel
            {
                Id = p.Id,
                Nome = p.Nome,

                PrecoNormal = p.PrecoOriginal != p.Preco ? p.PrecoOriginal : p.Preco,
                PrecoPromocional = p.PrecoOriginal != p.Preco ? p.Preco : (decimal?)null,

                PrecoPix = p.PrecoPix,
                Parcelamento = p.Parcelamento,

                ImagemBase64 = (p.ImagemData != null && p.ImagemData.Length > 0)
                               ? $"data:image/jpeg;base64,{Convert.ToBase64String(p.ImagemData)}"
                               : "/images/placeholder.jpg"
            }).ToList();

            return View(destaquesViewModel);
        }

    }
}
