using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimitiveClash.Backend.DTOs.Auth.Responses;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Services;
using PrimitiveClash.Backend.Utils.Mappers;

namespace PrimitiveClash.Backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces(MediaTypeNames.Application.Json)]
    public class AuthController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        [Authorize]
        [HttpPost("upsert")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthSuccessResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpsertUser()
        {
            try
            {
                string? oid = User.FindFirst("oid")?.Value;
                string? email = User.FindFirst("preferred_username")?.Value;

                if (string.IsNullOrWhiteSpace(oid))
                    return BadRequest(new { error = "Token inválido: no contiene el OID." });

                if (string.IsNullOrWhiteSpace(email))
                    return BadRequest(new { error = "Token inválido: no contiene el email." });

                User user = await _userService.GetOrCreateUser(oid, email);

                return Ok(user.ToAuthSuccessResponse());
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { error = "An unexpected error occurred.", details = ex.Message }
                );
            }
        }
    }
}
