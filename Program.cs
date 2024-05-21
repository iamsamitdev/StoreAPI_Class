using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StoreAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// For Entity Framework with Npgsql
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Adding Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Adding Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})

// Adding Jwt Bearer
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration.GetSection("JWT:ValidAudience").Value!,
        ValidIssuer = builder.Configuration.GetSection("JWT:ValidIssuer").Value!,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JWT:Secret").Value!))
    };
});

// CORS with default policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("MultipleOrigins",
        policy =>
        {
            policy.WithOrigins(
                "http://localhost:4200", // Angular framework
                "http://localhost:8080", // Angular framework
                "http://localhost:3000", // React/NextJS/ Framework
                "https://angualr-aoc.netlify.app"
            )
            .AllowAnyMethod()
            .AllowAnyHeader();
        });
});

// Cors Allow Specific
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowSpecificOrigin",
//         builder =>
//         {
//             builder.WithOrigins("http://localhost:3000", "http://localhost:4200");
//             builder.WithHeaders("Authorization", "Content-Type", "Accept", "Origin", "X-Request-With");
//             builder.WithMethods("GET", "POST", "PUT", "DELETE");
//         }
//     );
// });


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
    {
        opt.SwaggerDoc(
            "v1",
            new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Store API with .NET 8 and PostgreSQL",
                Description = "Sample Store API with .NET 8 and PostgreSQL",
            }
        );

        opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
        });

        opt.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}

            }
        });
    }
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction()) // Adjust according to your needs
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use Static Files
app.UseStaticFiles();
// app.UseStaticFiles(new StaticFileOptions()
// {
//     FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Resources")),
//     RequestPath = new PathString("/Resources")
// });

// Redirect HTTP to HTTPS
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();  // Only use HTTPS redirection in non-development environments
}

app.UseCors("MultipleOrigins");

// Add Authentication
app.UseAuthentication();

// Add Authorization
app.UseAuthorization();

// Add Controllers
app.MapControllers();

app.Run();
