using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SiteLoja.Models
{
    public class Usuario
    {
        public string? Nome { get; set; } 
        public int Id { get; set; }
        [Required (ErrorMessage = "Obrigatorio inserir email.")]
        [EmailAddress(ErrorMessage = "Digite um e-mail válido.")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Obrigatorio inserir senha.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
        public string? Senha { get; set; }
        public int? EnderecoClienteId { get; set; }
        [Required(ErrorMessage = "Obrigatorio inserir o endereço.")]
        public Endereco? EnderecoCliente { get; set; }
    }
}