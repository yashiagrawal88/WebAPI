using System;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Models;
using WebApplication1.Services;
using System.Net.Mail;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationUserController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationSettings _appSettings;
        private readonly IEmailService _emailService;
        public ApplicationUserController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IOptions<ApplicationSettings> appSettings, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _appSettings = appSettings.Value;
            _emailService = emailService;

        }

        //to consume api
        //POST: /api/ApplicationUser/Register
        [HttpPost]
        [Route("Register")]
        public async Task<Object> PostApplicationUser(ApplicationUserModel model)
        {
           

            try
            {
                var applicationUser = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,

                };
                var result = await _userManager.CreateAsync(applicationUser, model.Password);
                if (result.Succeeded)
                {

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
                    //var callbackUrl = Url.Action(
                    //   "ConfirmEmail", "Account",
                    //   new { userId = applicationUser.Id, code = code },
                    //   protocol: Request.Url.Scheme);

                    //await _userManager.SendEmailAsync(applicationUser.Id,
                    //   "Confirm your account",
                    //   "Please confirm your account by clicking this link: <a href=\""
                    //                                   + callbackUrl + "\">link</a>");
                    // ViewBag.Link = callbackUrl;   // Used only for initial demo.
                    // return View("DisplayEmail");

                    string confirmationLink = Url.Action("ConfirmEmail",
                      "ApplicationUser", new
                      {
                          userid = applicationUser.Id,
                          token = code
                      },
                       protocol: HttpContext.Request.Scheme);
                    await _emailService.SendConfirmMail(model.Email, confirmationLink, model.FirstName);
                    return RedirectToRoute("client_URL");
                 

                    //SmtpClient client = new SmtpClient();
                    //client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    //client.PickupDirectoryLocation = @"C:\Test";

                    //client.Send("test@localhost", applicationUser.Email, "Confirm your email", confirmationLink);
                }
                //AddErrors(result);
                return Ok(result);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpPost]
        [Route("TestEmail")]
        public async Task<IActionResult> SendEmailAsync(EMailDetails model)
        {
            string email = "yashiagrawal88@gmail.com"; 
            await _emailService.SendEmail(email, model.subject, model.message);
            return Ok();
        }


        [HttpPost]
        [Route("Login")]
        //POST: /api/ApplicationUser/Login
        public async Task<IActionResult> Login(LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if(user!=null && !user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "email is not confirmed yet");
                
            }

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim("UserID", user.Id.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JWT_Secret)), SecurityAlgorithms.HmacSha256Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);
                return Ok(new { token });
            }
            else
                return BadRequest(new { message = "Email or Password is incorrect" });
        }
    }
}

