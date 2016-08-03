using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ReciclaFacil.Models;
using ReciclaFacil.Models.Entities_RF;
using System.Collections.Generic;

namespace ReciclaFacil.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ReciclaFacil_Contexto db = new ReciclaFacil_Contexto();

        #region Tudo sobre LOGIN

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "E-mail ou senha inválidos!");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Código inválido!");
                    return View(model);
            }
        }

        #endregion

        #region TODOS OS CADASTROS
        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult RegisterCliente(string urlRetorno)
        {
            var SexoList = new List<dynamic>();
            SexoList.Add(new { Id = "M", Text = "Masculino" });
            SexoList.Add(new { Id = "F", Text = "Feminino" });
            ViewBag.sexo = new SelectList(SexoList, "Id", "Text");

            var TipoList = new List<dynamic>();
            TipoList.Add(new { Id = "D", Text = "Doador" });
            TipoList.Add(new { Id = "V", Text = "Vendedor" });
            ViewBag.tipo = new SelectList(TipoList, "Id", "Text");

            #region Preenchedo Lista de Estados do BR

            var EstadoList = new List<dynamic>();
            EstadoList.Add(new { Id = "AC", Text = "Acre" });
            EstadoList.Add(new { Id = "AL", Text = "Alagoas" });
            EstadoList.Add(new { Id = "AP", Text = "Amapá" });
            EstadoList.Add(new { Id = "AM", Text = "Amazonas" });
            EstadoList.Add(new { Id = "BA", Text = "Bahia" });
            EstadoList.Add(new { Id = "CE", Text = "Ceará" });
            EstadoList.Add(new { Id = "DF", Text = "Distrito Federal" });
            EstadoList.Add(new { Id = "ES", Text = "Espírito Santo" });
            EstadoList.Add(new { Id = "GO", Text = "Goiás" });
            EstadoList.Add(new { Id = "MA", Text = "Maranhão" });
            EstadoList.Add(new { Id = "MT", Text = "Mato Grosso" });
            EstadoList.Add(new { Id = "MS", Text = "Mato Grosso do Sul" });
            EstadoList.Add(new { Id = "MG", Text = "Minas Gerais" });
            EstadoList.Add(new { Id = "PA", Text = "Pará" });
            EstadoList.Add(new { Id = "PB", Text = "Paraíba" });
            EstadoList.Add(new { Id = "PR", Text = "Paraná" });
            EstadoList.Add(new { Id = "PE", Text = "Pernambuco" });
            EstadoList.Add(new { Id = "PI", Text = "Piauí" });
            EstadoList.Add(new { Id = "RJ", Text = "Rio de Janeiro" });
            EstadoList.Add(new { Id = "RN", Text = "Rio Grande do Norte" });
            EstadoList.Add(new { Id = "RS", Text = "Rio Grande do Sul" });
            EstadoList.Add(new { Id = "RO", Text = "Rondônia" });
            EstadoList.Add(new { Id = "RR", Text = "Roraima" });
            EstadoList.Add(new { Id = "SC", Text = "Santa Catarina" });
            EstadoList.Add(new { Id = "SP", Text = "São Paulo" });
            EstadoList.Add(new { Id = "SE", Text = "Sergipe" });
            EstadoList.Add(new { Id = "TO", Text = "Tocantins" });

            ViewBag.estado = new SelectList(EstadoList, "Id", "Text");

            #endregion

            ViewBag.cooperativas = new SelectList(db.Cooperativas, "cooperativaId", "razaoSocial");

            if(urlRetorno == "login")
                ViewBag.retorno = @Url.Action("Login", "Account"); 
            else
                ViewBag.retorno = @Url.Action("Index", "Home");

            return View();
        }
               
        [AllowAnonymous]
        public ActionResult RegisterEmpresa(string urlRetorno)
        {
            if (urlRetorno == "login")
                ViewBag.retorno = @Url.Action("Login", "Account");
            else
                ViewBag.retorno = @Url.Action("Index", "Home");

            return View();
        }

        [AllowAnonymous]
        public ActionResult RegisterCooperativa(string urlRetorno)
        {
            #region Preenchedo Lista de Estados do BR

            var EstadoList = new List<dynamic>();
            EstadoList.Add(new { Id = "AC", Text = "Acre" });
            EstadoList.Add(new { Id = "AL", Text = "Alagoas" });
            EstadoList.Add(new { Id = "AP", Text = "Amapá" });
            EstadoList.Add(new { Id = "AM", Text = "Amazonas" });
            EstadoList.Add(new { Id = "BA", Text = "Bahia" });
            EstadoList.Add(new { Id = "CE", Text = "Ceará" });
            EstadoList.Add(new { Id = "DF", Text = "Distrito Federal" });
            EstadoList.Add(new { Id = "ES", Text = "Espírito Santo" });
            EstadoList.Add(new { Id = "GO", Text = "Goiás" });
            EstadoList.Add(new { Id = "MA", Text = "Maranhão" });
            EstadoList.Add(new { Id = "MT", Text = "Mato Grosso" });
            EstadoList.Add(new { Id = "MS", Text = "Mato Grosso do Sul" });
            EstadoList.Add(new { Id = "MG", Text = "Minas Gerais" });
            EstadoList.Add(new { Id = "PA", Text = "Pará" });
            EstadoList.Add(new { Id = "PB", Text = "Paraíba" });
            EstadoList.Add(new { Id = "PR", Text = "Paraná" });
            EstadoList.Add(new { Id = "PE", Text = "Pernambuco" });
            EstadoList.Add(new { Id = "PI", Text = "Piauí" });
            EstadoList.Add(new { Id = "RJ", Text = "Rio de Janeiro" });
            EstadoList.Add(new { Id = "RN", Text = "Rio Grande do Norte" });
            EstadoList.Add(new { Id = "RS", Text = "Rio Grande do Sul" });
            EstadoList.Add(new { Id = "RO", Text = "Rondônia" });
            EstadoList.Add(new { Id = "RR", Text = "Roraima" });
            EstadoList.Add(new { Id = "SC", Text = "Santa Catarina" });
            EstadoList.Add(new { Id = "SP", Text = "São Paulo" });
            EstadoList.Add(new { Id = "SE", Text = "Sergipe" });
            EstadoList.Add(new { Id = "TO", Text = "Tocantins" });

            ViewBag.estado = new SelectList(EstadoList, "Id", "Text");

            #endregion

            if (urlRetorno == "login")
                ViewBag.retorno = @Url.Action("Login", "Account");
            else
                ViewBag.retorno = @Url.Action("Index", "Home");

            return View();
        }

        [Authorize(Roles = "Cooperativa")]
        public ActionResult RegisterFuncionario()
        {

            return View();
        }

        //
        // POST: /Account/RegisterCliente
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterCliente(RegisterClienteViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.email,
                    Email = model.email,
                    ativo = true,
                    dataCadastro = DateTime.Now
                };

                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    
                    NetGoogleGeocoding geoCoder = new NetGoogleGeocoding();

                    string endereco = model.endereco + ", " + model.numero;
                    string enderecoConsulta = endereco + " - " + model.cidade + " - " + model.estado;

                    Clientes c = new Clientes();
                    c.clienteId = user.Id;
                    //c.cpf = model.cpf.Replace(".", "").Replace("-", ""); // Se Habilitar o cadastro com CPF, descomente está linha
                    c.tipo = model.tipo.ToString();
                    c.nome = model.nome;
                    c.endereco = enderecoConsulta;
                    c.email = model.email;
                    c.sexo = model.sexo.ToString();
                    c.dataNascimento = model.dataNascimento;
                    if(model.telefone != null)
                        c.telefone = model.telefone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", ""); // adicionei o Replace (Teste) - Titans
                    c.celular = model.celular.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", ""); // adicionei o Replace (Teste) - Titans
                    c.cooperativaId = model.cooperativaId;

                    try
                    {
                        var response = geoCoder.GoogleGeocode(enderecoConsulta).GeoCodes[0];
                        if (response != null)
                        {
                            c.enderecoCoordenada = geoCoder.ConvertLatLonToDbGeometry(response.Longitude, response.Latitude);
                        }
                        else
                        {
                            c.enderecoCoordenada = null;
                        }
                    }
                    catch (Exception)
                    {
                        c.enderecoCoordenada = null;
                    }

                    Roles r = db.Roles.Find("1");
                    if (r == null)
                    {
                        r = new Roles();
                        r.Id = "1";
                        r.Name = "Cliente";
                        db.Roles.Add(r);
                    }

                    UsuarioRole ur = new UsuarioRole();
                    ur.UserId = user.Id;
                    ur.RoleId = "1";

                    db.UsuarioRole.Add(ur);

                    db.Clientes.Add(c);
                    db.SaveChanges();

                    LoginViewModel lvm = new LoginViewModel()
                    {
                        Email = model.email,
                        Password = model.Password,
                        RememberMe = false
                    };

                    return await Login(lvm, null);
                }
                AddErrors(result);
            }

            #region ValidaçãoFalha   
            var SexoList = new List<dynamic>();
            SexoList.Add(new { Id = "M", Text = "Masculino" });
            SexoList.Add(new { Id = "F", Text = "Feminino" });
            ViewBag.sexo = new SelectList(SexoList, "Id", "Text");

            var TipoList = new List<dynamic>();
            TipoList.Add(new { Id = "D", Text = "Doador" });
            TipoList.Add(new { Id = "V", Text = "Vendedor" });
            ViewBag.tipo = new SelectList(TipoList, "Id", "Text");

            var EstadoList = new List<dynamic>();
            EstadoList.Add(new { Id = "SE", Text = "Sergipe" });
            EstadoList.Add(new { Id = "BA", Text = "Bahia" });
            ViewBag.estado = new SelectList(EstadoList, "Id", "Text");

            ViewBag.cooperativas = new SelectList(db.Cooperativas, "cooperativaId", "cnpj", model.cooperativaId);
            
            ViewBag.retorno = @Url.Action("Login", "Account");

            #endregion

            return View(model);
        }

        //
        // POST: /Account/RegisterEmpresa 
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterEmpresa(RegisterEmpresaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.email,
                    Email = model.email,
                    ativo = true,
                    dataCadastro = DateTime.Now                    
                };

                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    
                    NetGoogleGeocoding geoCoder = new NetGoogleGeocoding();

                    string endereco = model.endereco + ", " + model.numero;
                    string enderecoConsulta = endereco + " - " + model.cidade + " - " + model.estado;

                    Empresas e = new Empresas();
                    e.empresaId = user.Id;
                    e.cnpj = model.cnpj;
                    e.razaoSocial = model.razaoSocial;
                    e.email = model.email;
                    e.telefone = model.telefone;
                    e.fax = model.fax;
                    e.endereco = enderecoConsulta;

                    try
                    {
                        var response = geoCoder.GoogleGeocode(enderecoConsulta).GeoCodes[0];
                        if (response != null)
                        {
                            e.enderecoCoordenada = geoCoder.ConvertLatLonToDbGeometry(response.Longitude, response.Latitude);
                        }
                        else
                        {
                            e.enderecoCoordenada = null;
                        }
                    }
                    catch (Exception)
                    {
                        e.enderecoCoordenada = null;
                    }

                    Roles r = db.Roles.Find("3");
                    if (r == null)
                    {
                        r = new Roles();
                        r.Id = "3";
                        r.Name = "Empresa";
                        db.Roles.Add(r);
                    }
                    
                    UsuarioRole ur = new UsuarioRole();
                    ur.UserId = user.Id;
                    ur.RoleId = r.Id;

                    db.UsuarioRole.Add(ur);
                    db.Empresas.Add(e);
                    db.SaveChanges();

                    LoginViewModel lvm = new LoginViewModel()
                    {
                        Email = model.email,
                        Password = model.Password,
                        RememberMe = false
                    };

                    return await Login(lvm, null);
                }
                AddErrors(result);
            }

            ViewBag.retorno = @Url.Action("Login", "Account");

            return View(model);
        }

        //
        // POST: /Account/RegisterCooperativa
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterCooperativa(RegisterCooperativaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.email,
                    Email = model.email,
                    ativo = true,
                    dataCadastro = DateTime.Now
                };

                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    
                    NetGoogleGeocoding geoCoder = new NetGoogleGeocoding();

                    string endereco = model.endereco + ", " + model.numero;
                    string enderecoConsulta = endereco + " - " + model.cidade + " - " + model.estado;

                    Cooperativas c = new Cooperativas();
                    c.cooperativaId = user.Id;
                    c.cnpj = model.cnpj;
                    c.razaoSocial = model.razaoSocial;
                    c.endereco = endereco;
                    c.cidade = model.cidade;
                    c.estado = model.estado;                    

                    try
                    {
                        var response = geoCoder.GoogleGeocode(enderecoConsulta).GeoCodes[0];
                        if (response != null)
                        {
                            c.enderecoCoordenada = geoCoder.ConvertLatLonToDbGeometry(response.Longitude, response.Latitude);
                        }
                        else
                        {
                            c.enderecoCoordenada = null;
                        }
                    }
                    catch (Exception)
                    {
                        c.enderecoCoordenada = null;
                    }

                    Roles r = db.Roles.Find("2");
                    if (r == null)
                    {
                        r = new Roles();
                        r.Id = "2";
                        r.Name = "Cooperativa";
                        db.Roles.Add(r);
                    }

                    UsuarioRole ur = new UsuarioRole();
                    ur.UserId = user.Id;
                    ur.RoleId = "2";

                    db.UsuarioRole.Add(ur);

                    db.Cooperativas.Add(c);
                    db.SaveChanges();

                    LoginViewModel lvm = new LoginViewModel()
                    {
                        Email = model.email,
                        Password = model.Password,
                        RememberMe = false
                    };

                    return await Login(lvm, null);
                }
                AddErrors(result);
            }

            ViewBag.retorno = @Url.Action("Login", "Account");

            return View(model);
        }

        //
        // POST: /Account/RegisterFuncionario
        [HttpPost]
        [Authorize(Roles = "Cooperativa")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterFuncionario(RegFuncionarioViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.email,
                    Email = model.email,
                    ativo = true,
                    dataCadastro = DateTime.Now
                };

                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    string idCoop = User.Identity.GetUserId();
                    Funcionarios f = new Funcionarios()
                    {
                        funcionarioId = user.Id,
                        cooperativaId = idCoop,
                        nome = model.nome,
                        dataNascimento = model.dataNascimento
                    };

                    Roles r = db.Roles.Find("4");
                    if (r == null)
                    {
                        r = new Roles();
                        r.Id = "4";
                        r.Name = "Funcionario";
                        db.Roles.Add(r);
                    }

                    UsuarioRole ur = new UsuarioRole();
                    ur.UserId = user.Id;
                    ur.RoleId = "4";

                    db.UsuarioRole.Add(ur);
                    db.Funcionarios.Add(f);
                    db.SaveChanges();

                    return RedirectToAction("Funcionarios", "Cooperativas");
                } else {
                    ViewBag.UsuarioExiste = "E-mail já cadastrado!";
                } 

            }

            return View(model);
        }

        #endregion

        #region Tudo sobre RECUPERAR LOGIN

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }


        #endregion

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}