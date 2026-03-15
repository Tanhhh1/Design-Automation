using Autodesk.Authentication.Model;
using DesignAutomation.Services;
using Microsoft.AspNetCore.Mvc;

namespace DesignAutomation.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly TokenService _tokenService;

        public AuthController(TokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpGet("token")]
        public async Task<IActionResult> GetToken()
        {
            TwoLeggedToken token = await _tokenService.GetTokenAsync();
            return Ok(new
            {
                access_token = token.AccessToken,
                expires_in = token.ExpiresIn
            });
        }
    }
}
