using ARD_Grup_WebApi.Data;
using ARD_Grup_WebApi.Models;
using ARD_Grup_WebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ARD_Grup_WebApiControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ARD_DbContext _context;
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountController(UserService userService, IConfiguration configuration, ARD_DbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _userService = userService;
            _configuration = configuration;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _userService.IsUserValid(model.Email, model.Password))
                {
                    var user = await _userService.GetUserByEmail(model.Email);

                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("PhoneNumber", user.PhoneNumber),
                new Claim("TeamName", user.Teams.TeamName)
            };

                    claims.AddRange(user.Roles.Select(ur => new Claim(ClaimTypes.Role, ur.Name)));

                    if (user.Roles.Any(ur => ur.Name == "TeamLeader"))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, "TeamLeader"));
                    }

                    if (user.Roles.Any(ur => ur.Name == "TeamMember"))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, "TeamMember"));
                    }

                    var identity = new ClaimsIdentity(claims);

                    user.Token = CreateJwt(identity);

                    var newAccessToken = user.Token;
                    var newRefreshToken = CreateRefreshToken();
                    user.RefreshToken = newRefreshToken;
                    user.RefreshTokenExpiryTime = DateTime.Now.AddDays(5);
                    await _context.SaveChangesAsync();

                    var cookieOptions = new CookieOptions
                    {
                        Expires = DateTime.UtcNow.AddHours(2),
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict
                    };
                    Response.Cookies.Append("AuthToken", newAccessToken, cookieOptions);
                    var rolenAME = await _userService.GetUserFirstRole(user.Id);

                    return Ok(new TokenApi()
                    {
                        AccessToken = newAccessToken,
                        RefreshToken = newRefreshToken,
                        RoleName = rolenAME ?? string.Empty,
                    });
                }
                else
                {
                    return BadRequest(new { Message = "Invalid email or password" });
                }
            }

            return BadRequest(new { Message = "Invalid model data" });
        }




        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _userService.IsEmailUnique(model.Email))
                
                    return BadRequest(new { Message = "Email already exists" });
                

                if (model.Password != model.RePassword)
                
                    return BadRequest(new { Message = "Passwords do not match" });
                

                var user = new User
                {
                    NameSurname = model.NameSurname,
                    Email = model.Email,
                    Password = model.Password,
                    PhoneNumber = model.PhoneNumber,
                    IsLeader = model.Roles.Contains("TeamLeader"),
                    Roles = new List<Role>()
                };

                if (user.IsLeader)
                
                    user.Roles.Add(new Role { Name = "TeamLeader" });
                
                else
                
                    user.Roles.Add(new Role { Name = "TeamMember" });
               

                var t = new Team()
                {

                    TeamName = model.TeamName
                };

                user.Teams = t;

                if (await _userService.CreateUser(user))
                {
                    foreach (var roleName in model.Roles)
                    {
                        var selectedRole = await _context.Roles.SingleOrDefaultAsync(r => r.Name == roleName);
                        if (selectedRole != null)
                        
                            user.Roles.Add(new Role { Name = selectedRole.Name });
                        
                    }

                    await _context.SaveChangesAsync();

                    return Ok(new { Message = "Registration successful" });
                }
                else
                
                    return BadRequest(new { Message = "Registration failed" });
                
            }

            return BadRequest(new { Message = "Invalid model data" });
        }



        [Authorize]
        [HttpGet("Profile")]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.Name);
            if (userIdClaim != null && int.TryParse(userIdClaim, out int userId))
            {
                var user = await _userService.GetUserById(userId);
                if (user != null)
                {
                    var roles = user.Roles.Select(ur => ur.Name).ToList();

                    return Ok(new
                    {
                        NameSurname = user.NameSurname,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        Role = roles,
                        TeamName = user.Teams.TeamName,
                        Password = user.Password,
                    });
                }
            }

            return NotFound(new { Message = "User not found" });
        }


        [Authorize]
        [HttpPut("EditProfile")]
        public async Task<IActionResult> EditProfile(EditProfileModel model)
        {
             var userId = int.Parse(User.FindFirstValue(ClaimTypes.Name));
             var user = await _userService.GetUserById(userId);

             if (user == null)

                return NotFound(new { Message = "User not found" });

    if (!string.IsNullOrWhiteSpace(model.NameSurname))
        user.NameSurname = model.NameSurname;


    if (!string.IsNullOrWhiteSpace(model.Email))
    {
        if (await _userService.IsEmailUnique(model.Email))
        
            user.Email = model.Email;

        else
        
            return BadRequest(new { Message = "Email already exists" });
        
    }

    if (!string.IsNullOrWhiteSpace(model.NewPassword) && !string.IsNullOrWhiteSpace(model.CurrentPassword))
    {
        if (await _userService.IsUserValid(user.Email, model.CurrentPassword))
        
            user.Password = model.NewPassword;
        
        else
        
            return BadRequest(new { Message = "Invalid current password" });
        
    }

    await _context.SaveChangesAsync();

    return Ok(new { Message = "Profile updated successfully" });
}


        [Authorize]
        [HttpPost("UploadProfilePhoto")]
        public async Task<IActionResult> UploadProfilePhoto(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            
                return BadRequest("Invalid file.");
            

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath??string.Empty, "uploads");

            var fileName = Path.GetFileName(file.FileName);
            string name = $"{Guid.NewGuid()}__{fileName}";

            
            var filePath = Path.Combine(uploadsFolder, name);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.Name));
            var user = await _userService.GetUserById(userId); 
            user.ProfilePhotoPath = $"/uploads/{name}"; 

            await _context.SaveChangesAsync(); 

            return Ok(new { Message = "File uploaded successfully." });
        }

        [Authorize]
        [HttpGet("Photo")]
        public async Task<IActionResult> GetPhoto()
        {
         var userId = int.Parse(User.FindFirstValue(ClaimTypes.Name));
        var user = await _userService.GetUserById(userId);
            return Ok(new { ProfilePhotoPath= "https://localhost:7020" + user.ProfilePhotoPath });

    }


        [Authorize]
        [HttpDelete("Logout")]
        public IActionResult Logout()
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(-1),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };
            Response.Cookies.Append("AuthToken", "", cookieOptions);

            return Ok(new { Message = "Logout successful" });
        }


        [AllowAnonymous]
        [HttpGet("UserRole")]
        public async Task<IActionResult> GetAllUserRoles()
        {
            try
            {
                var userRoles = new Dictionary<int, string?>();

                var users = await _context.Users.ToListAsync();

                foreach (var user in users)
                {
                    var userRole = await _userService.GetUserFirstRole(user.Id);
                    userRoles[user.Id] = userRole;
                }

                return Ok(userRoles);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while processing your request" });
            }
        }


        [AllowAnonymous]
        [HttpGet("UserId")]
        public async Task<IActionResult> GetUserIdByEmail(string email)
        {
            try
            {
                var userId = await _userService.GetUserIdByEmail(email);

                if (userId != null)
                    return Ok(userId);

                return NotFound();
            }
            catch (Exception ex)
            {
                
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        private string CreateJwt(ClaimsIdentity identity)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("onursecuritykey32948723897423897");

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.Now.AddDays(10),
                SigningCredentials = credentials
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }



        private string CreateRefreshToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var refreshToken = Convert.ToBase64String(tokenBytes);

            var tokenInUser = _context.Users
                .Any(a => a.RefreshToken == refreshToken);

            if (tokenInUser)
            {
                return refreshToken;
            }

            return refreshToken;
        }


        private ClaimsPrincipal GetPrincipleFromExpiredToken(string token)
        {
            var key = Encoding.ASCII.GetBytes("Jwt:Key");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("This is Invalid Token");
            return principal;

        }

    }
}
