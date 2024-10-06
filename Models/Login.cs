using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Models; 

public class Login
{
    [Required]
    public required string User { get; set; }

    [Required]
    public required string Password { get; set; }
}