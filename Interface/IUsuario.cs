using SiteLoja.Models;
using SiteLoja.ViewModels;

namespace SiteLoja.Interface
{
    public interface IUsuario
    {
        bool Cadastrar(Usuario usuario);
        Usuario? Autenticar (string email, string senha);
        Task<Usuario?> BuscarPorIdComEndereco(int id);
        Task<bool> AtualizarPerfil(PerfilViewModel model);
    }
}
