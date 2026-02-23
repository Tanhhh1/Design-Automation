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
        public async Task<Token> GetToken()
        {
            var token = await _tokenService.GetTokenAsync();
            return token;
        }
    }
}
