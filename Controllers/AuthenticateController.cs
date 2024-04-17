using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StoreAPI.Models;

namespace StoreAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticateController : ControllerBase
{

    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    // Constructor
    public AuthenticateController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    // Register for user
    [HttpPost]
    [Route("register-user")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterModel model)
    {
        var userExists = await _userManager.FindByNameAsync(model.Username!);
        if (userExists != null)
            return StatusCode(
                StatusCodes.Status500InternalServerError, 
                new Response { 
                    Status = "Error", 
                    Message = "User already exists!" 
                }
            );

        IdentityUser user = new()
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Username
        };

        var result = await _userManager.CreateAsync(user, model.Password!);

        if (!result.Succeeded)
            return StatusCode(
                StatusCodes.Status500InternalServerError, 
                new Response { 
                    Status = "Error", 
                    Message = "User creation failed! Please check user details and try again." 
                }
            );

        if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
        }

        if (!await _roleManager.RoleExistsAsync(UserRoles.Manager))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Manager));
        }

        if (!await _roleManager.RoleExistsAsync(UserRoles.User))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));
            await _userManager.AddToRoleAsync(user, UserRoles.User);
        }

        return Ok(new Response { Status = "Success", Message = "User created successfully!" });
    }

    // Register for manager
    [HttpPost]
    [Route("register-manager")]
    public async Task<IActionResult> RegisterManager([FromBody] RegisterModel model)
    {
        var userExists = await _userManager.FindByNameAsync(model.Username!);
        if (userExists != null)
            return StatusCode(
                StatusCodes.Status500InternalServerError, 
                new Response { 
                    Status = "Error", 
                    Message = "User already exists!" 
                }
            );

        IdentityUser user = new()
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Username
        };

        var result = await _userManager.CreateAsync(user, model.Password!);

        if (!result.Succeeded)
            return StatusCode(
                StatusCodes.Status500InternalServerError, 
                new Response { 
                    Status = "Error", 
                    Message = "User creation failed! Please check user details and try again." 
                }
            );

        if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
        }

        if (!await _roleManager.RoleExistsAsync(UserRoles.Manager))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Manager));
            await _userManager.AddToRoleAsync(user, UserRoles.Manager);
        }

        if (!await _roleManager.RoleExistsAsync(UserRoles.User))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));
        }

        return Ok(new Response { Status = "Success", Message = "User created successfully!" });
    }
    
    // Register for admin
    [HttpPost]
    [Route("register-admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
    {
        var userExists = await _userManager.FindByNameAsync(model.Username!);
        if (userExists != null)
            return StatusCode(
                StatusCodes.Status500InternalServerError, 
                new Response { 
                    Status = "Error", 
                    Message = "User already exists!" 
                }
            );

        IdentityUser user = new()
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Username
        };

        var result = await _userManager.CreateAsync(user, model.Password!);

        if (!result.Succeeded)
            return StatusCode(
                StatusCodes.Status500InternalServerError, 
                new Response { 
                    Status = "Error", 
                    Message = "User creation failed! Please check user details and try again." 
                }
            );

        if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            await _userManager.AddToRoleAsync(user, UserRoles.Admin);
        }

        if (!await _roleManager.RoleExistsAsync(UserRoles.Manager))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Manager));
        }

        if (!await _roleManager.RoleExistsAsync(UserRoles.User))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));
        }

        return Ok(new Response { Status = "Success", Message = "User created successfully!" });
    }

    // Login
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByNameAsync(model.Username!);

        // ถ้า login สำเร็จ
        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password!))
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = GetToken(authClaims);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }

        // ถ้า login ไม่สำเร็จ
        return Unauthorized();
    }
    
    // Refresh Token
    [HttpPost]
    [Route("refresh-token")]
    public IActionResult RefreshToken([FromBody] RefreshTokenModel model)
    {
        var authHeader = Request.Headers["Authorization"];
        if (authHeader.ToString().StartsWith("Bearer"))
        {
            var token = authHeader.ToString().Substring(7);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]!);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var user = new
                {
                    Name = jwtToken.Claims.First(x => x.Type == "unique_name").Value,
                    Role = jwtToken.Claims.First(x => x.Type == ClaimTypes.Role).Value
                };

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var newToken = GetToken(authClaims);
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(newToken),
                    expiration = newToken.ValidTo
                });
            }
            catch
            {
                return Unauthorized();
            }
        }

        return Unauthorized();
    }
    
    // Logout
    [HttpPost]
    [Route("logout")]
    public async Task<IActionResult> Logout()
    {
        var userName = User.Identity?.Name;
        if (userName != null)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user != null)
            {
                await _userManager.UpdateSecurityStampAsync(user);
                return Ok(new Response { Status = "Success", Message = "User logged out!" });
            }
        }
        return Ok();
    }
    
    // Method for generating JWT token
    private JwtSecurityToken GetToken(List<Claim> authClaims)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: DateTime.Now.AddDays(1),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );
        return token; 
    }

    public class RefreshTokenModel
    {
        public required string Token { get; set; }
        
    }

}