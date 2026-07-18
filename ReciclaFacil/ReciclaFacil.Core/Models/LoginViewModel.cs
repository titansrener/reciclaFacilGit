using System.ComponentModel.DataAnnotations;

namespace ReciclaFacil.Core.Models;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Informe a senha.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Password { get; set; } = "";

    [Display(Name = "Continuar conectado")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
