using System.ComponentModel.DataAnnotations;

public class RefreshToken
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public required string Token { get; set; }
    [Required]
    public DateTime ExpirationDate { get; set; }

    // Relaciones
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public required User User { get; set; }
}