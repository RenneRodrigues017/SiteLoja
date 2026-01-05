using System.ComponentModel.DataAnnotations;
using SiteLoja.Models; // Para referenciar o modelo Endereco

namespace SiteLoja.ViewModels
{
    public class PerfilViewModel
    {
        // --- Dados do Usuário ---

        // O ID é necessário para identificar qual registro atualizar no banco de dados
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
        public string? Nome { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Digite um e-mail válido.")]
        public string? Email { get; set; }

        // Senha atual e nova senha (opcional para alteração)
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
        public string? NovaSenha { get; set; }

        [DataType(DataType.Password)]
        [Compare("NovaSenha", ErrorMessage = "A nova senha e a confirmação não coincidem.")]
        public string? ConfirmaNovaSenha { get; set; }

        // --- Dados do Endereço (Herdados/Mapeados do seu Endereco Model) ---

        [Required(ErrorMessage = "O campo CEP é obrigatório.")]
        [StringLength(9, MinimumLength = 8, ErrorMessage = "O CEP deve ter 8 dígitos.")]
        [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "Formato de CEP inválido (Ex: 12345-678).")]
        public string CEP { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Rua é obrigatório.")]
        [StringLength(150)]
        public string Rua { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Número é obrigatório.")]
        [StringLength(10)]
        public string Numero { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Cidade é obrigatório.")]
        [StringLength(100)]
        public string Cidade { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Estado é obrigatório.")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "O Estado deve ser a sigla de 2 letras.")]
        public string Estado { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Complemento { get; set; }
    }
}