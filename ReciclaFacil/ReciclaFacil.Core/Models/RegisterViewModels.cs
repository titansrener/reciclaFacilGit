using System.ComponentModel.DataAnnotations;

namespace ReciclaFacil.Core.Models;

public abstract class RegisterBaseViewModel
{
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(256)]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Informe a senha.")]
    [StringLength(100, MinimumLength = 6,
        ErrorMessage = "A senha deve ter entre 6 e 100 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Confirme a senha.")]
    [Compare(nameof(Password), ErrorMessage = "As senhas não conferem.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar senha")]
    public string ConfirmPassword { get; set; } = "";
}

public sealed class RegisterCooperativaViewModel : RegisterBaseViewModel
{
    [Required]
    [RegularExpression(@"^\d{14}$", ErrorMessage = "O CNPJ deve conter 14 números.")]
    [Display(Name = "CNPJ")]
    public string Cnpj { get; set; } = "";

    [Required]
    [StringLength(100)]
    [Display(Name = "Razão social")]
    public string RazaoSocial { get; set; } = "";

    [Required]
    [StringLength(150)]
    [Display(Name = "Endereço")]
    public string Endereco { get; set; } = "";

    [Required]
    [StringLength(80)]
    [Display(Name = "Cidade")]
    public string Cidade { get; set; } = "";

    [Required]
    [RegularExpression(@"^[A-Za-z]{2}$", ErrorMessage = "Informe a sigla do estado.")]
    [Display(Name = "Estado")]
    public string Estado { get; set; } = "";
}

public sealed class RegisterClienteViewModel : RegisterBaseViewModel
{
    [RegularExpression(@"^\d{11}$", ErrorMessage = "O CPF deve conter 11 números.")]
    [Display(Name = "CPF")]
    public string? Cpf { get; set; }

    [Required]
    [StringLength(75)]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = "";

    [Required]
    [RegularExpression("^[VF]$", ErrorMessage = "Selecione o tipo de cliente.")]
    [Display(Name = "Tipo")]
    public string Tipo { get; set; } = "F";

    [Required]
    [RegularExpression("^[FMO]$", ErrorMessage = "Selecione uma opção válida.")]
    [Display(Name = "Sexo")]
    public string Sexo { get; set; } = "";

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Data de nascimento")]
    public DateTime DataNascimento { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Endereço")]
    public string Endereco { get; set; } = "";

    [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Informe somente números com DDD.")]
    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [Required]
    [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Informe somente números com DDD.")]
    [Display(Name = "Celular")]
    public string Celular { get; set; } = "";

    [Required(ErrorMessage = "Selecione uma cooperativa.")]
    [Display(Name = "Cooperativa")]
    public string CooperativaId { get; set; } = "";

    public IReadOnlyList<CooperativaOpcaoViewModel> Cooperativas { get; set; } = [];
}

public sealed record CooperativaOpcaoViewModel(string Id, string Nome, string Cidade, string Estado);
