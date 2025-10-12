using System.ComponentModel.DataAnnotations;

namespace PrimitiveClash.Backend.DTOs.Auth.Requests
{
    public record LoginRequest(
        [Required][EmailAddress] string Email,
        [Required] string Password
    );
}