using AnyPortalBE.Auth;
using AnyPortalBE.Models;
using AnyPortalBE.Models.JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
        public AuthController(UserManager<AppUser> userManager, IJwtFactory jwtFactory, IOptions<JwtIssuerOptions> jwtIssuerOptions)
        {
            _jwtFactory = jwtFactory;
            _userManager = userManager;
            _jwtIssuerOptions = jwtIssuerOptions.Value;
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
                            expires_in = (int)(new JwtIssuerOptions().ValidFor.TotalSeconds)
                        }, new JsonSerializerSettings { Formatting = Formatting.Indented}));
                    }
                }
            }

            return BadRequest("Invalid UserName or Password");
        }
    }
}
