using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Supplier.Auth.Controllers
{
    [Route("api/protected")]
    [ApiController]
    [Authorize]  // Exige um JWT válido
    public class ProtectedController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetSecretData()
        {
            return Ok(new { Message = "Acesso autorizado com JWT!" });
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAdminData()
        {
            return Ok(new { Message = "Somente Admins podem ver isso!" });
        }
    }
}
