using LibraryApi.Services;
using LibraryApi.Data;
using LibraryApi.Data.Seeding;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using LibraryApi.Authentication;
using Microsoft.AspNetCore.Identity;
using LibraryApi.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace LibraryApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            builder.Services.AddOpenApi();
            builder.Services.AddDbContext<LibraryDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                options.UseSqlite(connectionString);

                // Sample data for local development only
                if (builder.Environment.IsDevelopment())
                {
                    options.UseSeeding((context, _) =>
                          DevDataSeeder.Seed((LibraryDbContext)context));
                    options.UseAsyncSeeding((context, _, ct) =>
                          DevDataSeeder.SeedAsync((LibraryDbContext)context, ct));
                }

            }, ServiceLifetime.Scoped);

            //Services
            builder.Services.AddScoped<BookService>();
            builder.Services.AddScoped<ReservationService>();
            builder.Services.AddSingleton<IPasswordHasher<Member>, PasswordHasher<Member>>();
            builder.Services.AddSingleton<TokenService>();
            builder.Services.AddScoped<AuthService>();

            //Authentication
            var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
            builder.Services.AddOptions<JwtOptions>()
                            .Bind(jwtSection)
                            .ValidateDataAnnotations()
                            .ValidateOnStart();

            var jwt = jwtSection.Get<JwtOptions>() ?? throw new InvalidOperationException("Missing 'Jwt' configuration section.");

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(
                options =>
                {
                    // Keep claim names as written ("sub", "role") instead of mapping them to long URIs
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwt.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwt.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = TokenService.CreateSigningKey(jwt),
                        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30),
                        NameClaimType = AuthClaims.Name,
                        RoleClaimType = AuthClaims.Role,


                    };
                }
            );

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
