using System.Text;
using AuthenticationService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var key = builder.Configuration["JwtSettings:SecretKey"];

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        builder =>
        {
            builder.WithOrigins("http://localhost:4200")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

builder.Services.AddControllers();


builder.Services.AddDbContext<AuthenticationContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthorization();
builder.Services.AddAuthentication("Bearer").AddJwtBearer(opt =>
{
    var signinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));
    new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256Signature);

    opt.RequireHttpsMetadata = false;

    opt.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidateAudience = false,
        ValidateIssuer = false,
        IssuerSigningKey = signinKey,
    };

    opt.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.OnStarting(async state =>
                {
                    var httpContext = (HttpContext)state;
                    if (!httpContext.Response.HasStarted)
                    {
                        httpContext.Response.StatusCode = 401;
                        httpContext.Response.ContentType = "application/json";
                        await httpContext.Response.WriteAsync("{\"message\": \"Token expired\"}");
                    }
                }, context.HttpContext);
            }
            else
            {
                context.Response.OnStarting(async state =>
                {
                    var httpContext = (HttpContext)state;
                    if (!httpContext.Response.HasStarted)
                    {
                        httpContext.Response.StatusCode = 401;
                        httpContext.Response.ContentType = "application/json";
                        await httpContext.Response.WriteAsync("{\"message\": \"Invalid token\"}");
                    }
                }, context.HttpContext);
            }

            return Task.CompletedTask;
        }
    };


});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<Jwt>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();

app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseHttpsRedirection();

app.Run();
