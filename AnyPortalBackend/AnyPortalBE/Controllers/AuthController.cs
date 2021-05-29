using AnyPortalBE.Auth;
using AnyPortalBE.Models;
using AnyPortalBE.Models.JWT;
using AnyPortalBE.Models.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Security.Claims;
using System.Threading.Tasks;

namespace AnyPortalBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtFactory _jwtFactory;
        private readonly JwtIssuerOptions _jwtIssuerOptions;
        private readonly MailSettings _mailSettings;
        private readonly UIApplicationSettings _uiApplicationSettings;
        public AuthController(UserManager<AppUser> userManager, IJwtFactory jwtFactory, IOptions<JwtIssuerOptions> jwtIssuerOptions, IOptions<MailSettings> mailSettings, IOptions<UIApplicationSettings> uiApplicationSettings)
        {
            _jwtFactory = jwtFactory;
            _userManager = userManager;
            _jwtIssuerOptions = jwtIssuerOptions.Value;
            _mailSettings = mailSettings.Value;
            _uiApplicationSettings = uiApplicationSettings.Value;
        }
        [HttpPost("")]
        public async Task<IActionResult> Login([FromBody] CredentialsViewModel credentials)
        {
            bool isValidUser = false;
            ClaimsIdentity claimsIdentity;
            if (credentials.UserName != null && credentials.Password != null)
            {
                var userToVerify = await _userManager.FindByNameAsync(credentials.UserName);

                if (userToVerify != null)
                {
                    if(await _userManager.CheckPasswordAsync(userToVerify, credentials.Password))
                    {
                        isValidUser = true;
                        claimsIdentity = _jwtFactory.GenerateClaimsIdentity(credentials.UserName, userToVerify.Id);
                        return new OkObjectResult(JsonConvert.SerializeObject(new
                        {
                            id = claimsIdentity.Claims.Single(x => x.Type == "id").Value,
                            auth_token = await _jwtFactory.GenerateEncodedToken(credentials.UserName, claimsIdentity),
                            expires_in = (int)(new JwtIssuerOptions().ValidFor.TotalSeconds),
                            success = true,
                            fullName = userToVerify.FirstName
                        }, new JsonSerializerSettings { Formatting = Formatting.Indented}));
                    }
                }
            }

            return BadRequest("Invalid UserName or Password");
        }

        [HttpPost("ResetPasswordRequest")]
        public async Task<IActionResult> ResetPasswordRequest([FromBody] CredentialsViewModel credentials)
        {
            if (!string.IsNullOrWhiteSpace(credentials.UserName))
            {
                AppUser userToVerify = await _userManager.FindByNameAsync(credentials.UserName);
                if (userToVerify != null)
                {
                    string resetToken = await _userManager.GeneratePasswordResetTokenAsync(userToVerify);
                    var emailMime = new MimeMessage();
                    emailMime.Sender = new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail);
                    emailMime.To.Add(MailboxAddress.Parse(userToVerify.Email));
                    BodyBuilder builder = new BodyBuilder();
                    Uri uri = new Uri(_uiApplicationSettings.Url + "/ResetPassword?UserName=" + userToVerify.UserName + "&Act=" + resetToken);
                    builder.TextBody = "Hi " + userToVerify.FirstName + "! \n Click the below link to reset your password " + uri.AbsoluteUri;
                    emailMime.Body = builder.ToMessageBody();
                    using (var smtp = new SmtpClient())
                    {
                        smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                        smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                        await smtp.SendAsync(emailMime);
                    }
                    return new OkObjectResult(true);
                }
            }

            return new OkObjectResult(false);
        }
    }
}
