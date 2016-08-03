using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ReciclaFacil.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "E-mail:")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Senha:")]
        public string Password { get; set; }

        [Display(Name = "Lembrar senha?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterClienteViewModel
    {
        //[Required(ErrorMessage = "O campo CPF: é obrigatório.")]
        //[RegularExpression("([0-9]+)", ErrorMessage = "Somente números.")]
        //[MaxLength(11, ErrorMessage = "O cpf deve conter no máximo 11 digítos.")]
        //[MinLength(11, ErrorMessage = "O cpf deve conter no minímo 11 digítos.")]
        [Display(Name = "CPF:")]
        public string cpf { get; set; }

        [Required]
        [MaxLength(75, ErrorMessage = "Máximo 75 caracteres.")]
        [Display(Name = "Nome:")]
        public string nome { get; set; }

        [Required]
        [Display(Name = "Tipo:")]
        public string tipo { get; set; }

        [Required]
        [Display(Name = "Sexo:")]
        public string sexo { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        [Display(Name = "Data Nasc.:")]
        public DateTime dataNascimento { get; set; }

        [Required]
        [MaxLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        [Display(Name = "Endereço:")]
        public string endereco { get; set; }

        [Required]
        [RegularExpression("([0-9]+)", ErrorMessage = "Somente números.")]
        [Display(Name = "Número:")]
        public int numero { get; set; }

        [Required]
        [Display(Name = "Cidade:")]
        public string cidade { get; set; }

        [Required]
        [Display(Name = "Estado:")]
        public string estado { get; set; }
        
        //[MaxLength(11, ErrorMessage = "O telefone deve conter 11 digitos.")]
        //[RegularExpression("([0-9]+)", ErrorMessage = "Somente números.")]
        [Display(Name = "Telefone:")]
        public string telefone { get; set; }

        [Required]
        //[RegularExpression("([0-9]+)", ErrorMessage = "Somente números.")]
        //[MaxLength(11, ErrorMessage = "O celular deve conter 11 digitos.")]
        [Display(Name = "Celular:")]
        public string celular { get; set; }

        [Required]
        [Display(Name = "Cooperativa:")]
        public string cooperativaId { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(45, ErrorMessage = "Máximo 45 caracteres.")]
        [Display(Name = "E-mail:")]
        public string email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "A {0} deve ser de pelo menos {2} caracteres", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha:")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar senha:")]
        [Compare("Password", ErrorMessage = "As senhas não coincidem.")]
        public string ConfirmPassword { get; set; }
    }

    public class RegisterCooperativaViewModel
    {
        [Required]
        [RegularExpression("([0-9]+)", ErrorMessage = "Somente números.")]
        [MaxLength(14, ErrorMessage = "O CNPJ deve conter 14 dígitos.")]
        [MinLength(14, ErrorMessage = "O CNPJ deve conter 14 dígitos.")]
        [Display(Name = "CNPJ:")]
        public string cnpj { get; set; }

        [Required]
        [MaxLength(100, ErrorMessage = "Máximo de 100 caracteres.")]
        [Display(Name = "Razão Social:")]
        public string razaoSocial { get; set; }

        [Required]
        [MaxLength(150, ErrorMessage = "Máximo de 150 caracteres.")]
        [Display(Name = "Endereço:")]
        public string endereco { get; set; }

        [Required]
        [RegularExpression("([0-9]+)", ErrorMessage = "Somente números.")]
        [Display(Name = "Número:")]
        public int numero { get; set; }

        [Required]
        [Display(Name = "Cidade:")]
        public string cidade { get; set; }

        [Required]
        [Display(Name = "Estado:")]
        public string estado { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(45, ErrorMessage = "Máximo 45 caracteres.")]
        [Display(Name = "E-mail:")]
        public string email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "A {0} deve ser de pelo menos {2} caracteres", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha:")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar senha:")]
        [Compare("Password", ErrorMessage = "A senha e confirmação da senha não coincidem.")]
        public string ConfirmPassword { get; set; }
    }

    public class RegisterEmpresaViewModel
    {
        [Required]
        [RegularExpression("([0-9]+)", ErrorMessage = "Somente números.")]
        [MaxLength(14, ErrorMessage = "O CNPJ deve conter 14 dígitos.")]
        [Display(Name = "CNPJ:")]
        public string cnpj { get; set; }

        [Required]
        [MaxLength(150, ErrorMessage = "Máximo de 150 caracteres.")]
        [Display(Name = "Razão Social:")]
        public string razaoSocial { get; set; }

        [Required]
        [MaxLength(150, ErrorMessage = "Máximo de 150 caracteres.")]
        [Display(Name = "Endereço:")]
        public string endereco { get; set; }

        [Required]
        [RegularExpression("([0-9]+)", ErrorMessage = "Somente números.")]
        [Display(Name = "Número:")]
        public int numero { get; set; }

        [Required]
        [Display(Name = "Cidade:")]
        public string cidade { get; set; }

        [Required]
        [Display(Name = "Estado:")]
        public string estado { get; set; }

        [Required]
        [RegularExpression("([0-9]+)", ErrorMessage = "Somente números.")]
        [MaxLength(11, ErrorMessage = "O telefone deve conter 11 dígitos.")]
        [Display(Name = "Telefone:")]
        public string telefone { get; set; }
        
        [Display(Name = "Fax:")]
        [RegularExpression("([0-9]+)", ErrorMessage = "Somente números.")]
        [MaxLength(25, ErrorMessage = "O fax deve conter 25 dígitos.")]
        public string fax { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(45, ErrorMessage = "Máximo 45 caracteres.")]
        [Display(Name = "E-mail:")]
        public string email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "A {0} deve ser de pelo menos {2} caracteres", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha:")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar senha:")]
        [Compare("Password", ErrorMessage = "A senha e confirmação da senha não coincidem.")]
        public string ConfirmPassword { get; set; }
    }

    public class RegFuncionarioViewModel
    {
        public string idFunc { get; set; }

        [Required]
        [MaxLength(45, ErrorMessage = "Máximo 45 caracteres.")]
        [Display(Name = "Nome:")]
        public string nome { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        [Display(Name = "Data Nasc.:")]
        public DateTime dataNascimento { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(45, ErrorMessage = "Máximo 45 caracteres.")]
        [Display(Name = "E-mail:")]
        public string email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "A {0} deve ser de pelo menos {2} caracteres", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha:")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar senha:")]
        [Compare("Password", ErrorMessage = "A senha e confirmação da senha não coincidem.")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email:")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password:")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password:")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
