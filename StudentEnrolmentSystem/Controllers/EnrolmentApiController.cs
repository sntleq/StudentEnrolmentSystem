using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using StudentEnrolmentSystem.Models;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

[ApiController]
[Route("Enrolment")]
public class EnrolmentApiController(
    IConfiguration config, ILogger<EnrolmentApiController> logger,
    CurriculumApiController curriculumApi, ScheduleApiController scheduleApi, CourseApiController courseApi
    ) : ControllerBase
{
    private readonly string _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;

    [NonAction]
    public async Task<List<Enrolment>> GetEnrolments()
    {
        var list = new List<Enrolment>();
    
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
    
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT *
            FROM enrolment
            ORDER BY ay_id, sem_id DESC";
    
        await using var reader = await cmd.ExecuteReaderAsync();
    
        while (await reader.ReadAsync())
        {
            var enrl = new Enrolment
            {
                EnrlId = reader.GetInt32(reader.GetOrdinal("enrl_id")),
                AyId = reader.GetInt32(reader.GetOrdinal("ay_id")),
                SemId = reader.GetInt32(reader.GetOrdinal("sem_id")),
                ProgId = reader.GetInt32(reader.GetOrdinal("prog_id")),
                StudId = reader.GetInt32(reader.GetOrdinal("stud_id")),
                SchedId = reader.GetInt32(reader.GetOrdinal("sched_id")),
                EnrlIsApproved = reader.GetBoolean(reader.GetOrdinal("enrl_is_approved")),
                EnrlIsCompleted = reader.GetBoolean(reader.GetOrdinal("enrl_is_completed")),
            };
        
            list.Add(enrl);
        }
    
        return list;
    }
    
    [HttpPost("", Name = "Enrolment")]
    public async Task<IActionResult> Enroll([FromForm] EnrollDto form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE student
                SET 
                    stud_status = @studStatus
                WHERE stud_id = @studId";

            cmd.Parameters.AddWithValue("studStatus", form.StudStatus);
            cmd.Parameters.AddWithValue("studId", form.StudId);
            
            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
            {
                return NotFound(new { success = false, message = "Student not found" });
            }

            var curId = curriculumApi.GetCurricula().Result.First(c => c.ProgId == form.ProgId && c.AyId == form.AyId).CurId;

            return RedirectToAction(
                "ChooseCourses", 
                "Enrolment", 
                new { curId, studId = form.StudId }
            );
        }
        catch (NpgsqlException ex)
        {
            logger.LogError(ex, "Database error.");
            return StatusCode(500, new { success = false, message = "Database error", error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error.");
            return StatusCode(500, new { success = false, message = "Unexpected error", error = ex.Message });
        }
    }
    
    [HttpPost("Enrolment/Courses", Name = "Enrolment.Courses")]
    public IActionResult ChooseCourses([FromForm] ChooseCoursesDto form)
    {
        var selectedCourses = courseApi
            .GetCourses().Result
            .Where(c => form.CrsIds.Contains(c.CrsId))
            .ToList();

        var totalUnits = selectedCourses.Sum(c => c.CrsUnits);
        var totalHours = selectedCourses.Sum(c => c.CrsHrsLec + c.CrsHrsLab);
        
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (totalHours > 36)
            return BadRequest(new { success = false, message = "Total hours must be at most 36." });
        
        if (totalUnits < 15)
            return BadRequest(new { success = false, message = "Total units must be at least 15." });
        
        var redirectUrl = Url.Action(
            "ChooseSchedules",
            "Enrolment",
            new {
                curId  = form.CurId,
                studId = form.StudId,
                crsIds = form.CrsIds
            }
        );

        // return JSON instead of a server-side redirect
        return Ok(new {
            success     = true,
            redirectUrl = redirectUrl
        });
    }
    
    [HttpPost("Enrolment/Schedules", Name = "Enrolment.Schedules")]
    public IActionResult ChooseSchedules([FromForm] ChooseSchedulesDto form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (form.SchedIds.Count != form.CrsCount)
            return BadRequest(new { success = false, message = "Enroll for exactly one schedule per course." });
        
        var redirectUrl = Url.Action(
            "Confirm",
            "Enrolment",
            new {
                curId  = form.CurId,
                studId = form.StudId,
                schedIds = form.SchedIds
            }
        );

        // return JSON instead of a server-side redirect
        return Ok(new {
            success     = true,
            redirectUrl = redirectUrl
        });
    }
    
    [HttpPost("Enrolment/Confirm", Name = "Enrolment.Confirm")]
    public async Task<IActionResult> ConfirmEnrolment([FromForm] ChooseSchedulesDto form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            await using var tx = await conn.BeginTransactionAsync();
            
            if (form.SchedIds.Count > 0)
            {
                await using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT INTO enrolment (
                        stud_id, prog_id, sched_id, ay_id, sem_id, enrl_is_completed
                    ) VALUES (
                        @studId, @progId, @schedId, @ayId, @semId, false
                    )";

                cmd.Parameters.AddWithValue("studId", form.StudId);
                var curr = curriculumApi.GetCurricula().Result.First(c => c.CurId == form.CurId);
                cmd.Parameters.AddWithValue("progId", curr.ProgId);
                cmd.Parameters.AddWithValue("ayId", curr.AyId);
                cmd.Parameters.AddWithValue("semId", (int)form.SemId!);
                cmd.Parameters.Add("schedId", NpgsqlTypes.NpgsqlDbType.Integer);

                foreach (var schedId in form.SchedIds)
                {
                    cmd.Parameters["schedId"].Value = schedId;
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            
            await tx.CommitAsync();
            
            return Ok(new
            {
                success = true,
                data = new { }
            });
        }
        catch (NpgsqlException ex)
        {
            logger.LogError(ex, "Database error.");
            return StatusCode(500, new { success = false, message = "Database error", error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error.");
            return StatusCode(500, new { success = false, message = "Unexpected error", error = ex.Message });
        }
    }
}