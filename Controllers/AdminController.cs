using Microsoft.AspNetCore.Mvc;
using SiteLoja.Models;
using SiteLoja.Services;
using SiteLoja.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.IO;

namespace SiteLoja.Controllers;

public class AdminController : Controller
{
    private readonly ProdutoService _produtoService;

    public AdminController(ProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    [HttpGet]
    public IActionResult Novo()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Novo(ProdutoFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // ... Lógica de Novo Produto (mantida) ...
        byte[]? imagemBytes = null;
        if (model.ImagemUpload != null)
        {
            using (var memoryStream = new MemoryStream())
            {
                await model.ImagemUpload.CopyToAsync(memoryStream);
                imagemBytes = memoryStream.ToArray();
            }
        }

        var novoProduto = new Produto
        {
            Nome = model.Nome,
            Preco = model.Preco,
            Desconto = model.Desconto,
            PrecoPix = model.PrecoPix,
            Categoria = model.Categoria,
            ImagemData = imagemBytes
        };

        await _produtoService.AdicionarProduto(novoProduto);

        TempData["SuccessMessage"] = $"Produto '{novoProduto.Nome}' cadastrado com sucesso!";
        return RedirectToAction("Novo", "Admin");
    }

    // -------------------------------------------------------------
    // GET: Admin/Atualizar (Tela de Busca e Lista)
    // -------------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> Atualizar(string? termoBusca)
    {
        var modelPrincipal = new AtualizarProdutoViewModel
        {
            TermoBusca = termoBusca
        };

        if (!string.IsNullOrWhiteSpace(termoBusca))
        {
            var resultados = await _produtoService.BuscarProdutosPorNome(termoBusca);
            modelPrincipal.ResultadosBusca = resultados;

            if (!resultados.Any())
            {
                TempData["ErrorMessage"] = $"Nenhum produto encontrado com o nome '{termoBusca}'.";
            }
        }

        return View(modelPrincipal);
    }

    // -------------------------------------------------------------
    // GET: Admin/Editar (NOVA ACTION - Tela de Formulário de Edição)
    // -------------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> Editar(int id, string termoBuscaOriginal)
    {
        var produto = await _produtoService.BuscarPorId(id);

        if (produto == null)
        {
            TempData["ErrorMessage"] = $"Produto com ID {id} não encontrado.";
            return RedirectToAction("Atualizar");
        }

        var model = new EditarProdutoCompletoViewModel // Usaremos um ViewModel que carrega os dois dados
        {
            TermoBuscaOriginal = termoBuscaOriginal, // Armazena a busca para o retorno
            ProdutoParaEdicao = new ProdutoUpdateViewModel
            {
                Id = produto.Id,
                Nome = produto.Nome,
                Categoria = produto.Categoria
                // Adicione outros campos necessários aqui
            }
        };

        return View(model); // Retorna a nova View Editar.cshtml
    }

    // -------------------------------------------------------------
    // POST: Admin/SalvarEdicao (NOVA ACTION - Salva e Redireciona)
    // -------------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarEdicao(EditarProdutoCompletoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Se a validação falhar, retorna à View de Edição com os dados
            return View("Editar", model);
        }

        try
        {
            await _produtoService.AtualizarProdutoSimplificado(model.ProdutoParaEdicao);

            TempData["SuccessMessage"] = $"Produto '{model.ProdutoParaEdicao.Nome}' (ID: {model.ProdutoParaEdicao.Id}) atualizado com sucesso!";

            // ✅ REDIRECIONAMENTO CORRETO: Volta para a lista com o termo de busca original.
            if (!string.IsNullOrWhiteSpace(model.TermoBuscaOriginal))
            {
                return RedirectToAction("Atualizar", "Admin", new { termoBusca = model.TermoBuscaOriginal });
            }

            // Se não houver termo de busca, volta para a tela de busca vazia
            return RedirectToAction("Atualizar");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Erro ao atualizar o produto. Detalhe: {ex.Message}";
            // Retorna para a tela de edição em caso de erro no DB
            return View("Editar", model);
        }
    }
}