using ARD_Grup_WebApi.Data;
using ARD_Grup_WebApi.Models;
using ARD_Grup_WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;

namespace ARD_Grup_WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly ARD_DbContext _databaseContext;
        private readonly UserService _userService;

        public ReportController(ARD_DbContext databaseContext,UserService userService )
        {
            _databaseContext = databaseContext;
            _userService = userService;
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var userEmailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(userEmailClaim))
            {
                var user = await _userService.GetUserByEmail(userEmailClaim);
                if (user != null)
                {
                    IQueryable<Report> reports = _databaseContext.Reports;

                    var userRoleNames = user.Roles.Select(ur => ur.Name).ToList();

                    if (!userRoleNames.Contains("Admin"))
                    {
                        reports = reports.Where(x => x.CurrentUserId == user.Id);
                    }

                    return Ok(reports.ToList());
                }
            }

            return NotFound("Report cannot be found");
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateAsync([FromBody] ReportModel model)
        {
            if (ModelState.IsValid)
            {

                var userEmailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(userEmailClaim))
                {
                    var user = await _userService.GetUserByEmail(userEmailClaim);
                    if (user != null)
                    {
                        var report = new Report
                        {
                            ReportName = model.ReportName,
                            CreatedAt = DateTime.Now,
                            LastUpdatedAt = DateTime.Now,
                            CurrentUserId = user.Id,
                            ActivitiesCarriedOut = model.ActivitiesCarriedOut,
                            WaitingActivities = model.WaitingActivities,
                            RequestCommentsAndSuggestions = model.RequestCommentsAndSuggestions,
                            ProblemsRisks = model.ProblemsRisks
                        };

                        _databaseContext.Reports.Add(report);
                        await _databaseContext.SaveChangesAsync();

                        return Ok(new { Message = "Report created successfully" });
                    }
                    else
                        return BadRequest(new { Message = "User not found" });
                    
                }
            }
            return BadRequest(new { Message = "Invalid model data" });
        }



        [HttpGet("Edit/{id}")]
        public IActionResult Edit(int id)
        {
            var report = _databaseContext.Reports.Find(id);

            if (report == null)
            {
                return NotFound(new { Message = "Report not found" });
            }

            return Ok(report);
        }

        [HttpPut("Edit/{id}")]
        public IActionResult Edit(int id, [FromBody] Report editedReport)
        {
            if (id != editedReport.Id)
            {
                return BadRequest(new { Message = "Invalid model data" });
            }

            if (ModelState.IsValid)
            {
                var report = _databaseContext.Reports.Find(id);

                if (report == null)
                {
                    return NotFound(new { Message = "Report not found" });
                }

                report.ReportName = editedReport.ReportName;
                report.ActivitiesCarriedOut = editedReport.ActivitiesCarriedOut;
                report.ProblemsRisks = editedReport.ProblemsRisks;
                report.WaitingActivities = editedReport.WaitingActivities;
                report.RequestCommentsAndSuggestions = editedReport.RequestCommentsAndSuggestions;
                report.LastUpdatedAt = DateTime.Now;

                _databaseContext.SaveChanges();

                return Ok(new { Message = "Report updated successfully" });
            }

            return BadRequest(new { Message = "Invalid model data" });
        }

        [HttpGet("ViewReport/{id}")]
        public IActionResult ViewReport(int id)
        {
            var report = _databaseContext.Reports.Find(id);

            if (report == null)

                return NotFound(new { Message = "Report not found" });
            

            return Ok(report);
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            var report = _databaseContext.Reports.Find(id);

            if (report == null)
            {
                return NotFound(new { Message = "Report not found" });
            }

            _databaseContext.Reports.Remove(report);
            _databaseContext.SaveChanges();

            return Ok(new { Message = "Report deleted successfully" });
        }
    }
}
