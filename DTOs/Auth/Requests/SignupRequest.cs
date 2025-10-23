using System.ComponentModel.DataAnnotations;

namespace PrimitiveClash.Backend.DTOs.Auth.Requests
{
    public record SignupRequest(
        [Required] string Username,
        [Required][EmailAddress] string Email,
        [Required] string Password
    );
}