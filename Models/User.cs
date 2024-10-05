using System.ComponentModel.DataAnnotations;

public class User
{
    [Key]
    [Required]
    public string? IdUUID { get; set; }

    [Required]
    [MaxLength(20)]
    public string? Username { get; set; }

    [Required]
    public string? PasswordHash { get; set; }

    [Required]
    [MaxLength(30)]
    public string? Email { get; set; }


    [Required]
    public bool EmailConfirmed { get; set; }

    [Required]
    [MaxLength(4)]
    public string? CodeCountry { get; set; }
    [Required]
    public int Phone { get; set; }

    [Required]
    public bool PhoneConfirmed { get; set; }

    [Required]
    [MaxLength(30)]
    public string? Name { get; set; }

    [MaxLength(30)]
    public string? LastName { get; set; }

    [MaxLength(255)]
    public string? AboutMe { get; set; }

    [Required]
    [MaxLength(2)]
    public int PaidPlan { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime DeletedAt { get; set; }
}