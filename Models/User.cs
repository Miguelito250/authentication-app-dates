using System.ComponentModel.DataAnnotations;

public class User
{
    [Key]
    [Required]
    public Guid UUID { get; set; }

    [Required]
    [MaxLength(20)]
    public required string Username { get; set; }

    [Required]
    public required string PasswordHash { get; set; }

    [Required]
    [MaxLength(30)]
    public required string Email { get; set; }


    [Required]
    public bool EmailConfirmed { get; set; }

    [Required]
    [MaxLength(4)]
    public required string CodeCountry { get; set; }

    [Required]
    [MaxLength(15)]
    public required string Phone { get; set; }

    [Required]
    public bool PhoneConfirmed { get; set; }

    [Required]
    [MaxLength(30)]
    public required string FullName { get; set; }

    [MaxLength(255)]
    public string? AboutMe { get; set; }

    [Required]
    public int PaidPlan { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime DeletedAt { get; set; }

    //Atributos de Google
    public string? GoogleId {get; set;}
    public string? ProfilePicture {get; set;}

    // Relaciones
    public required RefreshToken RefreshToken {get; set;}

}