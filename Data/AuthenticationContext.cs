using Microsoft.EntityFrameworkCore;
using AuthenticationService.Models;

public class AuthenticationContext : DbContext{
    public AuthenticationContext(DbContextOptions<AuthenticationContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
}