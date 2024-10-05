using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Models; 

public class Login
{
    [Required]
    public string User { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}