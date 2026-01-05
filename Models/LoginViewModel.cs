using System.ComponentModel.DataAnnotations;

namespace SiteLoja.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "O Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "O formato do Email é inválido.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "A Senha é obrigatória.")]
        [DataType(DataType.Password)]
        public string? Senha { get; set; }
    }
}