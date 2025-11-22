using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimitiveClash.Backend.DTOs.Card.Responses;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Services;
using PrimitiveClash.Backend.Utils.Mappers;

namespace PrimitiveClash.Backend.Controllers;

[ApiController]
[Route("api/cards")]
[Produces(MediaTypeNames.Application.Json)]
public class CardController(ICardService cardService) : ControllerBase
{
    private readonly ICardService _cardService = cardService;

    [Authorize]
    [HttpGet("{cardId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CardResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCardDetails([FromRoute] Guid cardId)
    {
        try
        {
            Card card = await _cardService.GetCardDetails(cardId);
            CardResponse response = card.ToCardResponse();

            return Ok(response);
        }
        catch (CardNotFoundException)
        {
            return NotFound($"Card with ID {cardId} not found.");
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
