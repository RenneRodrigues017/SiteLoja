using SiteLoja.Models;
using SiteLoja.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiteLoja.Interface
{
    public interface IProduto
    {
        bool Excluir(int id);
        Task AdicionarProduto(Produto produto);
        Task<List<Produto>> BuscarTodos();
        Task<List<Produto>> BuscarProdutosPorTermo(string termo);
        Task<List<Produto>> BuscarProdutosPorCategoria(string categoria);
        Task<Produto?> BuscarPorId(int id);
        Task<IEnumerable<Produto>> BuscarDestaquesPorCategoria();
        Task<IEnumerable<CategoriaViewModel>> BuscarTodasCategoriasComDestaque();
    }
}