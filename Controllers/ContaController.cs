using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteLoja.Models;
using SiteLoja.Services;
using SiteLoja.ViewModels;
using System.Security.Claims;

namespace SiteLoja.Controllers
{
    public class ContaController : Controller
    {
        private readonly UsuarioService _usuarioService;

        public ContaController(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            // ADICIONAR ISSO: Armazena a URL de retorno na ViewData
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel usuario, string returnUrl = null) // ADICIONAR ISSO: o 'returnUrl = null'
        {
            if (!ModelState.IsValid)
            {
                // ADICIONAR ISSO: Garante que o ReturnUrl volte para a View em caso de erro
                ViewData["ReturnUrl"] = returnUrl;
                return View(usuario);
            }

            var resultado = _usuarioService.Autenticar(usuario!.Email!, usuario!.Senha!);

            if (resultado == null)
            {
                ModelState.AddModelError(string.Empty, "Email ou senha inválidos.");

                // ADICIONAR ISSO: Garante que o ReturnUrl volte para a View em caso de erro
                ViewData["ReturnUrl"] = returnUrl;

                // CORRIGIR ISSO: Você estava retornando 'resultado' (que é null),
                // deve retornar o modelo de entrada 'usuario'.
                return View(usuario);
            }

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, resultado!.Id.ToString()), 
            // O nome de usuário a ser exibido (Email)
            new Claim(ClaimTypes.Name, resultado!.Email!) 
            // Você pode adicionar mais Claims aqui (Ex: Role, Nome Completo, etc.)
        };

            var claimsIdentity = new ClaimsIdentity(
                claims, "CookieAuthentication");

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Torna o cookie persistente (lembrar do usuário)
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) // Tempo de expiração (opcional, mas bom)
            };

            // 2. CRIAR O COOKIE (Login) - Sua lógica ORIGINAL
            await HttpContext.SignInAsync(
                "CookieAuthentication", // Deve ser o mesmo nome do esquema do Program.cs
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // ------------------------------------------------------------------
            // 🚀 ADIÇÃO DA LÓGICA DE REDIRECIONAMENTO COM RETURNURL
            // ------------------------------------------------------------------

            // 1. Prioridade: Se for admin, vai para a área de admin
            if (usuario.Email == "admin1@gmail.com")
            {
                return RedirectToAction("Novo", "Admin");
            }

            // ADICIONAR ISSO: Lógica para usar o ReturnUrl
            // 2. Verifica se o ReturnUrl existe E se ele é um URL local (segurança)
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl); // Redireciona para a tela do produto
            }

            // 3. Fallback: Se não tem ReturnUrl (veio do cadastro) ou é inválido, vai para a home
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Cadastro()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Cadastro(Usuario novoUsuario)
        {
            // 1. Validação do ModelState (incluirá as validações do EnderecoCliente)
            if (!ModelState.IsValid)
            {
                return View(novoUsuario); // Retorna a View para mostrar os erros
            }

            // 2. Tenta Cadastrar o Usuário e seu Endereço Relacionado
            var resultado = _usuarioService.Cadastrar(novoUsuario);

            if (resultado == false)
            {
                // 🚨 MELHORIA: Adiciona um erro genérico e retorna a View. 
                // Não é recomendado usar NotFound() para erros de lógica de negócio.
                ModelState.AddModelError("", "Erro ao cadastrar usuário. O e-mail pode já estar em uso.");
                return View(novoUsuario);
            }

            // Sucesso
            return RedirectToAction("Login");
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuthentication"); // Remove o cookie

            // Redireciona para a página inicial
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public IActionResult Detalhes()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            // 1. Obter o ID do usuário logado (string)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Tenta converter o userId para int.
            // O bloco 'if' anterior era: if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))

            // Novo bloco de verificação para garantir que o ID é válido e positivo (id > 0)
            if (!int.TryParse(userId, out int id) || id <= 0)
            {
                // Se a string for nula/vazia, não for número, ou for zero/negativo:
                // O usuário não está logado corretamente ou o Claim é inválido.
                return RedirectToAction("Login", "Conta");
            }

            // 2. Buscar o usuário completo (com endereço)
            var usuario = await _usuarioService.BuscarPorIdComEndereco(id);

            if (usuario == null)
            {
                // 🚨 CORREÇÃO DE UX: Se o usuário *tinha* um token mas não existe mais no BD,
                // force o logout para limpar o estado de autenticação e redirecione para o login.
                // Isso assume que você está usando Autenticação por Cookies.
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // Retorna para o login em vez do NotFound("Usuário não encontrado.").
                return RedirectToAction("Login", "Conta");
            }

            // 3. Mapear o Model completo para o ViewModel
            var model = new PerfilViewModel
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,

                // Mapeamento do Endereço (usando o null-coalescing operator ??)
                CEP = usuario.EnderecoCliente?.CEP ?? string.Empty,
                Rua = usuario.EnderecoCliente?.Rua ?? string.Empty,
                Numero = usuario.EnderecoCliente?.Numero ?? string.Empty,
                Cidade = usuario.EnderecoCliente?.Cidade ?? string.Empty,
                Estado = usuario.EnderecoCliente?.Estado ?? string.Empty,
                Complemento = usuario.EnderecoCliente?.Complemento,

                NovaSenha = null,
                ConfirmaNovaSenha = null
            };

            return View(model);
        }

        // --- POST: Salvar as alterações ---
        [HttpPost]
        public async Task<IActionResult> Perfil(PerfilViewModel model)
        {
            // Se a validação do ViewModel falhar
            if (!ModelState.IsValid)
            {
                return View(model); // Retorna a View com os erros de validação
            }

            // 1. Obter o ID do usuário logado (Segurança)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id) || id != model.Id)
            {
                ModelState.AddModelError("", "Erro de segurança. Tente logar novamente.");
                return View(model);
            }

            // 2. Tentar atualizar o usuário
            var sucesso = await _usuarioService.AtualizarPerfil(model);

            if (sucesso)
            {
                TempData["Sucesso"] = "Seu cadastro foi atualizado com sucesso!";
                return RedirectToAction("Perfil");
            }
            else
            {
                ModelState.AddModelError("", "Erro ao salvar as alterações. O e-mail pode já estar em uso.");
                return View(model);
            }
        }
    }
}