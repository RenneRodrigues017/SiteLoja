using Microsoft.EntityFrameworkCore; // Necessário para usar o .Include()
using SiteLoja.Data;
using SiteLoja.Interface;
using SiteLoja.Models;
using SiteLoja.ViewModels; // Importe o ViewModel que criamos

namespace SiteLoja.Services
{
    public class UsuarioService : IUsuario
    {
        private readonly AppDbContext _appDbContext;

        public UsuarioService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public bool Cadastrar(Usuario usuario)
        {
            if (usuario == null)
                return false;

            usuario.EnderecoClienteId = null;

            // Se o EnderecoCliente for null, isso pode indicar que o usuário não preencheu os campos.
            if (usuario.EnderecoCliente == null)
            {
                return false;
            }

            try
            {
                var UsuarioExistente = _appDbContext.Usuarios.FirstOrDefault(u => u.Email == usuario.Email);
                if (UsuarioExistente == null)
                {
                    string senhaCodificada = BCrypt.Net.BCrypt.HashPassword(usuario.Senha);
                    usuario.Senha = senhaCodificada;
                    _appDbContext.Usuarios.Add(usuario);
                    _appDbContext.SaveChanges();
                    return true;
                }

                else { return false; }
            }
            catch (Exception ex)
            {
                // Em caso de exceção (ex: Email duplicado, falha no banco de dados)
                // Você pode logar o erro aqui (usando um Logger)
                Console.WriteLine($"Erro ao salvar novo usuário: {ex.Message}");
                return false;
            }
        }

        public Usuario? Autenticar(string email, string senha)
        {
            var usuario = _appDbContext.Usuarios.FirstOrDefault(c => c.Email == email);
            if (usuario == null)
                return null;

            if (BCrypt.Net.BCrypt.Verify(senha, usuario.Senha))
            {
                return usuario;
            }

            return null;
        }
        // 1. Método para buscar o usuário e o endereço (necessário para o GET)
        public async Task<Usuario?> BuscarPorIdComEndereco(int id)
        {
            // Usamos .Include() para garantir que o EnderecoCliente seja carregado
            return await _appDbContext.Usuarios
                .Include(u => u.EnderecoCliente)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        // 2. Método para atualizar o perfil (necessário para o POST)
        public async Task<bool> AtualizarPerfil(PerfilViewModel model)
        {
            // 1. Encontra o usuário existente no banco de dados
            var usuarioDb = await _appDbContext.Usuarios
                .Include(u => u.EnderecoCliente)
                .FirstOrDefaultAsync(u => u.Id == model.Id);

            if (usuarioDb == null)
            {
                return false; // Usuário não encontrado
            }

            // 2. Checa se o novo e-mail já está em uso por outro usuário
            if (usuarioDb.Email != model.Email)
            {
                var emailExiste = await _appDbContext.Usuarios.AnyAsync(u => u.Email == model.Email);
                if (emailExiste)
                {
                    // Você pode adicionar uma mensagem mais específica no Controller
                    return false; // E-mail já em uso
                }
            }

            // 3. Atualiza as propriedades do Modelo Usuario
            usuarioDb.Nome = model.Nome;
            usuarioDb.Email = model.Email;

            // Atualiza a senha se a NovaSenha foi preenchida
            if (!string.IsNullOrEmpty(model.NovaSenha))
            {
                // A nova senha deve ser HASHED antes de ser salva
                string novaSenhaCodificada = BCrypt.Net.BCrypt.HashPassword(model.NovaSenha);
                usuarioDb.Senha = novaSenhaCodificada;
            }

            // 4. Atualiza o Endereço
            if (usuarioDb.EnderecoCliente == null)
            {
                // Se o EnderecoCliente for null, cria um novo objeto Endereco
                usuarioDb.EnderecoCliente = new Endereco();
                // A chave estrangeira deve ser tratada pelo EF Core
            }

            usuarioDb.EnderecoCliente.CEP = model.CEP;
            usuarioDb.EnderecoCliente.Rua = model.Rua;
            usuarioDb.EnderecoCliente.Numero = model.Numero;
            usuarioDb.EnderecoCliente.Cidade = model.Cidade;
            usuarioDb.EnderecoCliente.Estado = model.Estado;
            usuarioDb.EnderecoCliente.Complemento = model.Complemento;

            // 5. Salva as alterações no banco de dados
            try
            {
                await _appDbContext.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                // Tratar erros de concorrência, se necessário
                return false;
            }
            catch (Exception)
            {
                // Tratar outros erros de salvamento
                return false;
            }
        }
    }
}