using ARD_Grup_WebApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARDGrupRapor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ARD_DbContext _databaseContext;

        public AdminController(ARD_DbContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        [HttpGet("AllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _databaseContext.Users.ToListAsync();
            return Ok(users);
        }

        [HttpGet("AllTeams")]
        public async Task<IActionResult> GetAllTeams()
        {
            var teams =await _databaseContext.Teams.ToListAsync();
            return Ok(teams);
        }

        [HttpPut("UpdateRole")]
        public async Task<IActionResult> UpdateRole(int userId, string newRole)
        {
            var user = await _databaseContext.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            var validRoles = new List<string> { "Admin", "TeamLeader", "TeamMember" };
            if (!validRoles.Contains(newRole))
            {
                return BadRequest("Invalid role.");
            }

            var existingRole = user.Roles.FirstOrDefault();
            if (existingRole != null)
            {
                user.Roles.Remove(existingRole);
            }

            user.Roles.Add(new Role { Name = newRole });
            await _databaseContext.SaveChangesAsync(); 

            return Ok();
        }


    }
}
