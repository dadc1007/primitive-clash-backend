using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using PrimitiveClash.Backend.DTOs.Decks.Responses;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Services;
using PrimitiveClash.Backend.Utils.Mappers;

namespace PrimitiveClash.Backend.Controllers
{
    [ApiController]
    [Route("api/decks")]
    [Produces(MediaTypeNames.Application.Json)]
    public class DeckController(IDeckService deckService) : ControllerBase
    {
        private readonly IDeckService _deckService = deckService;

        [HttpGet("user/{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DeckResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDeckByUserId([FromRoute] Guid userId)
        {
            try
            {
                Deck deck = await _deckService.GetDeckByUserId(userId);
                DeckResponse response = deck.ToDeckResponse();

                return Ok(response);
            }
            catch (DeckNotFoundException)
            {
                return NotFound($"Deck for user with ID {userId} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}