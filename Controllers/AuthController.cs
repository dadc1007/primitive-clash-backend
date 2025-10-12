using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using PrimitiveClash.Backend.DTOs.Auth.Requests;
using PrimitiveClash.Backend.DTOs.Auth.Responses;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Services;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Utils.Mappers;

namespace PrimitiveClash.Backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces(MediaTypeNames.Application.Json)]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpPost("signup")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(AuthSuccessResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SignUp([FromBody] SignupRequest request)
        {
            try
            {
                User newUser = await _authService.RegisterUser(request.Username, request.Email, request.Password);
                AuthSuccessResponse response = newUser.ToAuthSuccessResponse("");

                return StatusCode(StatusCodes.Status201Created, response);
            }
            catch (UsernameExistsException ex)
            {
                return Conflict(new { error = ex.Message, field = "username" });
            }
            catch (EmailExistsException ex)
            {
                return Conflict(new { error = ex.Message, field = "email" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthSuccessResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                User user = await _authService.LoginUser(request.Email, request.Password);
                AuthSuccessResponse response = user.ToAuthSuccessResponse("");

                return Ok(response);
            }
            catch (InvalidCredentialsException)
            {
                return Unauthorized(new { error = "Invalid email or password." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}