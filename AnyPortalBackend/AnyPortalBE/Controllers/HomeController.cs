using AnyPortalBE.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AnyPortalBE.Controllers
{
    [Authorize]
    [Route("api")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ClaimsPrincipal _caller;
        private readonly ApplicationDbContext _appDbContext;
        public HomeController(ApplicationDbContext appDbContext, IHttpContextAccessor httpContextAccessor)
        {
            _caller = httpContextAccessor.HttpContext.User;
            _appDbContext = appDbContext;
        }

        [HttpGet("[Action]")]        
        public async Task<IActionResult> HomePage()
        {
            var userId = _caller.Claims.Single(c => c.Type == "id");
            //var customer = _appDbContext.as.Where(x=> x.Id == userId.Value).fi;
            return new OkObjectResult(new
            {
                Message = "This is secure API and user data!",
                userId.Value
                //customer.Identity.FirstName,
                //customer.Identity.LastName,
                //customer.Identity.PictureUrl,
                //customer.Identity.FacebookId,
                //customer.Location,
                //customer.Locale,
                //customer.Gender
            });
        }
    }
}
