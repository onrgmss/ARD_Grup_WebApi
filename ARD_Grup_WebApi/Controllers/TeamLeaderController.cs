using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;

namespace ARD_Grup_WebApi.Controllers
{
    [Authorize(Roles = "TeamLeader")]
    [ApiController]
    [Route("api/[controller]")]
    public class TeamLeaderController : ControllerBase
    {
        private readonly ARD_DbContext _databaseContext;

        public TeamLeaderController(ARD_DbContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        [HttpGet("TeamUsers")]
        public IActionResult GetTeamUsers()
        {
            var userEmailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(userEmailClaim))
            {
                var teamLeader = _databaseContext.Users.FirstOrDefault(u => u.Email == userEmailClaim);
                if (teamLeader != null)
                {
                    var team = teamLeader.Teams;
                    if (team != null)
                    {
                        var teamMemberId = team.TeamMembers.Select(x => x.Id);

                        var teamReports = _databaseContext.Reports
                           .Where(r => teamMemberId.Contains(r.CurrentUserId))
                            .ToList();

                        return Ok(teamReports);
                    }
                }
            }

            return NotFound("User not found or no reports available for the team.");
        }
    }
}
