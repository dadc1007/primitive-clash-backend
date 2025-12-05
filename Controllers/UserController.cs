using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimitiveClash.Backend.DTOs.User.Responses;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Services;

namespace PrimitiveClash.Backend.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        [Authorize]
        [HttpGet("{userId:guid}/match")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserMatchStatusResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserMatchStatus([FromRoute] Guid userId)
        {
            try
            {
                Guid? matchId = await _userService.GetMatchId(userId);

                var response = new UserMatchStatusResponse(
                    UserId: userId,
                    IsInMatch: matchId.HasValue,
                    MatchId: matchId
                );

                return Ok(response);
            }
            catch (UserNotFoundException)
            {
                return NotFound($"User with ID {userId} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}